using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace OCIApiGateway.Authentication
{
    public class IdsAuthOptionsReader
    {
        /// <summary>
        /// 身份验证方案
        /// </summary>
        public IdsAuthOptions[] AuthOptions { get; }

        public IdsAuthOptionsReader(IHostingEnvironment env)
        {
            IdsAuthOptions[] options;
            string jsonFile = "apiauthentication";
            if (string.IsNullOrWhiteSpace(env.EnvironmentName))
            {
                jsonFile = $"{jsonFile}.json";
            }
            else
            {
                jsonFile = $"{jsonFile}.{env.EnvironmentName}.json";
            }

            jsonFile = $"{AppContext.BaseDirectory}{jsonFile}";

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
