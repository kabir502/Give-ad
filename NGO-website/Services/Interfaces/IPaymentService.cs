using NGO_website.Models.Enums;

namespace NGO_website.Services.Interfaces
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? Cvv { get; set; }
        public string? TransactionId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderMobile { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? ReceiptFilePath { get; set; }
    }

    public class PaymentProcessingResult
    {
        public bool IsSuccessful { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string? MaskedCardNumber { get; set; }
        public string? CardExpiry { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? GatewayResponseCode { get; set; }
    }

    public interface IPaymentService
    {
        PaymentMethodType SupportedMethod { get; }
        Task<PaymentProcessingResult> ProcessPaymentAsync(PaymentRequest request);
    }
}
