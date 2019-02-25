using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Setter;
using Ocelot.Configuration.Validator;
using OCIApiGateway.ConfMgr;
using OCIApiGateway.ConfMgr.Data;
using OCIApiGateway.ConfMgr.Validation;
using OCIApiGateway.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OCIApiGateway.Controllers
{
    /// <summary>
    /// ocelot映射配置
    /// </summary>
    [Produces("application/json")]
    [Route("admin/Configuration")]
    [Authorize]
    [ApiController]
    [CustomExceptionFilter]
    public class ConfigurationController : Controller
    {
        private readonly IFileConfigurationSetter _setter;
        private readonly IFileConfigurationRepository _configRepo;
        private readonly OcelotConfigSectionRepository _sectionRepo;
        private readonly OcelotConfigSectionValidation _validator;
        private readonly ILogger<ConfigurationController> _logger;

        public ConfigurationController(
            IFileConfigurationRepository configRepo,
            IFileConfigurationSetter setter,
            OcelotConfigSectionRepository sectionRepo,
            IConfigurationValidator validator,
            ILogger<ConfigurationController> logger)
        {
            _setter = setter;
            _configRepo = configRepo;
            _sectionRepo = sectionRepo;
            _logger = logger;
            _validator = new OcelotConfigSectionValidation(_sectionRepo, validator);
        }

        /// <summary>
        /// 获取所有映射配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public Task<JsonResult> GetAllSections(bool includeDisabled = true)
        {
            OcelotConfigSection[] sections = _sectionRepo.GetAll(includeDisabled);
            return Task.FromResult(Json(sections));
        }

        /// <summary>
        /// 获取映射配置 
        /// </summary>
        /// <param name="name">映射配置名称</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public Task<JsonResult> GetSection(string name)
        {
            OcelotConfigSection section = _sectionRepo.Get(name);
            return Task.FromResult(Json(section));
        }

        /// <summary>
        /// 保存映射配置 <paramref name="configSection"/>
        /// </summary>
        /// <param name="configSection">映射配置</param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SaveSection([Required]OcelotConfigSection configSection)
        {
            ConfigValidationResult result = await _validator.Validate(configSection);
            if (result.IsError)
            {
                _logger.LogWarning(result.Errors);
                return BadRequest(result.Errors);
            }
            else
            {
                _sectionRepo.SaveOrUpdate(configSection);
                return Ok(null);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult DeleteSection([FromBody]int id)
        {
            if (id <= 0) return BadRequest("输入的id不合法");

            _sectionRepo.Delete(id);
            return Ok();
        }

        /// <summary>
        /// 映射配置检查
        /// </summary>
        /// <param name="configSections">映射配置</param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> ValidateSections([Required, FromBody]OcelotConfigSection[] configSections)
        {
            ConfigValidationResult validationResult = await _validator.Validate(configSections);

            if (validationResult.IsError)
            {
                _logger.LogWarning(validationResult.Errors);
            }

            return Json(validationResult);
        }

        /// <summary>
        /// 映射配置检查
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> ValidateConfiguration()
        {
            OcelotConfigSection[] sections = _sectionRepo.GetAll(false);
            return await ValidateSections(sections);
        }

        /// <summary>
        /// 获取完成映射配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<JsonResult> GetConfiguration()
        {
            var response = await _configRepo.Get();
            return Json(response.Data);
        }

        /// <summary>
        /// 检查所有映射配置并合并所有合法配置生成完整配置并应用
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> ReBuiltConfiguration()
        {
            var getResponse = await _configRepo.Get();
            if (getResponse.IsError)
            {
                _logger.LogWarning(getResponse.Errors);
                return BadRequest(getResponse.Errors);
            }
            else
            {
                ConfigValidationResult validationResult = await _validator.Validate(getResponse.Data);
                if (validationResult.IsError)
                {
                    _logger.LogWarning(validationResult.Errors);
                    return BadRequest(validationResult.Errors);
                }
                else
                {
                    var setResponse = await _setter.Set(getResponse.Data);
                    if (setResponse.IsError)
                    {
                        _logger.LogWarning(setResponse.Errors);
                        return BadRequest(setResponse.Errors);
                    }

                    return Ok(getResponse);
                }
            }
        }
    }
}
