using System;

namespace OCIApiGateway.ConfMgr
{
    public class OcelotConfigTemplate
    {
        public OcelotConfigTemplate() { }

        public int Id { get; set; } = -1;
        public string Version { get; set; }
        public string JsonString { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
    }
}
