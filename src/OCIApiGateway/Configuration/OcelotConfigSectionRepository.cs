using Dapper;
using Microsoft.Extensions.Configuration;
using OCIApiGateway.Exceptions;
using System;
using System.Data;
using System.Linq;

namespace OCIApiGateway.Configuration
{
    class OcelotConfigSectionRepository
    {
        string _tableName = "ConfigSections";
        MsSqlConnectionProvider _connectionProvider;

        public OcelotConfigSectionRepository(IConfiguration configuration)
        {
            _connectionProvider = new MsSqlConnectionProvider(configuration);
        }

        public OcelotConfigSection GetSection(string name)
        {
            string sql = $"select * from {_tableName} where name = '{name}'";
            using (IDbConnection cone = _connectionProvider.Get())
            {
                var section = _Get(name);
                if (section == null)
                    section = OcelotConfigSection.Empty;
                return section;
            }
        }

        public OcelotConfigSection[] GetAllSections(bool includeDisabled)
        {
            string where = string.Empty;
            if (!includeDisabled)
                where = "where enable = 1";

            string sql = $"select * from {_tableName} {where} order by {nameof(OcelotConfigSection.CreateTime)} desc";
            using (IDbConnection cone = _connectionProvider.Get())
            {
                return cone.Query<OcelotConfigSection>(sql).ToArray();
            }
        }

        public bool SectionExists(OcelotConfigSection configSection)
        {
            return _Get(configSection.Name) != null;
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
                configSection.CreateTime = DateTime.Now;
                Create(configSection);
            }
        }

        public void Update(OcelotConfigSection configSection)
        {
            if (configSection.Id <= 0) throw new UserFriendlyException("数据库中没有找到此条数据.");
            if (string.IsNullOrWhiteSpace(configSection.Name)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.Name)}不可以为空.");
            if (string.IsNullOrWhiteSpace(configSection.JsonString)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.JsonString)}不可以为空.");
            if (string.Equals(configSection.Name, "empty", StringComparison.CurrentCultureIgnoreCase)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.Name)}不可以为\"empty\".");

            string sql = $"update {_tableName} set " +
                $"name = '{configSection.Name}', " +
                $"jsonString = '{configSection.JsonString}', " +
                $"description = '{configSection.Description}', " +
                $"modifiedTime = '{configSection.ModifiedTime}' " +
                $"enable = {configSection.Enable} " +
                $"where id = {configSection.Id}";
            using (IDbConnection cone = _connectionProvider.Get())
            {
                cone.Execute(sql);
            }
        }

        public void Create(OcelotConfigSection configSection)
        {
            if (string.IsNullOrWhiteSpace(configSection.Name)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.Name)}不可以为空.");
            if (string.IsNullOrWhiteSpace(configSection.JsonString)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.JsonString)}不可以为空.");
            if (string.Equals(configSection.Name, "empty", StringComparison.CurrentCultureIgnoreCase)) throw new UserFriendlyException($"{nameof(OcelotConfigSection.Name)}不可以为\"empty\".");

            OcelotConfigSection section = _Get(configSection.Name);
            if (section != null)
            {
                throw new UserFriendlyException("数据库中已存在相同的Key.");
            }

            string sql = $"insert into {_tableName} (name, jsonString, description, createTime, enable) " +
                $"values ('{configSection.Name}', '{configSection.JsonString}', '{configSection.Description}', '{configSection.CreateTime}', {configSection.Enable})";
            using (IDbConnection cone = _connectionProvider.Get())
            {
                cone.Execute(sql);
            }
        }

        public void Delete(int id)
        {
            if (id <= 0) throw new UserFriendlyException("数据库中没有找到此条数据.");

            string sql = $"delete from {_tableName} where id = {id}";
            using (IDbConnection cone = _connectionProvider.Get())
            {
                cone.Execute(sql);
            }
        }

        OcelotConfigSection _Get(string name)
        {
            string sql = $"select * from {_tableName} where name = '{name}'";
            using (IDbConnection cone = _connectionProvider.Get())
            {
                return cone.QueryFirstOrDefault<OcelotConfigSection>(sql);
            }
        }
    }
}
