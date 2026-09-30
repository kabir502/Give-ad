using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.ViewModels.Home;

namespace NGO_website.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Contact
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new ContactViewModel();
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.Name = user.FullName;
                    model.Email = user.Email ?? "";
                    model.Phone = user.PhoneNumber;
                }
            }
            return View(model);
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var message = new ContactMessage
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                Subject = model.Subject,
                Message = model.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for contacting GIVE-AID! Our support team will review your inquiry and get back to you shortly.";
            return RedirectToAction("Index");
        }
    }
}
