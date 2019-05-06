using Newtonsoft.Json;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Validator;
using Ocelot.Errors;
using Ocelot.Responses;
using ApiGatewayManager.ConfMgr.Data;
using ApiGatewayManager.ConfMgr.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGatewayManager.ConfMgr
{
    class OcelotConfigItemValidation
    {
        private readonly IConfigurationValidator _validator;
        private readonly OcelotConfigSectionRepository _repository;

        public OcelotConfigItemValidation(OcelotConfigSectionRepository repository, IConfigurationValidator validator)
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

        public async Task<ConfigValidationResult> Validate(params OcelotConfigItem[] sections)
        {
            if (sections.Length == 0) return new ConfigValidationResult();

            List<Error> errors = new List<Error>();
            foreach (OcelotConfigItem item in sections)
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

        public CheckError NotNullOrEmpty(OcelotConfigItem section)
        {
            if (string.IsNullOrWhiteSpace(section.Name))
            {
                return new CheckError($"[{nameof(OcelotConfigItem.Name)}]不能为空.");
            }

            if (string.IsNullOrWhiteSpace(section.JsonString))
            {
                return new CheckError($"[{nameof(OcelotConfigItem.JsonString)}]不能为空.");
            }

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
