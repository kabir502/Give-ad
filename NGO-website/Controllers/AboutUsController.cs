using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;

namespace NGO_website.Controllers
{
    public class AboutUsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutUsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /AboutUs
        public async Task<IActionResult> Index()
        {
            var contents = await _context.AboutUsContents
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(contents);
        }
    }
}
