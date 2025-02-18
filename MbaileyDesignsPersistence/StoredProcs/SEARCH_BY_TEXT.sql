CREATE OR REPLACE FUNCTION search_by_text(
    json_params JSONB
)
RETURNS SETOF JSONB
LANGUAGE plpgsql
AS $$
DECLARE
    json_table_name TEXT;
    search_text TEXT;
    search_query TEXT;
    column_names TEXT;
    column_list TEXT;
BEGIN
    json_table_name := json_params ->> 'table_name';
    search_text := json_params ->> 'search_text';
    
    IF json_table_name IS NULL OR search_text IS NULL THEN
        RAISE EXCEPTION 'Input JSON is missing required fields';
    END IF;
    
    -- Retrieve column names and build the search query
    SELECT string_agg(column_name || '::TEXT ILIKE ''%' || search_text || '%''', ' OR ') 
    INTO column_list 
    FROM information_schema.columns 
    WHERE table_name = json_table_name;
    
    search_query := format(
        'SELECT row_to_json(subquery)::jsonb FROM (SELECT * FROM %I WHERE %s) AS subquery',
        json_table_name,
        column_list
    );
    
    RETURN QUERY EXECUTE search_query;
END;
$$;
