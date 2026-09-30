using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.ViewModels.Admin;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminUsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /AdminUsers
        public async Task<IActionResult> Index(string? search)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Email!.Contains(search) || (u.City != null && u.City.Contains(search)));
            }

            var users = await query.OrderByDescending(u => u.RegistrationDate).ToListAsync();
            var userList = new List<UserManagementViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var donations = await _context.Donations.Where(d => d.UserId == u.Id || d.DonorEmail == u.Email).ToListAsync();

                userList.Add(new UserManagementViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber,
                    City = u.City,
                    Profession = u.Profession,
                    RegistrationDate = u.RegistrationDate,
                    IsActive = u.IsActive,
                    Roles = roles,
                    DonationsCount = donations.Count,
                    TotalDonated = donations.Where(d => d.Status == DonationStatus.Approved).Sum(d => d.Amount)
                });
            }

            ViewData["Search"] = search;
            return View(userList);
        }

        // POST: /AdminUsers/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent deactivating own account
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own administrative account.";
                return RedirectToAction("Index");
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"User '{user.FullName}' is now {(user.IsActive ? "Active" : "Deactivated")}.";
            return RedirectToAction("Index");
        }
    }
}
