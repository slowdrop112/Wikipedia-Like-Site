using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;

namespace ProiectPAW1.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Article> RecentArticles { get; set; } = new List<Article>();
        public IList<DomainStats> Domains { get; set; } = new List<DomainStats>();

        public async Task OnGetAsync()
        {
            // Get 10 most recent articles
            RecentArticles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Images)
                .Include(a => a.Chapters)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            // Get domain statistics
            Domains = await _context.Articles
                .GroupBy(a => a.Domain)
                .Select(g => new DomainStats
                {
                    Name = g.Key,
                    ArticleCount = g.Count()
                })
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
    }

    public class DomainStats
    {
        public string Name { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
    }
}
