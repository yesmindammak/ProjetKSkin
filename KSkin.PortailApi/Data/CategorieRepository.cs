using LoginRegisterApp.Helpers;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    // Used only by ProductSyncService: the external system sends family/category as
    // plain names, and this app's structure must stay generic (5.4), so we match an
    // existing row by name or create one on the fly - never a manual insert button.
    public static class CategorieRepository
    {
        public static int FindOrCreateFamille(string nom)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();

            const string findQuery = "SELECT FamilleId FROM FamilleProduit WHERE Nom = @Nom";
            using (var findCommand = new SqlCommand(findQuery, connection))
            {
                findCommand.Parameters.AddWithValue("@Nom", nom);
                object result = findCommand.ExecuteScalar();
                if (result != null) return (int)result;
            }

            const string insertQuery = "INSERT INTO FamilleProduit (Nom) OUTPUT INSERTED.FamilleId VALUES (@Nom)";
            using var insertCommand = new SqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@Nom", nom);
            return (int)insertCommand.ExecuteScalar();
        }

        public static int FindOrCreateCategorie(int familleId, string nom)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();

            const string findQuery = "SELECT CategorieId FROM CategorieProduit WHERE FamilleId = @FamilleId AND Nom = @Nom";
            using (var findCommand = new SqlCommand(findQuery, connection))
            {
                findCommand.Parameters.AddWithValue("@FamilleId", familleId);
                findCommand.Parameters.AddWithValue("@Nom", nom);
                object result = findCommand.ExecuteScalar();
                if (result != null) return (int)result;
            }

            const string insertQuery = "INSERT INTO CategorieProduit (FamilleId, Nom) OUTPUT INSERTED.CategorieId VALUES (@FamilleId, @Nom)";
            using var insertCommand = new SqlCommand(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@FamilleId", familleId);
            insertCommand.Parameters.AddWithValue("@Nom", nom);
            return (int)insertCommand.ExecuteScalar();
        }

        public static System.Collections.Generic.List<LoginRegisterApp.Models.CategorieProduit> GetAllCategories()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "SELECT CategorieId, FamilleId, Nom, Description FROM CategorieProduit ORDER BY Nom";
            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var list = new System.Collections.Generic.List<LoginRegisterApp.Models.CategorieProduit>();
            while (reader.Read())
            {
                list.Add(new LoginRegisterApp.Models.CategorieProduit
                {
                    CategorieId = reader.GetInt32(0),
                    FamilleId = reader.GetInt32(1),
                    Nom = reader.GetString(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                });
            }
            return list;
        }
    }
}
