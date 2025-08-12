using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProiectPAW1.Data;
using ProiectPAW1.Models;
using System.Text.RegularExpressions;

namespace ProiectPAW1.Pages
{
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SearchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? SearchInContent { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Domain { get; set; }

        public List<SearchResult> SearchResults { get; set; } = new();

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            var query = _context.Articles
                .Include(a => a.Author)
                .Include(a => a.Chapters)
                .AsQueryable();

            // Apply domain filter
            if (!string.IsNullOrEmpty(Domain))
            {
                query = query.Where(a => a.Domain == Domain);
            }

            // Search in title and chapters
            var searchTerms = SearchQuery.ToLower().Split(' ');
            query = query.Where(a => searchTerms.All(term =>
                a.Title.ToLower().Contains(term) ||
                (SearchInContent == true &&
                    a.Chapters.Any(c =>
                        c.Title.ToLower().Contains(term) ||
                        c.Content.ToLower().Contains(term))
                )
            ));

            var articles = await query.ToListAsync();

            // Process results and highlight matches
            foreach (var article in articles)
            {
                var result = new SearchResult
                {
                    Id = article.Id,
                    Title = article.Title,
                    Domain = article.Domain,
                    Author = article.Author,
                    IsProtected = article.IsProtected,
                    LastModifiedAt = article.LastModifiedAt
                };

                if (SearchInContent == true && article.Chapters.Any())
                {
                    var content = string.Join(" ", article.Chapters.Select(c => c.Content));

                    var matchIndex = -1;
                    foreach (var term in searchTerms)
                    {
                        matchIndex = content.ToLower().IndexOf(term.ToLower());
                        if (matchIndex >= 0)
                            break;
                    }

                    if (matchIndex >= 0)
                    {
                        var start = Math.Max(0, matchIndex - 50);
                        var length = Math.Min(200, content.Length - start);
                        var snippet = content.Substring(start, length);

                        if (start > 0) snippet = "..." + snippet;
                        if (start + length < content.Length) snippet += "...";

                        foreach (var term in searchTerms)
                        {
                            snippet = Regex.Replace(
                                snippet,
                                Regex.Escape(term),
                                m => $"<span class=\"search-highlight\">{m.Value}</span>",
                                RegexOptions.IgnoreCase
                            );
                        }

                        result.MatchedContent = snippet;
                    }
                }

                SearchResults.Add(result);
            }

            // Sort results by relevance
            SearchResults = SearchResults
                .OrderByDescending(r => searchTerms.All(term => r.Title.ToLower().Contains(term.ToLower())))
                .ThenByDescending(r => r.LastModifiedAt)
                .ToList();
        }
    }

    public class SearchResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public ApplicationUser? Author { get; set; }
        public bool IsProtected { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public string? MatchedContent { get; set; }
    }
}
