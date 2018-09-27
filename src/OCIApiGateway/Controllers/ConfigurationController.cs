using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Setter;
using Ocelot.Configuration.Validator;
using OCIApiGateway.Auth;
using OCIApiGateway.Configuration;
using OCIApiGateway.Configuration.Validation;
using OCIApiGateway.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OCIApiGateway.Controllers
{
    /// <summary>
    /// ocelot映射配置
    /// </summary>
    [ApiController]
    [CustomExceptionFilter]
    [Produces("application/json")]
    [Route("admin/[controller]")]
    [Authorize(AuthenticationSchemes = AdminAPI.AuthenticationSchemes, Policy = AdminAPI.AuthorizePolicy)]
    public class ConfigurationController : Controller
    {
        private readonly IFileConfigurationSetter _setter;
        private readonly IFileConfigurationRepository _repo;
        private readonly OcelotConfigSectionRepository _configSectionRepo;
        private readonly OcelotConfigSectionValidation _configSectionValidation;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="setter"></param>
        /// <param name="serviceProvider"></param>
        public ConfigurationController(
            IFileConfigurationRepository repo,
            IFileConfigurationSetter setter,
            IConfiguration configuration,
            IConfigurationValidator validator)
        {
            _repo = repo;
            _setter = setter;
            _configSectionRepo = new OcelotConfigSectionRepository(configuration);
            _configSectionValidation = new OcelotConfigSectionValidation(validator);
        }

        /// <summary>
        /// 获取所有映射配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public Task<JsonResult> GetAllSections()
        {
            OcelotConfigSection[] sections = _configSectionRepo.GetAllSections();
            return Task.FromResult(new JsonResult(sections));
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
            OcelotConfigSection section = _configSectionRepo.GetSection(name);
            return Task.FromResult(new JsonResult(section));
        }

        /// <summary>
        /// 保存映射配置 <paramref name="ocelotSection"/>
        /// </summary>
        /// <param name="ocelotSection">映射配置</param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SaveSection([Required]OcelotConfigSection ocelotSection)
        {
            ConfigValidationResult result = await _configSectionValidation.Validate(ocelotSection);
            if (result.IsError)
            {
                return new BadRequestObjectResult(result.Errors);
            }
            else
            {
                _configSectionRepo.SaveOrUpdate(ocelotSection);
                return new OkResult();
            }
        }

        /// <summary>
        /// 保存映射配置 <paramref name="ocelotSection"/>
        /// </summary>
        /// <param name="ocelotSection">映射配置</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("[action]")]
        public IActionResult DeleteSection(int id)
        {
            _configSectionRepo.Delete(id);
            return new OkResult();
        }

        /// <summary>
        /// 映射配置检查
        /// </summary>
        /// <param name="ocelotSection">映射配置</param>
        /// <returns></returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> ValidateSection([Required]OcelotConfigSection ocelotSection)
        {
            if (string.IsNullOrWhiteSpace(ocelotSection.Name))
            {
                return new JsonResult(new ConfigValidationResult("Name不可以为空"));
            }
            else
            {
                OcelotConfigSection section = _configSectionRepo.GetSection(ocelotSection.Name);
                if (section.IsEmpty())
                {
                    ConfigValidationResult result = await _configSectionValidation.Validate(ocelotSection);
                    return new JsonResult(result);
                }
                else
                {
                    return new JsonResult(new ConfigValidationResult("Name跟其他配置项重复"));
                }
            }
        }

        /// <summary>
        /// 获取完成映射配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<JsonResult> GetFullConfiguration()
        {
            var response = await _repo.Get();
            return new JsonResult(response.Data);
        }

        /// <summary>
        /// 检查所有映射配置并合并所有合法配置生成完整配置并应用
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> ReBuiltOcelotConfiguration()
        {
            try
            {
                var sections = _configSectionRepo.GetAllSections();
                ConfigValidationResult validationResult = await _configSectionValidation.Validate(sections);

                if (validationResult.IsError)
                {
                    return new BadRequestObjectResult(validationResult.Errors);
                }
                else
                {
                    var getResponse = await _repo.Get();
                    if (getResponse.IsError)
                    {
                        return new BadRequestObjectResult(getResponse.Errors);
                    }
                    else
                    {
                        var setResponse = await _setter.Set(getResponse.Data);
                        if (setResponse.IsError)
                        {
                            return new BadRequestObjectResult(setResponse.Errors);
                        }

                        return new OkObjectResult(getResponse);
                    }
                }
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult($"{e.Message}:{e.StackTrace}");
            }
        }
    }
}
