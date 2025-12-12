using Auth.Application.Abstractions.Interfaces.Common;

namespace Auth.Infrastructure.Services
{
    internal sealed class DateTimeProvider : IDateTimeProvider
    {
        public DateTime currentTime => DateTime.UtcNow;
    }
}
