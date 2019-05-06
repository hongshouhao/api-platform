using Dapper;
using NLog;
using ApiGatewayManager.Exceptions;
using System;
using System.Data;
using System.Linq;

namespace ApiGatewayManager.ConfMgr.Data
{
    public class OcelotConfigSectionRepository
    {
        private const string _tableName = "ConfigSections";
        private readonly DbConnectionFactory _connectionProvider;
        private readonly Logger _logger;

        public OcelotConfigSectionRepository(string connectionString)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _connectionProvider = new DbConnectionFactory(connectionString);
        }

        public OcelotConfigItem Get(string name)
        {
            return _Get(name);
        }

        public OcelotConfigItem[] GetAll(bool includeDisabled)
        {
            string where = string.Empty;
            if (!includeDisabled)
                where = "where enable = 1";

            string sql = $"select * from {_tableName} {where} order by {nameof(OcelotConfigItem.CreateTime)} desc";
            _logger.Debug($"{nameof(GetAll)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                return cone.Query<OcelotConfigItem>(sql).ToArray();
            }
        }

        public bool Exists(string name)
        {
            return _Get(name) != null;
        }

        public void SaveOrUpdate(OcelotConfigItem configSection)
        {
            if (Exists(configSection.Name))
            {
                configSection.ModifiedTime = DateTime.Now;
                Update(configSection);
            }
            else
            {
                configSection.CreateTime = DateTime.Now;
                Create(configSection);
            }
        }

        void Update(OcelotConfigItem configSection)
        {
            Check(configSection);

            string sql = $"update {_tableName} set " +
                $"jsonString = '{configSection.JsonString}', " +
                $"description = '{configSection.Description}', " +
                $"modifiedTime = '{configSection.ModifiedTime}', " +
                $"enable = {Convert.ToInt16(configSection.Enable)} " +
                $"where name = '{configSection.Name}'";

            _logger.Debug($"{nameof(Update)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        void Create(OcelotConfigItem configSection)
        {
            Check(configSection);

            string sql = $"insert into {_tableName} " +
                $"(name, jsonString, description, createTime, enable) " +
                $"values ('{configSection.Name}', '{configSection.JsonString}', '{configSection.Description}', '{configSection.CreateTime}', {Convert.ToInt16(configSection.Enable)})";

            _logger.Debug($"{nameof(Create)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        static void Check(OcelotConfigItem configSection)
        {
            if (string.IsNullOrWhiteSpace(configSection.Name)) throw new UserFriendlyException($"{nameof(OcelotConfigItem.Name)}不可以为空.");
            if (string.IsNullOrWhiteSpace(configSection.JsonString)) throw new UserFriendlyException($"{nameof(OcelotConfigItem.JsonString)}不可以为空.");
        }

        public void Delete(string name)
        {
            string sql = $"delete from {_tableName} where name = '{name}'";
            _logger.Debug($"{nameof(Delete)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                cone.Execute(sql);
            }
        }

        OcelotConfigItem _Get(string name)
        {
            string sql = $"select * from {_tableName} where name = '{name}'";
            _logger.Debug($"{nameof(_Get)} sql:{sql}");
            using (IDbConnection cone = _connectionProvider.Create())
            {
                cone.Open();
                return cone.QueryFirstOrDefault<OcelotConfigItem>(sql);
            }
        }
    }
}
