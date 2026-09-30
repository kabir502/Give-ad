using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;

namespace NGO_website.Controllers
{
    public class NGOsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NGOsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /NGOs
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.NGOs
                .Include(n => n.Causes.Where(c => c.IsActive))
                .Where(n => n.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(n => n.Name.Contains(search) || n.Description.Contains(search) || n.Address.Contains(search));
            }

            ViewData["Search"] = search;
            var ngos = await query.OrderBy(n => n.Name).ToListAsync();
            return View(ngos);
        }

        // GET: /NGOs/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ngo = await _context.NGOs
                .Include(n => n.Causes.Where(c => c.IsActive))
                .Include(n => n.Campaigns.Where(c => c.IsActive))
                .Include(n => n.Programmes.Where(p => p.IsActive))
                .Include(n => n.Events.Where(e => e.IsActive))
                .Include(n => n.GalleryItems.Where(g => g.IsActive))
                .FirstOrDefaultAsync(n => n.NGOId == id && n.IsActive);

            if (ngo == null)
            {
                return NotFound();
            }

            return View(ngo);
        }
    }
}
