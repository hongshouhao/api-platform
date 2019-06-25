using ApiGateway.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using Ocelot.Configuration.Setter;
using Ocelot.Middleware;
using Steeltoe.Discovery.Client;

namespace ApiGateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Environment = env;
            Configuration = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", false, true)
                .AddEnvironmentVariables().Build();
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDb(Configuration)
                    .AddSingleton<IFileConfigurationSetter, InternalConfigurationSetter>()
                    .AddOcelotSuit(Configuration)
                    .AddDiscoveryClient(Configuration)
                    .AddCors(options =>
                    {
                        options.AddPolicy("CorsPolicy", builder =>
                            builder.AllowAnyOrigin()
                                   .AllowAnyHeader()
                                   .AllowCredentials()
                                   .AllowAnyMethod());
                    })
                    .AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    });
        }

        public void Configure(IApplicationBuilder app, ConfigurationController controller)
        {
            NLogBuilder.ConfigureNLog($"nlog.{Environment.EnvironmentName}.config");

            app.UseMiddleware<ErrorHandlingMiddleware>()
                .UseCors("CorsPolicy")
                .UseAuthentication()
                .UseDiscoveryClient()
                .UseMvc()
                .UseOcelot().Wait();
            controller.ReLoad();
        }
    }
}
