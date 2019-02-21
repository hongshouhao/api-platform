using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocelot.Configuration.File;
using OCIApiGateway.ConfMgr;
using OCIApiGateway.ConfMgr.Data;
using OCIApiGateway.Exceptions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OCIApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("admin/template")]
    [Authorize]
    [ApiController]
    [CustomExceptionFilter]
    public class TemplateController : Controller
    {
        private readonly OcelotConfigTemplateRepository _repo;
        private readonly ILogger<TemplateController> _logger;

        public TemplateController(
            OcelotConfigTemplateRepository sectionRepo,
            ILogger<TemplateController> logger)
        {
            _repo = sectionRepo;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        public Task _(FileConfiguration configuration)
        {
            return Task.CompletedTask;
        }

        [HttpGet]
        [Route("[action]")]
        public Task<JsonResult> Get(string version)
        {
            OcelotConfigTemplate template = _repo.Get(version);
            if (template == null)
            {
                template = new OcelotConfigTemplate
                {
                    Version = DateTime.Now.ToString("yyyyMMdd"),
                    CreateTime = DateTime.Now,
                    JsonString = JsonConvert.SerializeObject(new FileConfiguration())
                };
            }

            return Task.FromResult(new JsonResult(template));
        }

        [HttpPost]
        [Route("[action]")]
        public Task Save([Required]OcelotConfigTemplate template)
        {
            _repo.SaveOrUpdate(template);
            return Task.CompletedTask;
        }

    }
}
