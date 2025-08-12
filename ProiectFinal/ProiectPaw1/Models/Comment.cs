using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectPAW1.Models
{
    public class Comment
    {
        public Comment()
        {
            Replies = new List<Comment>();
        }

        public int Id { get; set; }

        public int ArticleId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }

        public bool IsModerated { get; set; }
        public string? ModeratorId { get; set; }
        public string? ModerationReason { get; set; }
        public DateTime? ModeratedAt { get; set; }

        // For hierarchical comments
        public int? ParentCommentId { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("ModeratorId")]
        public virtual ApplicationUser? Moderator { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment? ParentComment { get; set; }

        public virtual ICollection<Comment> Replies { get; set; }
    }
} 