using ApiGatewayManager.OcelotConf;
using Dapper;
using Newtonsoft.Json;
using NLog;
using Ocelot.Configuration.File;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ApiGatewayManager.Data
{
    public class OcelotCompleteConfigRepository
    {
        private readonly Logger _debugLogger;
        private readonly Logger _adminLogger;
        private const string _tableName = "Configs";
        private readonly DbConnectionFactory _connectionProvider;
        private readonly OcelotConfigSectionRepository _configItemRepository;

        public OcelotCompleteConfigRepository(string connectionString)
        {
            _debugLogger = LogManager.GetCurrentClassLogger();
            _adminLogger = LogManager.GetLogger("apigatewayadmin");

            _connectionProvider = new DbConnectionFactory(connectionString);
            _configItemRepository = new OcelotConfigSectionRepository(connectionString);

        }

        public OcelotCompleteConfig[] GetAll()
        {
            string sql = $"select * from {_tableName} order by {nameof(OcelotConfigSection.CreateTime)} desc";

            _debugLogger.Debug($"{nameof(GetAll)} sql:{sql}");

            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                return cone.Query<OcelotCompleteConfig>(sql).ToArray();
            }
        }

        public void Delete(string id)
        {
            LogEventInfo logInfo = new LogEventInfo()
            {
                Message = "删除Ocelot配置",
                Level = LogLevel.Warn
            };
            logInfo.Properties.Add("config", GetJsonString(id));

            string sql = $"delete from {_tableName} where id = '{id}'";
            _debugLogger.Debug($"{nameof(Delete)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);

                _adminLogger.Log(logInfo);
            }
        }

        public OcelotCompleteConfig Create(OcelotConfigSection[] configItems, string description)
        {
            FileConfiguration configuration = configItems.Build();

            string json = JsonConvert.SerializeObject(configuration);
            var uid = Guid.NewGuid();

            OcelotCompleteConfig config = new OcelotCompleteConfig
            {
                Id = uid,
                CreateTime = DateTime.Now,
                Enable = false,
                JsonString = json,
                Description = description
            };

            string sql = $"insert into {_tableName} " +
                $"(id, jsonString, description, createTime, enable) " +
                $"values ('{config.Id}', '{config.JsonString}', '{config.Description}', '{config.CreateTime}', 0)";

            _debugLogger.Debug($"{nameof(Create)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }

            return config;
        }

        public void Enable(string id)
        {
            LogEventInfo logInfo = new LogEventInfo()
            {
                Message = "应用Ocelot配置",
                Level = LogLevel.Warn
            };
            logInfo.Properties.Add("config", GetJsonString(id));

            string sqlEnable = $"update {_tableName} set enable = 1 where id = '{id}'";
            string sqlDisable = $"update {_tableName} set enable = 0 where id <> '{id}'";
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                using (IDbTransaction trans = cone.BeginTransaction())
                {
                    int result = cone.Execute(sqlEnable, null, trans);
                    if (result > 0)
                    {
                        cone.Execute(sqlDisable, null, trans);
                        trans.Commit();

                        _adminLogger.Log(logInfo);
                    }
                    else
                    {
                        throw new ArgumentException($"id[{id}]不存在");
                    }
                }
            }
        }

        public bool Exists(string id)
        {
            string sql = $"select * from {_tableName} where id = '{id}'";
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                return cone.Query(sql).ToArray().Length > 0;
            }
        }

        string GetJsonString(string id)
        {
            string sql = $"select jsonString from {_tableName} where id = '{id}'";
            using (SqlConnection conn = _connectionProvider.Create() as SqlConnection)
            {
                conn.Open();

                using (SqlDataReader reader = new SqlCommand(sql, conn).ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Convert.ToString(reader["jsonString"]);
                    }
                }
            }
            return string.Empty;
        }
    }
}
