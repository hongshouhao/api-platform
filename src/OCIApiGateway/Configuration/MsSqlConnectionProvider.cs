using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace OCIApiGateway.Configuration
{
    public class MsSqlConnectionProvider
    {
        string _connectionString = null;
        public MsSqlConnectionProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OcelotAdminConnection");
            if (string.IsNullOrWhiteSpace(_connectionString)) throw new Exception("数据库连接字符串为空.");
        }

        public IDbConnection Get()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
