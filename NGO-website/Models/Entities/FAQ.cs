using System.ComponentModel.DataAnnotations;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Frequently Asked Questions for Help Centre
    /// </summary>
    public class FAQ
    {
        [Key]
        public int FAQId { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Donation Help"; // Donation Help, Payment Methods, Account & Security, Programmes, NGO Partnership

        [Required]
        [StringLength(300)]
        public string Question { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Answer { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
