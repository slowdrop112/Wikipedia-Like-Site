using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectPAW1.Models
{
    public class ArticleEdit
    {
        public int Id { get; set; }

        public int ArticleId { get; set; }
        
        [Required]
        public string EditorId { get; set; } = string.Empty;

        public DateTime EditDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string PreviousContent { get; set; } = string.Empty;

        [Required]
        public string NewContent { get; set; } = string.Empty;

        public string? EditSummary { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;

        [ForeignKey("EditorId")]
        public virtual ApplicationUser Editor { get; set; } = null!;
    }
} 