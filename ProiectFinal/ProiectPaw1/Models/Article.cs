using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ProiectPAW1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string Size { get; set; }
        public decimal Price { get; set; }
        public bool IsNegotiable { get; set; }
        public List<ArticleImage> Images { get; set; }
    }

    public class Article
    {
        public Article()
        {
            Chapters = new List<Chapter>();
            Images = new List<ArticleImage>();
            ArticleEdits = new List<ArticleEdit>();
            ArticleRatings = new List<ArticleRating>();
            Comments = new List<Comment>();
            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime LastModifiedAt { get; set; }

        [Required(ErrorMessage = "Please select a domain")]
        public string Domain { get; set; } = string.Empty;

        public bool IsProtected { get; set; } = false;

        public string? AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public virtual ApplicationUser? Author { get; set; }

        public List<Chapter> Chapters { get; set; }
        public virtual ICollection<ArticleEdit> ArticleEdits { get; set; }
        public virtual ICollection<ArticleImage> Images { get; set; }
        public virtual ICollection<ArticleRating> ArticleRatings { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        [NotMapped]
        public double AverageRating => ArticleRatings.Any() ? ArticleRatings.Average(r => r.Rating) : 0;

        [NotMapped]
        public int RatingCount => ArticleRatings.Count;

        [NotMapped]
        public int CommentCount => Comments.Count + Comments.SelectMany(c => c.Replies).Count();

        // Temporary property to handle database requirement
        public string Content { get; set; } = string.Empty;
    }
} 