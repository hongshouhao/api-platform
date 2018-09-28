using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace OCIApiGateway.Auth
{
    public class AdminRoleRequirement : AuthorizationHandler<AdminRoleRequirement>, IAuthorizationRequirement
    {
        private readonly string[] _roles;
        public AdminRoleRequirement(string[] roles)
        {
            _roles = roles;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRoleRequirement requirement)
        {
            if (_roles.Length > 0)
            {
                var userIsInRole = _roles.Any(role => context.User.IsInRole(role));
                if (!userIsInRole)
                {
                    context.Fail();
                }
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
