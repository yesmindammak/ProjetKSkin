using System.Configuration;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Helpers
{
    public static class DatabaseHelper
    {
        // Reads the "KSkinManager" entry from App.config instead of a hardcoded
        // string, so changing servers/databases never means touching C# code.
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["KSkinManager"].ConnectionString;

        private static readonly string StockConnectionString =
            ConfigurationManager.ConnectionStrings["KSkinStockExterne"].ConnectionString;

        // Every method that needs to talk to the DB calls this to get a fresh, open connection.
        public static SqlConnection GetConnection()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        // Only ProductSyncService should call this - it's the one piece of code
        // allowed to read the external stock database (architecture, section 3).
        public static SqlConnection GetStockConnection()
        {
            var connection = new SqlConnection(StockConnectionString);
            connection.Open();
            return connection;
        }
    }
}
