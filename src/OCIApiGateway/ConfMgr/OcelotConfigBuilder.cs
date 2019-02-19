using Newtonsoft.Json;
using Ocelot.Configuration.File;
using System.Collections.Generic;
using System;

namespace OCIApiGateway.ConfMgr
{
    public static class OcelotConfigBuilder
    {
        public static FileConfiguration Build(this IEnumerable<OcelotConfigSection> sections)
        {
            FileConfiguration result = new FileConfiguration();
            foreach (OcelotConfigSection item in sections)
            {
                var config = JsonConvert.DeserializeObject<FileConfiguration>(item.JsonString);
                OcelotConfigSectionType type = (OcelotConfigSectionType)item.SectionType;

                switch (type)
                {
                    case OcelotConfigSectionType.GlobalConfiguration:
                        result.GlobalConfiguration = config.GlobalConfiguration;
                        break;
                    case OcelotConfigSectionType.ReRoutes:
                        result.ReRoutes.AddRange(config.ReRoutes);
                        break;
                    case OcelotConfigSectionType.DynamicReRoutes:
                        result.DynamicReRoutes.AddRange(config.DynamicReRoutes);
                        break;
                    case OcelotConfigSectionType.Aggregates:
                        result.Aggregates.AddRange(config.Aggregates);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return result;
        }
    }
}
