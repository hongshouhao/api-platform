using ApiGateway.Authentication;
using ApiGatewayManager.Data;
using ApiGatewayManager.Exceptions;
using Dapper;
using Newtonsoft.Json;
using NLog;
using System;
using System.Data;
using System.Linq;

namespace ApiGatewayManager.Ids4Conf
{
    public class IdentityAuthOptionsConfigRepository
    {
        private const string _tableName = "AuthOptions";
        private readonly DbConnectionFactory _connectionProvider;
        private readonly Logger _logger;

        public IdentityAuthOptionsConfigRepository(string connectionString)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _connectionProvider = new DbConnectionFactory(connectionString);
        }

        public IdentityAuthOptionsConfig Get(string id)
        {
            return _Get(id);
        }

        public bool Exists(string id)
        {
            return _Get(id) != null;
        }

        public IdentityAuthOptionsConfig[] GetAll()
        {
            string sql = $"select * from {_tableName} order by {nameof(IdentityAuthOptionsConfig.CreateTime)} desc";
            _logger.Debug($"{nameof(GetAll)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                return cone.Query<IdentityAuthOptionsConfig>(sql).ToArray();
            }
        }

        public void SaveOrUpdate(IdentityAuthOptionsConfig authOptions)
        {
            if (Exists(authOptions.Id.ToString()))
            {
                authOptions.ModifiedTime = DateTime.Now;
                Update(authOptions);
            }
            else
            {
                authOptions.CreateTime = DateTime.Now;
                Create(authOptions);
            }
        }

        void Update(IdentityAuthOptionsConfig authOptions)
        {
            Check(authOptions);
            string sql = $"update {_tableName} set " +
                $"jsonString = '{authOptions.JsonString}', " +
                $"description = '{authOptions.Description}', " +
                $"modifiedTime = '{authOptions.ModifiedTime}'" +
                $"where id = '{authOptions.Id}'";

            _logger.Debug($"{nameof(Update)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        void Create(IdentityAuthOptionsConfig authOptions)
        {
            Check(authOptions);
            authOptions.Id = Guid.NewGuid();
            string sql = $"insert into {_tableName} " +
                $"(id, jsonString, description, createTime) " +
                $"values (" +
                $"'{authOptions.Id}', " +
                $"'{authOptions.JsonString}', " +
                $"'{authOptions.Description}', " +
                $"'{authOptions.CreateTime}')";

            _logger.Debug($"{nameof(Create)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        public void Delete(string id)
        {
            string sql = $"delete from {_tableName} where id = '{id}'";
            _logger.Debug($"{nameof(Delete)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        static void Check(IdentityAuthOptionsConfig optionsConfig)
        {
            if (string.IsNullOrWhiteSpace(optionsConfig.JsonString))
                throw new UserFriendlyException($"{nameof(IdentityAuthOptionsConfig.JsonString)}不可以为空.");

            IdentityAuthOptions authop;
            try
            {
                authop = JsonConvert.DeserializeObject<IdentityAuthOptions>(optionsConfig.JsonString);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Json反序列化出错: {ex.Message}");
            }

            if (string.IsNullOrWhiteSpace(authop.ApiName))
                throw new UserFriendlyException($"配置错误: [{nameof(IdentityAuthOptions.ApiName)}]不可以为空");

            if (string.IsNullOrWhiteSpace(authop.Authority))
                throw new UserFriendlyException($"配置错误: [{nameof(IdentityAuthOptions.Authority)}]不可以为空");

            if (string.IsNullOrWhiteSpace(authop.AuthScheme))
                throw new UserFriendlyException($"配置错误: [{nameof(IdentityAuthOptions.AuthScheme)}]不可以为空");
        }

        IdentityAuthOptionsConfig _Get(string id)
        {
            string sql = $"select * from {_tableName} where id = '{id}'";
            _logger.Debug($"{nameof(_Get)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                return cone.QueryFirstOrDefault<IdentityAuthOptionsConfig>(sql);
            }
        }
    }
}
