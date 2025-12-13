namespace FileControl.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
        bool HasPermission(string permission);
    }
}
