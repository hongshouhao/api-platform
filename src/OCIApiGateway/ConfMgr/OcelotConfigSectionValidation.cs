using Newtonsoft.Json;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Validator;
using Ocelot.Errors;
using Ocelot.Responses;
using OCIApiGateway.ConfMgr.Data;
using OCIApiGateway.ConfMgr.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OCIApiGateway.ConfMgr
{
    class OcelotConfigSectionValidation
    {
        private readonly IConfigurationValidator _validator;
        private readonly OcelotConfigSectionRepository _repository;

        public OcelotConfigSectionValidation(OcelotConfigSectionRepository repository, IConfigurationValidator validator)
        {
            _validator = validator;
            _repository = repository;
        }

        public async Task<ConfigValidationResult> Validate(FileConfiguration fileConfiguration)
        {
            Response<ConfigurationValidationResult> response = await _validator.IsValid(fileConfiguration);
            if (response.IsError)
            {
                return new ConfigValidationResult(response.Errors);
            }
            else
            {
                return new ConfigValidationResult();
            }
        }

        public async Task<ConfigValidationResult> Validate(params OcelotConfigSection[] sections)
        {
            if (sections.Length == 0) return new ConfigValidationResult();

            List<Error> errors = new List<Error>();
            foreach (OcelotConfigSection item in sections)
            {
                Error e = NotNullOrEmpty(item);
                if (e != null)
                {
                    errors.Add(e);
                }
            }

            if (errors.Count > 0)
            {
                return new ConfigValidationResult(errors);
            }
            else
            {
                return await Validate(sections.Build());
            }
        }

        public CheckError NotNullOrEmpty(OcelotConfigSection section)
        {
            if (string.IsNullOrWhiteSpace(section.Name))
            {
                return new CheckError($"[{nameof(OcelotConfigSection.Name)}]不能为空.");
            }

            if (string.IsNullOrWhiteSpace(section.JsonString))
            {
                return new CheckError($"[{nameof(OcelotConfigSection.JsonString)}]不能为空.");
            }

            OcelotConfigSectionType type = (OcelotConfigSectionType)section.SectionType;

            if (type != OcelotConfigSectionType.GlobalConfiguration
                && type != OcelotConfigSectionType.ReRoutes)
            {
                return new CheckError($"[{nameof(OcelotConfigSection.SectionType)}]非法.");
            }

            //if (type == OcelotConfigSectionType.GlobalConfiguration
            //      && section.IsEmptyGlobal())
            //{
            //    return new CheckError("[GlobalConfiguration]配置项中不包含任何有效信息");
            //}

            //if (type == OcelotConfigSectionType.ReRoutes
            //    && section.IsEmptyReRoutes())
            //{
            //    return new CheckError("[ReRoutes]配置项中不包含任何路由");
            //}

            try
            {
                FileConfiguration configuration = JsonConvert.DeserializeObject<FileConfiguration>(section.JsonString);
                return null;
            }
            catch (Exception ex)
            {
                return new CheckError(ex.Message);
            }
        }
    }
}
