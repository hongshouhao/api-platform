using Ocelot.Configuration.File;

namespace OCIApiGateway.Configuration.Validation
{
    public class JsonCheckResult
    {
        public JsonCheckResult(string msg)
        {
            IsError = true;
            Error = new CheckError(msg);
        }

        public JsonCheckResult(FileConfiguration configuration)
        {
            IsError = false;
            Data = configuration;
        }

        public FileConfiguration Data { get; set; }
        public bool IsError { get; private set; }
        public CheckError Error { get; private set; }
    }
}
