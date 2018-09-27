using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Responses;
using System;
using System.Threading.Tasks;

namespace OCIApiGateway.Configuration
{
    public class ConfigurationDbRepository : IFileConfigurationRepository
    {
        private readonly OcelotConfigSectionRepository _repository;
        public ConfigurationDbRepository(IConfiguration configuration)
        {
            _repository = new OcelotConfigSectionRepository(configuration);
        }

        public async Task<Response<FileConfiguration>> Get()
        {
            var fileConfiguration = new FileConfiguration();
            OcelotConfigSection[] sections = _repository.GetAllSections();
            foreach (OcelotConfigSection item in sections)
            {
                var config = JsonConvert.DeserializeObject<FileConfiguration>(item.JsonString);
                if (item.IsGlobal())
                {
                    fileConfiguration.GlobalConfiguration = config.GlobalConfiguration;
                }
                else
                {
                    fileConfiguration.Aggregates.AddRange(config.Aggregates);
                    fileConfiguration.ReRoutes.AddRange(config.ReRoutes);
                }
            }
            return await Task.FromResult<Response<FileConfiguration>>(new OkResponse<FileConfiguration>(fileConfiguration));
        }

        public Task<Response> Set(FileConfiguration fileConfiguration)
        {
            throw new NotImplementedException();
        }
    }
}
