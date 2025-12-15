namespace Auth.Application.UseCases.Auth.Queries.GetCurrentUser
{
    public sealed record CurrentUserResponse(
        Guid Id,
        string Email,
        string Name,
        string LastName,
        string Phone,
        IEnumerable<string> Roles,
        IEnumerable<string> Permissions
    );
}
