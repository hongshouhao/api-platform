using Microsoft.Extensions.Configuration;
using System;

namespace OCIApiGateway
{
    public class StartOptions
    {
        internal StartOptions(IConfiguration configuration)
        {
            string addConsulStr = configuration["AddConsul"];
            bool.TryParse(addConsulStr, out AddConsul);

            string storeConfigInConsulStr = configuration["StoreConfigInConsul"];
            bool.TryParse(storeConfigInConsulStr, out StoreConfigInConsul);

            string addButterflyStr = configuration["AddButterfly"];
            bool.TryParse(addButterflyStr, out AddButterfly);
            ButterflyHost = configuration["ButterflyHost"];
            if (string.IsNullOrWhiteSpace(ButterflyHost))
                AddButterfly = false;
            ButterflyLoggingKey = configuration["ButterflyLoggingKey"];
            if (string.IsNullOrWhiteSpace(ButterflyLoggingKey))
                ButterflyLoggingKey = "ocelot1";

            string addAdministrationStr = configuration["AddAdministration"];
            bool.TryParse(addAdministrationStr, out AddAdministration);
            AdministrationAuthScheme = configuration["AdministrationAuthScheme"];
            if (string.IsNullOrWhiteSpace(AdministrationAuthScheme))
                AddAdministration = false;
        }

        public readonly bool AddConsul;
        public readonly bool StoreConfigInConsul;

        public readonly bool AddButterfly;
        public readonly string ButterflyHost;
        public readonly string ButterflyLoggingKey;

        public readonly bool AddAdministration;
        public readonly string AdministrationAuthScheme;
    }
}
