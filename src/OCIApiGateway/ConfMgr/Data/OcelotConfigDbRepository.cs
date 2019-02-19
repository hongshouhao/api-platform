using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Responses;
using System;
using System.Threading.Tasks;

namespace OCIApiGateway.ConfMgr.Data
{
    public class OcelotConfigDbRepository : IFileConfigurationRepository
    {
        private readonly OcelotConfigSectionRepository _repository;
        public OcelotConfigDbRepository(OcelotConfigSectionRepository sectionRepo)
        {
            _repository = sectionRepo;
        }

        public async Task<Response<FileConfiguration>> Get()
        {
            OcelotConfigSection[] sections = _repository.GetAll(false);
            return await Task.FromResult<Response<FileConfiguration>>(
                new OkResponse<FileConfiguration>(sections.Build()));
        }

        public Task<Response> Set(FileConfiguration fileConfiguration)
        {
            throw new NotImplementedException();
        }
    }
}
