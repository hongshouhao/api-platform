using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace Test_WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AuthenticationBuilder atbuilder = services.AddAuthentication("Bearer");
            atbuilder.AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.Authority = "http://192.168.84.124:8610";
                    options.ApiName = "auth_test_webapi";
                    options.ApiSecret = "secret";
                    options.SupportedTokens = SupportedTokens.Both;
                    options.NameClaimType = "name";
                    options.RoleClaimType = "role";
                    options.RequireHttpsMetadata = false;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("HasReadScope", arbuilder => arbuilder.RequireClaim("scope", "auth_test_webapi.read"));
                options.AddPolicy("HasWriteScope", arbuilder => arbuilder.RequireClaim("scope", "auth_test_webapi.write"));
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Test API", Version = "v1" });
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test API V1");
            });
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
