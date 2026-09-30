using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;

namespace NGO_website.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Ensure database is created and up to date
                await context.Database.MigrateAsync();

                // 1. Seed Roles
                string[] roles = { "Admin", "User" };
                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // 2. Seed Admin User
                var adminEmail = configuration["AdminUser:Email"] ?? "admin@giveaid.local";
                var adminPassword = configuration["AdminUser:Password"] ?? "Admin@123456";

                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "GIVE-AID Administrator",
                        PhoneNumber = "+92 300 1234567",
                        City = "Islamabad",
                        Address = "GIVE-AID Central Secretariat, Blue Area",
                        Profession = "NGO System Administrator",
                        EmailConfirmed = true,
                        IsActive = true,
                        RegistrationDate = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                // 3. Seed Sample Regular User
                var sampleUserEmail = "donor@giveaid.local";
                var sampleUser = await userManager.FindByEmailAsync(sampleUserEmail);
                if (sampleUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = sampleUserEmail,
                        Email = sampleUserEmail,
                        FullName = "Farhan Ahmed",
                        PhoneNumber = "+92 321 9876543",
                        City = "Lahore",
                        Address = "DHA Phase 5",
                        Profession = "Software Engineer & Philanthropist",
                        EmailConfirmed = true,
                        IsActive = true,
                        RegistrationDate = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, "Donor@123456");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "User");
                    }
                }

                // 4. Seed NGOs if empty
                if (!await context.NGOs.AnyAsync())
                {
                    var ngos = new List<NGO>
                    {
                        new NGO
                        {
                            Name = "Edhi Foundation",
                            Description = "Edhi Foundation is Pakistan's largest social welfare organization, operating round-the-clock emergency medical services, orphanages, old-age homes, and disaster relief.",
                            Email = "info@edhi.org.pk",
                            Phone = "+92 21 32413232",
                            Address = "Boulton Market, Karachi, Pakistan",
                            Website = "https://edhi.org",
                            LogoUrl = "https://images.unsplash.com/photo-1532629345422-7515f3d16bb9?w=300&auto=format&fit=crop&q=80",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new NGO
                        {
                            Name = "The Citizens Foundation (TCF)",
                            Description = "The Citizens Foundation is professionally managed, non-profit organization established to bring positive social change through quality education for underprivileged children.",
                            Email = "info@tcf.org.pk",
                            Phone = "+92 21 111 823 823",
                            Address = "Plot 20, Sector 22, Korangi Industrial Area, Karachi",
                            Website = "https://tcf.org.pk",
                            LogoUrl = "https://images.unsplash.com/photo-1509062522246-3755977927d7?w=300&auto=format&fit=crop&q=80",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new NGO
                        {
                            Name = "Saylani Welfare International Trust",
                            Description = "Saylani Welfare provides free food distribution, specialized vocational training, youth development, and support for disabled persons.",
                            Email = "info@saylaniwelfare.com",
                            Phone = "+92 21 111 729 526",
                            Address = "A-25, Bahadurabad Chowrangi, Karachi",
                            Website = "https://saylaniwelfare.com",
                            LogoUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=300&auto=format&fit=crop&q=80",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new NGO
                        {
                            Name = "Shaukat Khanum Memorial Cancer Trust",
                            Description = "A premier cancer hospital network providing financially supported and free comprehensive cancer diagnosis and treatment to deserving patients across Pakistan.",
                            Email = "fundraising@skm.org.pk",
                            Phone = "+92 42 35905000",
                            Address = "7A Block R-3, Johar Town, Lahore",
                            Website = "https://shaukatkhanum.org.pk",
                            LogoUrl = "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=300&auto=format&fit=crop&q=80",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new NGO
                        {
                            Name = "Indus Hospital & Health Network",
                            Description = "A nationwide health network delivering 100% free-of-cost quality medical care to millions of needy patients without discrimination.",
                            Email = "info@tih.org.pk",
                            Phone = "+92 21 35112709",
                            Address = "Plot C-76, Sector 31/5, Korangi Crossing, Karachi",
                            Website = "https://indushospital.org.pk",
                            LogoUrl = "https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?w=300&auto=format&fit=crop&q=80",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        }
                    };

                    await context.NGOs.AddRangeAsync(ngos);
                    await context.SaveChangesAsync();
                }

                // 5. Seed Causes if empty
                if (!await context.Causes.AnyAsync())
                {
                    var edhi = await context.NGOs.FirstOrDefaultAsync(n => n.Name.Contains("Edhi"));
                    var tcf = await context.NGOs.FirstOrDefaultAsync(n => n.Name.Contains("Citizens"));
                    var saylani = await context.NGOs.FirstOrDefaultAsync(n => n.Name.Contains("Saylani"));
                    var skm = await context.NGOs.FirstOrDefaultAsync(n => n.Name.Contains("Shaukat"));

                    var causes = new List<Cause>
                    {
                        new Cause
                        {
                            NGOId = edhi?.NGOId ?? 1,
                            Name = "Children Welfare & Orphan Support",
                            Category = "Children Welfare",
                            Description = "Providing complete boarding, quality nutrition, clothing, healthcare, and educational mentorship to abandoned and orphaned children.",
                            ImageUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 2500000,
                            RaisedAmount = 1450000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-2)
                        },
                        new Cause
                        {
                            NGOId = tcf?.NGOId ?? 2,
                            Name = "Underprivileged Education Fund",
                            Category = "Education",
                            Description = "Financing school fees, books, digital tablets, and school uniforms for children from low-income rural and urban slum communities.",
                            ImageUrl = "https://images.unsplash.com/photo-1509062522246-3755977927d7?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 3000000,
                            RaisedAmount = 2100000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-3)
                        },
                        new Cause
                        {
                            NGOId = saylani?.NGOId ?? 3,
                            Name = "Disabled Persons Rehabilitation & Wheelchairs",
                            Category = "Disabled Persons",
                            Description = "Providing customized wheelchairs, prosthetic limbs, hearing aids, and accessibility modifications to enable independent living.",
                            ImageUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 1800000,
                            RaisedAmount = 950000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-1)
                        },
                        new Cause
                        {
                            NGOId = saylani?.NGOId ?? 3,
                            Name = "Women Welfare & Vocational Training",
                            Category = "Women Welfare",
                            Description = "Equipping vulnerable and widowed women with tailoring, culinary, and digital skills along with seed micro-grants for self-reliance.",
                            ImageUrl = "https://images.unsplash.com/photo-1573497019940-1c28c88b4f3e?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 2000000,
                            RaisedAmount = 1200000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-2)
                        },
                        new Cause
                        {
                            NGOId = saylani?.NGOId ?? 3,
                            Name = "Youth Welfare & Tech Skills Bootcamps",
                            Category = "Youth Welfare",
                            Description = "Free coding, AI, and graphic design courses for underprivileged youth to prepare them for global freelance and remote careers.",
                            ImageUrl = "https://images.unsplash.com/photo-1524178232363-1fb2b075b655?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 1500000,
                            RaisedAmount = 880000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-1)
                        },
                        new Cause
                        {
                            NGOId = edhi?.NGOId ?? 1,
                            Name = "Elderly Welfare & Nursing Care Homes",
                            Category = "Elderly Welfare",
                            Description = "Comfortable shelter, warm meals, specialized geriatric care, and psychological companionship for destitute senior citizens.",
                            ImageUrl = "https://images.unsplash.com/photo-1581579438747-1dc8d17bbce4?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 2200000,
                            RaisedAmount = 1350000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-2)
                        },
                        new Cause
                        {
                            NGOId = skm?.NGOId ?? 4,
                            Name = "Free Cancer Treatment for Needy Patients",
                            Category = "Healthcare",
                            Description = "Covering comprehensive chemotherapy, radiation therapy, and surgical oncology costs for underprivileged cancer patients.",
                            ImageUrl = "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 5000000,
                            RaisedAmount = 3750000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-4)
                        },
                        new Cause
                        {
                            NGOId = saylani?.NGOId ?? 3,
                            Name = "Daily Food Assistance & Dastarkhwan",
                            Category = "Food Assistance",
                            Description = "Serving two hygienic hot meals daily to laborers, daily wage earners, and families living below the poverty line.",
                            ImageUrl = "https://images.unsplash.com/photo-1593113598332-cd288d649433?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 3500000,
                            RaisedAmount = 2800000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-3)
                        },
                        new Cause
                        {
                            NGOId = edhi?.NGOId ?? 1,
                            Name = "Emergency Disaster & Winter Relief",
                            Category = "Emergency Relief",
                            Description = "Emergency ration packs, waterproof family tents, clean drinking water, and winter blankets for disaster-hit regions.",
                            ImageUrl = "https://images.unsplash.com/photo-1547496502-affa22d38842?w=600&auto=format&fit=crop&q=80",
                            TargetAmount = 4000000,
                            RaisedAmount = 3100000,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow.AddMonths(-2)
                        }
                    };

                    await context.Causes.AddRangeAsync(causes);
                    await context.SaveChangesAsync();
                }

                // 6. Seed Campaigns if empty
                if (!await context.Campaigns.AnyAsync())
                {
                    var causesList = await context.Causes.ToListAsync();
                    var edhiCause = causesList.FirstOrDefault(c => c.Category == "Children Welfare");
                    var tcfCause = causesList.FirstOrDefault(c => c.Category == "Education");
                    var skmCause = causesList.FirstOrDefault(c => c.Category == "Healthcare");
                    var emergencyCause = causesList.FirstOrDefault(c => c.Category == "Emergency Relief");

                    if (edhiCause != null && tcfCause != null && skmCause != null)
                    {
                        var campaigns = new List<Campaign>
                        {
                            new Campaign
                            {
                                NGOId = tcfCause.NGOId,
                                CauseId = tcfCause.CauseId,
                                Title = "Educate 1,000 Girls in Rural Sindh & Balochistan",
                                Description = "Help build purpose-built classrooms, provide textbooks and train female educators to illuminate the future of young girls.",
                                GoalAmount = 2000000,
                                RaisedAmount = 1420000,
                                StartDate = DateTime.UtcNow.AddMonths(-1),
                                EndDate = DateTime.UtcNow.AddMonths(2),
                                ImageUrl = "https://images.unsplash.com/photo-1509062522246-3755977927d7?w=600&auto=format&fit=crop&q=80",
                                Status = CampaignStatus.Active,
                                IsActive = true,
                                CreatedDate = DateTime.UtcNow.AddMonths(-1)
                            },
                            new Campaign
                            {
                                NGOId = skmCause.NGOId,
                                CauseId = skmCause.CauseId,
                                Title = "Zakat & Hope for 250 Pediatric Cancer Patients",
                                Description = "Your Zakat can cure childhood cancer. Every donation directly finances life-saving chemotherapy regimens.",
                                GoalAmount = 4000000,
                                RaisedAmount = 3200000,
                                StartDate = DateTime.UtcNow.AddMonths(-2),
                                EndDate = DateTime.UtcNow.AddMonths(1),
                                ImageUrl = "https://images.unsplash.com/photo-1576091160399-112ba8d25d1d?w=600&auto=format&fit=crop&q=80",
                                Status = CampaignStatus.Active,
                                IsActive = true,
                                CreatedDate = DateTime.UtcNow.AddMonths(-2)
                            },
                            new Campaign
                            {
                                NGOId = (emergencyCause ?? edhiCause).NGOId,
                                CauseId = (emergencyCause ?? edhiCause).CauseId,
                                Title = "Emergency Winter Relief Drive 2026",
                                Description = "Distributing heavy thermal quilts, winter jackets, and food supplies to mountainous and high-altitude rural valleys.",
                                GoalAmount = 1500000,
                                RaisedAmount = 1100000,
                                StartDate = DateTime.UtcNow.AddDays(-15),
                                EndDate = DateTime.UtcNow.AddMonths(1),
                                ImageUrl = "https://images.unsplash.com/photo-1547496502-affa22d38842?w=600&auto=format&fit=crop&q=80",
                                Status = CampaignStatus.Active,
                                IsActive = true,
                                CreatedDate = DateTime.UtcNow.AddDays(-15)
                            }
                        };

                        await context.Campaigns.AddRangeAsync(campaigns);
                        await context.SaveChangesAsync();
                    }
                }

                // 7. Seed Programmes if empty
                if (!await context.Programmes.AnyAsync())
                {
                    var ngosList = await context.NGOs.ToListAsync();
                    var edhi = ngosList.FirstOrDefault(n => n.Name.Contains("Edhi")) ?? ngosList.First();
                    var tcf = ngosList.FirstOrDefault(n => n.Name.Contains("Citizens")) ?? ngosList.First();
                    var saylani = ngosList.FirstOrDefault(n => n.Name.Contains("Saylani")) ?? ngosList.First();

                    var programmes = new List<Programme>
                    {
                        new Programme
                        {
                            NGOId = saylani.NGOId,
                            Title = "Youth AI & Web Development Bootcamp 2026",
                            Description = "A 6-week hands-on training workshop for talented youth from underprivileged families to build modern web solutions.",
                            Date = DateTime.UtcNow.AddDays(10),
                            StartTime = "09:00 AM",
                            EndTime = "01:00 PM",
                            Location = "Saylani Tech Hub, Gulshan Campus, Karachi",
                            ImageUrl = "https://images.unsplash.com/photo-1531482615713-2afd69097998?w=600&auto=format&fit=crop&q=80",
                            Capacity = 120,
                            Status = ProgrammeStatus.Upcoming,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new Programme
                        {
                            NGOId = edhi.NGOId,
                            Title = "Free Medical Camp & Blood Donation Drive",
                            Description = "Specialist physicians, pediatricians, free diagnostic tests, and urgent blood donation collection for thalassemic children.",
                            Date = DateTime.UtcNow.AddDays(18),
                            StartTime = "10:00 AM",
                            EndTime = "05:00 PM",
                            Location = "Edhi Welfare Complex, Rawalpindi",
                            ImageUrl = "https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?w=600&auto=format&fit=crop&q=80",
                            Capacity = 300,
                            Status = ProgrammeStatus.Upcoming,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new Programme
                        {
                            NGOId = tcf.NGOId,
                            Title = "Volunteer Teacher & Mentorship Weekend",
                            Description = "Spend your Saturday inspiring young minds. Conduct reading circles, science experiments, and art workshops.",
                            Date = DateTime.UtcNow.AddDays(25),
                            StartTime = "11:00 AM",
                            EndTime = "03:00 PM",
                            Location = "TCF Qayyumabad Campus, Karachi",
                            ImageUrl = "https://images.unsplash.com/photo-1524178232363-1fb2b075b655?w=600&auto=format&fit=crop&q=80",
                            Capacity = 50,
                            Status = ProgrammeStatus.Upcoming,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        }
                    };

                    await context.Programmes.AddRangeAsync(programmes);
                    await context.SaveChangesAsync();
                }

                // 8. Seed Events if empty
                if (!await context.Events.AnyAsync())
                {
                    var ngosList = await context.NGOs.ToListAsync();
                    var edhi = ngosList.FirstOrDefault(n => n.Name.Contains("Edhi")) ?? ngosList.First();
                    var skm = ngosList.FirstOrDefault(n => n.Name.Contains("Shaukat")) ?? ngosList.First();

                    var events = new List<Event>
                    {
                        new Event
                        {
                            NGOId = skm.NGOId,
                            Title = "GIVE-AID Annual Charity Gala & Cancer Survivor Recognition",
                            Description = "An evening honoring our brave survivors, generous benefactors, and medical staff. Includes keynotes, dinner, and awards.",
                            EventDate = DateTime.UtcNow.AddDays(30).Date.AddHours(19),
                            Location = "Serena Hotel, Islamabad",
                            ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?w=600&auto=format&fit=crop&q=80",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new Event
                        {
                            NGOId = edhi.NGOId,
                            Title = "National Humanitarian Day & Volunteer Summit",
                            Description = "Gathering humanitarians, social activists, and youth ambassadors to discuss sustainable poverty alleviation strategies.",
                            EventDate = DateTime.UtcNow.AddDays(45).Date.AddHours(10),
                            Location = "Expo Centre, Lahore",
                            ImageUrl = "https://images.unsplash.com/photo-1475721027785-f74eccf877e2?w=600&auto=format&fit=crop&q=80",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        }
                    };

                    await context.Events.AddRangeAsync(events);
                    await context.SaveChangesAsync();
                }

                // 9. Seed Gallery if empty
                if (!await context.GalleryItems.AnyAsync())
                {
                    var ngosList = await context.NGOs.ToListAsync();
                    var edhi = ngosList.FirstOrDefault(n => n.Name.Contains("Edhi"));

                    var gallery = new List<GalleryItem>
                    {
                        new GalleryItem
                        {
                            NGOId = edhi?.NGOId,
                            Title = "Orphanage Children Birthday Celebrations & Gift Distribution",
                            ImageUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800&auto=format&fit=crop&q=80",
                            Category = "Children Welfare",
                            DateTaken = DateTime.UtcNow.AddDays(-10),
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new GalleryItem
                        {
                            NGOId = edhi?.NGOId,
                            Title = "Distribution of Food Ration Bags to Flood Affected Families",
                            ImageUrl = "https://images.unsplash.com/photo-1593113598332-cd288d649433?w=800&auto=format&fit=crop&q=80",
                            Category = "Food Assistance",
                            DateTaken = DateTime.UtcNow.AddDays(-20),
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new GalleryItem
                        {
                            NGOId = edhi?.NGOId,
                            Title = "Wheelchair & Mobility Aid Handover Ceremony",
                            ImageUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=800&auto=format&fit=crop&q=80",
                            Category = "Disabled Welfare",
                            DateTaken = DateTime.UtcNow.AddDays(-35),
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new GalleryItem
                        {
                            NGOId = edhi?.NGOId,
                            Title = "TCF Classrooms: Lighting the Candle of Knowledge",
                            ImageUrl = "https://images.unsplash.com/photo-1509062522246-3755977927d7?w=800&auto=format&fit=crop&q=80",
                            Category = "Education",
                            DateTaken = DateTime.UtcNow.AddDays(-40),
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        }
                    };

                    await context.GalleryItems.AddRangeAsync(gallery);
                    await context.SaveChangesAsync();
                }

                // 10. Seed Partners if empty
                if (!await context.Partners.AnyAsync())
                {
                    var partners = new List<Partner>
                    {
                        new Partner
                        {
                            Name = "Habib Bank Limited (HBL)",
                            Description = "Official banking and financial inclusion partner supporting transparent digital donations and micro-charity solutions.",
                            LogoUrl = "https://images.unsplash.com/photo-1559526324-4b87b5e36e44?w=300&auto=format&fit=crop&q=80",
                            WebsiteUrl = "https://hbl.com",
                            Category = "Financial Partner",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new Partner
                        {
                            Name = "Jazz Cash & Telecom",
                            Description = "Mobile payment processing partner enabling seamless wallet donations for millions of mobile subscribers.",
                            LogoUrl = "https://images.unsplash.com/photo-1563986768609-322da13575f3?w=300&auto=format&fit=crop&q=80",
                            WebsiteUrl = "https://jazzcash.com.pk",
                            Category = "Payment Partner",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new Partner
                        {
                            Name = "Systems Limited",
                            Description = "Technology and cloud infrastructure sponsor providing platform reliability, security audits, and developer expertise.",
                            LogoUrl = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=300&auto=format&fit=crop&q=80",
                            WebsiteUrl = "https://systemsltd.com",
                            Category = "Technology Partner",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new Partner
                        {
                            Name = "Engro Foundation",
                            Description = "Strategic social impact partner investing in rural health, clean water filtration plants, and women empowerment.",
                            LogoUrl = "https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=300&auto=format&fit=crop&q=80",
                            WebsiteUrl = "https://engrofoundation.com",
                            Category = "Strategic Partner",
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        }
                    };

                    await context.Partners.AddRangeAsync(partners);
                    await context.SaveChangesAsync();
                }

                // 11. Seed FAQs if empty
                if (!await context.FAQs.AnyAsync())
                {
                    var faqs = new List<FAQ>
                    {
                        new FAQ
                        {
                            Category = "Donation Help",
                            Question = "How do I donate to a specific welfare cause or NGO?",
                            Answer = "Click on the 'Donate' button in the navigation bar or on any Cause/Campaign card. Select the NGO, chosen welfare cause, enter your donation amount, choose your payment method (Credit/Debit Card, EasyPaisa, or JazzCash), and complete the payment information.",
                            DisplayOrder = 1,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new FAQ
                        {
                            Category = "Payment Methods",
                            Question = "How does EasyPaisa & JazzCash payment work?",
                            Answer = "When you choose EasyPaisa or JazzCash on the Donate page, a payment modal will open showing the designated account details. Send the donation amount from your mobile account app or retailer, copy your Transaction ID (TID), upload the payment receipt/screenshot, and click Submit. Our admin team will verify it promptly.",
                            DisplayOrder = 2,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new FAQ
                        {
                            Category = "Payment Methods",
                            Question = "Are credit/debit card transactions secure?",
                            Answer = "Yes, absolutely! GIVE-AID implements strict industry-grade security protocols. We never store CVV security numbers or full credit card digits. All transactions are securely processed and encrypted.",
                            DisplayOrder = 3,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new FAQ
                        {
                            Category = "Donation Help",
                            Question = "Can I get an official tax-exempt donation receipt?",
                            Answer = "Yes! Once your donation is verified and approved, you can navigate to 'My Donations' in your dashboard and click 'Print Receipt' to generate and print your official receipt with unique Donation and Payment references.",
                            DisplayOrder = 4,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new FAQ
                        {
                            Category = "Programmes",
                            Question = "How can I volunteer or participate in upcoming NGO programmes?",
                            Answer = "Browse our 'Programmes' section, select an upcoming initiative, and click 'Show Interest / Register'. Fill out your details and our programme coordinator will contact you with logistics.",
                            DisplayOrder = 5,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        },
                        new FAQ
                        {
                            Category = "Account & Security",
                            Question = "Can I donate as a guest without creating an account?",
                            Answer = "Yes, guest donations are supported. However, creating a free account allows you to track all your donation history, print consolidated receipts, and receive updates on cause outcomes.",
                            DisplayOrder = 6,
                            IsActive = true,
                            CreatedDate = DateTime.UtcNow
                        }
                    };

                    await context.FAQs.AddRangeAsync(faqs);
                    await context.SaveChangesAsync();
                }

                // 12. Seed AboutUsContents if empty
                if (!await context.AboutUsContents.AnyAsync())
                {
                    var contents = new List<AboutUsContent>
                    {
                        new AboutUsContent
                        {
                            SectionKey = "WhatWeDo",
                            Title = "What We Do",
                            Subtitle = "Connecting Compassionate Donors with Verified Humanitarian Causes",
                            Content = "GIVE-AID bridges the gap between generous benefactors and impactful non-governmental organizations across the country. We provide a transparent, secure, and accountable digital ecosystem where every rupee donated is tracked, verified, and directed towards transforming vulnerable lives. From orphan care and emergency flood relief to cancer treatments and underprivileged education, GIVE-AID amplifies collective impact.",
                            ImageUrl = "https://images.unsplash.com/photo-1593113598332-cd288d649433?w=800&auto=format&fit=crop&q=80",
                            IconClass = "fa-solid fa-hands-holding-child",
                            DisplayOrder = 1,
                            LastUpdated = DateTime.UtcNow
                        },
                        new AboutUsContent
                        {
                            SectionKey = "OurMission",
                            Title = "Our Mission & Vision",
                            Subtitle = "100% Transparency, Zero Compromise on Human Dignity",
                            Content = "Our mission is to democratize philanthropic giving through technology, ensuring maximum transparency, real-time auditability, and immediate relief to needy populations. We envision a society where no child is deprived of education, no patient is turned away due to lack of medical funds, and every distressed individual finds swift support.",
                            ImageUrl = "https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800&auto=format&fit=crop&q=80",
                            IconClass = "fa-solid fa-bullseye",
                            DisplayOrder = 2,
                            LastUpdated = DateTime.UtcNow
                        },
                        new AboutUsContent
                        {
                            SectionKey = "OurTeam",
                            Title = "Our Leadership & Team",
                            Subtitle = "Dedicated Professionals Driving Social Change",
                            Content = "GIVE-AID is powered by an accomplished multidisciplinary team of social workers, software engineers, financial auditors, and community volunteers. Our governance board ensures rigorous compliance and vetting for every participating NGO and welfare initiative.",
                            ImageUrl = "https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=800&auto=format&fit=crop&q=80",
                            IconClass = "fa-solid fa-users",
                            DisplayOrder = 3,
                            LastUpdated = DateTime.UtcNow
                        },
                        new AboutUsContent
                        {
                            SectionKey = "CareerWithUs",
                            Title = "Career With Us",
                            Subtitle = "Do Meaningful Work That Creates Real Impact",
                            Content = "Join a passionate group of changemakers. We offer fulfilling career and volunteer opportunities in non-profit management, digital outreach, humanitarian logistics, and software development. Explore our current openings and volunteer fellowships to make a difference.",
                            ImageUrl = "https://images.unsplash.com/photo-1521737711867-e3b97375f902?w=800&auto=format&fit=crop&q=80",
                            IconClass = "fa-solid fa-briefcase",
                            DisplayOrder = 4,
                            LastUpdated = DateTime.UtcNow
                        },
                        new AboutUsContent
                        {
                            SectionKey = "OurAchievements",
                            Title = "Our Achievements",
                            Subtitle = "Milestones of Hope & Empowerment",
                            Content = "Over the past 5 years, GIVE-AID has facilitated over PKR 150 Million in verified donations, supported 50,000+ school students with scholarships, funded 12,000+ medical surgeries, and dispatched emergency relief to over 200,000 disaster survivors across 40+ districts.",
                            ImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865?w=800&auto=format&fit=crop&q=80",
                            IconClass = "fa-solid fa-trophy",
                            DisplayOrder = 5,
                            LastUpdated = DateTime.UtcNow
                        },
                        new AboutUsContent
                        {
                            SectionKey = "OurSupporters",
                            Title = "Our Supporters & Benefactors",
                            Subtitle = "United for Humanity",
                            Content = "We are deeply grateful to hundreds of thousands of individual donors, corporate CSR foundations, and philanthropic institutions who place their trust in GIVE-AID every day. Together, we are building stronger, more resilient communities.",
                            ImageUrl = "https://images.unsplash.com/photo-1559526324-4b87b5e36e44?w=800&auto=format&fit=crop&q=80",
                            IconClass = "fa-solid fa-heart",
                            DisplayOrder = 6,
                            LastUpdated = DateTime.UtcNow
                        },
                        new AboutUsContent
                        {
                            SectionKey = "ReadAboutUs",
                            Title = "Read About Us & Media Coverage",
                            Subtitle = "Stories of Hope, Transformation, and Progress",
                            Content = "Discover independent journalistic reports, documentary features, and verified case studies highlighting how GIVE-AID and partner NGOs are turning donations into life-saving outcomes. Read our quarterly audit reports and impact publications.",
                            ImageUrl = "https://images.unsplash.com/photo-1504711434969-e33886168f5c?w=800&auto=format&fit=crop&q=80",
                            IconClass = "fa-solid fa-newspaper",
                            DisplayOrder = 7,
                            LastUpdated = DateTime.UtcNow
                        }
                    };

                    await context.AboutUsContents.AddRangeAsync(contents);
                    await context.SaveChangesAsync();
                }

                logger.LogInformation("Database seeded successfully with initial administration, NGOs, causes, campaigns, programmes, and content.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database migration and seeding.");
            }
        }
    }
}
