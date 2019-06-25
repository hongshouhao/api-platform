using Ocelot.Errors;

namespace ApiGatewayManager.OcelotConf.Validation
{
    public class CheckError : Error
    {
        public CheckError(string msg)
            : base(msg, OcelotErrorCode.UnknownError)
        {
        }
    }
}
