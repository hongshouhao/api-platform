using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Administration;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using Ocelot.Tracing.Butterfly;
using System;
using System.Linq;

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
            StartOptions = new StartOptions(Configuration);
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public StartOptions StartOptions { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentications(Environment);
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowCredentials()
                           .AllowAnyMethod());
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            IOcelotBuilder ocelotBuilder = services.AddOcelot(Configuration);

            if (StartOptions.AddConsul)
                ocelotBuilder.AddConsul();

            if (StartOptions.StoreConfigInConsul)
                ocelotBuilder.AddConfigStoredInConsul();

            if (StartOptions.AddButterfly)
                ocelotBuilder.AddButterfly(option =>
                {
                    option.CollectorUrl = StartOptions.ButterflyHost;
                    option.Service = StartOptions.ButterflyLoggingKey;
                });

            if (StartOptions.AddAdministration)
            {
                IdsAuthOptions authOption = ApiAuthentication.ReadAuthOptions(Environment).FirstOrDefault(s => s.AuthScheme == StartOptions.AdministrationAuthScheme);
                if (authOption == null)
                {
                    throw new Exception($"Ocelot管理接口授权配置错误: [apiauthentication.{Environment.EnvironmentName}.json]中找不到[AuthScheme = '{StartOptions.AdministrationAuthScheme}']");
                }

                ocelotBuilder.AddAdministration("/administration", ApiAuthentication.BuildIdentityServerAuthenticationOptions(authOption));
            }

            ocelotBuilder.AddPolly()
                .AddCacheManager(x =>
                {
                    x.WithDictionaryHandle();
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("CorsPolicy");
            app.UseMvc();
            app.UseOcelot_().Wait();
        }
    }
}
