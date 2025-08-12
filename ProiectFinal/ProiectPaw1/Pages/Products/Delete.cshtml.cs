using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;

namespace ProiectPAW1.Pages.Articles
{
    [Authorize(Roles = "Administrator,Moderator")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Article Article { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Article = await _context.Articles
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Article == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Article == null || Article.Id == 0)
            {
                return NotFound();
            }

            var articleToDelete = await _context.Articles.FindAsync(Article.Id);
            
            if (articleToDelete == null)
            {
                return NotFound();
            }

            _context.Articles.Remove(articleToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Article was successfully deleted.";
            return RedirectToPage("./Index");
        }
    }
} 