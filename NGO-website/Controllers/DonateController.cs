using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.Donations;

namespace NGO_website.Controllers
{
    public class DonateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DonateController> _logger;

        public DonateController(
            ApplicationDbContext context,
            IDonationService donationService,
            UserManager<ApplicationUser> userManager,
            ILogger<DonateController> logger)
        {
            _context = context;
            _donationService = donationService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Donate
        [HttpGet]
        public async Task<IActionResult> Index(int? ngoId, int? causeId, int? campaignId, decimal? amount)
        {
            var model = new DonationFormViewModel
            {
                Amount = amount ?? 1000,
                PaymentMethod = PaymentMethodType.CreditDebitCard
            };

            // Pre-fill user data if logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.DonorName = user.FullName;
                    model.DonorEmail = user.Email ?? "";
                    model.DonorPhone = user.PhoneNumber;
                }
            }

            if (causeId.HasValue && causeId.Value > 0)
            {
                var cause = await _context.Causes.FindAsync(causeId.Value);
                if (cause != null)
                {
                    model.CauseId = cause.CauseId;
                    model.NGOId = cause.NGOId;
                }
            }
            else if (ngoId.HasValue && ngoId.Value > 0)
            {
                model.NGOId = ngoId.Value;
            }

            if (campaignId.HasValue && campaignId.Value > 0)
            {
                var campaign = await _context.Campaigns.FindAsync(campaignId.Value);
                if (campaign != null)
                {
                    model.CampaignId = campaign.CampaignId;
                    model.CauseId = campaign.CauseId;
                    model.NGOId = campaign.NGOId;
                }
            }

            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /Donate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DonationFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            // Server-side payment method specific validations
            if (model.PaymentMethod == PaymentMethodType.CreditDebitCard)
            {
                if (string.IsNullOrWhiteSpace(model.CardHolderName))
                    ModelState.AddModelError(nameof(model.CardHolderName), "Card holder name is required.");
                if (string.IsNullOrWhiteSpace(model.CardNumber))
                    ModelState.AddModelError(nameof(model.CardNumber), "Card number is required.");
                if (string.IsNullOrWhiteSpace(model.ExpiryMonth) || string.IsNullOrWhiteSpace(model.ExpiryYear))
                    ModelState.AddModelError(nameof(model.ExpiryMonth), "Card expiration date is required.");
                if (string.IsNullOrWhiteSpace(model.Cvv))
                    ModelState.AddModelError(nameof(model.Cvv), "CVV code is required.");
            }
            else if (model.PaymentMethod == PaymentMethodType.EasyPaisa || model.PaymentMethod == PaymentMethodType.JazzCash)
            {
                if (string.IsNullOrWhiteSpace(model.TransactionId))
                    ModelState.AddModelError(nameof(model.TransactionId), "Transaction ID is required for mobile banking.");
                if (string.IsNullOrWhiteSpace(model.SenderMobile))
                    ModelState.AddModelError(nameof(model.SenderMobile), "Sender mobile number is required.");
                if (model.ReceiptFile == null)
                    ModelState.AddModelError(nameof(model.ReceiptFile), "Please upload your payment screenshot/receipt.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            string? currentUserId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                currentUserId = _userManager.GetUserId(User);
            }

            var result = await _donationService.CreateDonationAsync(model, currentUserId);
            if (!result.Success || result.Donation == null || result.Payment == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Payment processing failed. Please try again.");
                await PopulateDropdowns(model);
                return View(model);
            }

            return RedirectToAction("Confirmation", new { id = result.Donation.DonationId });
        }

        // GET: /Donate/Confirmation/5
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var donation = await _context.Donations
                .Include(d => d.Payment)
                .Include(d => d.NGO)
                .Include(d => d.Cause)
                .FirstOrDefaultAsync(d => d.DonationId == id);

            if (donation == null)
            {
                return NotFound();
            }

            var model = new DonationConfirmationViewModel
            {
                DonationId = donation.DonationId,
                DonationNumber = donation.DonationNumber,
                PaymentReference = donation.Payment?.PaymentReference ?? "N/A",
                Amount = donation.Amount,
                NGOName = donation.NGO?.Name ?? "GIVE-AID NGO",
                CauseName = donation.Cause?.Name ?? "General Welfare",
                PaymentMethod = donation.PaymentMethod,
                DonationStatus = donation.Status,
                PaymentStatus = donation.Payment?.Status ?? PaymentStatus.Pending,
                TransactionId = donation.Payment?.TransactionId ?? "N/A",
                CreatedAt = donation.CreatedAt,
                Message = donation.Status == DonationStatus.Approved
                    ? "Thank you! Your payment has been verified and your donation is confirmed."
                    : "Your donation has been submitted successfully and is currently waiting for administrator payment verification."
            };

            return View(model);
        }

        // GET: /Donate/Receipt/5
        [HttpGet]
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

            return View(model);
        }

        // AJAX API: Get Causes for NGO
        [HttpGet]
        public async Task<IActionResult> GetCausesByNgo(int ngoId)
        {
            var causes = await _context.Causes
                .Where(c => c.NGOId == ngoId && c.IsActive)
                .Select(c => new { c.CauseId, c.Name, c.Category })
                .ToListAsync();

            return Json(causes);
        }

        // AJAX API: Get Campaigns for Cause
        [HttpGet]
        public async Task<IActionResult> GetCampaignsByCause(int causeId)
        {
            var campaigns = await _context.Campaigns
                .Where(c => c.CauseId == causeId && c.IsActive && c.Status == CampaignStatus.Active)
                .Select(c => new { c.CampaignId, c.Title, c.GoalAmount, c.RaisedAmount })
                .ToListAsync();

            return Json(campaigns);
        }

        private async Task PopulateDropdowns(DonationFormViewModel model)
        {
            var ngos = await _context.NGOs.Where(n => n.IsActive).OrderBy(n => n.Name).ToListAsync();
            model.NgoList = ngos.Select(n => new SelectListItem
            {
                Value = n.NGOId.ToString(),
                Text = n.Name,
                Selected = n.NGOId == model.NGOId
            });

            if (model.NGOId > 0)
            {
                var causes = await _context.Causes.Where(c => c.NGOId == model.NGOId && c.IsActive).OrderBy(c => c.Name).ToListAsync();
                model.CauseList = causes.Select(c => new SelectListItem
                {
                    Value = c.CauseId.ToString(),
                    Text = $"{c.Name} ({c.Category})",
                    Selected = c.CauseId == model.CauseId
                });
            }
            else
            {
                var allCauses = await _context.Causes.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
                model.CauseList = allCauses.Select(c => new SelectListItem
                {
                    Value = c.CauseId.ToString(),
                    Text = $"{c.Name} ({c.NGO?.Name ?? ""})",
                    Selected = c.CauseId == model.CauseId
                });
            }

            if (model.CauseId > 0)
            {
                var campaigns = await _context.Campaigns.Where(c => c.CauseId == model.CauseId && c.IsActive).OrderBy(c => c.Title).ToListAsync();
                model.CampaignList = campaigns.Select(c => new SelectListItem
                {
                    Value = c.CampaignId.ToString(),
                    Text = c.Title,
                    Selected = c.CampaignId == model.CampaignId
                });
            }
        }
    }
}
