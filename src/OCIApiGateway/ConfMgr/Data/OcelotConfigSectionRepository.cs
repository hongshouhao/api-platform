using Dapper;
using NLog;
using OCIApiGateway.Exceptions;
using System;
using System.Data;
using System.Linq;

namespace OCIApiGateway.ConfMgr.Data
{
    public class OcelotConfigSectionRepository
    {
        private const string _tableName = "ConfigSections";
        private readonly DbConnectionFactory _connectionProvider;
        private readonly Logger _logger;

        public OcelotConfigSectionRepository(string connectionString)
        {
            _logger = LogManager.GetLogger(nameof(OcelotConfigSectionRepository));
            _connectionProvider = new DbConnectionFactory(connectionString);
        }

        public OcelotConfigSection Get(string name)
        {
            using (IDbConnection cone = _connectionProvider.Create())
            {
                return _Get(name);
            }
        }

        public OcelotConfigSection[] GetAll(bool includeDisabled)
        {
            string where = string.Empty;
            if (!includeDisabled)
                where = "where enable = 1";

            string sql = $"select * from {_tableName} {where} order by {nameof(OcelotConfigSection.CreateTime)} desc";
            _logger.Debug($"{nameof(GetAll)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                return cone.Query<OcelotConfigSection>(sql).ToArray();
            }
        }

        public bool Exists(string name)
        {
            return _Get(name) != null;
        }

        public void SaveOrUpdate(OcelotConfigSection configSection)
        {
            if (configSection.Id > 0)
            {
                configSection.ModifiedTime = DateTime.Now;
                Update(configSection);
            }
            else
            {
                if (Exists(configSection.Name))
                {
                    throw new UserFriendlyException($"[{nameof(OcelotConfigSection.Name)}={configSection.Name}]跟其他配置项重复");
                }

                configSection.CreateTime = DateTime.Now;
                Create(configSection);
            }
        }

        void Update(OcelotConfigSection configSection)
        {
            if (string.IsNullOrWhiteSpace(configSection.Name)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.Name)}不可以为空.");
            if (string.IsNullOrWhiteSpace(configSection.JsonString)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.JsonString)}不可以为空.");

            string sql = $"update {_tableName} set " +
                $"name = '{configSection.Name}', " +
                $"sectionType = {configSection.SectionType}, " +
                $"jsonString = '{configSection.JsonString}', " +
                $"description = '{configSection.Description}', " +
                $"modifiedTime = '{configSection.ModifiedTime}' " +
                $"enable = {Convert.ToInt16(configSection.Enable)} " +
                $"where id = {configSection.Id}";

            _logger.Debug($"{nameof(Update)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Execute(sql);
            }
        }

        void Create(OcelotConfigSection configSection)
        {
            if (string.IsNullOrWhiteSpace(configSection.Name)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.Name)}不可以为空.");
            if (string.IsNullOrWhiteSpace(configSection.JsonString)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.JsonString)}不可以为空.");

            string sql = $"insert into {_tableName} (name, sectionType, jsonString, description, createTime, enable) " +
                $"values ('{configSection.Name}', {configSection.SectionType}, '{configSection.JsonString}', '{configSection.Description}', '{configSection.CreateTime}', {Convert.ToInt16(configSection.Enable)})";

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

        OcelotConfigSection _Get(string name)
        {
            string sql = $"select * from {_tableName} where name = '{name}'";
            _logger.Debug($"{nameof(_Get)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                return cone.QueryFirstOrDefault<OcelotConfigSection>(sql);
            }
        }
    }
}
