using Auth.Infrastructure.Identity.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Auth.Infrastructure.Identity.Authorization
{
    internal sealed class PermissionAuthorizationHandler
         : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Task.CompletedTask;

            var permissions = context.User.Claims
                .Where(x => x.Type == CustomClaims.Permissions)
                .Select(x => x.Value)
                .ToHashSet();

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
