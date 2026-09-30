using NGO_website.Models.Entities;

namespace NGO_website.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendRegistrationEmailAsync(string toEmail, string userName);
        Task SendDonationSubmittedEmailAsync(Donation donation, Payment payment);
        Task SendDonationApprovedEmailAsync(Donation donation, Payment payment);
        Task SendDonationRejectedEmailAsync(Donation donation, Payment payment, string? reason);
        Task SendProgrammeInterestEmailAsync(ProgrammeInterest interest, Programme programme);
        Task SendQueryReplyEmailAsync(Query query, QueryReply reply);
        Task SendFriendInvitationEmailAsync(FriendInvitation invitation);
    }
}
