using Dapper;
using NLog;
using ApiGatewayManager.Exceptions;
using System;
using System.Data;
using System.Linq;

namespace ApiGatewayManager.ConfMgr.Data
{
    public class OcelotConfigTemplateRepository
    {
        private const string _tableName = "ConfigTemplates";
        private readonly DbConnectionFactory _connectionProvider;
        private readonly Logger _logger;

        public OcelotConfigTemplateRepository(string connectionString)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _connectionProvider = new DbConnectionFactory(connectionString);
        }

        public OcelotConfigTemplate Get(string version)
        {
            return _Get(version);
        }

        public bool Exists(string version)
        {
            return _Get(version) != null;
        }

        public OcelotConfigTemplate[] GetAll()
        {
            string sql = $"select * from {_tableName} order by {nameof(OcelotConfigTemplate.CreateTime)} desc";
            _logger.Debug($"{nameof(GetAll)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                return cone.Query<OcelotConfigTemplate>(sql).ToArray();
            }
        }

        public void SaveOrUpdate(OcelotConfigTemplate template)
        {
            if (Exists(template.Version))
            {
                template.ModifiedTime = DateTime.Now;
                Update(template);
            }
            else
            {
                template.CreateTime = DateTime.Now;
                Create(template);
            }
        }

        void Update(OcelotConfigTemplate template)
        {
            Check(template);

            string sql = $"update {_tableName} set " +
                $"jsonString = '{template.JsonString}', " +
                $"description = '{template.Description}', " +
                $"modifiedTime = '{template.ModifiedTime}' " +
                $"where version = '{template.Version}'";

            _logger.Debug($"{nameof(Update)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        void Create(OcelotConfigTemplate template)
        {
            Check(template);

            string sql = $"insert into {_tableName} " +
                $"(version, jsonString, description, createTime) " +
                $"values ('{template.Version}', '{template.JsonString}', '{template.Description}', '{template.CreateTime}')";

            _logger.Debug($"{nameof(Create)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        static void Check(OcelotConfigTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.Version)) throw new UserFriendlyException($"{nameof(OcelotConfigTemplate.Version)}不可以为空.");
            if (string.IsNullOrWhiteSpace(template.JsonString)) throw new UserFriendlyException($"{nameof(OcelotConfigTemplate.JsonString)}不可以为空.");
        }

        public void Delete(string version)
        {
            string sql = $"delete from {_tableName} where version = '{version}'";
            _logger.Debug($"{nameof(Delete)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        OcelotConfigTemplate _Get(string version)
        {
            string sql = $"select * from {_tableName} where version = '{version}'";
            _logger.Debug($"{nameof(_Get)} sql:{sql}");

            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                return cone.QueryFirstOrDefault<OcelotConfigTemplate>(sql);
            }
        }
    }
}
