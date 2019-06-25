using System;

namespace ApiGatewayManager.OcelotConf
{
    public class OcelotCompleteConfig
    {
        public OcelotCompleteConfig() { }

        public Guid Id { get; set; }
        public string JsonString { get; set; }
        public string Description { get; set; }
        public bool Enable { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
