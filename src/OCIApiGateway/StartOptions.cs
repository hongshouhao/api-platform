using System.Net;

namespace OCIApiGateway.Opions
{
    public class StartOptions
    {
        public bool UseConsul { get; set; }
        public bool UseSwagger { get; set; }
        public string ButterflyHost { get; set; }

        internal bool UseButterfly => !string.IsNullOrWhiteSpace(ButterflyHost);
        internal string ButterflyLoggingKey => $"oci_{Dns.GetHostName()}";
    }
}
