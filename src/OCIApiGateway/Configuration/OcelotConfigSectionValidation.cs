using Newtonsoft.Json;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Validator;
using Ocelot.Errors;
using Ocelot.Responses;
using OCIApiGateway.Configuration.Validation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OCIApiGateway.Configuration
{
    public class OcelotConfigSectionValidation
    {
        private readonly IConfigurationValidator _validator;

        public OcelotConfigSectionValidation(IConfigurationValidator validator)
        {
            _validator = validator;
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
            List<Error> errors = new List<Error>();
            foreach (OcelotConfigSection item in sections)
            {
                JsonCheckResult jsonCheckResult = ValidateJson(item.JsonString);
                if (jsonCheckResult.IsError)
                {
                    errors.Add(jsonCheckResult.Error);
                    continue;
                }

                Response<ConfigurationValidationResult> response = await _validator.IsValid(jsonCheckResult.Data);
                if (response.IsError)
                {
                    errors.AddRange(response.Errors);
                    continue;
                }
            }

            if (errors.Count > 0)
            {
                return new ConfigValidationResult(errors);
            }
            else
            {
                return new ConfigValidationResult();
            }
        }

        JsonCheckResult ValidateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new JsonCheckResult("Json为空");
            }

            try
            {
                FileConfiguration configuration = JsonConvert.DeserializeObject<FileConfiguration>(json);
                return new JsonCheckResult(configuration);
            }
            catch (Exception ex)
            {
                return new JsonCheckResult(ex.Message);
            }
        }
    }
}
