using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;

namespace NGO_website.Controllers
{
    public class QueriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QueriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Queries
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var queries = await _context.Queries
                .Include(q => q.Replies)
                .Where(q => q.UserId == user.Id)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(queries);
        }

        // GET: /Queries/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var query = new Query();
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    query.FullName = user.FullName;
                    query.Email = user.Email ?? "";
                    query.Phone = user.PhoneNumber;
                }
            }
            return View(query);
        }

        // POST: /Queries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Query model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.UserId = user.Id;
                }
            }

            model.CreatedAt = DateTime.UtcNow;
            model.Status = QueryStatus.Open;

            _context.Queries.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Your query has been submitted successfully (Ticket #{model.QueryId}). Our team will reply shortly.";

            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Details", new { id = model.QueryId });
        }

        // GET: /Queries/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var query = await _context.Queries
                .Include(q => q.Replies)
                .FirstOrDefaultAsync(q => q.QueryId == id);

            if (query == null)
            {
                return NotFound();
            }

            // If user is authenticated, ensure they own the query unless they are Admin
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(query.UserId) && query.UserId != userId)
                {
                    return Forbid();
                }
            }

            return View(query);
        }
    }
}
