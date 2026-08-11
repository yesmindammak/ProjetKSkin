using System.Collections.Generic;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Models;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    public static class ProduitRepository
    {
        public static Produit? GetByReferenceExterne(string referenceExterne)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT ProduitId, CategorieId, Nom, Prix, Stock, Couleur, Type, Ingredients, Description, ImageUrl, ReferenceExterne, DateAjout,
                       CASE WHEN COL_LENGTH('Produit', 'Marque') IS NOT NULL THEN Marque ELSE NULL END AS Marque,
                       CASE WHEN COL_LENGTH('Produit', 'MarqueImageUrl') IS NOT NULL THEN MarqueImageUrl ELSE NULL END AS MarqueImageUrl
                FROM Produit WHERE ReferenceExterne = @Ref";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Ref", referenceExterne);
            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read()) return null;

            return new Produit
            {
                ProduitId = reader.GetInt32(0),
                CategorieId = reader.GetInt32(1),
                Nom = reader.GetString(2),
                Prix = reader.GetDecimal(3),
                Stock = reader.GetInt32(4),
                Couleur = reader.IsDBNull(5) ? null : reader.GetString(5),
                Type = reader.IsDBNull(6) ? null : reader.GetString(6),
                Ingredients = reader.IsDBNull(7) ? null : reader.GetString(7),
                Description = reader.IsDBNull(8) ? null : reader.GetString(8),
                ImageUrl = reader.IsDBNull(9) ? null : reader.GetString(9),
                ReferenceExterne = reader.IsDBNull(10) ? null : reader.GetString(10),
                DateAjout = reader.GetDateTime(11),
                Marque = reader.IsDBNull(12) ? null : reader.GetString(12),
                MarqueImageUrl = reader.IsDBNull(13) ? null : reader.GetString(13),
            };
        }

        public static Produit CreateFromSync(Produit produit)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();

            bool hasMarque = false;
            using (var checkCmd = new SqlCommand("SELECT COL_LENGTH('Produit', 'Marque')", connection))
            {
                hasMarque = checkCmd.ExecuteScalar() != System.DBNull.Value && checkCmd.ExecuteScalar() != null;
            }

            string query = hasMarque ? @"
                INSERT INTO Produit (CategorieId, Nom, Prix, Stock, Couleur, Type, Ingredients, Description, ImageUrl, ReferenceExterne, Marque, MarqueImageUrl)
                OUTPUT INSERTED.ProduitId, INSERTED.DateAjout
                VALUES (@CategorieId, @Nom, @Prix, @Stock, @Couleur, @Type, @Ingredients, @Description, @ImageUrl, @ReferenceExterne, @Marque, @MarqueImageUrl)"
            : @"
                INSERT INTO Produit (CategorieId, Nom, Prix, Stock, Couleur, Type, Ingredients, Description, ImageUrl, ReferenceExterne)
                OUTPUT INSERTED.ProduitId, INSERTED.DateAjout
                VALUES (@CategorieId, @Nom, @Prix, @Stock, @Couleur, @Type, @Ingredients, @Description, @ImageUrl, @ReferenceExterne)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@CategorieId", produit.CategorieId);
            command.Parameters.AddWithValue("@Nom", produit.Nom);
            command.Parameters.AddWithValue("@Prix", produit.Prix);
            command.Parameters.AddWithValue("@Stock", produit.Stock);
            command.Parameters.AddWithValue("@Couleur", (object?)produit.Couleur ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Type", (object?)produit.Type ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Ingredients", (object?)produit.Ingredients ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Description", (object?)produit.Description ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@ImageUrl", (object?)produit.ImageUrl ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@ReferenceExterne", (object?)produit.ReferenceExterne ?? System.DBNull.Value);
            if (hasMarque)
            {
                command.Parameters.AddWithValue("@Marque", (object?)produit.Marque ?? System.DBNull.Value);
                command.Parameters.AddWithValue("@MarqueImageUrl", (object?)produit.MarqueImageUrl ?? System.DBNull.Value);
            }

            using SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            produit.ProduitId = reader.GetInt32(0);
            produit.DateAjout = reader.GetDateTime(1);
            return produit;
        }

        public static void UpdateFromSync(Produit produit)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();

            bool hasMarque = false;
            using (var checkCmd = new SqlCommand("SELECT COL_LENGTH('Produit', 'Marque')", connection))
            {
                hasMarque = checkCmd.ExecuteScalar() != System.DBNull.Value && checkCmd.ExecuteScalar() != null;
            }

            string query = hasMarque ? @"
                UPDATE Produit
                SET Nom = @Nom, Prix = @Prix, Stock = @Stock, Couleur = @Couleur, Type = @Type,
                    Ingredients = @Ingredients, Description = @Description, ImageUrl = @ImageUrl,
                    Marque = @Marque, MarqueImageUrl = @MarqueImageUrl
                WHERE ProduitId = @ProduitId"
            : @"
                UPDATE Produit
                SET Nom = @Nom, Prix = @Prix, Stock = @Stock, Couleur = @Couleur, Type = @Type,
                    Ingredients = @Ingredients, Description = @Description, ImageUrl = @ImageUrl
                WHERE ProduitId = @ProduitId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nom", produit.Nom);
            command.Parameters.AddWithValue("@Prix", produit.Prix);
            command.Parameters.AddWithValue("@Stock", produit.Stock);
            command.Parameters.AddWithValue("@Couleur", (object?)produit.Couleur ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Type", (object?)produit.Type ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Ingredients", (object?)produit.Ingredients ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Description", (object?)produit.Description ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@ImageUrl", (object?)produit.ImageUrl ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@ProduitId", produit.ProduitId);
            if (hasMarque)
            {
                command.Parameters.AddWithValue("@Marque", (object?)produit.Marque ?? System.DBNull.Value);
                command.Parameters.AddWithValue("@MarqueImageUrl", (object?)produit.MarqueImageUrl ?? System.DBNull.Value);
            }
            command.ExecuteNonQuery();
        }

        public static List<Produit> GetAll()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT ProduitId, CategorieId, Nom, Prix, Stock, Couleur, Type, Ingredients, Description, ImageUrl, DateAjout,
                       CASE WHEN COL_LENGTH('Produit', 'Marque') IS NOT NULL THEN Marque ELSE NULL END AS Marque,
                       CASE WHEN COL_LENGTH('Produit', 'MarqueImageUrl') IS NOT NULL THEN MarqueImageUrl ELSE NULL END AS MarqueImageUrl
                FROM Produit ORDER BY Nom";

            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var produits = new List<Produit>();
            while (reader.Read())
            {
                produits.Add(new Produit
                {
                    ProduitId = reader.GetInt32(0),
                    CategorieId = reader.GetInt32(1),
                    Nom = reader.GetString(2),
                    Prix = reader.GetDecimal(3),
                    Stock = reader.GetInt32(4),
                    Couleur = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Type = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Ingredients = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Description = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ImageUrl = reader.IsDBNull(9) ? null : reader.GetString(9),
                    DateAjout = reader.GetDateTime(10),
                    Marque = reader.IsDBNull(11) ? null : reader.GetString(11),
                    MarqueImageUrl = reader.IsDBNull(12) ? null : reader.GetString(12),
                });
            }
            return produits;
        }

        public static Produit? GetById(int produitId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT ProduitId, CategorieId, Nom, Prix, Stock, Couleur, Type, Ingredients, Description, ImageUrl, DateAjout,
                       CASE WHEN COL_LENGTH('Produit', 'Marque') IS NOT NULL THEN Marque ELSE NULL END AS Marque,
                       CASE WHEN COL_LENGTH('Produit', 'MarqueImageUrl') IS NOT NULL THEN MarqueImageUrl ELSE NULL END AS MarqueImageUrl
                FROM Produit WHERE ProduitId = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", produitId);
            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read()) return null;

            return new Produit
            {
                ProduitId = reader.GetInt32(0),
                CategorieId = reader.GetInt32(1),
                Nom = reader.GetString(2),
                Prix = reader.GetDecimal(3),
                Stock = reader.GetInt32(4),
                Couleur = reader.IsDBNull(5) ? null : reader.GetString(5),
                Type = reader.IsDBNull(6) ? null : reader.GetString(6),
                Ingredients = reader.IsDBNull(7) ? null : reader.GetString(7),
                Description = reader.IsDBNull(8) ? null : reader.GetString(8),
                ImageUrl = reader.IsDBNull(9) ? null : reader.GetString(9),
                DateAjout = reader.GetDateTime(10),
                Marque = reader.IsDBNull(11) ? null : reader.GetString(11),
                MarqueImageUrl = reader.IsDBNull(12) ? null : reader.GetString(12),
            };
        }

        public static void SetExactStock(int produitId, int exactStock)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "UPDATE Produit SET Stock = @Stock WHERE ProduitId = @Id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Stock", exactStock);
            command.Parameters.AddWithValue("@Id", produitId);
            command.ExecuteNonQuery();
        }

        public static void AdjustStock(int produitId, int quantiteDelta)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "UPDATE Produit SET Stock = Stock + @Delta WHERE ProduitId = @Id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Delta", quantiteDelta);
            command.Parameters.AddWithValue("@Id", produitId);
            command.ExecuteNonQuery();
        }

        public static bool TryDeductStockAtomic(int produitId, int quantite)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                UPDATE Produit
                SET Stock = Stock - @Quantite
                WHERE ProduitId = @Id AND Stock >= @Quantite";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Quantite", quantite);
            command.Parameters.AddWithValue("@Id", produitId);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public static void DeleteMissingFromSync(HashSet<string> validExternalRefs)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string selectQuery = "SELECT ProduitId, ReferenceExterne FROM Produit WHERE ReferenceExterne IS NOT NULL";
            using var selectCmd = new SqlCommand(selectQuery, connection);
            using var reader = selectCmd.ExecuteReader();

            var idsToDelete = new List<int>();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                string refExt = reader.GetString(1);
                if (!validExternalRefs.Contains(refExt))
                {
                    idsToDelete.Add(id);
                }
            }
            reader.Close();

            foreach (int id in idsToDelete)
            {
                try
                {
                    const string deleteQuery = "DELETE FROM Produit WHERE ProduitId = @Id";
                    using var delCmd = new SqlCommand(deleteQuery, connection);
                    delCmd.Parameters.AddWithValue("@Id", id);
                    delCmd.ExecuteNonQuery();
                }
                catch
                {
                    const string updateQuery = "UPDATE Produit SET Stock = 0 WHERE ProduitId = @Id";
                    using var updCmd = new SqlCommand(updateQuery, connection);
                    updCmd.Parameters.AddWithValue("@Id", id);
                    updCmd.ExecuteNonQuery();
                }
            }
        }

        public class MarqueInfo
        {
            public string Nom { get; set; } = "";
            public string? ImageUrl { get; set; }
        }

        public static List<MarqueInfo> GetDistinctMarques()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT DISTINCT
                       CASE WHEN COL_LENGTH('Produit', 'Marque') IS NOT NULL THEN Marque ELSE NULL END AS Marque,
                       MAX(CASE WHEN COL_LENGTH('Produit', 'MarqueImageUrl') IS NOT NULL THEN MarqueImageUrl ELSE NULL END) AS MarqueImageUrl
                FROM Produit
                WHERE Marque IS NOT NULL AND Marque <> ''
                GROUP BY Marque
                ORDER BY Marque";

            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var list = new List<MarqueInfo>();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    list.Add(new MarqueInfo
                    {
                        Nom = reader.GetString(0),
                        ImageUrl = reader.IsDBNull(1) ? null : reader.GetString(1)
                    });
                }
            }
            return list;
        }
    }
}
