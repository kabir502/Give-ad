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
    public class AdminGalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public AdminGalleryController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /AdminGallery
        public async Task<IActionResult> Index()
        {
            var items = await _context.GalleryItems
                .Include(g => g.NGO)
                .Include(g => g.Programme)
                .OrderByDescending(g => g.DateTaken)
                .ToListAsync();

            return View(items);
        }

        // GET: /AdminGallery/Create
        public async Task<IActionResult> Create()
        {
            var model = new GalleryFormViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /AdminGallery/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GalleryFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            string? imageUrl = null;
            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "gallery");
                if (upload.Success)
                {
                    imageUrl = upload.FilePath;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.ImageFile), upload.ErrorMessage!);
                    await PopulateDropdowns(model);
                    return View(model);
                }
            }

            var item = new GalleryItem
            {
                NGOId = model.NGOId,
                ProgrammeId = model.ProgrammeId,
                Title = model.Title,
                Category = model.Category,
                DateTaken = model.DateTaken,
                ImageUrl = imageUrl ?? "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800&auto=format&fit=crop&q=80",
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.GalleryItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gallery photo added successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminGallery/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null) return NotFound();

            var model = new GalleryFormViewModel
            {
                GalleryItemId = item.GalleryItemId,
                NGOId = item.NGOId,
                ProgrammeId = item.ProgrammeId,
                Title = item.Title,
                Category = item.Category,
                DateTaken = item.DateTaken,
                ExistingImageUrl = item.ImageUrl,
                IsActive = item.IsActive
            };

            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /AdminGallery/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GalleryFormViewModel model)
        {
            if (id != model.GalleryItemId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null) return NotFound();

            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "gallery");
                if (upload.Success)
                {
                    if (!string.IsNullOrEmpty(item.ImageUrl) && !item.ImageUrl.StartsWith("http"))
                    {
                        _fileService.DeleteFile(item.ImageUrl);
                    }
                    item.ImageUrl = upload.FilePath!;
                }
            }

            item.NGOId = model.NGOId;
            item.ProgrammeId = model.ProgrammeId;
            item.Title = model.Title;
            item.Category = model.Category;
            item.DateTaken = model.DateTaken;
            item.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Gallery photo updated successfully.";
            return RedirectToAction("Index");
        }

        // POST: /AdminGallery/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null) return NotFound();

            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Gallery item is now {(item.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }

        // POST: /AdminGallery/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null) return NotFound();

            if (!string.IsNullOrEmpty(item.ImageUrl) && !item.ImageUrl.StartsWith("http"))
            {
                _fileService.DeleteFile(item.ImageUrl);
            }

            _context.GalleryItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Gallery photo deleted successfully.";
            return RedirectToAction("Index");
        }

        private async Task PopulateDropdowns(GalleryFormViewModel model)
        {
            var ngos = await _context.NGOs.Where(n => n.IsActive).OrderBy(n => n.Name).ToListAsync();
            model.NgoList = ngos.Select(n => new SelectListItem
            {
                Value = n.NGOId.ToString(),
                Text = n.Name,
                Selected = n.NGOId == model.NGOId
            });

            var programmes = await _context.Programmes.Where(p => p.IsActive).OrderBy(p => p.Title).ToListAsync();
            model.ProgrammeList = programmes.Select(p => new SelectListItem
            {
                Value = p.ProgrammeId.ToString(),
                Text = p.Title,
                Selected = p.ProgrammeId == model.ProgrammeId
            });
        }
    }
}
