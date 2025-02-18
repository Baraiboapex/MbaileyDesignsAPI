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
    has_table BOOLEAN;
    filters_index_counter INTEGER := 0;
    current_key_name TEXT;
    filter_rec RECORD;
    dynamic_column_name TEXT;
    column_list TEXT;
    search_vector TEXT;
    main_table_alias TEXT;
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
    main_table_alias := 'main_table';  -- Default alias to use in join conditions
    
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
        
        IF current_key_name != 'table_name' AND current_key_name != 'search_text' THEN
            has_table_query := format('SELECT EXISTS(SELECT 1 FROM pg_catalog.pg_tables WHERE tablename = %L)', current_key_name);
            EXECUTE has_table_query INTO has_table;
            
            IF has_table THEN
                -- Dynamically construct the column name for the join condition
                dynamic_column_name := CASE 
                    WHEN right(json_table_name, 1) = 's' THEN left(json_table_name, length(json_table_name) - 1) || '_id'
                    ELSE json_table_name || '_id'
                END;

                join_query := format(join_query || ' RIGHT JOIN %I ON %s.%I = %I.%I',
                    current_key_name, CASE WHEN search_text = '' THEN main_table_alias ELSE 'sr' END, 'id', current_key_name, dynamic_column_name);
                
                IF table_tail_query <> '' THEN
                    table_tail_query := table_tail_query || ' AND ' || format('%I.id IN %s', current_key_name, convert_array_text_to_table_array(array_value));
                ELSE
                    table_tail_query := format('%I.id IN %s', current_key_name, convert_array_text_to_table_array(array_value));
                END IF;
            ELSE
                IF values_tail_query <> '' THEN
                    values_tail_query := values_tail_query || ' AND ' || format('%I IN %s', current_key_name, convert_array_text_to_table_array(array_value));
                ELSE
                    values_tail_query := format('%I IN %s', current_key_name, convert_array_text_to_table_array(array_value));
                END IF;
            END IF;
        END IF;
        
        filters_index_counter := filters_index_counter + 1;
    END LOOP;
    
    RAISE NOTICE 'VALUES QUERY : %', values_tail_query;
    
    -- Retrieve column names to ensure alignment
    SELECT string_agg(format('main_table.%I', column_name), ', ') 
    INTO column_list 
    FROM information_schema.columns 
    WHERE table_name = json_table_name;

    -- Construct the final query ensuring proper WHERE clause handling
    IF search_text = '' THEN
        current_query := format(
            'SELECT row_to_json(main_table)::jsonb
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
            SELECT row_to_json(sr)::jsonb
            FROM search_results AS sr
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
    
    RETURN QUERY EXECUTE current_query;
END;
