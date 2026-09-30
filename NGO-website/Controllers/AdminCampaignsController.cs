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
    public class AdminCampaignsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public AdminCampaignsController(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // GET: /AdminCampaigns
        public async Task<IActionResult> Index()
        {
            var campaigns = await _context.Campaigns
                .Include(c => c.NGO)
                .Include(c => c.Cause)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(campaigns);
        }

        // GET: /AdminCampaigns/Create
        public async Task<IActionResult> Create()
        {
            var model = new CampaignFormViewModel();
            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /AdminCampaigns/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CampaignFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            string? imageUrl = null;
            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "campaigns");
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

            var campaign = new Campaign
            {
                NGOId = model.NGOId,
                CauseId = model.CauseId,
                Title = model.Title,
                Description = model.Description,
                GoalAmount = model.GoalAmount,
                RaisedAmount = 0,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                ImageUrl = imageUrl ?? "https://images.unsplash.com/photo-1509062522246-3755977927d7?w=600&auto=format&fit=crop&q=80",
                Status = model.Status,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Campaign '{campaign.Title}' created successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminCampaigns/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            var model = new CampaignFormViewModel
            {
                CampaignId = campaign.CampaignId,
                NGOId = campaign.NGOId,
                CauseId = campaign.CauseId,
                Title = campaign.Title,
                Description = campaign.Description,
                GoalAmount = campaign.GoalAmount,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                ExistingImageUrl = campaign.ImageUrl,
                Status = campaign.Status,
                IsActive = campaign.IsActive
            };

            await PopulateDropdowns(model);
            return View(model);
        }

        // POST: /AdminCampaigns/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CampaignFormViewModel model)
        {
            if (id != model.CampaignId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(model);
                return View(model);
            }

            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            if (model.ImageFile != null)
            {
                var upload = await _fileService.SaveFileAsync(model.ImageFile, "campaigns");
                if (upload.Success)
                {
                    if (!string.IsNullOrEmpty(campaign.ImageUrl) && !campaign.ImageUrl.StartsWith("http"))
                    {
                        _fileService.DeleteFile(campaign.ImageUrl);
                    }
                    campaign.ImageUrl = upload.FilePath;
                }
            }

            campaign.NGOId = model.NGOId;
            campaign.CauseId = model.CauseId;
            campaign.Title = model.Title;
            campaign.Description = model.Description;
            campaign.GoalAmount = model.GoalAmount;
            campaign.StartDate = model.StartDate;
            campaign.EndDate = model.EndDate;
            campaign.Status = model.Status;
            campaign.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Campaign '{campaign.Title}' updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /AdminCampaigns/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.NGO)
                .Include(c => c.Cause)
                .Include(c => c.Donations)
                .FirstOrDefaultAsync(c => c.CampaignId == id);

            if (campaign == null) return NotFound();
            return View(campaign);
        }

        // POST: /AdminCampaigns/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            campaign.IsActive = !campaign.IsActive;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Campaign '{campaign.Title}' is now {(campaign.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Index");
        }

        private async Task PopulateDropdowns(CampaignFormViewModel model)
        {
            var ngos = await _context.NGOs.Where(n => n.IsActive).OrderBy(n => n.Name).ToListAsync();
            model.NgoList = ngos.Select(n => new SelectListItem
            {
                Value = n.NGOId.ToString(),
                Text = n.Name,
                Selected = n.NGOId == model.NGOId
            });

            var causes = await _context.Causes.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            model.CauseList = causes.Select(c => new SelectListItem
            {
                Value = c.CauseId.ToString(),
                Text = c.Name,
                Selected = c.CauseId == model.CauseId
            });
        }
    }
}
