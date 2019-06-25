using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ApiGateway.Authentication
{
    public class IdentityAuthOptionsDb : IIdentityAuthOptionsProvider
    {
        private readonly string sql = "select jsonString from AuthOptions";
        private readonly string _connString;
        private readonly ILogger _logger;

        public IdentityAuthOptionsDb(string connString)
        {
            _connString = connString;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public IdentityAuthOptions[] GetOptions()
        {
            using (SqlConnection conn = new SqlConnection(_connString))
            {
                conn.Open();
                _logger.Debug("读取Identity配置 - 打开数据库成功: " + _connString);

                string completeJson;
                using (SqlDataReader reader = new SqlCommand(sql, conn).ExecuteReader())
                {
                    List<string> jsonList = new List<string>();
                    while (reader.Read())
                    {
                        string json = Convert.ToString(reader["jsonString"]);
                        jsonList.Add(json);
                    }

                    completeJson = $"[{string.Join(", ", jsonList)}]";
                }

                _logger.Debug("读取Identity配置 - 读取数据成功: " + completeJson);

                return JsonConvert.DeserializeObject<IdentityAuthOptions[]>(completeJson);
            }
        }
    }
}
