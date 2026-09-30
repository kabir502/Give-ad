using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace NGO_website.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalNGOs { get; set; }
        public int TotalCauses { get; set; }
        public int TotalCampaigns { get; set; }
        public int TotalDonations { get; set; }
        public decimal TotalAmountReceived { get; set; }
        public int PendingPaymentsCount { get; set; }
        public int ApprovedPaymentsCount { get; set; }
        public int RejectedPaymentsCount { get; set; }
        public int TotalProgrammes { get; set; }
        public int TotalOpenQueries { get; set; }

        public List<Donation> RecentDonations { get; set; } = new();
        public List<Payment> PendingPayments { get; set; } = new();
        public List<Query> RecentQueries { get; set; } = new();
        public List<ApplicationUser> RecentUsers { get; set; } = new();

        // Chart Data Lists
        public List<string> MonthlyLabels { get; set; } = new();
        public List<decimal> MonthlyDonationAmounts { get; set; } = new();
        public List<string> CauseLabels { get; set; } = new();
        public List<decimal> CauseDonationAmounts { get; set; } = new();
        public List<string> PaymentMethodLabels { get; set; } = new();
        public List<int> PaymentMethodCounts { get; set; } = new();
    }

    public class PaymentVerificationViewModel
    {
        public string? SearchTerm { get; set; }
        public PaymentStatus? StatusFilter { get; set; }
        public PaymentMethodType? MethodFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int TotalPending { get; set; }
        public int TotalVerified { get; set; }
        public int TotalRejected { get; set; }
        public decimal TotalVerifiedAmount { get; set; }
        public List<Payment> Payments { get; set; } = new();
    }

    public class PaymentVerificationActionViewModel
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        public bool IsApprove { get; set; }

        [StringLength(1000)]
        [Display(Name = "Admin Remarks / Reason")]
        public string? Remarks { get; set; }
    }

    public class ReportsFilterViewModel
    {
        [Display(Name = "Report Type")]
        public string ReportType { get; set; } = "Donations"; // Donations, Payments, CauseWise, NgoWise, Monthly, Users

        [DataType(DataType.Date)]
        [Display(Name = "From Date")]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "To Date")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "NGO")]
        public int? NGOId { get; set; }

        [Display(Name = "Cause")]
        public int? CauseId { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethodType? PaymentMethod { get; set; }

        [Display(Name = "Status")]
        public DonationStatus? Status { get; set; }

        public IEnumerable<SelectListItem>? NgoList { get; set; }
        public IEnumerable<SelectListItem>? CauseList { get; set; }
    }

    public class ReportResultViewModel
    {
        public ReportsFilterViewModel Filter { get; set; } = new();
        public int TotalRecords { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ReportRowViewModel> Rows { get; set; } = new();
    }

    public class ReportRowViewModel
    {
        public string ReferenceNumber { get; set; } = string.Empty;
        public string DonorName { get; set; } = string.Empty;
        public string DonorEmail { get; set; } = string.Empty;
        public string NGOName { get; set; } = string.Empty;
        public string CauseName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public class NGOFormViewModel
    {
        public int NGOId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "NGO Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Current Logo URL")]
        public string? ExistingLogoUrl { get; set; }

        [Display(Name = "Upload Logo")]
        public IFormFile? LogoFile { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Url]
        public string? Website { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class CauseFormViewModel
    {
        public int CauseId { get; set; }

        [Required]
        [Display(Name = "Select NGO")]
        public int NGOId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Cause Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Children Welfare";

        [Required]
        [StringLength(3000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Current Image URL")]
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [Range(100, 100000000)]
        [Display(Name = "Target Amount (PKR)")]
        public decimal TargetAmount { get; set; }

        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem>? NgoList { get; set; }
    }

    public class CampaignFormViewModel
    {
        public int CampaignId { get; set; }

        [Required]
        [Display(Name = "Select NGO")]
        public int NGOId { get; set; }

        [Required]
        [Display(Name = "Select Cause")]
        public int CauseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(500, 100000000)]
        [Display(Name = "Goal Amount (PKR)")]
        public decimal GoalAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(3);

        [Display(Name = "Current Image URL")]
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Upload Campaign Image")]
        public IFormFile? ImageFile { get; set; }

        public CampaignStatus Status { get; set; } = CampaignStatus.Active;

        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem>? NgoList { get; set; }
        public IEnumerable<SelectListItem>? CauseList { get; set; }
    }

    public class ProgrammeFormViewModel
    {
        public int ProgrammeId { get; set; }

        [Required]
        [Display(Name = "Select NGO")]
        public int NGOId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.UtcNow.AddDays(7);

        [Display(Name = "Start Time (e.g. 10:00 AM)")]
        public string? StartTime { get; set; }

        [Display(Name = "End Time (e.g. 02:00 PM)")]
        public string? EndTime { get; set; }

        [Required]
        [StringLength(250)]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Current Image URL")]
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Upload Banner Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Volunteer / Attendee Capacity")]
        public int? Capacity { get; set; }

        public ProgrammeStatus Status { get; set; } = ProgrammeStatus.Upcoming;

        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem>? NgoList { get; set; }
    }

    public class EventFormViewModel
    {
        public int EventId { get; set; }

        [Required]
        [Display(Name = "Select NGO")]
        public int NGOId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(3000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Event Date & Time")]
        public DateTime EventDate { get; set; } = DateTime.UtcNow.AddDays(14);

        [Required]
        [StringLength(250)]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Current Image URL")]
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Upload Event Image")]
        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem>? NgoList { get; set; }
    }

    public class GalleryFormViewModel
    {
        public int GalleryItemId { get; set; }

        [Display(Name = "Select NGO (Optional)")]
        public int? NGOId { get; set; }

        [Display(Name = "Select Programme (Optional)")]
        public int? ProgrammeId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Photo Caption / Title")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Current Image URL")]
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Upload Image")]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Activities";

        [DataType(DataType.Date)]
        public DateTime DateTaken { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public IEnumerable<SelectListItem>? NgoList { get; set; }
        public IEnumerable<SelectListItem>? ProgrammeList { get; set; }
    }

    public class PartnerFormViewModel
    {
        public int PartnerId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Current Logo URL")]
        public string? ExistingLogoUrl { get; set; }

        [Display(Name = "Upload Logo")]
        public IFormFile? LogoFile { get; set; }

        [Url]
        [Display(Name = "Website URL")]
        public string? WebsiteUrl { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Strategic Partner";

        public bool IsActive { get; set; } = true;
    }

    public class UserManagementViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Profession { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsActive { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public int DonationsCount { get; set; }
        public decimal TotalDonated { get; set; }
    }
}
