using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO;

namespace OCIApiGateway
{
    public static class ApiAuthentication
    {
        public static void AddAuthentications(this IServiceCollection services, IHostingEnvironment env)
        {
            IdsAuthOptions[] authOptions = ReadAuthOptions(env);
            AuthenticationBuilder builder = services.AddAuthentication();
            foreach (var authop in authOptions)
            {
                builder.AddIdentityServerAuthentication(authop.AuthScheme, BuildIdentityServerAuthenticationOptions(authop));
            }
        }

        public static Action<IdentityServerAuthenticationOptions> BuildIdentityServerAuthenticationOptions(IdsAuthOptions authop)
        {
            return options =>
            {
                options.Authority = authop.Authority;
                options.ApiName = authop.ApiName;
                options.ApiSecret = authop.ApiSecret;
                options.SupportedTokens = authop.SupportedTokens;
                options.EnableCaching = authop.EnableCaching;
                options.TokenRetriever = GetTokenRetrievalOptions(authop);
                options.CacheDuration = TimeSpan.FromMinutes(authop.CacheDuration);
                options.ClaimsIssuer = authop.ClaimsIssuer;
                options.ForwardAuthenticate = authop.ForwardAuthenticate;
                options.ForwardChallenge = authop.ForwardChallenge;
                options.ForwardDefault = authop.ForwardDefault;
                options.ForwardForbid = authop.ForwardForbid;
                options.ForwardSignIn = authop.ForwardSignIn;
                options.ForwardSignOut = authop.ForwardSignOut;
                options.NameClaimType = authop.NameClaimType;
                options.RequireHttpsMetadata = authop.RequireHttpsMetadata;
                options.RoleClaimType = authop.RoleClaimType;
            };
        }

        public static IdsAuthOptions[] ReadAuthOptions(IHostingEnvironment env)
        {
            string jsonFile = "apiauthentication";
            if (string.IsNullOrWhiteSpace(env.EnvironmentName))
            {
                jsonFile = $"{jsonFile}.json";
            }
            else
            {
                jsonFile = $"{jsonFile}.{env.EnvironmentName}.json";
            }

            jsonFile = $"{AppContext.BaseDirectory}{jsonFile}";

            if (File.Exists(jsonFile))
            {
                string text = File.ReadAllText(jsonFile);
                IdsAuthOptions[] authOptions = JsonConvert.DeserializeObject<IdsAuthOptions[]>(text);
                foreach (var authop in authOptions)
                {
                    if (string.IsNullOrWhiteSpace(authop.ApiName))
                        throw new Exception("配置项错误: [apiauthoptions.json]中某项[ApiName]为空");

                    if (string.IsNullOrWhiteSpace(authop.Authority))
                        throw new Exception("配置项错误: [apiauthoptions.json]中某项[Authority]为空");

                    if (string.IsNullOrWhiteSpace(authop.AuthScheme))
                        throw new Exception("配置项错误: [apiauthoptions.json]中某项[AuthScheme]为空");
                }

                return authOptions;
            }
            else
            {
                return new IdsAuthOptions[] { };
            }

        }

        static Func<HttpRequest, string> GetTokenRetrievalOptions(IdsAuthOptions authOptions)
        {
            if (authOptions.TokenRetrievalOptions == null)
            {
                return TokenRetrieval.FromAuthorizationHeader();
            }

            switch (authOptions.TokenRetrievalOptions.Source)
            {
                default:
                case TokenSource.FromAuthorizationHeader:
                    if (string.IsNullOrWhiteSpace(authOptions.TokenRetrievalOptions.NameOrSchema))
                        authOptions.TokenRetrievalOptions.NameOrSchema = "Bearer";
                    return TokenRetrieval.FromAuthorizationHeader(authOptions.TokenRetrievalOptions.NameOrSchema);
                case TokenSource.FromQueryString:
                    if (string.IsNullOrWhiteSpace(authOptions.TokenRetrievalOptions.NameOrSchema))
                        authOptions.TokenRetrievalOptions.NameOrSchema = "access_token";
                    return TokenRetrieval.FromQueryString(authOptions.TokenRetrievalOptions.NameOrSchema);
            }
        }
    }
}
