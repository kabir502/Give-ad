using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.Admin;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProgrammesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public AdminProgrammesController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /AdminProgrammes
        public async Task<IActionResult> Index()
        {
            var programmes = await _context.Programmes
                .Include(p => p.NGO)
                .Include(p => p.ProgrammeInterests)
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return View(programmes);
        }

        // GET: /AdminProgrammes/Create
        public async Task<IActionResult> Create()
        {
            var model = new ProgrammeFormViewModel();
            await PopulateNgoDropdown(model);
            return View(model);
        }

        // POST: /AdminProgrammes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProgrammeFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateNgoDropdown(model);
                return View(model);
            }

            string? imageUrl = null;
            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "programmes");
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

            var programme = new Programme
            {
                NGOId = model.NGOId,
                Title = model.Title,
                Description = model.Description,
                Date = model.Date,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Location = model.Location,
                Capacity = model.Capacity,
                ImageUrl = imageUrl ?? "https://images.unsplash.com/photo-1531482615713-2afd69097998?w=600&auto=format&fit=crop&q=80",
                Status = model.Status,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Programmes.Add(programme);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Programme '{programme.Title}' created successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminProgrammes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var programme = await _context.Programmes.FindAsync(id);
            if (programme == null) return NotFound();

            var model = new ProgrammeFormViewModel
            {
                ProgrammeId = programme.ProgrammeId,
                NGOId = programme.NGOId,
                Title = programme.Title,
                Description = programme.Description,
                Date = programme.Date,
                StartTime = programme.StartTime,
                EndTime = programme.EndTime,
                Location = programme.Location,
                Capacity = programme.Capacity,
                ExistingImageUrl = programme.ImageUrl,
                Status = programme.Status,
                IsActive = programme.IsActive
            };

            await PopulateNgoDropdown(model);
            return View(model);
        }

        // POST: /AdminProgrammes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProgrammeFormViewModel model)
        {
            if (id != model.ProgrammeId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateNgoDropdown(model);
                return View(model);
            }

            var programme = await _context.Programmes.FindAsync(id);
            if (programme == null) return NotFound();

            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "programmes");
                if (upload.Success)
                {
                    if (!string.IsNullOrEmpty(programme.ImageUrl) && !programme.ImageUrl.StartsWith("http"))
                    {
                        _fileService.DeleteFile(programme.ImageUrl);
                    }
                    programme.ImageUrl = upload.FilePath;
                }
            }

            programme.NGOId = model.NGOId;
            programme.Title = model.Title;
            programme.Description = model.Description;
            programme.Date = model.Date;
            programme.StartTime = model.StartTime;
            programme.EndTime = model.EndTime;
            programme.Location = model.Location;
            programme.Capacity = model.Capacity;
            programme.Status = model.Status;
            programme.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Programme '{programme.Title}' updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminProgrammes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var programme = await _context.Programmes
                .Include(p => p.NGO)
                .Include(p => p.ProgrammeInterests)
                .Include(p => p.GalleryItems)
                .FirstOrDefaultAsync(p => p.ProgrammeId == id);

            if (programme == null) return NotFound();
            return View(programme);
        }

        // GET: /AdminProgrammes/Interests/5
        public async Task<IActionResult> Interests(int id)
        {
            var programme = await _context.Programmes
                .Include(p => p.NGO)
                .Include(p => p.ProgrammeInterests)
                .FirstOrDefaultAsync(p => p.ProgrammeId == id);

            if (programme == null) return NotFound();
            return View(programme);
        }

        // POST: /AdminProgrammes/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var programme = await _context.Programmes.FindAsync(id);
            if (programme == null) return NotFound();

            programme.IsActive = !programme.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Programme '{programme.Title}' is now {(programme.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }

        private async Task PopulateNgoDropdown(ProgrammeFormViewModel model)
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
