using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using NGO_website.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace NGO_website.ViewModels.Donations
{
    public class DonationFormViewModel
    {
        [Required(ErrorMessage = "Please select an NGO.")]
        [Display(Name = "Select NGO")]
        public int NGOId { get; set; }

        [Required(ErrorMessage = "Please select a Cause.")]
        [Display(Name = "Select Welfare Cause")]
        public int CauseId { get; set; }

        [Display(Name = "Campaign (Optional)")]
        public int? CampaignId { get; set; }

        [Required(ErrorMessage = "Please enter a donation amount.")]
        [Range(10, 10000000, ErrorMessage = "Donation amount must be at least Rs. 10.")]
        [Display(Name = "Donation Amount (PKR)")]
        public decimal Amount { get; set; } = 1000;

        [Required(ErrorMessage = "Please choose a payment method.")]
        [Display(Name = "Payment Method")]
        public PaymentMethodType PaymentMethod { get; set; } = PaymentMethodType.CreditDebitCard;

        [Required(ErrorMessage = "Donor name is required.")]
        [StringLength(100)]
        [Display(Name = "Your Full Name")]
        public string DonorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Your Email Address")]
        public string DonorEmail { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? DonorPhone { get; set; }

        [StringLength(500)]
        [Display(Name = "Message / Dedication (Optional)")]
        public string? DonorNotes { get; set; }

        // --- Card Payment Fields ---
        [Display(Name = "Card Holder Name")]
        public string? CardHolderName { get; set; }

        [Display(Name = "Card Number")]
        public string? CardNumber { get; set; }

        [Display(Name = "Expiry Month")]
        public string? ExpiryMonth { get; set; }

        [Display(Name = "Expiry Year")]
        public string? ExpiryYear { get; set; }

        [Display(Name = "CVV")]
        public string? Cvv { get; set; }

        // --- EasyPaisa / JazzCash Mobile Banking Fields ---
        [Display(Name = "Transaction ID")]
        public string? TransactionId { get; set; }

        [Display(Name = "Sender Name")]
        public string? SenderName { get; set; }

        [Display(Name = "Sender Mobile Number")]
        public string? SenderMobile { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Upload Payment Receipt (JPG, PNG, PDF)")]
        public IFormFile? ReceiptFile { get; set; }

        // Dropdown data
        public IEnumerable<SelectListItem>? NgoList { get; set; }
        public IEnumerable<SelectListItem>? CauseList { get; set; }
        public IEnumerable<SelectListItem>? CampaignList { get; set; }
    }

    public class DonationConfirmationViewModel
    {
        public int DonationId { get; set; }
        public string DonationNumber { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string NGOName { get; set; } = string.Empty;
        public string CauseName { get; set; } = string.Empty;
        public PaymentMethodType PaymentMethod { get; set; }
        public DonationStatus DonationStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class DonationReceiptViewModel
    {
        public int DonationId { get; set; }
        public string DonationNumber { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public string DonorName { get; set; } = string.Empty;
        public string DonorEmail { get; set; } = string.Empty;
        public string? DonorPhone { get; set; }
        public string NGOName { get; set; } = string.Empty;
        public string? NGOLogoUrl { get; set; }
        public string NGOAddress { get; set; } = string.Empty;
        public string NGOPhone { get; set; } = string.Empty;
        public string NGOEmail { get; set; } = string.Empty;
        public string CauseName { get; set; } = string.Empty;
        public string? CampaignTitle { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public DateTime DonationDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdminRemarks { get; set; }
        public string? MaskedCardNumber { get; set; }
    }
}
