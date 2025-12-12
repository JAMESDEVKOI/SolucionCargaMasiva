using Auth.Domain.User.ValueObject;

namespace Auth.Application.Abstractions.Interfaces.Common
{
    public interface IEmailService
    {
        Task SendAsync(Email recipient, string subject, string body);
    }
}
