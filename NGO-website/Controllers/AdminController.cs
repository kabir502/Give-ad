using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.ViewModels.Account;
using NGO_website.ViewModels.Admin;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalNgos = await _context.NGOs.CountAsync();
            var totalCauses = await _context.Causes.CountAsync();
            var totalCampaigns = await _context.Campaigns.CountAsync();
            var totalDonations = await _context.Donations.CountAsync();
            var totalAmountReceived = await _context.Donations
                .Where(d => d.Status == DonationStatus.Approved)
                .SumAsync(d => d.Amount);

            var pendingPaymentsCount = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Pending);
            var approvedPaymentsCount = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Verified);
            var rejectedPaymentsCount = await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Rejected);
            var totalProgrammes = await _context.Programmes.CountAsync();
            var totalOpenQueries = await _context.Queries.CountAsync(q => q.Status == QueryStatus.Open || q.Status == QueryStatus.InProgress);

            var recentDonations = await _context.Donations
                .Include(d => d.NGO)
                .Include(d => d.Cause)
                .Include(d => d.Payment)
                .OrderByDescending(d => d.CreatedAt)
                .Take(6)
                .ToListAsync();

            var pendingPayments = await _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.NGO)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Cause)
                .Where(p => p.Status == PaymentStatus.Pending)
                .OrderByDescending(p => p.SubmittedDate)
                .Take(6)
                .ToListAsync();

            var recentQueries = await _context.Queries
                .OrderByDescending(q => q.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.RegistrationDate)
                .Take(5)
                .ToListAsync();

            // Monthly Donation Trend (Last 6 Months)
            var monthlyLabels = new List<string>();
            var monthlyAmounts = new List<decimal>();
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = DateTime.UtcNow.AddMonths(-i);
                var monthName = monthDate.ToString("MMM yyyy");
                monthlyLabels.Add(monthName);

                var sum = await _context.Donations
                    .Where(d => d.Status == DonationStatus.Approved && d.CreatedAt.Year == monthDate.Year && d.CreatedAt.Month == monthDate.Month)
                    .SumAsync(d => (decimal?)d.Amount) ?? 0;
                monthlyAmounts.Add(sum);
            }

            // Cause-wise distribution
            var causeData = await _context.Causes
                .Where(c => c.RaisedAmount > 0)
                .OrderByDescending(c => c.RaisedAmount)
                .Take(5)
                .Select(c => new { c.Name, c.RaisedAmount })
                .ToListAsync();

            // Payment Method counts
            var cardCount = await _context.Donations.CountAsync(d => d.PaymentMethod == PaymentMethodType.CreditDebitCard);
            var epCount = await _context.Donations.CountAsync(d => d.PaymentMethod == PaymentMethodType.EasyPaisa);
            var jcCount = await _context.Donations.CountAsync(d => d.PaymentMethod == PaymentMethodType.JazzCash);

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalNGOs = totalNgos,
                TotalCauses = totalCauses,
                TotalCampaigns = totalCampaigns,
                TotalDonations = totalDonations,
                TotalAmountReceived = totalAmountReceived,
                PendingPaymentsCount = pendingPaymentsCount,
                ApprovedPaymentsCount = approvedPaymentsCount,
                RejectedPaymentsCount = rejectedPaymentsCount,
                TotalProgrammes = totalProgrammes,
                TotalOpenQueries = totalOpenQueries,
                RecentDonations = recentDonations,
                PendingPayments = pendingPayments,
                RecentQueries = recentQueries,
                RecentUsers = recentUsers,
                MonthlyLabels = monthlyLabels,
                MonthlyDonationAmounts = monthlyAmounts,
                CauseLabels = causeData.Select(c => c.Name).ToList(),
                CauseDonationAmounts = causeData.Select(c => c.RaisedAmount).ToList(),
                PaymentMethodLabels = new List<string> { "Credit/Debit Card", "EasyPaisa", "JazzCash" },
                PaymentMethodCounts = new List<int> { cardCount, epCount, jcCount }
            };

            return View(model);
        }

        // GET: /Admin/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Address = user.Address,
                City = user.City,
                Profession = user.Profession
            };

            return View(model);
        }

        // POST: /Admin/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;
            user.Profession = model.Profession;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Administrator profile updated successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: /Admin/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Password change failed. Please make sure the passwords match and are at least 6 characters.";
                return RedirectToAction("Profile");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Administrator password changed successfully!";
                return RedirectToAction("Profile");
            }

            TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction("Profile");
        }
    }
}
