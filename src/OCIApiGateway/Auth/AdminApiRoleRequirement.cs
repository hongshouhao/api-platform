using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace OCIApiGateway.Auth
{
    public class AdminApiRoleRequirement : AuthorizationHandler<AdminApiRoleRequirement>, IAuthorizationRequirement
    {
        private readonly string[] _roles;
        public AdminApiRoleRequirement(string[] roles)
        {
            _roles = roles;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminApiRoleRequirement requirement)
        {
            var userIsInRole = _roles.Any(role => context.User.IsInRole(role));
            if (!userIsInRole)
            {
                context.Fail();
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
