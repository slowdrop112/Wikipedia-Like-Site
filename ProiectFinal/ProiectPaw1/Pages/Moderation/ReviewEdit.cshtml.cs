using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProiectPAW1.Pages.Moderation
{
    [Authorize(Roles = "Moderator,Administrator")]
    public class ReviewEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReviewEditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PendingArticleEdit PendingEdit { get; set; }

        public List<Chapter> NewChapters { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PendingEdit = await _context.PendingArticleEdits
                .Include(p => p.Editor)
                .Include(p => p.Article)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status == EditStatus.Pending);

            if (PendingEdit == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(PendingEdit.ChaptersJson))
            {
                NewChapters = JsonSerializer.Deserialize<List<Chapter>>(PendingEdit.ChaptersJson);
            }

            return Page();
        }
    }
}
