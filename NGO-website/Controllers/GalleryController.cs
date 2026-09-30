using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;

namespace NGO_website.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GalleryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Gallery
        public async Task<IActionResult> Index(string? category)
        {
            var query = _context.GalleryItems
                .Include(g => g.NGO)
                .Include(g => g.Programme)
                .Where(g => g.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category) && category != "All")
            {
                query = query.Where(g => g.Category == category);
            }

            ViewData["SelectedCategory"] = category ?? "All";
            ViewBag.Categories = await _context.GalleryItems
                .Where(g => g.IsActive)
                .Select(g => g.Category)
                .Distinct()
                .ToListAsync();

            var items = await query.OrderByDescending(g => g.DateTaken).ToListAsync();
            return View(items);
        }
    }
}
