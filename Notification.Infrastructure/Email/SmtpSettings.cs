namespace Notification.Infrastructure.Email
{
    public class SmtpSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public bool UseSSL { get; set; } = true;
        public bool UseStartTLS { get; set; } = true;
    }
}
