using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;

namespace ProiectPAW1.Pages.Articles
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Article Article { get; set; } = default!;
        public ArticleRating? UserRating { get; set; }
        public IList<Comment> TopLevelComments { get; set; } = new List<Comment>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Chapters.OrderBy(c => c.OrderIndex))
                .Include(a => a.Images)
                .Include(a => a.ArticleEdits)
                    .ThenInclude(e => e.Editor)
                .Include(a => a.ArticleRatings)
                .Include(a => a.Comments.Where(c => c.ParentCommentId == null))
                    .ThenInclude(c => c.User)
                .Include(a => a.Comments.Where(c => c.ParentCommentId == null))
                    .ThenInclude(c => c.Replies)
                        .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            Article = article;
            TopLevelComments = Article.Comments.Where(c => c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = _userManager.GetUserId(User);
                UserRating = await _context.ArticleRatings
                    .FirstOrDefaultAsync(r => r.ArticleId == id && r.UserId == userId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCommentAsync(int articleId, string content)
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound("User not found");
            }

            var comment = new Comment
            {
                ArticleId = articleId,
                UserId = userId,
                Content = content
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = articleId });
        }

        public async Task<IActionResult> OnPostReplyAsync(int articleId, int parentCommentId, string content)
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound("User not found");
            }

            var reply = new Comment
            {
                ArticleId = articleId,
                UserId = userId,
                Content = content,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = articleId });
        }

        public async Task<IActionResult> OnPostEditCommentAsync(int commentId, string content)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            if (!(User.Identity?.Name == comment.User.UserName || 
                  User.IsInRole("Moderator") || 
                  User.IsInRole("Administrator")))
            {
                return Forbid();
            }

            comment.Content = content;
            comment.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = comment.ArticleId });
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            if (!(User.Identity?.Name == comment.User.UserName || 
                  User.IsInRole("Moderator") || 
                  User.IsInRole("Administrator")))
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = comment.ArticleId });
        }

        public async Task<IActionResult> OnPostModerateCommentAsync(int commentId, string reason)
        {
            if (!User.IsInRole("Moderator") && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound();
            }

            var moderatorId = _userManager.GetUserId(User);
            if (moderatorId == null)
            {
                return NotFound("Moderator not found");
            }

            comment.IsModerated = true;
            comment.ModeratorId = moderatorId;
            comment.ModerationReason = reason;
            comment.ModeratedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = comment.ArticleId });
        }

        public async Task<IActionResult> OnPostRateAsync(int articleId, int rating)
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (rating < 1 || rating > 5)
            {
                return BadRequest("Invalid rating value");
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return NotFound("User not found");
            }

            var existingRating = await _context.ArticleRatings
                .FirstOrDefaultAsync(r => r.ArticleId == articleId && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Rating = rating;
                existingRating.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var newRating = new ArticleRating
                {
                    ArticleId = articleId,
                    UserId = userId,
                    Rating = rating
                };
                _context.ArticleRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id = articleId });
        }

        public async Task<IActionResult> OnPostRevertAsync(int editId)
        {
            var edit = await _context.ArticleEdits
                .Include(e => e.Article)
                .ThenInclude(a => a.Chapters)
                .FirstOrDefaultAsync(e => e.Id == editId);

            if (edit == null)
            {
                return NotFound();
            }

            // Only moderators and administrators can revert changes
            if (!User.IsInRole("Moderator") && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            var article = edit.Article;
            
            // Split the previous content into chapters
            var previousChapters = edit.PreviousContent.Split("\n---\n")
                .Select((content, index) => new Chapter
                {
                    Title = $"Chapter {index + 1}",
                    Content = content.Trim(),
                    OrderIndex = index
                })
                .ToList();

            // Remove existing chapters
            _context.Chapters.RemoveRange(article.Chapters);
            
            // Add reverted chapters
            foreach (var chapter in previousChapters)
            {
                chapter.ArticleId = article.Id;
                _context.Chapters.Add(chapter);
            }

            article.LastModifiedAt = DateTime.UtcNow;

            // Add a new edit record for the reversion
            var revertEdit = new ArticleEdit
            {
                ArticleId = article.Id,
                EditorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!,
                EditDate = DateTime.UtcNow,
                PreviousContent = edit.NewContent,
                NewContent = edit.PreviousContent,
                EditSummary = $"Reverted to version from {edit.EditDate:g}"
            };

            _context.ArticleEdits.Add(revertEdit);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = article.Id });
        }
    }
} 