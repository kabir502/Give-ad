using NGO_website.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NGO_website.Models.Entities
{
    /// <summary>
    /// Payment record for card, EasyPaisa, or JazzCash transactions
    /// </summary>
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Payment Reference")]
        public string PaymentReference { get; set; } = string.Empty;

        [Required]
        public int DonationId { get; set; }

        [ForeignKey("DonationId")]
        public virtual Donation? Donation { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Payment Amount (PKR)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethodType PaymentMethod { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Transaction ID")]
        public string TransactionId { get; set; } = string.Empty;

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // Card Specific fields (No CVV, masked number only)
        [StringLength(100)]
        [Display(Name = "Card Holder Name")]
        public string? CardHolderName { get; set; }

        [StringLength(30)]
        [Display(Name = "Card Number (Masked)")]
        public string? MaskedCardNumber { get; set; }

        [StringLength(10)]
        public string? CardExpiry { get; set; }

        // Mobile Banking Specific fields (EasyPaisa / JazzCash)
        [StringLength(100)]
        [Display(Name = "Sender Name")]
        public string? SenderName { get; set; }

        [Phone]
        [StringLength(30)]
        [Display(Name = "Sender Mobile")]
        public string? SenderMobile { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Receipt File")]
        public string? ReceiptFilePath { get; set; }

        // Verification & Audit Fields
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public DateTime? VerificationDate { get; set; }

        public string? VerifiedById { get; set; }

        [ForeignKey("VerifiedById")]
        public virtual ApplicationUser? VerifiedBy { get; set; }

        [StringLength(1000)]
        [Display(Name = "Admin Remarks")]
        public string? AdminRemarks { get; set; }
    }
}
