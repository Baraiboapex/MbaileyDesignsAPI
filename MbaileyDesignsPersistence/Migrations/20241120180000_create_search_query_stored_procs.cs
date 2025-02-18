using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MbaileyDesignsPersistence.Migrations
{
    /// <inheritdoc />
    public partial class create_search_query_stored_procs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@" CREATE OR REPLACE FUNCTION search_by_text(IN json_params jsonb)
    RETURNS SETOF jsonb
    LANGUAGE 'plpgsql'
    VOLATILE
    PARALLEL UNSAFE
    COST 100    ROWS 1000 
    
AS $BODY$
DECLARE
    input_table_name TEXT;
    input_search_text TEXT;
    dynamic_query TEXT;
BEGIN
    input_table_name := json_params->>'table_name';
    input_search_text := TRIM(json_params->>'searchText');

    dynamic_query := format('
        SELECT row_to_json(sub_table)::jsonb
        FROM %I AS sub_table
        WHERE %s', input_table_name, (
            SELECT string_agg(
                format('%I::TEXT ILIKE %L', column_name, '%' || input_search_text || '%'),
                ' OR '
            )
            FROM information_schema.columns
            WHERE table_name = input_table_name
        )
    );

    RETURN QUERY EXECUTE dynamic_query;

END;
$BODY$; ");

            migrationBuilder.Sql(@" CREATE OR REPLACE FUNCTION public.search_by_filters(IN incomming_json jsonb)
    RETURNS SETOF jsonb
    LANGUAGE 'plpgsql'
    VOLATILE
    PARALLEL UNSAFE
    COST 100    ROWS 1000 
    
AS $BODY$
DECLARE
    current_query TEXT;
    values_tail_query TEXT;
    table_tail_query TEXT;
    join_query TEXT;
    json_table_name TEXT;
    search_text TEXT;
    wildcarded_search_text TEXT;
    array_key TEXT;
    array_value TEXT;
    has_table_query TEXT;
    has_column_query TEXT;
    has_table BOOLEAN;
    has_column BOOLEAN;
    filters_index_counter INTEGER := 0;
    current_key_name TEXT;
    filter_rec RECORD;
    dynamic_column_name TEXT;
    dynamic_category_id TEXT;
    column_list TEXT;
    search_vector TEXT;
    search_results_alias TEXT := 'sr';
    dynamic_table_name TEXT;
    dynamic_id_field TEXT;
    dynamic_project_id_field TEXT;
BEGIN
    json_table_name := incomming_json ->> 'table_name';
    search_text := coalesce(nullif(incomming_json ->> 'search_text', ''), '');  -- Coalesce to handle empty search_text
    wildcarded_search_text := '%' || search_text || '%';  -- Prepend and append wildcards

    IF json_table_name IS NULL THEN
        RAISE EXCEPTION 'Input JSON is missing required fields';
    END IF;

    join_query := '';
    table_tail_query := '';
    values_tail_query := '';

    -- Building search vector for text search using concatenated column values
    search_vector := (
        SELECT string_agg(
            format(
                CASE
                    WHEN data_type IN ('integer', 'boolean', 'timestamp with time zone', 'timestamp without time zone', 'date') THEN 'CAST(%I AS TEXT) ILIKE %L'
                    ELSE '%I ILIKE %L'
                END,
                column_name, wildcarded_search_text
            ),
            ' OR '
        )
        FROM information_schema.columns 
        WHERE table_name = json_table_name
    );

    FOR filter_rec IN SELECT * FROM jsonb_each_text(incomming_json)
    LOOP
        array_key := filter_rec.key;
        array_value := filter_rec.value;
        current_key_name := convert_json_fields_to_row_names(array_key);

        RAISE NOTICE 'CURRENT KEY NAME: %', current_key_name; -- Debugging current key name

        -- Skip non-table keys
        IF current_key_name = 'table_name' OR current_key_name = 'search_text' THEN
            CONTINUE;
        END IF;

        -- Check if key corresponds to a table
        has_table_query := format('SELECT EXISTS(SELECT 1 FROM pg_catalog.pg_tables WHERE tablename = %L)', current_key_name);
        EXECUTE has_table_query INTO has_table;

        RAISE NOTICE 'Table: %', current_key_name; -- Debugging table check
        RAISE NOTICE 'Exists: %', has_table; -- Debugging table existence

        IF has_table THEN
            -- Dynamically construct the column name for the join condition
            dynamic_column_name := json_table_name || '_id';

			RAISE NOTICE 'HAS CLOUMN CHECK %', dynamic_column_name;
			
            -- Validate if the dynamic_column_name exists in the current_key_name table
            has_column_query := format('SELECT EXISTS(SELECT 1 FROM information_schema.columns WHERE table_name = %L AND column_name = %L)', current_key_name, dynamic_column_name);
            EXECUTE has_column_query INTO has_column;

            IF has_column THEN
				
                -- Construct dynamic field for the project_id in the join and WHERE clause
                dynamic_id_field := dynamic_column_name; -- Ensure using project_id for the join condition

                join_query := join_query || format(' RIGHT JOIN %I %I ON %I.%I = %I.%I',
                    current_key_name, current_key_name, search_results_alias, 'id', current_key_name, dynamic_id_field);

                RAISE NOTICE 'JOIN QUERY DEBUG: %', join_query;  -- Debugging notice for each join

                IF table_tail_query <> '' THEN
                    table_tail_query := table_tail_query || ' AND ' || format('%I.%I_id IN %s', current_key_name, current_key_name, convert_array_text_to_table_array(array_value));
                ELSE
                    table_tail_query := format('%I.%I_id IN %s', current_key_name, current_key_name, convert_array_text_to_table_array(array_value));
                END IF;
            END IF;
        ELSE
            -- Handle columns separately
            IF current_key_name = 'date_posted' THEN
                IF values_tail_query <> '' THEN
                    values_tail_query := values_tail_query || ' AND ' || format('sr.date_posted IN %s', convert_array_text_to_table_array(array_value));
                ELSE
                    values_tail_query := format('sr.date_posted IN %s', convert_array_text_to_table_array(array_value));
                END IF;
            ELSIF current_key_name LIKE '%Category' THEN
                -- Dynamically construct the category ID field
                dynamic_category_id := lower(current_key_name) || '_id';
				
				RAISE NOTICE 'Table 2: %', current_key_name; -- Debugging table check
				
                IF values_tail_query <> '' THEN
                    values_tail_query := values_tail_query || ' AND ' || format('sr.%I IN %s', dynamic_category_id, convert_array_text_to_table_array(array_value));
                ELSE
                    values_tail_query := format('sr.%I IN %s', dynamic_category_id, convert_array_text_to_table_array(array_value));
                END IF;
            END IF;
        END IF;

        filters_index_counter := filters_index_counter + 1;
    END LOOP;

    RAISE NOTICE 'VALUES QUERY : %', values_tail_query;

    -- Retrieve column names to ensure alignment
    SELECT string_agg(format('%I.%I', search_results_alias, column_name), ', ') 
    INTO column_list 
    FROM information_schema.columns 
    WHERE table_name = json_table_name;

    -- Construct the final query ensuring proper WHERE clause handling
    IF search_text = '' THEN
        current_query := format(
            'SELECT main_table.* 
             FROM (
                 SELECT %s 
                 FROM %I AS main_table
                 %s 
                 WHERE %s %s
             ) AS main_table',
            column_list,
            json_table_name,
            join_query,
            COALESCE(NULLIF(values_tail_query, ''), 'TRUE'),
            CASE 
                WHEN table_tail_query <> '' THEN ' AND ' || table_tail_query
                ELSE ''
            END
        );
    ELSE
        current_query := format(
            'WITH search_results AS (
                SELECT * FROM %I WHERE %s
            )
            SELECT sr.*
            FROM search_results sr
            %s
            WHERE %s %s',
            json_table_name,
            search_vector,
            join_query,
            COALESCE(NULLIF(values_tail_query, ''), 'TRUE'),
            CASE 
                WHEN table_tail_query <> '' THEN ' AND ' || table_tail_query
                ELSE ''
            END
        );
    END IF;

    RAISE NOTICE 'QUERY : %', current_query;

    -- Parse the result as JSON after the querying is done
    RETURN QUERY EXECUTE format('SELECT row_to_json(results)::jsonb FROM (%s) AS results', current_query);
