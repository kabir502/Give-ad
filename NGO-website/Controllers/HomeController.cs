using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models;
using NGO_website.Models.Enums;
using NGO_website.ViewModels.Home;
using System.Diagnostics;

namespace NGO_website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                FeaturedCauses = await _context.Causes
                    .Include(c => c.NGO)
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.RaisedAmount)
                    .Take(6)
                    .ToListAsync(),

                ActiveCampaigns = await _context.Campaigns
                    .Include(c => c.NGO)
                    .Include(c => c.Cause)
                    .Where(c => c.IsActive && c.Status == CampaignStatus.Active)
                    .OrderByDescending(c => c.StartDate)
                    .Take(3)
                    .ToListAsync(),

                RegisteredNGOs = await _context.NGOs
                    .Where(n => n.IsActive)
                    .Take(4)
                    .ToListAsync(),

                UpcomingProgrammes = await _context.Programmes
                    .Include(p => p.NGO)
                    .Where(p => p.IsActive && p.Date >= DateTime.UtcNow.Date)
                    .OrderBy(p => p.Date)
                    .Take(3)
                    .ToListAsync(),

                UpcomingEvents = await _context.Events
                    .Include(e => e.NGO)
                    .Where(e => e.IsActive && e.EventDate >= DateTime.UtcNow.Date)
                    .OrderBy(e => e.EventDate)
                    .Take(2)
                    .ToListAsync(),

                GalleryPreview = await _context.GalleryItems
                    .Where(g => g.IsActive)
                    .OrderByDescending(g => g.DateTaken)
                    .Take(6)
                    .ToListAsync(),

                Partners = await _context.Partners
                    .Where(p => p.IsActive)
                    .Take(6)
                    .ToListAsync(),

                TotalFundsRaised = await _context.Donations
                    .Where(d => d.Status == DonationStatus.Approved)
                    .SumAsync(d => d.Amount),

                TotalDonationsCount = await _context.Donations
                    .CountAsync(d => d.Status == DonationStatus.Approved),

                TotalNGOsCount = await _context.NGOs
                    .CountAsync(n => n.IsActive),

                TotalCausesCount = await _context.Causes
                    .CountAsync(c => c.IsActive)
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("Home/HandleErrorCode")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }
            if (statusCode == 403)
            {
                return View("AccessDenied");
            }
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
