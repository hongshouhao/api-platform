using Ocelot.Errors;

namespace ApiGatewayManager.ConfMgr.Validation
{
    public class CheckError : Error
    {
        public CheckError(string msg)
            : base(msg, OcelotErrorCode.UnknownError)
        {
        }
    }
}
