using Microsoft.Extensions.Configuration;

namespace OCIApiGateway
{
    public class OcelotAPIAdminOptions
    {
        public const string AuthorizePolicy = "adminApiProtectByRole";

        public static void Config(IConfiguration configuration)
        {
            AdministrationAuthScheme = configuration["AdministrationAuthScheme"];
            AdministrationRole = configuration["AdministrationRole"];
        }

        public static string AdministrationAuthScheme;
        public static string AdministrationRole;
    }
}
