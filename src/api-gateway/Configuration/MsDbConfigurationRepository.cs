using Newtonsoft.Json;
using NLog;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Responses;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ApiGateway.Configuration
{
    public class MsDbConfigurationRepository : IFileConfigurationRepository
    {
        private readonly ILogger _logger;
        private readonly string sql = "select top 1 jsonString from Configs where enable = 1 order by createtime desc";
        private readonly string _connString;

        public MsDbConfigurationRepository(string connString)
        {
            _connString = connString;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task<Response<FileConfiguration>> Get()
        {
            string configJson = null;
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                _logger.Debug("读取Ocelot配置 - 打开数据库成功: " + _connString);

                SqlCommand command = new SqlCommand(sql, conn);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        configJson = Convert.ToString(reader["jsonString"]);
                        _logger.Debug("读取Ocelot配置 - 读取数据成功: " + configJson);
                    }
                    else
                    {
                        _logger.Debug("读取Ocelot配置 - 没有读取到任何有效数据: " + sql);
                    }
                }
            }

            FileConfiguration configuration;
            if (string.IsNullOrWhiteSpace(configJson))
            {
                configuration = new FileConfiguration();
            }
            else
            {
                configuration = JsonConvert.DeserializeObject<FileConfiguration>(configJson);
            }

            return await Task.FromResult<Response<FileConfiguration>>(
                new OkResponse<FileConfiguration>(configuration));
        }

        public Task<Response> Set(FileConfiguration fileConfiguration)
        {
            throw new NotImplementedException();
        }
    }
}
