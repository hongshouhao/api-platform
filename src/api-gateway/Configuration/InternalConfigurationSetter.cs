using NLog;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Setter;
using Ocelot.Responses;
using System.Threading.Tasks;

namespace ApiGateway.Configuration
{
    public class InternalConfigurationSetter : IFileConfigurationSetter
    {
        private readonly IInternalConfigurationCreator _configCreator;
        private readonly IInternalConfigurationRepository _internalConfigRepo;
        private readonly ILogger _logger;

        public InternalConfigurationSetter(
            IInternalConfigurationRepository configRepo,
            IInternalConfigurationCreator configCreator)
        {
            _internalConfigRepo = configRepo;
            _configCreator = configCreator;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<Response> Set(FileConfiguration fileConfig)
        {
            var config = await _configCreator.Create(fileConfig);
            if (!config.IsError)
            {
                _internalConfigRepo.AddOrReplace(config.Data);
            }

            return new ErrorResponse(config.Errors);
        }
    }
}
