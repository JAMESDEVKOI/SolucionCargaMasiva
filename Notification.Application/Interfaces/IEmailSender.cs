namespace Notification.Application.Interfaces
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = true,
            CancellationToken cancellationToken = default);
    }
}
