using System.Configuration;
using System.Data.SqlClient;

namespace FlitBit.Dapper
{
    public static class Db
    {
        // <summary>
        /// Connects and opens a connection using the first connection string in the configuration file
        /// </summary>
        /// <returns>An opened connection</returns>
        public static SqlConnection GetConnectionAndOpen()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[0];
            return GetConnectionAndOpen(connectionString.ConnectionString);
        }

        /// <summary>
        /// Connects and opens a connection
        /// </summary>
        /// <param name="connectionString">The sql connection string</param>
        /// <returns>An opened connection</returns>
        public static SqlConnection GetConnectionAndOpen(string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }
    }
}
