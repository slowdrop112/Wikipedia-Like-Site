using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProiectPAW1.Data;
using ProiectPAW1.Models;

namespace ProiectPAW1.Pages.Articles
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        public Article Article { get; set; } = new Article();

        [BindProperty]
        public IFormFile? FeaturedImage { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet()
        {
            // Initialize with one empty chapter
            if (!Article.Chapters.Any())
            {
                Article.Chapters.Add(new Chapter { OrderIndex = 0 });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Set timestamps
                Article.CreatedAt = DateTime.UtcNow;
                Article.LastModifiedAt = DateTime.UtcNow;

                // If user is authenticated, set the author
                if (User.Identity?.IsAuthenticated ?? false)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        Article.AuthorId = user.Id;
                        Article.Author = user;
                    }
                }

                // Validate and process chapters
                var validChapters = new List<Chapter>();
                foreach (var chapter in Article.Chapters)
                {
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

                Article.Chapters = validChapters;

                // Validate required fields
                if (string.IsNullOrEmpty(Article.Title))
                {
                    ModelState.AddModelError("Article.Title", "Title is required.");
                    return Page();
                }

                if (string.IsNullOrEmpty(Article.Domain))
                {
                    ModelState.AddModelError("Article.Domain", "Domain is required.");
                    return Page();
                }

                // Set the Content field to handle database requirement
                Article.Content = string.Join("\n---\n", Article.Chapters.Select(c => c.Content));

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Process featured image if uploaded
                if (FeaturedImage != null)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "articles");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{FeaturedImage.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await FeaturedImage.CopyToAsync(fileStream);
                    }

                    var articleImage = new ArticleImage
                    {
                        ImagePath = $"/uploads/articles/{uniqueFileName}",
                        Caption = Article.Title,
                        AltText = Article.Title
                    };

                    Article.Images.Add(articleImage);
                }

                // Add the article to the context
                _context.Articles.Add(Article);
                await _context.SaveChangesAsync();

                StatusMessage = "Article created successfully!";
                return RedirectToPage("./Details", new { id = Article.Id });
            }
            catch (Exception ex)
            {
                // Log the full exception details
                ModelState.AddModelError("", $"Error creating article: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", $"Additional details: {ex.InnerException.Message}");
                }
                return Page();
            }
        }
    }
} 