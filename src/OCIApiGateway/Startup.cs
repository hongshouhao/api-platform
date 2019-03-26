using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Ocelot.Configuration.Setter;
using Ocelot.Middleware;
using OCIApiGateway.Authentication;
using OCIApiGateway.ConfMgr;
using OCIApiGateway.Controllers;
using OCIApiGateway.Opions;
using OCIApiGateway.Web;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCIApiGateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Environment = env;
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddJsonFile($"ocelot.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        StartOptions _startOptions;

        public void ConfigureServices(IServiceCollection services)
        {
            _startOptions = Configuration.GetSection(nameof(StartOptions)).Get<StartOptions>();

            if (_startOptions.UseSwagger)
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Info { Title = "Ocelot Administrator API", Version = "v1" });
                    c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                    {
                        Name = "Authorization",
                        In = "header",
                        Type = "apiKey",
                        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    });
                    c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                    {
                        {"Bearer", new string[] { }},
                    });
                });

            services.AddDb(Configuration)
                    .AddSingleton<IFileConfigurationSetter, InternalConfigurationSetter>()
                    .AddOcelotSuit(Configuration, _startOptions)
                    .AddAuthentications(new IdsAuthOptionsReader(Environment))

                    .AddCors(options =>
                    {
                        options.AddPolicy("CorsPolicy", builder =>
                            builder.AllowAnyOrigin()
                                   .AllowAnyHeader()
                                   .AllowCredentials()
                                   .AllowAnyMethod());
                    })
                    .AddMvc()
                    .ConfigureApplicationPartManager(manager =>
                     {
                         manager.FeatureProviders.Add(new ControllerProvider());
                     })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    });
        }

        public void Configure(IApplicationBuilder app, ConfigurationController configurationController,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();
            LogManager.LoadConfiguration("nlog.config");

            //if (Environment.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseHsts();/*Use the HTTP Strict Transport Security Protocol(HSTS) Middleware.*/
            //}

            if (_startOptions.UseSwagger)
            {
                app.UseSwagger()
                   .UseSwaggerUI(c =>
                   {
                       c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ocelot Administrator API V1");
                   });
            }

            app.UseMiddleware<ErrorHandlingMiddleware>()
               .UseCors("CorsPolicy")
               .UseAuthentication()
               .UseMvc()
               .UseOcelot().Wait();

            configurationController.ReBuiltConfiguration().Wait();
        }
    }
}
