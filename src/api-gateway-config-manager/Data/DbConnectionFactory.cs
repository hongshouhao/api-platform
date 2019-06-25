using System.Data;
using System.Data.SqlClient;

namespace ApiGatewayManager.Data
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString = null;
        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection Create()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
