using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ProiectPAW1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
        public virtual ICollection<ArticleEdit> ArticleEdits { get; set; } = new List<ArticleEdit>();
    }
} 