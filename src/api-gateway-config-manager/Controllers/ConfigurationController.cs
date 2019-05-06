using ApiGatewayManager.ConfMgr;
using ApiGatewayManager.ConfMgr.Data;
using ApiGatewayManager.ConfMgr.Validation;
using ApiGatewayManager.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog;
using Ocelot.Configuration.Setter;
using Ocelot.Configuration.Validator;
using System;
using System.ComponentModel.DataAnnotations;
using ILogger = NLog.ILogger;

namespace ApiGatewayManager.Controllers
{
    /// <summary>
    /// ocelot映射配置
    /// </summary>
    [Produces("application/json")]
    [Route("admin/configuration")]
    [ApiController]
    [CustomExceptionFilter]
    public class ConfigurationController : Controller
    {
        private readonly OcelotConfigSectionRepository _sectionRepo;
        private readonly OcelotFullConfigRepository _fullConfigRepo;
        private readonly OcelotConfigItemValidation _validator;
        private readonly ILogger<ConfigurationController> _logger;
        private readonly ILogger _adminLogger;

        public ConfigurationController(
            IFileConfigurationSetter setter,
            OcelotConfigSectionRepository sectionRepo,
            OcelotFullConfigRepository fullConfigRepo,
            IConfigurationValidator validator,
            ILogger<ConfigurationController> logger)
        {
            _logger = logger;
            _sectionRepo = sectionRepo;
            _fullConfigRepo = fullConfigRepo;
            _validator = new OcelotConfigItemValidation(_sectionRepo, validator);
            _adminLogger = LogManager.GetLogger("apigatewayadmin");
        }

        /// <summary>
        /// 获取所有映射配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public OcelotConfigItem[] GetAllSections(bool includeDisabled = true)
        {
            return _sectionRepo.GetAll(includeDisabled);
        }

        /// <summary>
        /// 获取映射配置 
        /// </summary>
        /// <param name="name">映射配置名称</param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public OcelotConfigItem GetSection([Required, FromQuery]string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return _sectionRepo.Get(name);
        }

        /// <summary>
        /// 保存映射配置 <paramref name="configSection"/>
        /// </summary>
        /// <param name="configSection">映射配置</param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public IActionResult SaveSection([Required]OcelotConfigItem configSection)
        {
            ConfigValidationResult result = _validator.Validate(configSection).Result;
            if (result.IsError)
            {
                return BadRequest(result.Errors);
            }
            else
            {
                _sectionRepo.SaveOrUpdate(configSection);
                return Ok(configSection);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public void DeleteSection([Required, FromQuery]string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            _sectionRepo.Delete(name);
        }

        /// <summary>
        /// 映射配置检查
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public ConfigValidationResult ValidateSections()
        {
            return _validator.Validate(_sectionRepo.GetAll(false)).Result;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult BuildConfig([Required, FromQuery]string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));

            var configItems = _sectionRepo.GetAll(false);
            ConfigValidationResult validationResult = _validator.Validate(configItems).Result;
            if (validationResult.IsError)
            {
                return BadRequest(validationResult.Errors);
            }

            var config = _fullConfigRepo.Create(configItems, description);
            return Ok(config);
        }

        [HttpPost]
        [Route("[action]")]
        public void EnableConfig([Required, FromQuery]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            _fullConfigRepo.Enable(id);
        }

        [HttpGet]
        [Route("[action]")]
        public OcelotFullConfig[] GetAllConfigs()
        {
            return _fullConfigRepo.GetAll();
        }

        [HttpPost]
        [Route("[action]")]
        public void DeleteConfig([Required, FromQuery]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            _fullConfigRepo.Delete(id);
        }
    }
}
