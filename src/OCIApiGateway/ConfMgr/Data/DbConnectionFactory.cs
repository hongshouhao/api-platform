using System.Data;
using System.Data.SqlClient;

namespace OCIApiGateway.ConfMgr.Data
{
    public class DbConnectionFactory
    {
        string _connectionString = null;
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
