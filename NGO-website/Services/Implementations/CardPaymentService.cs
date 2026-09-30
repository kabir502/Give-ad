using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;
using System.Text.RegularExpressions;

namespace NGO_website.Services.Implementations
{
    /// <summary>
    /// Simulated / Demo Card Payment Gateway (Visa/Mastercard)
    /// In production, replace this implementation with Stripe/Checkout/PayCapsule/Bank API integration.
    /// </summary>
    public class CardPaymentService : IPaymentService
    {
        public PaymentMethodType SupportedMethod => PaymentMethodType.CreditDebitCard;

        public Task<PaymentProcessingResult> ProcessPaymentAsync(PaymentRequest request)
        {
            // Server-side validation
            if (string.IsNullOrWhiteSpace(request.CardHolderName))
            {
                return Task.FromResult(new PaymentProcessingResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Card holder name is required."
                });
            }

            var cleanCardNumber = (request.CardNumber ?? "").Replace(" ", "").Replace("-", "");
            if (cleanCardNumber.Length < 13 || cleanCardNumber.Length > 19 || !cleanCardNumber.All(char.IsDigit))
            {
                return Task.FromResult(new PaymentProcessingResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Invalid credit/debit card number format."
                });
            }

            if (string.IsNullOrWhiteSpace(request.ExpiryMonth) || string.IsNullOrWhiteSpace(request.ExpiryYear))
            {
                return Task.FromResult(new PaymentProcessingResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Card expiration date is required."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Cvv) || request.Cvv.Length < 3 || request.Cvv.Length > 4 || !request.Cvv.All(char.IsDigit))
            {
                return Task.FromResult(new PaymentProcessingResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "Invalid CVV security code."
                });
            }

            // Expiry date verification
            if (int.TryParse(request.ExpiryMonth, out int expMonth) && int.TryParse(request.ExpiryYear, out int expYear))
            {
                var fullYear = expYear < 100 ? 2000 + expYear : expYear;
                var now = DateTime.UtcNow;
                if (fullYear < now.Year || (fullYear == now.Year && expMonth < now.Month))
                {
                    return Task.FromResult(new PaymentProcessingResult
                    {
                        IsSuccessful = false,
                        ErrorMessage = "The card has expired."
                    });
                }
            }

            // Mask card number: ************1234
            var last4 = cleanCardNumber.Length >= 4 ? cleanCardNumber.Substring(cleanCardNumber.Length - 4) : cleanCardNumber;
            var maskedNumber = new string('*', Math.Max(12, cleanCardNumber.Length - 4)) + last4;

            // Simulated Gateway Transaction ID
            var simTxId = $"CARD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

            return Task.FromResult(new PaymentProcessingResult
            {
                IsSuccessful = true,
                TransactionId = simTxId,
                MaskedCardNumber = maskedNumber,
                CardExpiry = $"{request.ExpiryMonth}/{request.ExpiryYear}",
                Status = PaymentStatus.Verified,
                GatewayResponseCode = "00_APPROVED_DEMO"
            });
        }
    }
}
