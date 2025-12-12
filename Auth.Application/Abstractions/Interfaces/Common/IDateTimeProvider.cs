namespace Auth.Application.Abstractions.Interfaces.Common
{
    public interface IDateTimeProvider
    {
        DateTime currentTime { get; }
    }
}
