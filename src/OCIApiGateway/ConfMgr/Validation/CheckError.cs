using Ocelot.Errors;

namespace OCIApiGateway.ConfMgr.Validation
{
    public class CheckError : Error
    {
        public CheckError(string msg)
            : base(msg, OcelotErrorCode.UnknownError)
        {
        }
    }
}
