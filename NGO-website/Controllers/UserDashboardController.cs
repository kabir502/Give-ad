using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserDashboardController> _logger;

        public UserDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<UserDashboardController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: /UserDashboard
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var donations = await _context.Donations
                .Include(d => d.NGO)
                .Include(d => d.Cause)
                .Include(d => d.Payment)
                .Where(d => d.UserId == user.Id || d.DonorEmail == user.Email)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            var upcomingProgrammes = await _context.Programmes
                .Include(p => p.NGO)
                .Where(p => p.IsActive && p.Date >= DateTime.UtcNow.Date)
                .OrderBy(p => p.Date)
                .Take(3)
                .ToListAsync();

            var recentQueries = await _context.Queries
                .Include(q => q.Replies)
                .Where(q => q.UserId == user.Id || q.Email == user.Email)
                .OrderByDescending(q => q.CreatedAt)
                .Take(3)
                .ToListAsync();

            var model = new UserDashboardViewModel
            {
                UserName = user.FullName,
                UserEmail = user.Email ?? "",
                TotalDonationsCount = donations.Count,
                ApprovedDonationsCount = donations.Count(d => d.Status == DonationStatus.Approved),
                PendingDonationsCount = donations.Count(d => d.Status == DonationStatus.Pending),
                RejectedDonationsCount = donations.Count(d => d.Status == DonationStatus.Rejected),
                TotalAmountDonated = donations.Where(d => d.Status == DonationStatus.Approved).Sum(d => d.Amount),
                ActiveQueriesCount = recentQueries.Count(q => q.Status != QueryStatus.Closed),
                RegisteredProgrammesCount = await _context.ProgrammeInterests.CountAsync(pi => pi.UserId == user.Id || pi.Email == user.Email),
                RecentDonations = donations.Take(5).ToList(),
                UpcomingProgrammes = upcomingProgrammes,
                RecentQueries = recentQueries
            };

            return View(model);
        }

        // GET: /UserDashboard/MyDonations
        public async Task<IActionResult> MyDonations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var donations = await _context.Donations
                .Include(d => d.NGO)
                .Include(d => d.Cause)
                .Include(d => d.Campaign)
                .Include(d => d.Payment)
                .Where(d => d.UserId == user.Id || d.DonorEmail == user.Email)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(donations);
        }

        // GET: /UserDashboard/MyPayments
        public async Task<IActionResult> MyPayments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var payments = await _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.NGO)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Cause)
                .Where(p => p.Donation != null && (p.Donation.UserId == user.Id || p.Donation.DonorEmail == user.Email))
                .OrderByDescending(p => p.SubmittedDate)
                .ToListAsync();

            return View(payments);
        }

        // GET: /UserDashboard/MyQueries
        public async Task<IActionResult> MyQueries()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var queries = await _context.Queries
                .Include(q => q.Replies)
                .Where(q => q.UserId == user.Id || q.Email == user.Email)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(queries);
        }

        // GET: /UserDashboard/MyProgrammes
        public async Task<IActionResult> MyProgrammes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var interests = await _context.ProgrammeInterests
                .Include(pi => pi.Programme)
                    .ThenInclude(p => p!.NGO)
                .Where(pi => pi.UserId == user.Id || pi.Email == user.Email)
                .OrderByDescending(pi => pi.RegisteredAt)
                .ToListAsync();

            return View(interests);
        }

        // GET: /UserDashboard/InviteFriend
        [HttpGet]
        public IActionResult InviteFriend()
        {
            return View(new InviteFriendViewModel());
        }

        // POST: /UserDashboard/InviteFriend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteFriend(InviteFriendViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Simple rate limit: max 5 invites per 10 minutes
            var recentCount = await _context.FriendInvitations
                .CountAsync(f => f.SenderUserId == user.Id && f.SentAt >= DateTime.UtcNow.AddMinutes(-10));

            if (recentCount >= 5)
            {
                ModelState.AddModelError(string.Empty, "You have reached the maximum invitation limit for this hour. Please try again later.");
                return View(model);
            }

            var invitation = new FriendInvitation
            {
                SenderUserId = user.Id,
                SenderName = user.FullName,
                SenderEmail = user.Email ?? "",
                FriendName = model.FriendName,
                FriendEmail = model.FriendEmail,
                PersonalMessage = model.PersonalMessage,
                SentAt = DateTime.UtcNow
            };

            _context.FriendInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            // Send invitation email
            _ = Task.Run(async () =>
            {
                try { await _emailService.SendFriendInvitationEmailAsync(invitation); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to dispatch friend invitation email to {Email}", model.FriendEmail); }
            });

            TempData["SuccessMessage"] = $"Your invitation to {model.FriendName} ({model.FriendEmail}) has been sent successfully! Thank you for spreading the cause.";
            return RedirectToAction("InviteFriend");
        }
    }
}
