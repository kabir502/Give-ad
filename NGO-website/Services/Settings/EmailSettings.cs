namespace NGO_website.Services.Settings
{
    public class EmailSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FromEmail { get; set; } = "noreply@giveaid.local";
        public string FromName { get; set; } = "GIVE-AID NGO Platform";
    }
}
