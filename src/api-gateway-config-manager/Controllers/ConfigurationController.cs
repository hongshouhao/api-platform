using ApiGatewayManager.Data;
using ApiGatewayManager.Exceptions;
using ApiGatewayManager.OcelotConf;
using ApiGatewayManager.OcelotConf.Validation;
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
        private readonly OcelotCompleteConfigRepository _fullConfigRepo;
        private readonly OcelotConfigValidation _validator;

        private readonly ILogger _logger;

        public ConfigurationController(
            OcelotConfigSectionRepository sectionRepo,
            OcelotCompleteConfigRepository fullConfigRepo,
            IConfigurationValidator validator)
        {
            _sectionRepo = sectionRepo;
            _fullConfigRepo = fullConfigRepo;
            _validator = new OcelotConfigValidation(_sectionRepo, validator);
            _logger = LogManager.GetLogger("apigatewayadmin");
        }

        /// <summary>
        /// 获取所有映射配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public OcelotConfigSection[] GetAllSections(bool includeDisabled = true)
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
        public OcelotConfigSection GetSection([Required, FromQuery]string name)
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
        public IActionResult SaveSection([Required]OcelotConfigSection configSection)
        {
            ConfigValidationResult result = _validator.Validate(configSection).Result;
            if (result.IsError)
            {
                _logger.Warn("Ocelot配置片段错误: " + JsonConvert.SerializeObject(result.Errors));
                return BadRequest(result.Errors);
            }
            else
            {
                _sectionRepo.SaveOrUpdate(configSection);
                _logger.Info("保存Ocelot配置片段: " + JsonConvert.SerializeObject(configSection));
                return Ok(configSection);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public void DeleteSection([Required, FromQuery]string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            _logger.Info($"删除Ocelot配置片段: {name}");
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
            _logger.Info($"生成新Ocelot配置: {JsonConvert.SerializeObject(config)}");
            return Ok(config);
        }

        [HttpPost]
        [Route("[action]")]
        public void EnableConfig([Required, FromQuery]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            _fullConfigRepo.Enable(id);
            _logger.Info($"启用Ocelot配置: {id}");
        }

        [HttpGet]
        [Route("[action]")]
        public OcelotCompleteConfig[] GetAllConfigs()
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
            _logger.Info($"删除Ocelot配置: {id}");
        }
    }
}
