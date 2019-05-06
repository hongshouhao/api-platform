using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace ApiGateway.Authentication
{
    public class IdsAuthOptionsReader
    {
        /// <summary>
        /// 身份验证方案
        /// </summary>
        public IdsAuthOptions[] AuthOptions { get; }

        public IdsAuthOptionsReader(IHostingEnvironment env)
        {
            string jsonFile = Path.Combine(AppContext.BaseDirectory, $"apiauthorization.json");
            string jsonEnv = Path.Combine(AppContext.BaseDirectory, $"apiauthorization.{env.EnvironmentName}.json");
            if (File.Exists(jsonEnv))
            {
                jsonFile = jsonEnv;
            }

            IdsAuthOptions[] options;
            if (File.Exists(jsonFile))
            {
                string text = File.ReadAllText(jsonFile);
                options = JsonConvert.DeserializeObject<IdsAuthOptions[]>(text);
                foreach (var authop in options)
                {
                    if (string.IsNullOrWhiteSpace(authop.ApiName))
                        throw new Exception("配置项错误: [apiauthoptions.json]中某项[ApiName]为空");

                    if (string.IsNullOrWhiteSpace(authop.Authority))
                        throw new Exception("配置项错误: [apiauthoptions.json]中某项[Authority]为空");

                    if (string.IsNullOrWhiteSpace(authop.AuthScheme))
                        throw new Exception("配置项错误: [apiauthoptions.json]中某项[AuthScheme]为空");
                }
            }
            else
            {
                options = new IdsAuthOptions[] { };
            }
            AuthOptions = options;
        }
    }
}
