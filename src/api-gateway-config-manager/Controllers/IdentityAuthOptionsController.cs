using ApiGatewayManager.Exceptions;
using ApiGatewayManager.Ids4Conf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using ILogger = NLog.ILogger;

namespace ApiGatewayManager.Controllers
{
    /// <summary>
    /// ocelot映射配置
    /// </summary>
    [Produces("application/json")]
    [Route("admin/authoptions")]
    [ApiController]
    [CustomExceptionFilter]
    public class IdentityAuthOptionsController : Controller
    {
        private readonly IdentityAuthOptionsConfigRepository _repository;
        private readonly ILogger _logger;

        public IdentityAuthOptionsController(IdentityAuthOptionsConfigRepository repository)
        {
            _repository = repository;
            _logger = LogManager.GetLogger("apigatewayadmin");
        }

        [HttpGet]
        [Route("[action]")]
        public IdentityAuthOptionsConfig[] GetAll()
        {
            return _repository.GetAll();
        }

        [HttpGet]
        [Route("[action]")]
        public IdentityAuthOptionsConfig Get([Required, FromQuery]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            return _repository.Get(id);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Save([Required]IdentityAuthOptionsConfig optionsConfig)
        {
            _repository.SaveOrUpdate(optionsConfig);
            _logger.Info("保存Identity配置: " + JsonConvert.SerializeObject(optionsConfig));
            return Ok(optionsConfig);
        }

        [HttpPost]
        [Route("[action]")]
        public void Delete([Required, FromQuery]string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            _logger.Info($"删除Identity配置: {id}");
            _repository.Delete(id);
        }
    }
}
