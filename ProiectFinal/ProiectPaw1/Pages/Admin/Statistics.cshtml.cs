using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;

namespace ProiectPAW1.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class StatisticsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StatisticsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public int TotalArticles { get; set; }
        public int TotalUsers { get; set; }
        public int TotalEdits { get; set; }
        public int ProtectedArticles { get; set; }
        public List<DomainStatistics> DomainStats { get; set; } = new();
        public List<ContributorStatistics> TopContributors { get; set; } = new();
        public List<ActivityLog> RecentActivity { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Get basic statistics
            TotalArticles = await _context.Articles.CountAsync();
            TotalUsers = await _userManager.Users.CountAsync();
            TotalEdits = await _context.ArticleEdits.CountAsync();
            ProtectedArticles = await _context.Articles.CountAsync(a => a.IsProtected);

            // Get domain statistics
            var domainGroups = await _context.Articles
                .GroupBy(a => a.Domain)
                .Select(g => new { Domain = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var domain in domainGroups)
            {
                DomainStats.Add(new DomainStatistics
                {
                    Domain = domain.Domain,
                    Count = domain.Count,
                    Percentage = TotalArticles > 0 ? (domain.Count * 100.0 / TotalArticles) : 0
                });
            }

            // Get top contributors
            var users = await _userManager.Users
                .Include(u => u.Articles)
                .ToListAsync();

            var contributors = new List<ContributorStatistics>();
            foreach (var user in users)
            {
                var editCount = await _context.ArticleEdits.CountAsync(e => e.EditorId == user.Id);
                var roles = await _userManager.GetRolesAsync(user);

                contributors.Add(new ContributorStatistics
                {
                    UserName = user.UserName!,
                    ArticleCount = user.Articles.Count,
                    EditCount = editCount,
                    Roles = roles.ToList()
                });
            }

            TopContributors = contributors
                .OrderByDescending(c => c.ArticleCount)
                .ThenByDescending(c => c.EditCount)
                .Take(10)
                .ToList();

            // Get recent activity
            var recentEdits = await _context.ArticleEdits
                .Include(e => e.Editor)
                .Include(e => e.Article)
                .OrderByDescending(e => e.EditDate)
                .Take(20)
                .Select(e => new ActivityLog
                {
                    Date = e.EditDate,
                    UserName = e.Editor.UserName!,
                    Action = "Edited",
                    ArticleId = e.ArticleId,
                    ArticleTitle = e.Article.Title
                })
                .ToListAsync();

            var recentArticles = await _context.Articles
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .Select(a => new ActivityLog
                {
                    Date = a.CreatedAt,
                    UserName = a.Author.UserName!,
                    Action = "Created",
                    ArticleId = a.Id,
                    ArticleTitle = a.Title
                })
                .ToListAsync();

            RecentActivity = recentEdits.Concat(recentArticles)
                .OrderByDescending(a => a.Date)
                .Take(20)
                .ToList();
        }
    }

    public class DomainStatistics
    {
        public string Domain { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class ContributorStatistics
    {
        public string UserName { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public int EditCount { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class ActivityLog
    {
        public DateTime Date { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int ArticleId { get; set; }
        public string ArticleTitle { get; set; } = string.Empty;
    }
} 