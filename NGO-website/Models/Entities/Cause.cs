using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Welfare Cause supported by an NGO
    /// </summary>
    public class Cause
    {
        [Key]
        public int CauseId { get; set; }

        [Required]
        [Display(Name = "NGO")]
        public int NGOId { get; set; }

        [ForeignKey("NGOId")]
        public virtual NGO? NGO { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Cause Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Children Welfare"; // e.g. Children, Education, Disabled, Woman, Youth, Elderly, Healthcare, Food, Emergency

        [Required]
        [StringLength(3000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Image URL / Path")]
        public string? ImageUrl { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, 1000000000)]
        [Display(Name = "Target Amount (PKR)")]
        public decimal TargetAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Raised Amount (PKR)")]
        public decimal RaisedAmount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Progress calculation
        [NotMapped]
        public int ProgressPercentage => TargetAmount > 0 ? Math.Min(100, (int)Math.Round((RaisedAmount / TargetAmount) * 100)) : 0;

        // Navigation properties
        public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
