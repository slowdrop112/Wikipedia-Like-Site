using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectPAW1.Models
{
    public class Chapter
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Chapter title is required")]
        [StringLength(200, ErrorMessage = "Chapter title cannot be longer than 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chapter content is required")]
        public string Content { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        public int ArticleId { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article? Article { get; set; }
    }
} 