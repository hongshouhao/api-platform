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
using System;
using System.Linq;

namespace ApiGateway
{
    static class _AddOcelot
    {
        public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBConnection");
            if (string.IsNullOrWhiteSpace(connectionString)) throw new Exception("数据库连接字符串为空.");
            services.AddSingleton<IFileConfigurationRepository>(new ConfigurationDbRepository(connectionString))
                    .AddAuthentications(new IdentityAuthOptionsDb(connectionString));
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

            ocelotBuilder.AddPolly()
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                });

            return services;
        }

        public static IServiceCollection AddAuthentications(this IServiceCollection services,
            IIdentityAuthOptionsProvider authOptionsProvider)
        {
            AuthenticationBuilder builder = services.AddAuthentication();
            foreach (var authop in authOptionsProvider.GetOptions())
            {
                builder.AddIdentityServerAuthentication(authop.AuthScheme, authop.Build());
            }
            return services;
        }
    }
}
