namespace NGO_website.Models.Enums
{
    public enum DonationStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Verified = 2,
        Rejected = 3,
        Failed = 4
    }

    public enum QueryStatus
    {
        Open = 1,
        InProgress = 2,
        Resolved = 3,
        Closed = 4
    }

    public enum ProgrammeStatus
    {
        Upcoming = 1,
        Ongoing = 2,
        Completed = 3,
        Cancelled = 4
    }

    public enum CampaignStatus
    {
        Active = 1,
        Completed = 2,
        Paused = 3,
        Cancelled = 4
    }

    public enum PaymentMethodType
    {
        CreditDebitCard = 1,
        EasyPaisa = 2,
        JazzCash = 3
    }
}
