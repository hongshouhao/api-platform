using ApiGatewayManager.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog.Web;
using Ocelot.Middleware;
using Steeltoe.Discovery.Client;
using Swashbuckle.AspNetCore.Swagger;

namespace ApiGatewayManager
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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Ocelot Administrator API", Version = "v1" });
                //c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                //{
                //    Name = "Authorization",
                //    In = "header",
                //    Type = "apiKey",
                //    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                //});
                //c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                //{
                //        {"Bearer", new string[] { }},
                //});
            });
            services.AddDb(Configuration)
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
                    .ConfigureApplicationPartManager(manager =>
                     {
                         manager.FeatureProviders.Add(new ControllerProvider());
                     })
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    });
        }

        public void Configure(IApplicationBuilder app)
        {
            NLogBuilder.ConfigureNLog($"nlog.{Environment.EnvironmentName}.config");

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ocelot Administrator API V1");
               });

            app.UseMiddleware<ErrorHandlingMiddleware>()
               .UseCors("CorsPolicy")
               //.UseAuthentication()
               .UseMvc()
               .UseDiscoveryClient()
               .UseOcelot().Wait();
        }
    }
}
