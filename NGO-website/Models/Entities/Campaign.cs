using NGO_website.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Specific fundraising campaign linked to an NGO and Cause
    /// </summary>
    public class Campaign
    {
        [Key]
        public int CampaignId { get; set; }

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

        [Required]
        [StringLength(200)]
        [Display(Name = "Campaign Title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, 1000000000)]
        [Display(Name = "Goal Amount (PKR)")]
        public decimal GoalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Raised Amount (PKR)")]
        public decimal RaisedAmount { get; set; } = 0;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(3);

        [StringLength(500)]
        [Display(Name = "Image URL / Path")]
        public string? ImageUrl { get; set; }

        public CampaignStatus Status { get; set; } = CampaignStatus.Active;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public int ProgressPercentage => GoalAmount > 0 ? Math.Min(100, (int)Math.Round((RaisedAmount / GoalAmount) * 100)) : 0;

        // Navigation properties
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
