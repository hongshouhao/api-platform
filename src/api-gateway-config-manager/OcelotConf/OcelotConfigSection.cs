using System;

namespace ApiGatewayManager.OcelotConf
{
    public class OcelotConfigSection
    {
        public OcelotConfigSection() { }

        public string Name { get; set; }
        public string JsonString { get; set; }
        public bool Enable { get; set; } = false;
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public bool IsGlobal
        {
            get { return string.Equals(Name, "global", StringComparison.CurrentCultureIgnoreCase); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
