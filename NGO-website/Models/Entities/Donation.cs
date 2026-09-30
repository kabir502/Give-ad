using NGO_website.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Donation record tracking financial contributions
    /// </summary>
    public class Donation
    {
        [Key]
        public int DonationId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Donation Number")]
        public string DonationNumber { get; set; } = string.Empty;

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Donor Name")]
        public string DonorName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Donor Email")]
        public string DonorEmail { get; set; } = string.Empty;

        [Phone]
        [StringLength(30)]
        [Display(Name = "Donor Phone")]
        public string? DonorPhone { get; set; }

        [Required]
        [Display(Name = "NGO")]
        public int NGOId { get; set; }

        [ForeignKey("NGOId")]
        public virtual NGO? NGO { get; set; }

        [Required]
        [Display(Name = "Cause")]
        public int CauseId { get; set; }

        [ForeignKey("CauseId")]
        public virtual Cause? Cause { get; set; }

        [Display(Name = "Campaign (Optional)")]
        public int? CampaignId { get; set; }

        [ForeignKey("CampaignId")]
        public virtual Campaign? Campaign { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, 10000000)]
        [Display(Name = "Donation Amount (PKR)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethodType PaymentMethod { get; set; }

        public DonationStatus Status { get; set; } = DonationStatus.Pending;

        [StringLength(1000)]
        public string? DonorNotes { get; set; }

        // Audit Trail Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? ApprovedById { get; set; }

        [ForeignKey("ApprovedById")]
        public virtual ApplicationUser? ApprovedBy { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? RejectedById { get; set; }

        [ForeignKey("RejectedById")]
        public virtual ApplicationUser? RejectedBy { get; set; }

        [StringLength(1000)]
        [Display(Name = "Admin Remarks")]
        public string? AdminRemarks { get; set; }

        // 1-to-1 Navigation to Payment record
        public virtual Payment? Payment { get; set; }
    }
}
