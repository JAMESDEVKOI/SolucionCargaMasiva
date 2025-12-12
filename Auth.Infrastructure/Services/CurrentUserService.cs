using Auth.Application.Abstractions.Interfaces.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Auth.Infrastructure.Services
{
    internal sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User
                    ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
            }
        }

        public string? UserName =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        public IEnumerable<string> GetRoles()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value) ?? Enumerable.Empty<string>();
        }

        public string? IpAddress =>
            _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent =>
            _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
    }
}
