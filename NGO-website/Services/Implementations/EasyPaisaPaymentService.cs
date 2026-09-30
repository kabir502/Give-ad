using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;

namespace NGO_website.Services.Implementations
{
    /// <summary>
    /// EasyPaisa Manual/Direct Mobile Payment Service handler
    /// Prepares pending manual verification record with receipt upload
    /// </summary>
    public class EasyPaisaPaymentService : IPaymentService
    {
        public PaymentMethodType SupportedMethod => PaymentMethodType.EasyPaisa;

        public Task<PaymentProcessingResult> ProcessPaymentAsync(PaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TransactionId))
            {
                return Task.FromResult(new PaymentProcessingResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "EasyPaisa Transaction ID (TID) is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.SenderMobile))
            {
                return Task.FromResult(new PaymentProcessingResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Sender mobile number is required."
                });
            }

            return Task.FromResult(new PaymentProcessingResult
            {
                IsSuccessful = true,
                TransactionId = request.TransactionId.Trim(),
                Status = PaymentStatus.Pending,
                GatewayResponseCode = "EASYPAISA_MANUAL_PENDING"
            });
        }
    }
}
