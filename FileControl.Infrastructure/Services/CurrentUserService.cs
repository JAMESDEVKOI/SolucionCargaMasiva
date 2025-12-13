using FileControl.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Linq;

namespace FileControl.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool HasPermission(string permission)
        {
            var permissions = _httpContextAccessor.HttpContext?.User?
                .FindAll("permissions")
                .Select(c => c.Value)
                .ToList() ?? new List<string>();

            return permissions.Contains(permission);
        }
    }
}
