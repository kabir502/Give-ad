using NGO_website.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace NGO_website.ViewModels.Home
{
    public class HomeViewModel
    {
        public List<Cause> FeaturedCauses { get; set; } = new();
        public List<Campaign> ActiveCampaigns { get; set; } = new();
        public List<NGO> RegisteredNGOs { get; set; } = new();
        public List<Programme> UpcomingProgrammes { get; set; } = new();
        public List<Event> UpcomingEvents { get; set; } = new();
        public List<GalleryItem> GalleryPreview { get; set; } = new();
        public List<Partner> Partners { get; set; } = new();

        // Statistics
        public decimal TotalFundsRaised { get; set; }
        public int TotalDonationsCount { get; set; }
        public int TotalNGOsCount { get; set; }
        public int TotalCausesCount { get; set; }
        public int TotalLivesImpacted => (int)(TotalDonationsCount * 3.5);
    }

    public class ContactViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(100)]
        [Display(Name = "Your Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number (Optional)")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Please enter a subject.")]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your message.")]
        [StringLength(4000)]
        public string Message { get; set; } = string.Empty;
    }

    public class HelpCentreViewModel
    {
        public string? SearchQuery { get; set; }
        public string? SelectedCategory { get; set; }
        public List<string> Categories { get; set; } = new();
        public List<FAQ> FAQs { get; set; } = new();
    }
}
