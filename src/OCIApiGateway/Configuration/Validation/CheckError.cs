using Ocelot.Errors;

namespace OCIApiGateway.Configuration.Validation
{
    public class CheckError : Error
    {
        public CheckError(string msg)
            : base(msg, OcelotErrorCode.UnknownError)
        {
        }
    }
}
