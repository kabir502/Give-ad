using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NGO_website.Services.Settings;

namespace NGO_website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSettingsController : Controller
    {
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IConfiguration _configuration;

        public AdminSettingsController(IOptions<EmailSettings> emailSettings, IConfiguration configuration)
        {
            _emailSettings = emailSettings;
            _configuration = configuration;
        }

        // GET: /AdminSettings
        public IActionResult Index()
        {
            ViewBag.EmailSettings = _emailSettings.Value;
            ViewBag.AdminEmail = _configuration["AdminUser:Email"] ?? "admin@giveaid.local";
            ViewBag.DbConnection = "SQL Server LocalDB / GiveAidDb (Active)";
            return View();
        }
    }
}
