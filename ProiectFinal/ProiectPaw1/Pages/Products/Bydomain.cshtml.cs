using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;

namespace ProiectPAW1.Pages.Articles
{
    public class ByDomainModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ByDomainModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? Domain { get; set; }

        public List<Article> Articles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Domain))
                return RedirectToPage("/Index");

            Articles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Images)
                .Include(a => a.Chapters)
                .Include(a => a.ArticleRatings)
                .Where(a => a.Domain == Domain)
                .OrderByDescending(a => a.LastModifiedAt)
                .ToListAsync();

            return Page();
        }
    }
}
