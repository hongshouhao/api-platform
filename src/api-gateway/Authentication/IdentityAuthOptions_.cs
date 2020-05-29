using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace ApiGateway.Authentication
{
    public partial class IdentityAuthOptions
    {
        public Action<IdentityServerAuthenticationOptions> Build()
        {
            return options =>
            {
                options.Authority = NotNullOrEmpty(nameof(Authority), Authority);
                options.ApiName = NotNullOrEmpty(nameof(ApiName), ApiName);
                options.ApiSecret = NotNullOrEmpty(nameof(ApiSecret), ApiSecret);
                options.SupportedTokens = (IdentityServer4.AccessTokenValidation.SupportedTokens)(int)SupportedTokens;
                options.ClaimsIssuer = ClaimsIssuer;
                options.ForwardAuthenticate = NullOrNotEmpty(ForwardAuthenticate);
                options.ForwardChallenge = NullOrNotEmpty(ForwardChallenge);
                options.ForwardDefault = NullOrNotEmpty(ForwardDefault);
                options.ForwardForbid = NullOrNotEmpty(ForwardForbid);
                options.ForwardSignIn = NullOrNotEmpty(ForwardSignIn);
                options.ForwardSignOut = NullOrNotEmpty(ForwardSignOut);
                options.NameClaimType = NameClaimType;
                options.RoleClaimType = RoleClaimType;
                options.RequireHttpsMetadata = RequireHttpsMetadata;
                options.EnableCaching = EnableCaching;
                options.TokenRetriever = GetTokenRetrievalOptions(this);
                if (CacheDuration > 0)
                    options.CacheDuration = TimeSpan.FromMinutes(CacheDuration);
            };
        }

        string NotNullOrEmpty(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException($"参数{name}不能为空.");
            }
            else
            {
                return value.Trim();
            }
        }

        static string NullOrNotEmpty(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            else
            {
                return value.Trim();
            }
        }

        static Func<HttpRequest, string> GetTokenRetrievalOptions(IdentityAuthOptions options)
        {
            if (options.TokenRetrievalOptions == null)
            {
                return TokenRetrieval.FromAuthorizationHeader();
            }

            switch (options.TokenRetrievalOptions.Source)
            {
                default:
                case TokenSource.FromAuthorizationHeader:
                    if (string.IsNullOrWhiteSpace(options.TokenRetrievalOptions.NameOrSchema))
                        options.TokenRetrievalOptions.NameOrSchema = "Bearer";
                    return TokenRetrieval.FromAuthorizationHeader(options.TokenRetrievalOptions.NameOrSchema);
                case TokenSource.FromCustomHeader:
                    if (string.IsNullOrWhiteSpace(options.TokenRetrievalOptions.NameOrSchema))
                        options.TokenRetrievalOptions.NameOrSchema = "Bearer";
                    if (string.IsNullOrWhiteSpace(options.TokenRetrievalOptions.Header))
                        options.TokenRetrievalOptions.Header = "Geone-OAuth2-IS4";
                    return FromCustomHeader(options.TokenRetrievalOptions);
                case TokenSource.FromQueryString:
                    if (string.IsNullOrWhiteSpace(options.TokenRetrievalOptions.NameOrSchema))
                        options.TokenRetrievalOptions.NameOrSchema = "access_token";
                    return TokenRetrieval.FromQueryString(options.TokenRetrievalOptions.NameOrSchema);
            }
        }

        public static Func<HttpRequest, string> FromCustomHeader(TokenRetrievalOptions options)
        {
            return (request) =>
            {
                string authorization = request.Headers[options.Header].FirstOrDefault();

                if (string.IsNullOrEmpty(authorization))
                {
                    return null;
                }

                if (authorization.StartsWith(options.NameOrSchema + " ", StringComparison.OrdinalIgnoreCase))
                {
                    return authorization.Substring(options.NameOrSchema.Length + 1).Trim();
                }

                return null;
            };
        }
    }
}
