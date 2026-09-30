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
    public class AdminNgosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public AdminNgosController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /AdminNgos
        public async Task<IActionResult> Index()
        {
            var ngos = await _context.NGOs
                .Include(n => n.Causes)
                .Include(n => n.Programmes)
                .Include(n => n.Donations)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return View(ngos);
        }

        // GET: /AdminNgos/Create
        public IActionResult Create()
        {
            return View(new NGOFormViewModel());
        }

        // POST: /AdminNgos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NGOFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            string? logoUrl = null;
            if (model.LogoFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.LogoFile, "ngos");
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

            var ngo = new NGO
            {
                Name = model.Name,
                Description = model.Description,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                Website = model.Website,
                LogoUrl = logoUrl ?? "https://images.unsplash.com/photo-1532629345422-7515f3d16bb9?w=300&auto=format&fit=crop&q=80",
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.NGOs.Add(ngo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"NGO '{ngo.Name}' created successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminNgos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var ngo = await _context.NGOs.FindAsync(id);
            if (ngo == null) return NotFound();

            var model = new NGOFormViewModel
            {
                NGOId = ngo.NGOId,
                Name = ngo.Name,
                Description = ngo.Description,
                Email = ngo.Email,
                Phone = ngo.Phone,
                Address = ngo.Address,
                Website = ngo.Website,
                ExistingLogoUrl = ngo.LogoUrl,
                IsActive = ngo.IsActive
            };

            return View(model);
        }

        // POST: /AdminNgos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NGOFormViewModel model)
        {
            if (id != model.NGOId) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var ngo = await _context.NGOs.FindAsync(id);
            if (ngo == null) return NotFound();

            if (model.LogoFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.LogoFile, "ngos");
                if (upload.Success)
                {
                    if (!string.IsNullOrEmpty(ngo.LogoUrl) && !ngo.LogoUrl.StartsWith("http"))
                    {
                        _fileService.DeleteFile(ngo.LogoUrl);
                    }
                    ngo.LogoUrl = upload.FilePath;
                }
            }

            ngo.Name = model.Name;
            ngo.Description = model.Description;
            ngo.Email = model.Email;
            ngo.Phone = model.Phone;
            ngo.Address = model.Address;
            ngo.Website = model.Website;
            ngo.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"NGO '{ngo.Name}' updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminNgos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ngo = await _context.NGOs
                .Include(n => n.Causes)
                .Include(n => n.Campaigns)
                .Include(n => n.Programmes)
                .Include(n => n.Events)
                .Include(n => n.Donations)
                .FirstOrDefaultAsync(n => n.NGOId == id);

            if (ngo == null) return NotFound();
            return View(ngo);
        }

        // POST: /AdminNgos/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var ngo = await _context.NGOs.FindAsync(id);
            if (ngo == null) return NotFound();

            ngo.IsActive = !ngo.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"NGO '{ngo.Name}' is now {(ngo.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }
    }
}
