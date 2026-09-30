using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;

namespace NGO_website.Controllers
{
    public class CausesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CausesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Causes
        public async Task<IActionResult> Index(string? search, string? category, int? ngoId)
        {
            var query = _context.Causes
                .Include(c => c.NGO)
                .Where(c => c.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search) || c.Description.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                query = query.Where(c => c.Category == category);
            }

            if (ngoId.HasValue && ngoId.Value > 0)
            {
                query = query.Where(c => c.NGOId == ngoId.Value);
            }

            ViewData["Search"] = search;
            ViewData["SelectedCategory"] = category ?? "All";
            ViewData["SelectedNgoId"] = ngoId;

            ViewBag.Categories = await _context.Causes
                .Where(c => c.IsActive)
                .Select(c => c.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.NGOs = await _context.NGOs
                .Where(n => n.IsActive)
                .OrderBy(n => n.Name)
                .ToListAsync();

            var causes = await query.OrderByDescending(c => c.CreatedDate).ToListAsync();
            return View(causes);
        }

        // GET: /Causes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var cause = await _context.Causes
                .Include(c => c.NGO)
                .Include(c => c.Campaigns.Where(cmp => cmp.IsActive))
                .FirstOrDefaultAsync(c => c.CauseId == id && c.IsActive);

            if (cause == null)
            {
                return NotFound();
            }

            return View(cause);
        }
    }
}
