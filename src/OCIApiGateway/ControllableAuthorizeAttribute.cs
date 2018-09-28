using Microsoft.AspNetCore.Authorization;

namespace OCIApiGateway
{
    public class ControllableAuthorizeAttribute : AuthorizeAttribute
    {
        public ControllableAuthorizeAttribute() : base()
        {
            base.AuthenticationSchemes = OcelotAPIAdminOptions.AdministrationAuthScheme;
            base.Policy = OcelotAPIAdminOptions.AuthorizePolicy;
        }
    }
}
