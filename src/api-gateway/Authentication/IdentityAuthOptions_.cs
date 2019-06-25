using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Http;
using System;

namespace ApiGateway.Authentication
{
    public partial class IdentityAuthOptions
    {
        public Action<IdentityServerAuthenticationOptions> Build()
        {
            return options =>
            {
                options.Authority = Authority;
                options.ApiName = ApiName;
                options.ApiSecret = ApiSecret;
                options.SupportedTokens = (IdentityServer4.AccessTokenValidation.SupportedTokens)(int)SupportedTokens;
                options.EnableCaching = EnableCaching;
                options.TokenRetriever = GetTokenRetrievalOptions(this);
                options.CacheDuration = TimeSpan.FromMinutes(CacheDuration);
                options.ClaimsIssuer = ClaimsIssuer;
                options.ForwardAuthenticate = ForwardAuthenticate;
                options.ForwardChallenge = ForwardChallenge;
                options.ForwardDefault = ForwardDefault;
                options.ForwardForbid = ForwardForbid;
                options.ForwardSignIn = ForwardSignIn;
                options.ForwardSignOut = ForwardSignOut;
                options.NameClaimType = NameClaimType;
                options.RequireHttpsMetadata = RequireHttpsMetadata;
                options.RoleClaimType = RoleClaimType;
            };
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

                case TokenSource.FromQueryString:
                    if (string.IsNullOrWhiteSpace(options.TokenRetrievalOptions.NameOrSchema))
                        options.TokenRetrievalOptions.NameOrSchema = "access_token";
                    return TokenRetrieval.FromQueryString(options.TokenRetrievalOptions.NameOrSchema);
            }
        }
    }
}
