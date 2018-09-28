using Microsoft.Extensions.Configuration;
using System;

namespace OCIApiGateway
{
    public static class OcelotStartOptions
    {
        static internal void Config(IConfiguration configuration)
        {
            string addConsulStr = configuration["AddConsul"];
            bool.TryParse(addConsulStr, out AddConsul);

            string addSwaggerStr = configuration["AddSwagger"];
            bool.TryParse(addSwaggerStr, out AddSwagger);

            string addButterflyStr = configuration["AddButterfly"];
            bool.TryParse(addButterflyStr, out AddButterfly);
            ButterflyHost = configuration["ButterflyHost"];
            if (string.IsNullOrWhiteSpace(ButterflyHost))
                AddButterfly = false;
            ButterflyLoggingKey = configuration["ButterflyLoggingKey"];
            if (string.IsNullOrWhiteSpace(ButterflyLoggingKey))
                ButterflyLoggingKey = "ocelot1";
        }

        public static bool AddConsul;
        public static bool AddSwagger;

        public static bool AddButterfly;
        public static string ButterflyHost;
        public static string ButterflyLoggingKey;
    }
}
