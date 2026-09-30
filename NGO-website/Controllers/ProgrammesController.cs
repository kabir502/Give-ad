using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.User;

namespace NGO_website.Controllers
{
    public class ProgrammesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProgrammesController> _logger;

        public ProgrammesController(
            ApplicationDbContext context,
            IEmailService emailService,
            UserManager<ApplicationUser> userManager,
            ILogger<ProgrammesController> logger)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Programmes
        public async Task<IActionResult> Index(string? search, string? filter = "upcoming")
        {
            var query = _context.Programmes
                .Include(p => p.NGO)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Description.Contains(search) || p.Location.Contains(search));
            }

            var today = DateTime.UtcNow.Date;
            if (filter == "past")
            {
                query = query.Where(p => p.Date < today).OrderByDescending(p => p.Date);
            }
            else
            {
                query = query.Where(p => p.Date >= today).OrderBy(p => p.Date);
            }

            ViewData["Search"] = search;
            ViewData["Filter"] = filter ?? "upcoming";

            var programmes = await query.ToListAsync();
            return View(programmes);
        }

        // GET: /Programmes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var programme = await _context.Programmes
                .Include(p => p.NGO)
                .Include(p => p.GalleryItems.Where(g => g.IsActive))
                .FirstOrDefaultAsync(p => p.ProgrammeId == id && p.IsActive);

            if (programme == null)
            {
                return NotFound();
            }

            var model = new SubmitInterestViewModel
            {
                ProgrammeId = programme.ProgrammeId,
                ProgrammeTitle = programme.Title
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.FullName = user.FullName;
                    model.Email = user.Email ?? "";
                    model.Phone = user.PhoneNumber ?? "";
                }
            }

            ViewBag.Programme = programme;
            return View(model);
        }

        // POST: /Programmes/SubmitInterest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitInterest(SubmitInterestViewModel model)
        {
            var programme = await _context.Programmes.Include(p => p.NGO).FirstOrDefaultAsync(p => p.ProgrammeId == model.ProgrammeId);
            if (programme == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Programme = programme;
                return View("Details", model);
            }

            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = _userManager.GetUserId(User);
            }

            var interest = new ProgrammeInterest
            {
                ProgrammeId = model.ProgrammeId,
                UserId = userId,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Notes = model.Notes,
                RegisteredAt = DateTime.UtcNow,
                Status = "Confirmed"
            };

            _context.ProgrammeInterests.Add(interest);
            await _context.SaveChangesAsync();

            // Send confirmation email
            _ = Task.Run(async () =>
            {
                try { await _emailService.SendProgrammeInterestEmailAsync(interest, programme); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to send programme registration email."); }
            });

            TempData["SuccessMessage"] = $"Thank you, {model.FullName}! Your registration for '{programme.Title}' has been confirmed. A confirmation email has been dispatched.";
            return RedirectToAction("Details", new { id = model.ProgrammeId });
        }
    }
}
