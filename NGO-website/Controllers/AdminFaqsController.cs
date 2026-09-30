using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFaqsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminFaqsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AdminFaqs
        public async Task<IActionResult> Index()
        {
            var faqs = await _context.FAQs
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            return View(faqs);
        }

        // GET: /AdminFaqs/Create
        public IActionResult Create()
        {
            return View(new FAQ { DisplayOrder = 1, IsActive = true });
        }

        // POST: /AdminFaqs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FAQ faq)
        {
            if (!ModelState.IsValid) return View(faq);

            faq.CreatedDate = DateTime.UtcNow;
            _context.FAQs.Add(faq);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "FAQ created successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminFaqs/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null) return NotFound();

            return View(faq);
        }

        // POST: /AdminFaqs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FAQ faq)
        {
            if (id != faq.FAQId) return BadRequest();
            if (!ModelState.IsValid) return View(faq);

            var existing = await _context.FAQs.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Category = faq.Category;
            existing.Question = faq.Question;
            existing.Answer = faq.Answer;
            existing.DisplayOrder = faq.DisplayOrder;
            existing.IsActive = faq.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "FAQ updated successfully.";
            return RedirectToAction("Index");
        }

        // POST: /AdminFaqs/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null) return NotFound();

            faq.IsActive = !faq.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"FAQ is now {(faq.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }

        // POST: /AdminFaqs/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var faq = await _context.FAQs.FindAsync(id);
            if (faq == null) return NotFound();

            _context.FAQs.Remove(faq);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "FAQ deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
