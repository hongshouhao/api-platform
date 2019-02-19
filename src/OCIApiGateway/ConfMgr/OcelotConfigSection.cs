using System;

namespace OCIApiGateway.ConfMgr
{
    public class OcelotConfigSection
    {
        public OcelotConfigSection() { }

        public int Id { get; set; } = -1;
        public int SectionType { get; set; } = (int)OcelotConfigSectionType.ReRoutes;
        public string Name { get; set; }
        public string JsonString { get; set; }
        public bool Enable { get; set; } = true;
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public static OcelotConfigSection EmptyReRoutes
        {
            get
            {
                return new OcelotConfigSection()
                {
                    Name = "empty",
                    SectionType = (int)OcelotConfigSectionType.ReRoutes,
                    CreateTime = DateTime.Now,
                    JsonString = "{\"ReRoutes\":[]}",
                };
            }
        }

        public static OcelotConfigSection EmptyGlobal
        {
            get
            {
                return new OcelotConfigSection()
                {
                    Name = "global",
                    SectionType = (int)OcelotConfigSectionType.GlobalConfiguration,
                    CreateTime = DateTime.Now,
                    JsonString = "{\"GlobalConfiguration\":{}}",
                };
            }
        }

        public bool IsEmptyGlobal()
        {
            if (ReferenceEquals(this, EmptyGlobal))
            {
                return true;
            }
            else
            {
                string jsonFormated = JsonString.Replace("\n", "").Replace(" ", "");
                return string.Equals(jsonFormated, EmptyGlobal.JsonString, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public bool IsEmptyReRoutes()
        {
            if (ReferenceEquals(this, EmptyReRoutes))
            {
                return true;
            }
            else
            {
                string jsonFormated = JsonString.Replace("\n", "").Replace(" ", "");
                return string.Equals(jsonFormated, EmptyReRoutes.JsonString, StringComparison.CurrentCultureIgnoreCase);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
