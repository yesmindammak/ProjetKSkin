using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Helpers
{
    // Same public shape as the desktop app's DatabaseHelper (GetConnection /
    // GetStockConnection), so the Data/ repository classes copied over from
    // LoginRegisterApp (UserRepository, ProduitRepository, ContactRepository,
    // DemandeAchatRepository, NotificationRepository...) work here without
    // any changes to their own code.
    //
    // The only difference is where the connection string comes from: the
    // desktop app reads App.config via ConfigurationManager, ASP.NET Core
    // apps read appsettings.json via IConfiguration. Program.cs calls
    // Initialize() once at startup with the configuration that was bound
    // from appsettings.json / environment variables.
    public static class DatabaseHelper
    {
        private static string? _connectionString;
        private static string? _stockConnectionString;

        public static void Initialize(string connectionString, string? stockConnectionString = null)
        {
            _connectionString = connectionString;
            _stockConnectionString = stockConnectionString;
        }

        public static SqlConnection GetConnection()
        {
            if (_connectionString is null)
                throw new InvalidOperationException(
                    "DatabaseHelper.Initialize(...) must be called from Program.cs before any request is handled.");

            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        // Only present so ProduitRepository/ProductSyncService compile unmodified
        // if you copy them over - the portal API never calls this itself, since
        // it only ever reads from the app's own Produit table (5.4: the portal
        // is a client of KSkinManager, not of the external stock database).
        public static SqlConnection GetStockConnection()
        {
            if (_stockConnectionString is null)
                throw new InvalidOperationException("No stock connection string configured for this API.");

            var connection = new SqlConnection(_stockConnectionString);
            connection.Open();
            return connection;
        }
    }
}
