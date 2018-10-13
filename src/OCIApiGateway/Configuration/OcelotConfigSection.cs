using System;

namespace OCIApiGateway.Configuration
{
    public class OcelotConfigSection
    {
        public OcelotConfigSection() { }

        public int Id { get; set; } = -1;
        public string Name { get; set; }
        public string JsonString { get; set; }
        public bool Enable { get; set; } = true;
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public static OcelotConfigSection Empty
        {
            get
            {
                return new OcelotConfigSection()
                {
                    Name = "empty",
                    CreateTime = DateTime.Now,
                    JsonString = "{\"ReRoutes\": []}",
                };
            }
        }

        public static OcelotConfigSection Global
        {
            get
            {
                return new OcelotConfigSection()
                {
                    Name = "global",
                    CreateTime = DateTime.Now,
                    JsonString = "{\"GlobalConfiguration\": []}",
                };
            }
        }

        public bool IsGlobal()
        {
            return string.Equals("global", Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public bool IsEmpty()
        {
            return string.Equals("empty", Name, StringComparison.CurrentCultureIgnoreCase);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
