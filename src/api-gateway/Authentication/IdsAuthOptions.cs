using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Http;
using System;

namespace ApiGateway.Authentication
{
    public class IdsAuthOptions
    {
        public string AuthScheme { get; set; }
        public string Authority { get; set; }
        public string ApiName { get; set; }
        public string ApiSecret { get; set; }
        public string RoleClaimType { get; set; } = "role";
        public string NameClaimType { get; set; } = "name";
        public bool EnableCaching { get; set; }
        public string ClaimsIssuer { get; set; }
        public int CacheDuration { get; set; }
        public bool RequireHttpsMetadata { get; set; }

        public string ForwardDefault { get; set; }
        public string ForwardAuthenticate { get; set; }
        public string ForwardChallenge { get; set; }
        public string ForwardForbid { get; set; }
        public string ForwardSignIn { get; set; }
        public string ForwardSignOut { get; set; }

        public SupportedTokens SupportedTokens { get; set; }
        public TokenRetrievalOptions TokenRetrievalOptions { get; set; }

        public Action<IdentityServerAuthenticationOptions> Build()
        {
            return options =>
            {
                options.Authority = Authority;
                options.ApiName = ApiName;
                options.ApiSecret = ApiSecret;
                options.SupportedTokens = SupportedTokens;
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

    public class TokenRetrievalOptions
    {
        public TokenSource Source { get; set; }
        public string NameOrSchema { get; set; }
    }

    public enum TokenSource
    {
        FromAuthorizationHeader,
        FromQueryString
    }
}
