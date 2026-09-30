using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminQueriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AdminQueriesController> _logger;

        public AdminQueriesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<AdminQueriesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: /AdminQueries
        public async Task<IActionResult> Index(QueryStatus? status, string? search)
        {
            var query = _context.Queries
                .Include(q => q.Replies)
                .Include(q => q.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(q => q.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q => q.Subject.Contains(search) || q.FullName.Contains(search) || q.Email.Contains(search));
            }

            ViewData["SelectedStatus"] = status;
            ViewData["Search"] = search;

            var queries = await query.OrderByDescending(q => q.CreatedAt).ToListAsync();
            return View(queries);
        }

        // GET: /AdminQueries/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var query = await _context.Queries
                .Include(q => q.Replies)
                    .ThenInclude(r => r.AdminUser)
                .Include(q => q.User)
                .FirstOrDefaultAsync(q => q.QueryId == id);

            if (query == null) return NotFound();
            return View(query);
        }

        // POST: /AdminQueries/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int queryId, string replyMessage, QueryStatus? newStatus)
        {
            if (string.IsNullOrWhiteSpace(replyMessage))
            {
                TempData["ErrorMessage"] = "Reply message cannot be empty.";
                return RedirectToAction("Details", new { id = queryId });
            }

            var query = await _context.Queries.Include(q => q.Replies).FirstOrDefaultAsync(q => q.QueryId == queryId);
            if (query == null) return NotFound();

            var adminUser = await _userManager.GetUserAsync(User);
            var reply = new QueryReply
            {
                QueryId = queryId,
                AdminUserId = adminUser?.Id,
                AdminName = adminUser?.FullName ?? "GIVE-AID Support",
                Message = replyMessage,
                RepliedAt = DateTime.UtcNow
            };

            _context.QueryReplies.Add(reply);

            query.Status = newStatus ?? QueryStatus.Resolved;
            query.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send email to user
            _ = Task.Run(async () =>
            {
                try { await _emailService.SendQueryReplyEmailAsync(query, reply); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to send query reply email."); }
            });

            TempData["SuccessMessage"] = "Reply sent and email notification dispatched to the user.";
            return RedirectToAction("Details", new { id = queryId });
        }

        // POST: /AdminQueries/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int queryId, QueryStatus status)
        {
            var query = await _context.Queries.FindAsync(queryId);
            if (query == null) return NotFound();

            query.Status = status;
            query.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Query #{queryId} status updated to {status}.";
            return RedirectToAction("Details", new { id = queryId });
        }
    }
}
