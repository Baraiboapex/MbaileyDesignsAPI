using MBaileyDesignsDomain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MbaileyDesignsPersistence
{
    public class PostgresDataContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public PostgresDataContext(DbContextOptions<PostgresDataContext> options)
        : base(options)
        {
        }

        public DbSet<AboutPost> AboutPosts { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<SearchableProjectField> SearchableProjectFields { get; set; }
        public DbSet<SearchableBlogPostField> SearchableBlogPostFields { get; set; }
        public DbSet<ProjectComment> ProjectComments { get; set; }
        public DbSet<ProjectCategory> ProjectCategories { get; set; }
        public DbSet<Error> Errors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Project relationships
            modelBuilder.Entity<Project>()
                .HasMany(p => p.ProjectComments)
                .WithOne(c => c.Project)
                .HasForeignKey(c => c.ProjectId);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.ProjectCategories)
                .WithMany(c => c.Projects)
                .UsingEntity<Dictionary<string, object>>(
                "project_post_categories",
                j => j.HasOne<ProjectCategory>().WithMany().HasForeignKey("project_post_categories_id"),
                j => j.HasOne<Project>().WithMany().HasForeignKey("projects_id")
            );

            // Blog post relationships
            modelBuilder.Entity<BlogPost>()
                .HasMany(p => p.PostCategories)
                .WithMany(c => c.BlogPosts)
                .UsingEntity<Dictionary<string, object>>(
                "blog_post_categories",
                j => j.HasOne<BlogCategory>().WithMany().HasForeignKey("blog_post_categories_id"),
                j => j.HasOne<BlogPost>().WithMany().HasForeignKey("blog_posts_id")
            );

            modelBuilder.Entity<BlogPost>()
                .HasMany(p => p.PostComments)
                .WithOne(c => c.BlogPost)
                .HasForeignKey(c => c.BlogPostId);

            // Auto-incrementing set-up
            modelBuilder.Entity<Project>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<BlogPost>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<BlogCategory>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<BlogComment>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<ProjectCategory>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<ProjectComment>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<SearchableBlogPostField>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<SearchableProjectField>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<Error>().Property(p => p.Id).UseSerialColumn();
            modelBuilder.Entity<AboutPost>().Property(p => p.Id).UseSerialColumn();

            base.OnModelCreating(modelBuilder);
        }
    }
}
