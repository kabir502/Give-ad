using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Services.Interfaces;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAboutUsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public AdminAboutUsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /AdminAboutUs
        public async Task<IActionResult> Index()
        {
            var contents = await _context.AboutUsContents
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(contents);
        }

        // GET: /AdminAboutUs/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var content = await _context.AboutUsContents.FindAsync(id);
            if (content == null) return NotFound();

            return View(content);
        }

        // POST: /AdminAboutUs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AboutUsContent model, IFormFile? imageFile)
        {
            if (id != model.AboutUsContentId) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var content = await _context.AboutUsContents.FindAsync(id);
            if (content == null) return NotFound();

            if (imageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(imageFile, "about");
                if (upload.Success)
                {
                    if (!string.IsNullOrEmpty(content.ImageUrl) && !content.ImageUrl.StartsWith("http"))
                    {
                        _fileService.DeleteFile(content.ImageUrl);
                    }
                    content.ImageUrl = upload.FilePath;
                }
            }

            content.Title = model.Title;
            content.Subtitle = model.Subtitle;
            content.Content = model.Content;
            content.IconClass = model.IconClass;
            content.DisplayOrder = model.DisplayOrder;
            content.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Section '{content.Title}' updated successfully.";
            return RedirectToAction("Index");
        }
    }
}
