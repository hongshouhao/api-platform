using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Ocelot.Cache.CacheManager;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Setter;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using Ocelot.Tracing.Butterfly;
using OCIApiGateway.Auth;
using OCIApiGateway.Configuration;
using Swashbuckle.AspNetCore.Swagger;

namespace OCIApiGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Environment = env;

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                                  .AddJsonFile($"ocelot.{env.EnvironmentName}.json", true, true)
                                  .AddEnvironmentVariables();

            Configuration = builder.Build();
            OcelotStartOptions.Config(Configuration);
            OcelotAPIAdminOptions.Config(Configuration);
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFileConfigurationRepository, ConfigurationDbRepository>();
            services.AddSingleton<IFileConfigurationSetter, InternalConfigurationSetter>();

            services.AddAuthentications(Environment, Configuration);
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowCredentials()
                           .AllowAnyMethod());
            });
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            IOcelotBuilder ocelotBuilder = services.AddOcelot(Configuration);

            if (OcelotStartOptions.AddConsul)
                ocelotBuilder.AddConsul();

            if (OcelotStartOptions.AddButterfly)
                ocelotBuilder.AddButterfly(option =>
                {
                    option.CollectorUrl = OcelotStartOptions.ButterflyHost;
                    option.Service = OcelotStartOptions.ButterflyLoggingKey;
                });

            if (OcelotStartOptions.AddSwagger)
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "Ocelot Administrator API", Version = "v1" });
                });

            ocelotBuilder.AddPolly()
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();
            LogManager.LoadConfiguration("nlog.config");

            if (Environment.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();/*Use the HTTP Strict Transport Security Protocol(HSTS) Middleware.*/
            }

            if (OcelotStartOptions.AddSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ocelot Administrator API V1");
                });
            }

            app.UseHttpsRedirection();/*Use HTTPS Redirection Middleware to redirect HTTP requests to HTTPS.*/
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseCors("CorsPolicy");
            app.UseMvc();
            app.UseOcelot().Wait();
        }
    }
}
