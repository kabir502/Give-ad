using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Services.Interfaces;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminContactMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminContactMessagesController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /AdminContactMessages
        public async Task<IActionResult> Index()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(messages);
        }

        // GET: /AdminContactMessages/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return NotFound();

            if (!msg.IsRead)
            {
                msg.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return View(msg);
        }

        // POST: /AdminContactMessages/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string replyText)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return NotFound();

            if (string.IsNullOrWhiteSpace(replyText))
            {
                TempData["ErrorMessage"] = "Reply message cannot be empty.";
                return RedirectToAction("Details", new { id });
            }

            msg.AdminReply = replyText;
            msg.RepliedAt = DateTime.UtcNow;
            msg.IsRead = true;
            await _context.SaveChangesAsync();

            // Dispatch reply email
            var subject = $"GIVE-AID Response: {msg.Subject}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0;'>
                    <h2 style='color: #0d6efd;'>Response from GIVE-AID Support</h2>
                    <p>Dear {msg.Name},</p>
                    <p>Thank you for reaching out to us. In response to your inquiry regarding <em>""{msg.Subject}""</em>:</p>
                    <div style='background-color: #f8f9fa; padding: 12px; border-left: 4px solid #0d6efd; margin: 15px 0;'>
                        {replyText}
                    </div>
                    <p>If you have any further questions, feel free to contact us anytime.</p>
                </div>";

            await _emailService.SendEmailAsync(msg.Email, subject, body);

            TempData["SuccessMessage"] = "Reply saved and emailed to sender.";
            return RedirectToAction("Details", new { id });
        }

        // POST: /AdminContactMessages/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var msg = await _context.ContactMessages.FindAsync(id);
            if (msg == null) return NotFound();

            _context.ContactMessages.Remove(msg);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Message deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}
