using Auth.Domain.Permission;
using Microsoft.AspNetCore.Authorization;

namespace Auth.Infrastructure.Identity.Authorization
{
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(Permission permission)
        : base(policy: permission.Name)
        {
        }
    }
}
