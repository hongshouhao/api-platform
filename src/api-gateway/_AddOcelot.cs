using ApiGateway.Authentication;
using ApiGateway.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.Configuration.Repository;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using Ocelot.Tracing.Butterfly;
using System;
using System.Linq;
using System.Net;

namespace ApiGateway
{
    static class _AddOcelot
    {
        public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBConnection");
            if (string.IsNullOrWhiteSpace(connectionString)) throw new Exception("数据库连接字符串为空.");
            services.AddSingleton<IFileConfigurationRepository>(new MsDbConfigurationRepository(connectionString));
            return services;
        }

        public static IServiceCollection AddOcelotSuit(this IServiceCollection services,
            IConfiguration configuration)
        {
            IOcelotBuilder ocelotBuilder = services.AddOcelot(configuration);
            if (configuration.GetChildren().Any(item =>
                string.Equals(item.Key, "consul", StringComparison.CurrentCultureIgnoreCase)))
            {
                ocelotBuilder.AddConsul();
            }

            string butterflyHost = configuration.GetConnectionString("ButterflyHost");
            if (!string.IsNullOrWhiteSpace(butterflyHost))
                ocelotBuilder.AddButterfly(option =>
                {
                    option.CollectorUrl = butterflyHost;
                    option.Service = $"apigate_{Dns.GetHostName()}".Replace("-", "_");
                });

            ocelotBuilder.AddPolly()
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                });

            return services;
        }

        public static IServiceCollection AddAuthentications(this IServiceCollection services,
            IdsAuthOptionsReader idsAuthOptionsReader)
        {
            AuthenticationBuilder builder = services.AddAuthentication();
            foreach (var authop in idsAuthOptionsReader.AuthOptions)
            {
                builder.AddIdentityServerAuthentication(authop.AuthScheme, authop.Build());
            }
            return services;
        }
    }
}
