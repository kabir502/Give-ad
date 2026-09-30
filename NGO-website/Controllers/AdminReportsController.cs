using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.Admin;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;

        public AdminReportsController(ApplicationDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        // GET: /AdminReports
        [HttpGet]
        public async Task<IActionResult> Index(ReportsFilterViewModel? filter)
        {
            filter ??= new ReportsFilterViewModel();
            await PopulateDropdowns(filter);

            var reportResult = await _reportService.GenerateReportAsync(filter);
            return View(reportResult);
        }

        // POST: /AdminReports/ExportCsv
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportCsv(ReportsFilterViewModel filter)
        {
            var reportResult = await _reportService.GenerateReportAsync(filter);
            var csvBytes = _reportService.GenerateCsvExport(reportResult);
            var fileName = $"GIVE-AID-Report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

            return File(csvBytes, "text/csv", fileName);
        }

        // GET: /AdminReports/Print
        [HttpGet]
        public async Task<IActionResult> Print(ReportsFilterViewModel filter)
        {
            var reportResult = await _reportService.GenerateReportAsync(filter);
            return View(reportResult);
        }

        private async Task PopulateDropdowns(ReportsFilterViewModel filter)
        {
            var ngos = await _context.NGOs.Where(n => n.IsActive).OrderBy(n => n.Name).ToListAsync();
            filter.NgoList = ngos.Select(n => new SelectListItem
            {
                Value = n.NGOId.ToString(),
                Text = n.Name,
                Selected = n.NGOId == filter.NGOId
            });

            var causes = await _context.Causes.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            filter.CauseList = causes.Select(c => new SelectListItem
            {
                Value = c.CauseId.ToString(),
                Text = c.Name,
                Selected = c.CauseId == filter.CauseId
            });
        }
    }
}
