using NGO_website.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace NGO_website.ViewModels.User
{
    public class UserDashboardViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int TotalDonationsCount { get; set; }
        public int ApprovedDonationsCount { get; set; }
        public int PendingDonationsCount { get; set; }
        public int RejectedDonationsCount { get; set; }
        public decimal TotalAmountDonated { get; set; }
        public int ActiveQueriesCount { get; set; }
        public int RegisteredProgrammesCount { get; set; }

        public List<Donation> RecentDonations { get; set; } = new();
        public List<Programme> UpcomingProgrammes { get; set; } = new();
        public List<Query> RecentQueries { get; set; } = new();
    }

    public class InviteFriendViewModel
    {
        [Required(ErrorMessage = "Please enter your friend's name.")]
        [StringLength(100)]
        [Display(Name = "Friend's Full Name")]
        public string FriendName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your friend's email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Friend's Email Address")]
        public string FriendEmail { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Personal Message (Optional)")]
        public string? PersonalMessage { get; set; }
    }

    public class SubmitInterestViewModel
    {
        [Required]
        public int ProgrammeId { get; set; }

        public string ProgrammeTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Message or relevant experience")]
        public string? Notes { get; set; }
    }
}
