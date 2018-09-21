using IdentityServer4.AccessTokenValidation;

namespace OCIApiGateway
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
