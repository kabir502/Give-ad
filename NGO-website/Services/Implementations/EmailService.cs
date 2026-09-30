using Microsoft.Extensions.Options;
using NGO_website.Models.Entities;
using NGO_website.Services.Interfaces;
using NGO_website.Services.Settings;
using System.Net;
using System.Net.Mail;

namespace NGO_website.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                // If SMTP username or host is empty, log email in development mode
                if (string.IsNullOrWhiteSpace(_emailSettings.Username) || string.IsNullOrWhiteSpace(_emailSettings.Host))
                {
                    _logger.LogInformation("[DEV EMAIL SIMULATION] To: {ToEmail} | Subject: {Subject}\nBody:\n{Body}", 
                        toEmail, subject, htmlMessage);
                    return true;
                }

                using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
                {
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSsl
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}. Subject: {Subject}", toEmail, subject);
                return false;
            }
        }

        public async Task SendRegistrationEmailAsync(string toEmail, string userName)
        {
            var subject = "Welcome to GIVE-AID — Your Account is Ready!";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #0d6efd;'>Welcome to GIVE-AID, {userName}!</h2>
                    <p>Thank you for joining our community dedicated to helping vulnerable populations, children, elderly, and those facing hardship.</p>
                    <p>You can now browse verified welfare causes, participate in upcoming NGO programmes, and track all your contributions from your personalized dashboard.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777;'>GIVE-AID NGO & Donation Management Platform &bull; Every contribution transforms a life.</p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendDonationSubmittedEmailAsync(Donation donation, Payment payment)
        {
            var subject = $"Donation Submitted #{donation.DonationNumber} — Verification Pending";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #fd7e14;'>Donation Submitted Successfully!</h2>
                    <p>Dear <strong>{donation.DonorName}</strong>,</p>
                    <p>Your donation of <strong>PKR {donation.Amount:N2}</strong> has been submitted successfully and is waiting for verification by our administration team.</p>
                    <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
                        <tr><td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Donation Ref:</strong></td><td style='padding: 8px; border-bottom: 1px solid #eee;'>{donation.DonationNumber}</td></tr>
                        <tr><td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Payment Ref:</strong></td><td style='padding: 8px; border-bottom: 1px solid #eee;'>{payment.PaymentReference}</td></tr>
                        <tr><td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Payment Method:</strong></td><td style='padding: 8px; border-bottom: 1px solid #eee;'>{payment.PaymentMethod}</td></tr>
                        <tr><td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Transaction ID:</strong></td><td style='padding: 8px; border-bottom: 1px solid #eee;'>{payment.TransactionId}</td></tr>
                        <tr><td style='padding: 8px; border-bottom: 1px solid #eee;'><strong>Status:</strong></td><td style='padding: 8px; border-bottom: 1px solid #eee;'><span style='color: #fd7e14; font-weight: bold;'>Pending Verification</span></td></tr>
                    </table>
                    <p>You will receive an official confirmation and receipt as soon as your payment verification is complete.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777;'>GIVE-AID NGO & Donation Management Platform</p>
                </div>";

            await SendEmailAsync(donation.DonorEmail, subject, body);
        }

        public async Task SendDonationApprovedEmailAsync(Donation donation, Payment payment)
        {
            var subject = $"Donation Confirmed #{donation.DonationNumber} — Thank You for Your Generosity!";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #198754;'>Donation Received Successfully!</h2>
                    <p>Dear <strong>{donation.DonorName}</strong>,</p>
                    <p>We are pleased to confirm that your donation has been verified and received by <strong>{(donation.NGO != null ? donation.NGO.Name : "GIVE-AID Partner NGO")}</strong>.</p>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 6px; margin: 15px 0;'>
                        <h4 style='margin-top: 0; color: #0d6efd;'>Donation Receipt Summary</h4>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr><td style='padding: 6px 0;'><strong>Donation ID:</strong></td><td>{donation.DonationNumber}</td></tr>
                            <tr><td style='padding: 6px 0;'><strong>Amount:</strong></td><td><strong>PKR {donation.Amount:N2}</strong></td></tr>
                            <tr><td style='padding: 6px 0;'><strong>Cause:</strong></td><td>{(donation.Cause != null ? donation.Cause.Name : "General Welfare")}</td></tr>
                            <tr><td style='padding: 6px 0;'><strong>Payment Method:</strong></td><td>{payment.PaymentMethod}</td></tr>
                            <tr><td style='padding: 6px 0;'><strong>Transaction ID:</strong></td><td>{payment.TransactionId}</td></tr>
                            <tr><td style='padding: 6px 0;'><strong>Approval Date:</strong></td><td>{DateTime.UtcNow:dd MMM yyyy, hh:mm tt} UTC</td></tr>
                        </table>
                    </div>
                    <p>You can view and print your official donation receipt directly from your member dashboard.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777;'>GIVE-AID NGO &bull; Transparent Giving & Community Impact</p>
                </div>";

            await SendEmailAsync(donation.DonorEmail, subject, body);
        }

        public async Task SendDonationRejectedEmailAsync(Donation donation, Payment payment, string? reason)
        {
            var subject = $"Update regarding your donation #{donation.DonationNumber}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #dc3545;'>Payment Verification Notice</h2>
                    <p>Dear <strong>{donation.DonorName}</strong>,</p>
                    <p>We regret to inform you that your payment for donation <strong>#{donation.DonationNumber}</strong> (Amount: PKR {donation.Amount:N2}) could not be verified.</p>
                    <div style='background-color: #fff3cd; color: #664d03; padding: 12px; border-radius: 6px; margin: 15px 0;'>
                        <strong>Reason / Remarks:</strong> {reason ?? "Transaction ID or receipt could not be verified with the payment service provider."}
                    </div>
                    <p>If you believe this is an error or need assistance, please submit a query from our Help Centre or reply to this email with your payment proof.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777;'>GIVE-AID Support Team</p>
                </div>";

            await SendEmailAsync(donation.DonorEmail, subject, body);
        }

        public async Task SendProgrammeInterestEmailAsync(ProgrammeInterest interest, Programme programme)
        {
            var subject = $"Registration Confirmed: {programme.Title}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #0d6efd;'>Programme Participation Confirmed!</h2>
                    <p>Dear <strong>{interest.FullName}</strong>,</p>
                    <p>Thank you for expressing your interest and volunteering for <strong>{programme.Title}</strong>.</p>
                    <div style='background-color: #f8f9fa; padding: 12px; border-radius: 6px;'>
                        <p style='margin: 4px 0;'><strong>Date:</strong> {programme.Date:dddd, dd MMMM yyyy}</p>
                        <p style='margin: 4px 0;'><strong>Time:</strong> {programme.StartTime} - {programme.EndTime}</p>
                        <p style='margin: 4px 0;'><strong>Location:</strong> {programme.Location}</p>
                    </div>
                    <p>Our team will contact you prior to the event date with additional instructions.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777;'>GIVE-AID Programmes Department</p>
                </div>";

            await SendEmailAsync(interest.Email, subject, body);
        }

        public async Task SendQueryReplyEmailAsync(Query query, QueryReply reply)
        {
            var subject = $"Response to your query: {query.Subject} [Query #{query.QueryId}]";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #0d6efd;'>GIVE-AID Support Response</h2>
                    <p>Dear <strong>{query.FullName}</strong>,</p>
                    <p>Our team has replied to your query <em>""{query.Subject}""</em>:</p>
                    <div style='background-color: #e7f1ff; border-left: 4px solid #0d6efd; padding: 12px; margin: 15px 0;'>
                        <strong>Reply from {reply.AdminName}:</strong>
                        <p style='margin-top: 8px; white-space: pre-line;'>{reply.Message}</p>
                    </div>
                    <p><strong>Your original message:</strong></p>
                    <blockquote style='color: #666; font-style: italic; border-left: 3px solid #ccc; padding-left: 10px;'>
                        {query.Message}
                    </blockquote>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777;'>GIVE-AID Help & Support Centre</p>
                </div>";

            await SendEmailAsync(query.Email, subject, body);
        }

        public async Task SendFriendInvitationEmailAsync(FriendInvitation invitation)
        {
            var subject = $"{invitation.SenderName} invites you to support GIVE-AID Welfare Causes";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #0d6efd;'>Join Hands with GIVE-AID!</h2>
                    <p>Dear <strong>{invitation.FriendName}</strong>,</p>
                    <p>Your friend <strong>{invitation.SenderName}</strong> ({invitation.SenderEmail}) has invited you to explore GIVE-AID, a collaborative platform for humanitarian welfare, education, child support, and emergency relief.</p>
                    {(string.IsNullOrWhiteSpace(invitation.PersonalMessage) ? "" : $@"
                    <div style='background-color: #f8f9fa; border-left: 4px solid #0d6efd; padding: 12px; margin: 15px 0;'>
                        <strong>Personal Message from {invitation.SenderName}:</strong>
                        <p style='margin-top: 6px; font-style: italic;'>""{invitation.PersonalMessage}""</p>
                    </div>")}
                    <p>Visit GIVE-AID today to discover how even a small contribution can bring hope and transformation to people in need.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #777;'>GIVE-AID NGO & Donation Management System</p>
                </div>";

            await SendEmailAsync(invitation.FriendEmail, subject, body);
        }
    }
}
