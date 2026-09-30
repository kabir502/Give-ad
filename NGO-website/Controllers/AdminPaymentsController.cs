using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.Admin;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminPaymentsController(
            ApplicationDbContext context,
            IDonationService donationService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _donationService = donationService;
            _userManager = userManager;
        }

        // GET: /AdminPayments
        public async Task<IActionResult> Index(PaymentStatus? status, PaymentMethodType? method, string? search, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.NGO)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Cause)
                .Include(p => p.VerifiedBy)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (method.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == method.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.TransactionId.Contains(search) ||
                                         p.PaymentReference.Contains(search) ||
                                         (p.Donation != null && (p.Donation.DonorName.Contains(search) || p.Donation.DonorEmail.Contains(search))));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.SubmittedDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.SubmittedDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            var payments = await query.OrderByDescending(p => p.SubmittedDate).ToListAsync();

            var model = new PaymentVerificationViewModel
            {
                SearchTerm = search,
                StatusFilter = status,
                MethodFilter = method,
                FromDate = fromDate,
                ToDate = toDate,
                TotalPending = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Pending),
                TotalVerified = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Verified),
                TotalRejected = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Rejected),
                TotalVerifiedAmount = await _context.Payments.Where(p => p.Status == PaymentStatus.Verified).SumAsync(p => p.Amount),
                Payments = payments
            };

            return View(model);
        }

        // GET: /AdminPayments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.NGO)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Cause)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Campaign)
                .Include(p => p.VerifiedBy)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: /AdminPayments/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int paymentId, string? remarks)
        {
            var adminId = _userManager.GetUserId(User)!;
            var result = await _donationService.ApprovePaymentAsync(paymentId, adminId, remarks);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Payment ID #{paymentId} and Donation #{result.Donation?.DonationNumber} have been successfully verified and approved. Raised amounts updated and confirmation email dispatched.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Payment approval failed.";
            }

            return RedirectToAction("Details", new { id = paymentId });
        }

        // POST: /AdminPayments/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int paymentId, string? remarks)
        {
            var adminId = _userManager.GetUserId(User)!;
            var result = await _donationService.RejectPaymentAsync(paymentId, adminId, remarks);

            if (result.Success)
            {
                TempData["WarningMessage"] = $"Payment ID #{paymentId} has been rejected. Notification email sent to the donor.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Payment rejection failed.";
            }

            return RedirectToAction("Details", new { id = paymentId });
        }
    }
}
