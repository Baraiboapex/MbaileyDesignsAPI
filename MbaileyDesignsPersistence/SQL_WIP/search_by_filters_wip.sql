WITH search_results AS (
    SELECT *
    FROM blog_posts
    WHERE CAST(id AS TEXT) ILIKE '%e%'
       OR date_posted ILIKE '%e%'
       OR CAST(date_posted_iso AS TEXT) ILIKE '%e%'
       OR title ILIKE '%e%'
       OR content ILIKE '%e%'
       OR CAST(is_deleted AS TEXT) ILIKE '%e%'
)
SELECT row_to_json(sr)::jsonb
FROM search_results AS sr
RIGHT JOIN public."BlogPostCategories" AS bpc ON sr.id = bpc."BlogPostId"
RIGHT JOIN blog_categories AS bc ON bpc."BlogCategoryId" = bc.id
WHERE bc.id IN (3183184);