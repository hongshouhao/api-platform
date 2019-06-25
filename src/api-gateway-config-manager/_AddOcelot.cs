using ApiGatewayManager.Data;
using ApiGatewayManager.Ids4Conf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;
using System;
using System.Linq;

namespace ApiGatewayManager
{
    static class _AddOcelot
    {
        public static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DBConnection");
            if (string.IsNullOrWhiteSpace(connectionString)) throw new Exception("数据库连接字符串为空.");
            services.AddSingleton(new OcelotConfigSectionRepository(connectionString));
            services.AddSingleton(new OcelotConfigTemplateRepository(connectionString));
            services.AddSingleton(new OcelotCompleteConfigRepository(connectionString));
            services.AddSingleton(new IdentityAuthOptionsConfigRepository(connectionString));
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

            return services;
        }
    }
}
