using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Models;

namespace ProiectPAW1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ArticleEdit> ArticleEdits { get; set; }
        public DbSet<ArticleImage> ArticleImages { get; set; }
        public DbSet<PendingArticleEdit> PendingArticleEdits { get; set; }
        public DbSet<ArticleRating> ArticleRatings { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure cascade delete for Article relationships
            builder.Entity<Article>()
                .HasMany(a => a.Chapters)
                .WithOne(c => c.Article)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Article>()
                .HasMany(a => a.ArticleEdits)
                .WithOne(e => e.Article)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Article>()
                .HasMany(a => a.Images)
                .WithOne(i => i.Article)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Article>()
                .HasMany(a => a.ArticleRatings)
                .WithOne(r => r.Article)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Article>()
                .HasMany(a => a.Comments)
                .WithOne(c => c.Article)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationships for PendingArticleEdits
            builder.Entity<PendingArticleEdit>()
                .HasOne(p => p.Article)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PendingArticleEdit>()
                .HasOne(p => p.Editor)
                .WithMany()
                .HasForeignKey(p => p.EditorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<PendingArticleEdit>()
                .HasOne(p => p.Reviewer)
                .WithMany()
                .HasForeignKey(p => p.ReviewerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Prevent multiple cascade paths
            builder.Entity<ArticleEdit>()
                .HasOne(ae => ae.Editor)
                .WithMany(u => u.ArticleEdits)
                .HasForeignKey(ae => ae.EditorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Article>()
                .HasOne(a => a.Author)
                .WithMany(u => u.Articles)
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure relationships for Comments
            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.Moderator)
                .WithMany()
                .HasForeignKey(c => c.ModeratorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure unique constraint for user ratings (one rating per article per user)
            builder.Entity<ArticleRating>()
                .HasIndex(r => new { r.ArticleId, r.UserId })
                .IsUnique();
        }
    }
} 