END;
$BODY$;");
            migrationBuilder.Sql(@" CREATE OR REPLACE FUNCTION get_all_table_single_field_values(IN incomming_json jsonb)
    RETURNS SETOF jsonb
    LANGUAGE 'plpgsql'
    VOLATILE
    PARALLEL UNSAFE
    COST 100    ROWS 1000 
    
AS $BODY$
DECLARE 
	current_table_name_from_json TEXT;
	current_column_to_get TEXT;
BEGIN
	current_table_name_from_json := incomming_json ->> 'table_name';
	current_column_to_get := REGEXP_REPLACE(
		incomming_json ->> 'field_name',
		'([A-Z])',
		'_\1',
		'gm'
	);
	
	RETURN QUERY EXECUTE format(
		'SELECT row_to_json(curr_row)::jsonb FROM (SELECT %s FROM %I ) AS curr_row', 
		current_column_to_get, 
		current_table_name_from_json
	);
	
END;
$BODY$;");
            migrationBuilder.Sql(@" CREATE OR REPLACE FUNCTION convert_json_fields_to_row_names(IN incomming_json_key text)
    RETURNS text
    LANGUAGE 'plpgsql'
    VOLATILE
    PARALLEL UNSAFE
    COST 100
    
AS $BODY$
BEGIN
	RETURN LOWER(
		REGEXP_REPLACE(
			incomming_json_key,
			'([A-Z])',
			'_\1',
			'g'
		)
	);
END;
$BODY$; ");

			migrationBuilder.Sql(@"
		CREATE OR REPLACE FUNCTION convert_array_text_to_table_array(IN incomming_array_text text)
    RETURNS text
    LANGUAGE 'plpgsql'
    VOLATILE
    PARALLEL UNSAFE
    COST 100
    
AS $BODY$
BEGIN
RETURN REGEXP_REPLACE(
	'('|| 
	(REGEXP_REPLACE(
	incomming_array_text,
	'(\[)|(\])',
	'',
	'gm'
	)) ||')',
	'""',
	'''',
	'gm'
);
END;
$BODY$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS search_by_text(jsonb)");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS search_by_filters(jsonb)");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS get_all_table_single_field_values(jsonb)");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS convert_json_fields_to_table_names(jsonb)");
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS convert_array_text_to_table_array(text)");
        }
    }
}
