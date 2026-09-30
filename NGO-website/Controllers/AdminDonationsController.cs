using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Enums;
using NGO_website.ViewModels.Donations;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDonationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDonationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminDonations
        public async Task<IActionResult> Index(string? search, DonationStatus? status, PaymentMethodType? method, int? ngoId, int? causeId)
        {
            var query = _context.Donations
                .Include(d => d.NGO)
                .Include(d => d.Cause)
                .Include(d => d.Payment)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.DonationNumber.Contains(search) ||
                                         d.DonorName.Contains(search) ||
                                         d.DonorEmail.Contains(search) ||
                                         (d.Payment != null && d.Payment.TransactionId.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            if (method.HasValue)
            {
                query = query.Where(d => d.PaymentMethod == method.Value);
            }

            if (ngoId.HasValue && ngoId.Value > 0)
            {
                query = query.Where(d => d.NGOId == ngoId.Value);
            }

            if (causeId.HasValue && causeId.Value > 0)
            {
                query = query.Where(d => d.CauseId == causeId.Value);
            }

            ViewBag.NGOs = await _context.NGOs.Where(n => n.IsActive).OrderBy(n => n.Name).ToListAsync();
            ViewBag.Causes = await _context.Causes.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            ViewData["Search"] = search;
            ViewData["SelectedStatus"] = status;
            ViewData["SelectedMethod"] = method;

            var donations = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
            return View(donations);
        }

        // GET: /AdminDonations/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.NGO)
                .Include(d => d.Cause)
                .Include(d => d.Campaign)
                .Include(d => d.Payment)
                .Include(d => d.ApprovedBy)
                .Include(d => d.RejectedBy)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DonationId == id);

            if (donation == null)
            {
                return NotFound();
            }

            return View(donation);
        }

        // GET: /AdminDonations/Receipt/5
        public async Task<IActionResult> Receipt(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.Payment)
                .Include(d => d.NGO)
                .Include(d => d.Cause)
                .Include(d => d.Campaign)
                .FirstOrDefaultAsync(d => d.DonationId == id);

            if (donation == null)
            {
                return NotFound();
            }

            var model = new DonationReceiptViewModel
            {
                DonationId = donation.DonationId,
                DonationNumber = donation.DonationNumber,
                PaymentReference = donation.Payment?.PaymentReference ?? "N/A",
                DonorName = donation.DonorName,
                DonorEmail = donation.DonorEmail,
                DonorPhone = donation.DonorPhone,
                NGOName = donation.NGO?.Name ?? "GIVE-AID Organization",
                NGOLogoUrl = donation.NGO?.LogoUrl,
                NGOAddress = donation.NGO?.Address ?? "Karachi, Pakistan",
                NGOPhone = donation.NGO?.Phone ?? "+92 21 111-GIVEAID",
                NGOEmail = donation.NGO?.Email ?? "info@giveaid.local",
                CauseName = donation.Cause?.Name ?? "General Welfare Cause",
                CampaignTitle = donation.Campaign?.Title,
                Amount = donation.Amount,
                PaymentMethod = donation.PaymentMethod.ToString(),
                TransactionId = donation.Payment?.TransactionId ?? "N/A",
                DonationDate = donation.CreatedAt,
                ApprovalDate = donation.ApprovedAt,
                Status = donation.Status.ToString(),
                AdminRemarks = donation.AdminRemarks,
                MaskedCardNumber = donation.Payment?.MaskedCardNumber
            };

            return View("~/Views/Donate/Receipt.cshtml", model);
        }
    }
}
