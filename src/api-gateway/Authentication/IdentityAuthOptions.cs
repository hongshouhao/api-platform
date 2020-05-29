namespace ApiGateway.Authentication
{
    public partial class IdentityAuthOptions
    {
        public string AuthScheme { get; set; } = "Default";
        public string Authority { get; set; }
        public string ApiName { get; set; }
        public string ApiSecret { get; set; }
        public string RoleClaimType { get; set; } = "role";
        public string NameClaimType { get; set; } = "name";
        public bool EnableCaching { get; set; }
        public string ClaimsIssuer { get; set; }
        public int CacheDuration { get; set; } = 10;
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

    public enum SupportedTokens
    {
        Both = 0,
        Jwt = 1,
        Reference = 2
    }

    public enum TokenSource
    {
        FromAuthorizationHeader = 0,
        FromCustomHeader = 2,
        FromQueryString = 1
    }

    public class TokenRetrievalOptions
    {
        public TokenSource Source { get; set; }
        public string Header { get; set; } = "Geone-Auth-IS4";
        public string NameOrSchema { get; set; }
    }
}
