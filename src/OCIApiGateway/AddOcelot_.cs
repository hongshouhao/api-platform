using IdentityServer4.AccessTokenValidation;
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
using OCIApiGateway.Authentication;
using OCIApiGateway.ConfMgr.Data;
using OCIApiGateway.Opions;
using System;
using System.Net;

namespace OCIApiGateway
{
    static class AddOcelot_
    {
        public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBConnection");
            if (string.IsNullOrWhiteSpace(connectionString)) throw new Exception("数据库连接字符串为空.");
            services.AddSingleton(new OcelotConfigSectionRepository(connectionString));
            services.AddSingleton(new OcelotConfigTemplateRepository(connectionString));
            services.AddSingleton<IFileConfigurationRepository, OcelotConfigDbRepository>();
            return services;
        }

        public static IServiceCollection AddOcelotSuit(this IServiceCollection services,
            IConfiguration configuration,
            StartOptions startOptions)
        {
            IOcelotBuilder ocelotBuilder = services.AddOcelot(configuration);

            if (startOptions.UseConsul)
                ocelotBuilder.AddConsul();

            if (startOptions.UseButterfly)
                ocelotBuilder.AddButterfly(option =>
                {
                    option.CollectorUrl = startOptions.ButterflyHost;
                    option.Service = startOptions.ButterflyLoggingKey.Replace("-", "_");
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
            AuthenticationBuilder builder = services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme);
            foreach (var authop in idsAuthOptionsReader.AuthOptions)
            {
                builder.AddIdentityServerAuthentication(authop.AuthScheme, authop.Build());
            }
            return services;
        }
    }
}
