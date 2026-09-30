using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.Admin;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCausesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public AdminCausesController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /AdminCauses
        public async Task<IActionResult> Index()
        {
            var causes = await _context.Causes
                .Include(c => c.NGO)
                .Include(c => c.Donations)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(causes);
        }

        // GET: /AdminCauses/Create
        public async Task<IActionResult> Create()
        {
            var model = new CauseFormViewModel();
            await PopulateNgoDropdown(model);
            return View(model);
        }

        // POST: /AdminCauses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CauseFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateNgoDropdown(model);
                return View(model);
            }

            string? imageUrl = null;
            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "causes");
                if (upload.Success)
                {
                    imageUrl = upload.FilePath;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.ImageFile), upload.ErrorMessage!);
                    await PopulateNgoDropdown(model);
                    return View(model);
                }
            }

            var cause = new Cause
            {
                NGOId = model.NGOId,
                Name = model.Name,
                Category = model.Category,
                Description = model.Description,
                TargetAmount = model.TargetAmount,
                RaisedAmount = 0,
                ImageUrl = imageUrl ?? "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=600&auto=format&fit=crop&q=80",
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Causes.Add(cause);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cause '{cause.Name}' created successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminCauses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cause = await _context.Causes.FindAsync(id);
            if (cause == null) return NotFound();

            var model = new CauseFormViewModel
            {
                CauseId = cause.CauseId,
                NGOId = cause.NGOId,
                Name = cause.Name,
                Category = cause.Category,
                Description = cause.Description,
                TargetAmount = cause.TargetAmount,
                ExistingImageUrl = cause.ImageUrl,
                IsActive = cause.IsActive
            };

            await PopulateNgoDropdown(model);
            return View(model);
        }

        // POST: /AdminCauses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CauseFormViewModel model)
        {
            if (id != model.CauseId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateNgoDropdown(model);
                return View(model);
            }

            var cause = await _context.Causes.FindAsync(id);
            if (cause == null) return NotFound();

            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "causes");
                if (upload.Success)
                {
                    if (!string.IsNullOrEmpty(cause.ImageUrl) && !cause.ImageUrl.StartsWith("http"))
                    {
                        _fileService.DeleteFile(cause.ImageUrl);
                    }
                    cause.ImageUrl = upload.FilePath;
                }
            }

            cause.NGOId = model.NGOId;
            cause.Name = model.Name;
            cause.Category = model.Category;
            cause.Description = model.Description;
            cause.TargetAmount = model.TargetAmount;
            cause.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Cause '{cause.Name}' updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminCauses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var cause = await _context.Causes
                .Include(c => c.NGO)
                .Include(c => c.Campaigns)
                .Include(c => c.Donations)
                .FirstOrDefaultAsync(c => c.CauseId == id);

            if (cause == null) return NotFound();
            return View(cause);
        }

        // POST: /AdminCauses/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var cause = await _context.Causes.FindAsync(id);
            if (cause == null) return NotFound();

            cause.IsActive = !cause.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cause '{cause.Name}' is now {(cause.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }

        private async Task PopulateNgoDropdown(CauseFormViewModel model)
        {
            var ngos = await _context.NGOs.Where(n => n.IsActive).OrderBy(n => n.Name).ToListAsync();
            model.NgoList = ngos.Select(n => new SelectListItem
            {
                Value = n.NGOId.ToString(),
                Text = n.Name,
                Selected = n.NGOId == model.NGOId
            });
        }
    }
}
