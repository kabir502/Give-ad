using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Services.Implementations;
using NGO_website.Services.Interfaces;
using NGO_website.Services.Settings;

namespace NGO_website
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configure Database Context with SQL Server
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Configure ASP.NET Core Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Configure Authentication Cookie paths
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // 3. Configure Strongly Typed Options
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            // 4. Register Application & Domain Services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IDonationService, DonationService>();
            builder.Services.AddScoped<IReportService, ReportService>();

            // Register Payment Processors
            builder.Services.AddScoped<IPaymentService, CardPaymentService>();
            builder.Services.AddScoped<IPaymentService, EasyPaisaPaymentService>();
            builder.Services.AddScoped<IPaymentService, JazzCashPaymentService>();

            // 5. Add MVC Controllers & Views
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // 6. Seed Database on Application Startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    await DbInitializer.SeedAsync(services, app.Configuration);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during database seeding.");
                }
            }

            // Ensure upload directories exist
            var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadDirs = new[] { "ngos", "causes", "campaigns", "programmes", "events", "gallery", "partners", "about", "receipts" };
            foreach (var dir in uploadDirs)
            {
                var target = Path.Combine(webRoot, "uploads", dir);
                if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            }

            // 7. HTTP Request Pipeline Configuration
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePagesWithReExecute("/Home/HandleErrorCode", "?statusCode={0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
