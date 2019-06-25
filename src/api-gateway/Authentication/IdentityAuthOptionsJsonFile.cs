using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ApiGateway.Authentication
{
    public class IdentityAuthOptionsJsonFile : IIdentityAuthOptionsProvider
    {
        private readonly IdentityAuthOptions[] _options;
        public IdentityAuthOptionsJsonFile(IHostingEnvironment env)
        {
            string jsonFile = Path.Combine(AppContext.BaseDirectory, $"apiauthorization.json");
            string jsonEnv = Path.Combine(AppContext.BaseDirectory, $"apiauthorization.{env.EnvironmentName}.json");
            if (File.Exists(jsonEnv))
            {
                jsonFile = jsonEnv;
            }

            if (File.Exists(jsonFile))
            {
                string text = File.ReadAllText(jsonFile);
                _options = JsonConvert.DeserializeObject<IdentityAuthOptions[]>(text);
                foreach (var authop in _options)
                {
                    if (string.IsNullOrWhiteSpace(authop.ApiName))
                        throw new Exception($"配置错误: [{nameof(IdentityAuthOptions.ApiName)}]不可以为空");

                    if (string.IsNullOrWhiteSpace(authop.Authority))
                        throw new Exception($"配置错误: [{nameof(IdentityAuthOptions.Authority)}]不可以为空");

                    if (string.IsNullOrWhiteSpace(authop.AuthScheme))
                        throw new Exception($"配置错误: [{nameof(IdentityAuthOptions.AuthScheme)}]不可以为空");
                }
            }
            else
            {
                _options = new IdentityAuthOptions[] { };
            }
        }

        public IdentityAuthOptions[] GetOptions()
        {
            return _options;
        }
    }
}
