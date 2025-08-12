using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ProiectPAW1.Models
{
    public class PendingArticleEdit
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }
        
        [Required]
        public string EditorId { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public string? ReviewerId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewComment { get; set; }

        public EditStatus Status { get; set; } = EditStatus.Pending;

        // Store the proposed changes
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Domain { get; set; } = string.Empty;

        public bool IsProtected { get; set; }

        // Store chapters as JSON
        [Required]
        public string ChaptersJson { get; set; } = string.Empty;

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;

        [ForeignKey("EditorId")]
        public virtual ApplicationUser Editor { get; set; } = null!;

        [ForeignKey("ReviewerId")]
        public virtual ApplicationUser? Reviewer { get; set; }

        public int GetChapterCount()
        {
            if (string.IsNullOrEmpty(ChaptersJson))
                return 0;

            try
            {
                var chapters = JsonSerializer.Deserialize<List<Chapter>>(ChaptersJson);
                return chapters?.Count ?? 0;
            }
            catch
            {
                return 0;
            }
        }
    }

    public enum EditStatus
    {
        Pending,
        Approved,
        Rejected
    }
} 