using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;
using System.Text.Json;

namespace ProiectPAW1.Pages.Moderation
{
    [Authorize(Roles = "Moderator,Administrator")]
    public class PendingEditsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PendingEditsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PendingArticleEdit> PendingEdits { get; set; } = new();

        public async Task OnGetAsync()
        {
            PendingEdits = await _context.PendingArticleEdits
                .Include(p => p.Editor)
                .Include(p => p.Article)
                .Where(p => p.Status == EditStatus.Pending)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync(int editId)
        {
            var edit = await _context.PendingArticleEdits
                .Include(p => p.Article)
                .Include(p => p.Article.Chapters)
                .FirstOrDefaultAsync(p => p.Id == editId);

            if (edit == null)
            {
                return NotFound();
            }

            // Update article with approved changes
            var article = edit.Article;
            article.Title = edit.Title;
            article.Domain = edit.Domain;
            article.IsProtected = edit.IsProtected;
            article.LastModifiedAt = DateTime.UtcNow;

            // Update chapters
            var chapters = JsonSerializer.Deserialize<List<Chapter>>(edit.ChaptersJson);
            if (chapters != null)
            {
                // Remove existing chapters
                _context.Chapters.RemoveRange(article.Chapters);

                // Add new chapters with reset Ids
                foreach (var chapter in chapters)
                {
                    chapter.Id = 0;  // IMPORTANT: reset Id so EF treats this as new entity
                    chapter.ArticleId = article.Id;
                    _context.Chapters.Add(chapter);
                }
            }

            // Update edit status
            edit.Status = EditStatus.Approved;
            edit.ReviewerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            edit.ReviewedAt = DateTime.UtcNow;

            // Create edit history record
            var articleEdit = new ArticleEdit
            {
                ArticleId = article.Id,
                EditorId = edit.EditorId,
                EditDate = DateTime.UtcNow,
                PreviousContent = string.Join("\n---\n", article.Chapters.Select(c => c.Content)),
                NewContent = string.Join("\n---\n", chapters?.Select(c => c.Content) ?? Array.Empty<string>()),
                EditSummary = "Changes approved by moderator"
            };

            _context.ArticleEdits.Add(articleEdit);

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int editId)
        {
            var edit = await _context.PendingArticleEdits
                .FirstOrDefaultAsync(p => p.Id == editId);

            if (edit == null)
            {
                return NotFound();
            }

            edit.Status = EditStatus.Rejected;
            edit.ReviewerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            edit.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
