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
    public class AdminEventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public AdminEventsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /AdminEvents
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .Include(e => e.NGO)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            return View(events);
        }

        // GET: /AdminEvents/Create
        public async Task<IActionResult> Create()
        {
            var model = new EventFormViewModel();
            await PopulateNgoDropdown(model);
            return View(model);
        }

        // POST: /AdminEvents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateNgoDropdown(model);
                return View(model);
            }

            string? imageUrl = null;
            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "events");
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

            var evt = new Event
            {
                NGOId = model.NGOId,
                Title = model.Title,
                Description = model.Description,
                EventDate = model.EventDate,
                Location = model.Location,
                ImageUrl = imageUrl ?? "https://images.unsplash.com/photo-1511578314322-379afb476865?w=600&auto=format&fit=crop&q=80",
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Event '{evt.Title}' created successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminEvents/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null) return NotFound();

            var model = new EventFormViewModel
            {
                EventId = evt.EventId,
                NGOId = evt.NGOId,
                Title = evt.Title,
                Description = evt.Description,
                EventDate = evt.EventDate,
                Location = evt.Location,
                ExistingImageUrl = evt.ImageUrl,
                IsActive = evt.IsActive
            };

            await PopulateNgoDropdown(model);
            return View(model);
        }

        // POST: /AdminEvents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventFormViewModel model)
        {
            if (id != model.EventId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateNgoDropdown(model);
                return View(model);
            }

            var evt = await _context.Events.FindAsync(id);
            if (evt == null) return NotFound();

            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "events");
                if (upload.Success)
                {
                    if (!string.IsNullOrEmpty(evt.ImageUrl) && !evt.ImageUrl.StartsWith("http"))
                    {
                        _fileService.DeleteFile(evt.ImageUrl);
                    }
                    evt.ImageUrl = upload.FilePath;
                }
            }

            evt.NGOId = model.NGOId;
            evt.Title = model.Title;
            evt.Description = model.Description;
            evt.EventDate = model.EventDate;
            evt.Location = model.Location;
            evt.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Event '{evt.Title}' updated successfully.";
            return RedirectToAction("Index");
        }

        // POST: /AdminEvents/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null) return NotFound();

            evt.IsActive = !evt.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Event '{evt.Title}' is now {(evt.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }

        private async Task PopulateNgoDropdown(EventFormViewModel model)
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
