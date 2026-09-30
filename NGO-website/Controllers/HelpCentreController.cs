using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.ViewModels.Home;

namespace NGO_website.Controllers
{
    public class HelpCentreController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HelpCentreController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /HelpCentre
        public async Task<IActionResult> Index(string? search, string? category)
        {
            var query = _context.FAQs
                .Where(f => f.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f => f.Question.Contains(search) || f.Answer.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                query = query.Where(f => f.Category == category);
            }

            var model = new HelpCentreViewModel
            {
                SearchQuery = search,
                SelectedCategory = category ?? "All",
                Categories = await _context.FAQs.Where(f => f.IsActive).Select(f => f.Category).Distinct().ToListAsync(),
                FAQs = await query.OrderBy(f => f.DisplayOrder).ToListAsync()
            };

            return View(model);
        }
    }
}
