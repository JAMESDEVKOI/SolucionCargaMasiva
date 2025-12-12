namespace Auth.Application.Abstractions.Interfaces.Identity
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }

        string? UserName { get; }

        string? Email { get; }

        bool IsAuthenticated { get; }

        bool IsInRole(string role);

        IEnumerable<string> GetRoles();

        string? IpAddress { get; }

        string? UserAgent { get; }
    }
}
