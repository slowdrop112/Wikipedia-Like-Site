using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ProiectPAW1.Pages.Articles
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public EditModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public Article Article { get; set; } = default!;

        [BindProperty]
        public IFormFile? NewFeaturedImage { get; set; }

        [BindProperty]
        public string? EditSummary { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Chapters)
                .Include(a => a.Images)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            Article = article;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var originalArticle = await _context.Articles
                .Include(a => a.Chapters)
                .Include(a => a.Images)
                .FirstOrDefaultAsync(a => a.Id == Article.Id);

            if (originalArticle == null)
            {
                return NotFound();
            }

            // Prevent unregistered users from editing protected articles
            if (originalArticle.IsProtected && !(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Validate chapters
            var validChapters = new List<Chapter>();
            for (int i = 0; i < Article.Chapters.Count; i++)
            {
                var chapter = Article.Chapters[i];
                if (!string.IsNullOrEmpty(chapter.Title) && !string.IsNullOrEmpty(chapter.Content))
                {
                    chapter.OrderIndex = validChapters.Count;
                    validChapters.Add(chapter);
                }
            }

            if (!validChapters.Any())
            {
                ModelState.AddModelError("", "At least one chapter with title and content is required.");
                return Page();
            }

            // For protected articles, if user is not a moderator/admin, create a pending edit
            bool requiresModeration = originalArticle.IsProtected && 
                                    User.Identity?.IsAuthenticated == true && 
                                    !User.IsInRole("Moderator") && 
                                    !User.IsInRole("Administrator");

            if (requiresModeration)
            {
                var pendingEdit = new PendingArticleEdit
                {
                    ArticleId = Article.Id,
                    EditorId = _userManager.GetUserId(User) ?? string.Empty,
                    Title = Article.Title,
                    Domain = Article.Domain,
                    IsProtected = Article.IsProtected,
                    ChaptersJson = JsonSerializer.Serialize(validChapters)
                };

                _context.PendingArticleEdits.Add(pendingEdit);
                await _context.SaveChangesAsync();

                StatusMessage = "Your changes have been submitted for review by a moderator.";
                return RedirectToPage("./Details", new { id = Article.Id });
            }

            // Update article properties
            originalArticle.Title = Article.Title;
            originalArticle.Domain = Article.Domain;
            originalArticle.IsProtected = Article.IsProtected;
            originalArticle.LastModifiedAt = DateTime.UtcNow;

            // Update chapters
            _context.Chapters.RemoveRange(originalArticle.Chapters);
            originalArticle.Chapters = validChapters;

            // Create edit history only for registered users
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var edit = new ArticleEdit
                {
                    ArticleId = originalArticle.Id,
                    EditorId = _userManager.GetUserId(User) ?? string.Empty,
                    EditDate = DateTime.UtcNow,
                    EditSummary = EditSummary,
                    PreviousContent = string.Join("\n---\n", originalArticle.Chapters.Select(c => c.Content)),
                    NewContent = string.Join("\n---\n", validChapters.Select(c => c.Content))
                };

                _context.ArticleEdits.Add(edit);
            }

            try
            {
                await _context.SaveChangesAsync();
                StatusMessage = "Article updated successfully.";
                return RedirectToPage("./Details", new { id = Article.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArticleExists(Article.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }
    }
}
