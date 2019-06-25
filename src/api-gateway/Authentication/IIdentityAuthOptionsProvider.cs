namespace ApiGateway.Authentication
{
    interface IIdentityAuthOptionsProvider
    {
        IdentityAuthOptions[] GetOptions();
    }
}
