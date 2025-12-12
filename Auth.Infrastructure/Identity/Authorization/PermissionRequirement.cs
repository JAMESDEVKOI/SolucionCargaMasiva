using Microsoft.AspNetCore.Authorization;

namespace Auth.Infrastructure.Identity.Authorization
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }

        public string Permission { get; }
    }
}
