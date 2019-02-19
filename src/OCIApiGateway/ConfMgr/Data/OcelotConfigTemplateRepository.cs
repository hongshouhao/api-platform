using Dapper;
using NLog;
using OCIApiGateway.Exceptions;
using System;
using System.Data;
using System.Linq;

namespace OCIApiGateway.ConfMgr.Data
{
    public class OcelotConfigTemplateRepository
    {
        private const string _tableName = "ConfigTemplates";
        private readonly DbConnectionFactory _connectionProvider;
        private readonly Logger _logger;

        public OcelotConfigTemplateRepository(string connectionString)
        {
            _logger = LogManager.GetLogger(nameof(OcelotConfigTemplateRepository));
            _connectionProvider = new DbConnectionFactory(connectionString);
        }

        public OcelotConfigTemplate Get(string version)
        {
            using (IDbConnection cone = _connectionProvider.Create())
            {
                return _Get(version);
            }
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
                return cone.Query<OcelotConfigTemplate>(sql).ToArray();
            }
        }

        public void SaveOrUpdate(OcelotConfigTemplate template)
        {
            if (template.Id > 0)
            {
                template.ModifiedTime = DateTime.Now;
                Update(template);
            }
            else
            {
                if (Exists(template.Version))
                {
                    throw new UserFriendlyException($"版本号[{nameof(OcelotConfigTemplate.Version)}={template.Version}]与其他版本冲突");
                }

                template.CreateTime = DateTime.Now;
                Create(template);
            }
        }

        void Update(OcelotConfigTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.Version)) throw new UserFriendlyException($"{nameof(OcelotConfigTemplate.Version)}不可以为空.");
            if (string.IsNullOrWhiteSpace(template.JsonString)) throw new UserFriendlyException($"{nameof(OcelotConfigTemplate.JsonString)}不可以为空.");

            string sql = $"update {_tableName} set " +
                $"version = '{template.Version}', " +
                $"jsonString = '{template.JsonString}', " +
                $"description = '{template.Description}', " +
                $"modifiedTime = '{template.ModifiedTime}' " +
                $"where id = {template.Id}";

            _logger.Debug($"{nameof(Update)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Execute(sql);
            }
        }

        void Create(OcelotConfigTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.Version)) throw new UserFriendlyException($"{nameof(OcelotConfigTemplate.Version)}不可以为空.");
            if (string.IsNullOrWhiteSpace(template.JsonString)) throw new UserFriendlyException($"{nameof(OcelotConfigTemplate.JsonString)}不可以为空.");

            string sql = $"insert into {_tableName} (version, jsonString, description, createTime) " +
                $"values ('{template.Version}', '{template.JsonString}', '{template.Description}', '{template.CreateTime}')";

            _logger.Debug($"{nameof(Create)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Execute(sql);
            }
        }

        public void Delete(int id)
        {
            if (id <= 0) return;

            string sql = $"delete from {_tableName} where id = {id}";
            _logger.Debug($"{nameof(Delete)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Execute(sql);
            }
        }

        OcelotConfigTemplate _Get(string version)
        {
            string sql = $"select * from {_tableName} where version = '{version}'";
            _logger.Debug($"{nameof(_Get)} sql:{sql}");

            using (IDbConnection cone = _connectionProvider.Create())
            {
                return cone.QueryFirstOrDefault<OcelotConfigTemplate>(sql);
            }
        }
    }
}
