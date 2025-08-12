using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectPAW1.Models
{
    public class ArticleImage
    {
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Caption { get; set; } = string.Empty;

        public string? AltText { get; set; }

        public int ArticleId { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; } = null!;
    }
} 