using System;

namespace ApiGatewayManager.ConfMgr
{
    public class OcelotFullConfig
    {
        public OcelotFullConfig() { }

        public Guid Id { get; set; }
        public string JsonString { get; set; }
        public string Description { get; set; }
        public bool Enable { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
