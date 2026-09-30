using Microsoft.EntityFrameworkCore;
using NGO_website.Data;
using NGO_website.Models.Entities;
using NGO_website.Models.Enums;
using NGO_website.Services.Interfaces;
using NGO_website.ViewModels.Donations;

namespace NGO_website.Services.Implementations
{
    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;
        private readonly IEnumerable<IPaymentService> _paymentServices;
        private readonly ILogger<DonationService> _logger;

        public DonationService(
            ApplicationDbContext context,
            IFileService fileService,
            IEmailService emailService,
            IEnumerable<IPaymentService> paymentServices,
            ILogger<DonationService> logger)
        {
            _context = context;
            _fileService = fileService;
            _emailService = emailService;
            _paymentServices = paymentServices;
            _logger = logger;
        }

        public async Task<bool> IsTransactionIdDuplicateAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId)) return false;
            var cleanId = transactionId.Trim();
            return await _context.Payments.AnyAsync(p => p.TransactionId == cleanId);
        }

        public async Task<string> GenerateDonationNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.Donations.CountAsync(d => d.CreatedAt.Year == year);
            return $"GA-{year}-{(count + 1):D6}";
        }

        public async Task<string> GeneratePaymentReferenceAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.Payments.CountAsync(p => p.SubmittedDate.Year == year);
            return $"GAPAY-{year}-{(count + 1):D6}";
        }

        public async Task<DonationCreationResult> CreateDonationAsync(DonationFormViewModel model, string? currentUserId)
        {
            // 1. Validate Cause & NGO existence
            var cause = await _context.Causes.Include(c => c.NGO).FirstOrDefaultAsync(c => c.CauseId == model.CauseId && c.IsActive);
            if (cause == null)
            {
                return new DonationCreationResult { Success = false, ErrorMessage = "Selected welfare cause was not found or is inactive." };
            }

            // 2. Validate Payment Processor
            var processor = _paymentServices.FirstOrDefault(s => s.SupportedMethod == model.PaymentMethod);
            if (processor == null)
            {
                return new DonationCreationResult { Success = false, ErrorMessage = "Payment method provider is not available." };
            }

            // 3. Receipt Upload for Mobile Banking (EasyPaisa / JazzCash)
            string? receiptPath = null;
            if (model.PaymentMethod == PaymentMethodType.EasyPaisa || model.PaymentMethod == PaymentMethodType.JazzCash)
            {
                if (string.IsNullOrWhiteSpace(model.TransactionId))
                {
                    return new DonationCreationResult { Success = false, ErrorMessage = "Transaction ID is required for mobile banking payments." };
                }

                if (await IsTransactionIdDuplicateAsync(model.TransactionId))
                {
                    return new DonationCreationResult { Success = false, ErrorMessage = "This transaction ID has already been submitted. Please check your transaction record." };
                }

                if (model.ReceiptFile != null)
                {
                    var uploadResult = await _fileService.SaveFileAsync(
                        model.ReceiptFile,
                        "receipts",
                        new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" },
                        5 * 1024 * 1024);

                    if (!uploadResult.Success)
                    {
                        return new DonationCreationResult { Success = false, ErrorMessage = uploadResult.ErrorMessage };
                    }
                    receiptPath = uploadResult.FilePath;
                }
            }

            // 4. Process Payment Service
            var paymentRequest = new PaymentRequest
            {
                Amount = model.Amount,
                PaymentMethod = model.PaymentMethod,
                CardHolderName = model.CardHolderName,
                CardNumber = model.CardNumber,
                ExpiryMonth = model.ExpiryMonth,
                ExpiryYear = model.ExpiryYear,
                Cvv = model.Cvv,
                TransactionId = model.TransactionId,
                SenderName = model.SenderName,
                SenderMobile = model.SenderMobile,
                PaymentDate = model.PaymentDate,
                ReceiptFilePath = receiptPath
            };

            var processResult = await processor.ProcessPaymentAsync(paymentRequest);
            if (!processResult.IsSuccessful)
            {
                if (receiptPath != null) _fileService.DeleteFile(receiptPath);
                return new DonationCreationResult { Success = false, ErrorMessage = processResult.ErrorMessage ?? "Payment processing failed." };
            }

            // 5. Generate identifiers
            var donationNumber = await GenerateDonationNumberAsync();
            var paymentReference = await GeneratePaymentReferenceAsync();

            var isCardInstant = model.PaymentMethod == PaymentMethodType.CreditDebitCard && processResult.Status == PaymentStatus.Verified;

            // 6. Begin database transaction
            using var dbTx = await _context.Database.BeginTransactionAsync();
            try
            {
                var donation = new Donation
                {
                    DonationNumber = donationNumber,
                    UserId = currentUserId,
                    DonorName = model.DonorName,
                    DonorEmail = model.DonorEmail,
                    DonorPhone = model.DonorPhone,
                    NGOId = model.NGOId,
                    CauseId = model.CauseId,
                    CampaignId = model.CampaignId,
                    Amount = model.Amount,
                    PaymentMethod = model.PaymentMethod,
                    Status = isCardInstant ? DonationStatus.Approved : DonationStatus.Pending,
                    DonorNotes = model.DonorNotes,
                    CreatedAt = DateTime.UtcNow,
                    ApprovedAt = isCardInstant ? DateTime.UtcNow : null
                };

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                var payment = new Payment
                {
                    PaymentReference = paymentReference,
                    DonationId = donation.DonationId,
                    Amount = model.Amount,
                    PaymentMethod = model.PaymentMethod,
                    TransactionId = processResult.TransactionId,
                    Status = processResult.Status,
                    CardHolderName = model.CardHolderName,
                    MaskedCardNumber = processResult.MaskedCardNumber,
                    CardExpiry = processResult.CardExpiry,
                    SenderName = model.SenderName ?? model.DonorName,
                    SenderMobile = model.SenderMobile ?? model.DonorPhone,
                    PaymentDate = model.PaymentDate ?? DateTime.UtcNow,
                    ReceiptFilePath = receiptPath,
                    SubmittedDate = DateTime.UtcNow,
                    VerificationDate = isCardInstant ? DateTime.UtcNow : null
                };

                _context.Payments.Add(payment);

                // If instant verified card payment, increment cause and campaign raised amounts immediately
                if (isCardInstant)
                {
                    cause.RaisedAmount += model.Amount;

                    if (model.CampaignId.HasValue)
                    {
                        var campaign = await _context.Campaigns.FindAsync(model.CampaignId.Value);
                        if (campaign != null)
                        {
                            campaign.RaisedAmount += model.Amount;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await dbTx.CommitAsync();

                donation.Payment = payment;
                donation.Cause = cause;
                donation.NGO = cause.NGO;

                // Send Email Notification
                if (isCardInstant)
                {
                    _ = Task.Run(async () =>
                    {
                        try { await _emailService.SendDonationApprovedEmailAsync(donation, payment); }
                        catch (Exception ex) { _logger.LogError(ex, "Failed to send instant approval email."); }
                    });
                }
                else
                {
                    _ = Task.Run(async () =>
                    {
                        try { await _emailService.SendDonationSubmittedEmailAsync(donation, payment); }
                        catch (Exception ex) { _logger.LogError(ex, "Failed to send submission email."); }
                    });
                }

                return new DonationCreationResult
                {
                    Success = true,
                    Donation = donation,
                    Payment = payment
                };
            }
            catch (Exception ex)
            {
                await dbTx.RollbackAsync();
                _logger.LogError(ex, "Transaction failed while creating donation.");
                if (receiptPath != null) _fileService.DeleteFile(receiptPath);
                return new DonationCreationResult { Success = false, ErrorMessage = "A database error occurred while processing your donation." };
            }
        }

        public async Task<ApprovalResult> ApprovePaymentAsync(int paymentId, string adminUserId, string? adminRemarks)
        {
            var payment = await _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Cause)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Campaign)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.NGO)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null || payment.Donation == null)
            {
                return new ApprovalResult { Success = false, ErrorMessage = "Payment or associated donation record was not found." };
            }

            if (payment.Status == PaymentStatus.Verified || payment.Donation.Status == DonationStatus.Approved)
            {
                return new ApprovalResult { Success = false, ErrorMessage = "This payment has already been verified and approved." };
            }

            using var dbTx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update payment
                payment.Status = PaymentStatus.Verified;
                payment.VerificationDate = DateTime.UtcNow;
                payment.VerifiedById = adminUserId;
                payment.AdminRemarks = adminRemarks;

                // Update donation
                payment.Donation.Status = DonationStatus.Approved;
                payment.Donation.ApprovedAt = DateTime.UtcNow;
                payment.Donation.ApprovedById = adminUserId;
                payment.Donation.AdminRemarks = adminRemarks;
                payment.Donation.UpdatedAt = DateTime.UtcNow;

                // Update Cause RaisedAmount
                if (payment.Donation.Cause != null)
                {
                    payment.Donation.Cause.RaisedAmount += payment.Donation.Amount;
                }

                // Update Campaign RaisedAmount
                if (payment.Donation.Campaign != null)
                {
                    payment.Donation.Campaign.RaisedAmount += payment.Donation.Amount;
                }

                await _context.SaveChangesAsync();
                await dbTx.CommitAsync();

                // Send confirmation email
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendDonationApprovedEmailAsync(payment.Donation, payment);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send approval email for donation #{Num}", payment.Donation.DonationNumber);
                    }
                });

                return new ApprovalResult { Success = true, Donation = payment.Donation };
            }
            catch (Exception ex)
            {
                await dbTx.RollbackAsync();
                _logger.LogError(ex, "Failed to approve payment ID {PaymentId}", paymentId);
                return new ApprovalResult { Success = false, ErrorMessage = "Database transaction failed during approval." };
            }
        }

        public async Task<ApprovalResult> RejectPaymentAsync(int paymentId, string adminUserId, string? adminRemarks)
        {
            var payment = await _context.Payments
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.NGO)
                .Include(p => p.Donation)
                    .ThenInclude(d => d!.Cause)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null || payment.Donation == null)
            {
                return new ApprovalResult { Success = false, ErrorMessage = "Payment record was not found." };
            }

            if (payment.Status == PaymentStatus.Verified)
            {
                return new ApprovalResult { Success = false, ErrorMessage = "Cannot reject a payment that has already been verified and recorded." };
            }

            using var dbTx = await _context.Database.BeginTransactionAsync();
            try
            {
                payment.Status = PaymentStatus.Rejected;
                payment.VerificationDate = DateTime.UtcNow;
                payment.VerifiedById = adminUserId;
                payment.AdminRemarks = adminRemarks;

                payment.Donation.Status = DonationStatus.Rejected;
                payment.Donation.RejectedAt = DateTime.UtcNow;
                payment.Donation.RejectedById = adminUserId;
                payment.Donation.AdminRemarks = adminRemarks;
                payment.Donation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await dbTx.CommitAsync();

                // Send rejection email
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendDonationRejectedEmailAsync(payment.Donation, payment, adminRemarks);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send rejection email for donation #{Num}", payment.Donation.DonationNumber);
                    }
                });

                return new ApprovalResult { Success = true, Donation = payment.Donation };
            }
            catch (Exception ex)
            {
                await dbTx.RollbackAsync();
                _logger.LogError(ex, "Failed to reject payment ID {PaymentId}", paymentId);
                return new ApprovalResult { Success = false, ErrorMessage = "Database transaction failed during rejection." };
            }
        }
    }
}
