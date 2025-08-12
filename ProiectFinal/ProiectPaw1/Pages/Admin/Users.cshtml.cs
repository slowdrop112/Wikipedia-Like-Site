using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;

namespace ProiectPAW1.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Role { get; set; }

        public List<UserViewModel> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            var query = _userManager.Users
                .Include(u => u.Articles)
                .OrderBy(u => u.UserName)
                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(u => 
                    u.UserName!.Contains(SearchQuery) || 
                    u.Email!.Contains(SearchQuery));
            }

            var users = await query.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Filter by role if specified
                if (!string.IsNullOrEmpty(Role) && !roles.Contains(Role))
                    continue;

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName!,
                    Email = user.Email!,
                    Roles = roles.ToList(),
                    JoinDate = user.JoinDate,
                    ArticleCount = user.Articles.Count
                });
            }

            Users = userViewModels;
        }

        public async Task<IActionResult> OnPostAddRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Prevent removing the last administrator
            if (role == "Administrator")
            {
                var adminCount = (await _userManager.GetUsersInRoleAsync("Administrator")).Count;
                if (adminCount <= 1)
                {
                    ModelState.AddModelError("", "Cannot remove the last administrator.");
                    return RedirectToPage();
                }
            }

            if (await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return RedirectToPage();
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public DateTime JoinDate { get; set; }
        public int ArticleCount { get; set; }
    }
} 