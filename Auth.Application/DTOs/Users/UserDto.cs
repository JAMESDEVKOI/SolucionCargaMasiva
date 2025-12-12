namespace Auth.Application.DTOs.Users
{
    public sealed record UserDto(
        Guid Id,
        string Name,
        string LastName,
        string Email,
        string? Phone,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        IEnumerable<string> Roles);
}
