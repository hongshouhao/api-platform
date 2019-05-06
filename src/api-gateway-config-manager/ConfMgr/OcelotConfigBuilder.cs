using Newtonsoft.Json;
using Ocelot.Configuration.File;
using System.Collections.Generic;
using System;

namespace ApiGatewayManager.ConfMgr
{
    public static class OcelotConfigBuilder
    {
        public static FileConfiguration Build(this IEnumerable<OcelotConfigItem> items)
        {
            FileConfiguration result = new FileConfiguration();
            foreach (OcelotConfigItem item in items)
            {
                var config = JsonConvert.DeserializeObject<FileConfiguration>(item.JsonString);
                if (item.IsGlobal)
                {
                    result.GlobalConfiguration = config.GlobalConfiguration;
                }
                else
                {
                    result.ReRoutes.AddRange(config.ReRoutes);
                    result.DynamicReRoutes.AddRange(config.DynamicReRoutes);
                    result.Aggregates.AddRange(config.Aggregates);
                }
            }

            return result;
        }
    }
}
