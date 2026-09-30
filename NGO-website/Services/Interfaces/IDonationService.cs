using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.ViewModels.Donations;

namespace NGO_website.Services.Interfaces
{
    public class DonationCreationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Donation? Donation { get; set; }
        public Payment? Payment { get; set; }
    }

    public class ApprovalResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Donation? Donation { get; set; }
    }

    public interface IDonationService
    {
        Task<DonationCreationResult> CreateDonationAsync(DonationFormViewModel model, string? currentUserId);
        Task<ApprovalResult> ApprovePaymentAsync(int paymentId, string adminUserId, string? adminRemarks);
        Task<ApprovalResult> RejectPaymentAsync(int paymentId, string adminUserId, string? adminRemarks);
        Task<bool> IsTransactionIdDuplicateAsync(string transactionId);
        Task<string> GenerateDonationNumberAsync();
        Task<string> GeneratePaymentReferenceAsync();
    }
}
