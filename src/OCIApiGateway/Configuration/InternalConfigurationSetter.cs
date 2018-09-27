using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Setter;
using Ocelot.Responses;
using System.Threading.Tasks;

namespace OCIApiGateway.Configuration
{
    public class InternalConfigurationSetter : IFileConfigurationSetter
    {
        private readonly IInternalConfigurationRepository internalConfigRepo;
        private readonly IInternalConfigurationCreator _configCreator;

        public InternalConfigurationSetter(
            IInternalConfigurationRepository configRepo,
            IInternalConfigurationCreator configCreator)
        {
            internalConfigRepo = configRepo;
            _configCreator = configCreator;
        }

        public async Task<Response> Set(FileConfiguration fileConfig)
        {
            var config = await _configCreator.Create(fileConfig);

            if (!config.IsError)
            {
                internalConfigRepo.AddOrReplace(config.Data);
            }

            return new ErrorResponse(config.Errors);
        }
    }
}
