using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;

namespace ProiectPAW1.Pages.Articles
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Article> Articles { get; set; } = new List<Article>();
        public SelectList DomainList { get; set; } = null!;

        [BindProperty(SupportsGet = true)]
        public string? CurrentDomain { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "newest";

        public async Task OnGetAsync()
        {
            // Create domain list for dropdown
            var domains = await _context.Articles
                .Select(a => a.Domain)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
            DomainList = new SelectList(domains);

            // Build query
            var query = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Images)
                .Include(a => a.Chapters)
                .Include(a => a.ArticleRatings)
                .AsQueryable();

            // Apply domain filter
            if (!string.IsNullOrEmpty(CurrentDomain))
            {
                query = query.Where(a => a.Domain == CurrentDomain);
            }

            // Apply sorting
            query = SortOrder?.ToLower() switch
            {
                "oldest" => query.OrderBy(a => a.CreatedAt),
                "title" => query.OrderBy(a => a.Title),
                "title_desc" => query.OrderByDescending(a => a.Title),
                "rating" => query.OrderByDescending(a => a.ArticleRatings.Any() ? a.ArticleRatings.Average(r => r.Rating) : 0),
                "rating_asc" => query.OrderBy(a => a.ArticleRatings.Any() ? a.ArticleRatings.Average(r => r.Rating) : 0),
                _ => query.OrderByDescending(a => a.LastModifiedAt) // "newest" is default
            };

            Articles = await query.ToListAsync();
        }
    }
} 