using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Setter;
using Ocelot.Configuration.Validator;

namespace ApiGateway.Configuration
{
    /// <summary>
    /// ocelot映射配置
    /// </summary>
    [Produces("application/json")]
    [Route("admin/configuration")]
    [ApiController]
    public class ConfigurationController : Controller
    {
        private readonly IFileConfigurationSetter _setter;
        private readonly IFileConfigurationRepository _configRepo;
        private readonly IInternalConfigurationRepository _configUsingRepo;
        private readonly ILogger _logger;

        public ConfigurationController(
            IFileConfigurationRepository configDbRepo,
            IInternalConfigurationRepository configUsingRepo,
            IFileConfigurationSetter setter,
            IConfigurationValidator validator)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _setter = setter;
            _configRepo = configDbRepo;
            _configUsingRepo = configUsingRepo;
        }

        /// <summary>
        /// 获取完成映射配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public JsonResult Get()
        {
            var response = _configUsingRepo.Get();
            return Json(response);
        }

        /// <summary>
        /// 检查所有映射配置并合并所有合法配置生成完整配置并应用
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public IActionResult ReLoad()
        {
            var getResponse = _configRepo.Get().Result;
            if (getResponse.IsError)
            {
                _logger.Warn("加载Ocelot配置失败: " + JsonConvert.SerializeObject(getResponse.Errors));
                return BadRequest(getResponse.Errors);
            }

            var setResponse = _setter.Set(getResponse.Data).Result;
            if (setResponse.IsError)
            {
                _logger.Warn("加载Ocelot配置失败: " + JsonConvert.SerializeObject(setResponse.Errors));
                return BadRequest(setResponse.Errors);
            }

            _logger.Info("加载Ocelot配置成功: " + JsonConvert.SerializeObject(getResponse.Data));

            return Ok(getResponse.Data);
        }
    }
}
