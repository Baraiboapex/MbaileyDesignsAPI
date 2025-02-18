-- WITH search_results AS (
-- 	SELECT * FROM projects WHERE CAST(id AS TEXT) ILIKE '%e%' OR date_posted ILIKE '%e%' OR CAST(date_posted_iso AS TEXT) ILIKE '%e%' OR title ILIKE '%e%' OR project_image ILIKE '%e%' OR about_project ILIKE '%e%' OR project_link ILIKE '%e%' OR CAST(is_deleted AS TEXT) ILIKE '%e%'
-- )
-- SELECT sr.*
-- FROM search_results sr
--  RIGHT JOIN project_post_categories project_post_categories ON sr.id = project_post_categories.projects_id
-- WHERE TRUE  AND project_post_categories.project_post_categories_id IN (7410655)
select * from search_by_filters(
'{
	"search_text":"e",
	"table_name":"projects",
	"projectPostCategories":[5456247]
}'::jsonb);