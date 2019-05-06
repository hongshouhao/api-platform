using System;

namespace ApiGatewayManager.ConfMgr
{
    public class OcelotConfigTemplate
    {
        public OcelotConfigTemplate() { }

        public string Version { get; set; }
        public string JsonString { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
    }
}
