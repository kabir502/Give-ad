using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.Admin;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPartnersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public AdminPartnersController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /AdminPartners
        public async Task<IActionResult> Index()
        {
            var partners = await _context.Partners
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(partners);
        }

        // GET: /AdminPartners/Create
        public IActionResult Create()
        {
            return View(new PartnerFormViewModel());
        }

        // POST: /AdminPartners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartnerFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string? logoUrl = null;
            if (model.LogoFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.LogoFile, "partners");
                if (upload.Success)
                {
                    logoUrl = upload.FilePath;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.LogoFile), upload.ErrorMessage!);
                    return View(model);
                }
            }

            var partner = new Partner
            {
                Name = model.Name,
                Description = model.Description,
                WebsiteUrl = model.WebsiteUrl,
                Category = model.Category,
                LogoUrl = logoUrl ?? "https://images.unsplash.com/photo-1559526324-4b87b5e36e44?w=300&auto=format&fit=crop&q=80",
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Partner '{partner.Name}' added successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminPartners/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null) return NotFound();

            var model = new PartnerFormViewModel
            {
                PartnerId = partner.PartnerId,
                Name = partner.Name,
                Description = partner.Description,
                WebsiteUrl = partner.WebsiteUrl,
                Category = partner.Category,
                ExistingLogoUrl = partner.LogoUrl,
                IsActive = partner.IsActive
            };

            return View(model);
        }

        // POST: /AdminPartners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartnerFormViewModel model)
        {
            if (id != model.PartnerId) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var partner = await _context.Partners.FindAsync(id);
            if (partner == null) return NotFound();

            if (model.LogoFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.LogoFile, "partners");
                if (upload.Success)
                {
                    if (!string.IsNullOrEmpty(partner.LogoUrl) && !partner.LogoUrl.StartsWith("http"))
                    {
                        _fileService.DeleteFile(partner.LogoUrl);
                    }
                    partner.LogoUrl = upload.FilePath;
                }
            }

            partner.Name = model.Name;
            partner.Description = model.Description;
            partner.WebsiteUrl = model.WebsiteUrl;
            partner.Category = model.Category;
            partner.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Partner '{partner.Name}' updated successfully.";
            return RedirectToAction("Index");
        }

        // POST: /AdminPartners/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null) return NotFound();

            partner.IsActive = !partner.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Partner '{partner.Name}' is now {(partner.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }

        // POST: /AdminPartners/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null) return NotFound();

            if (!string.IsNullOrEmpty(partner.LogoUrl) && !partner.LogoUrl.StartsWith("http"))
            {
                _fileService.DeleteFile(partner.LogoUrl);
            }

            _context.Partners.Remove(partner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Partner removed successfully.";
            return RedirectToAction("Index");
        }
    }
}
