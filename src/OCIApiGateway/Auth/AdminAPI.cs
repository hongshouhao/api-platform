using Microsoft.Extensions.Configuration;

namespace OCIApiGateway.Auth
{
    public class AdminAPI
    {
        public const string AuthorizePolicy = "adminApiProtectByRole";
        public const string AuthenticationSchemes = "ocelot_administration";

        public static string GetRequiredRole(IConfiguration configuration)
        {
            string adminRole = configuration["AdministrationRole"];
            if (string.IsNullOrWhiteSpace(adminRole))
                adminRole = "admin";

            return adminRole;
        }
    }
}
