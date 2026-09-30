using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;

namespace NGO_website.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Events
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Events
                .Include(e => e.NGO)
                .Where(e => e.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search) || e.Location.Contains(search));
            }

            ViewData["Search"] = search;
            var events = await query.OrderBy(e => e.EventDate).ToListAsync();
            return View(events);
        }

        // GET: /Events/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var evt = await _context.Events
                .Include(e => e.NGO)
                .FirstOrDefaultAsync(e => e.EventId == id && e.IsActive);

            if (evt == null)
            {
                return NotFound();
            }

            return View(evt);
        }
    }
}
