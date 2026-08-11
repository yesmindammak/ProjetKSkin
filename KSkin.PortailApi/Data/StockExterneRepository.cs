using System;
using System.Collections.Generic;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Models;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    public static class StockExterneRepository
    {
        public static List<ProduitExterne> GetAll()
        {
            EnsureExternalTablesAndSeedDisponibilites();

            using SqlConnection connection = DatabaseHelper.GetStockConnection();
            const string query = @"
                SELECT ReferenceExterne, Nom, Prix, Stock, Couleur, Type, Ingredients, Description, ImageUrl, FamilleNom, CategorieNom,
                       CASE WHEN COL_LENGTH('ProduitExterne', 'Marque') IS NOT NULL THEN Marque ELSE NULL END AS Marque,
                       CASE WHEN COL_LENGTH('ProduitExterne', 'MarqueImageUrl') IS NOT NULL THEN MarqueImageUrl ELSE NULL END AS MarqueImageUrl
                FROM ProduitExterne";

            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var produits = new List<ProduitExterne>();
            while (reader.Read())
            {
                produits.Add(new ProduitExterne
                {
                    ReferenceExterne = reader.GetString(0),
                    Nom = reader.GetString(1),
                    Prix = reader.GetDecimal(2),
                    Stock = reader.GetInt32(3),
                    Couleur = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Type = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Ingredients = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                    ImageUrl = reader.IsDBNull(8) ? null : reader.GetString(8),
                    FamilleNom = reader.GetString(9),
                    CategorieNom = reader.GetString(10),
                    Marque = reader.IsDBNull(11) ? null : reader.GetString(11),
                    MarqueImageUrl = reader.IsDBNull(12) ? null : reader.GetString(12),
                });
            }
            return produits;
        }

        public static HashSet<string> GetAllExternalStoreNames()
        {
            EnsureExternalTablesAndSeedDisponibilites();
            var set = new HashSet<string>();
            try
            {
                using SqlConnection connection = DatabaseHelper.GetStockConnection();
                const string query = "SELECT Nom FROM PointDeVente";
                using var command = new SqlCommand(query, connection);
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    set.Add(reader.GetString(0));
                }
            }
            catch
            {
            }
            return set;
        }

        public static void AdjustStock(string? referenceExterne, string nom, int quantiteDelta)
        {
            try
            {
                using SqlConnection connection = DatabaseHelper.GetStockConnection();
                const string query = @"
                    UPDATE ProduitExterne
                    SET Stock = Stock + @Delta
                    WHERE (ReferenceExterne = @Ref AND @Ref IS NOT NULL AND @Ref <> '') OR Nom = @Nom";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Delta", quantiteDelta);
                command.Parameters.AddWithValue("@Ref", (object?)referenceExterne ?? System.DBNull.Value);
                command.Parameters.AddWithValue("@Nom", nom);
                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        public static void SetExactStock(string? referenceExterne, string nom, int exactStock)
        {
            try
            {
                using SqlConnection connection = DatabaseHelper.GetStockConnection();
                const string query = @"
                    UPDATE ProduitExterne
                    SET Stock = @Stock
                    WHERE (ReferenceExterne = @Ref AND @Ref IS NOT NULL AND @Ref <> '') OR Nom = @Nom";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Stock", exactStock);
                command.Parameters.AddWithValue("@Ref", (object?)referenceExterne ?? System.DBNull.Value);
                command.Parameters.AddWithValue("@Nom", nom);
                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        public class ProduitDisponibilite
        {
            public string PointDeVenteNom { get; set; } = "";
            public string StatutDisponibilite { get; set; } = "";
            public int QuantiteStock { get; set; }
            public string? Ville { get; set; }
            public string? Adresse { get; set; }
        }

        public static List<ProduitDisponibilite> GetDisponibilitesForExternalProduct(string? referenceExterne, string nom)
        {
            EnsureExternalTablesAndSeedDisponibilites();

            var list = new List<ProduitDisponibilite>();
            try
            {
                using SqlConnection connection = DatabaseHelper.GetStockConnection();
                const string query = @"
                    SELECT pdv.Nom AS PointDeVenteNom, ppdv.StatutDisponibilite, ppdv.QuantiteStock, pdv.Ville, pdv.Adresse
                    FROM ProduitPointDeVente ppdv
                    JOIN PointDeVente pdv ON ppdv.PointDeVenteId = pdv.PointDeVenteId
                    WHERE (@Ref IS NOT NULL AND @Ref <> '' AND ppdv.ReferenceExterne = @Ref)
                       OR ((@Ref IS NULL OR @Ref = '') AND EXISTS (SELECT 1 FROM ProduitExterne pe WHERE pe.ReferenceExterne = ppdv.ReferenceExterne AND pe.Nom = @Nom))
                    ORDER BY pdv.PointDeVenteId ASC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Ref", (object?)referenceExterne ?? System.DBNull.Value);
                command.Parameters.AddWithValue("@Nom", nom);

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new ProduitDisponibilite
                    {
                        PointDeVenteNom = reader.GetString(0),
                        StatutDisponibilite = reader.GetString(1),
                        QuantiteStock = reader.GetInt32(2),
                        Ville = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Adresse = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }
            }
            catch
            {
            }

            return list;
        }

        public static void EnsureExternalTablesAndSeedDisponibilites()
        {
            try
            {
                using SqlConnection connection = DatabaseHelper.GetStockConnection();

                const string createPdv = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PointDeVente')
                    BEGIN
                        CREATE TABLE PointDeVente (
                            PointDeVenteId INT IDENTITY(1,1) PRIMARY KEY,
                            Nom NVARCHAR(100) NOT NULL,
                            Ville NVARCHAR(50) NULL,
                            Adresse NVARCHAR(255) NULL
                        );
                    END";
                using var cmd1 = new SqlCommand(createPdv, connection);
                cmd1.ExecuteNonQuery();

                const string createPpdv = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProduitPointDeVente')
                    BEGIN
                        CREATE TABLE ProduitPointDeVente (
                            ProduitPointDeVenteId INT IDENTITY(1,1) PRIMARY KEY,
                            ReferenceExterne VARCHAR(50) NULL,
                            PointDeVenteId INT NOT NULL,
                            StatutDisponibilite NVARCHAR(50) NOT NULL DEFAULT 'En Stock',
                            QuantiteStock INT NOT NULL DEFAULT 10
                        );
                    END";
                using var cmd2 = new SqlCommand(createPpdv, connection);
                cmd2.ExecuteNonQuery();

                var stores = new (string Nom, string Ville, string Adresse)[]
                {
                    ("Achat En Ligne", "Tunis", "Service Expédition Client"),
                    ("Megastore Tunis Charguia 1", "Tunis", "Z.I. Charguia 1"),
                    ("Magasin Av. Liberté", "Tunis", "Avenue de la Liberté, Tunis"),
                    ("Magasin Manar City", "Tunis", "Centre Commercial El Manar 2"),
                    ("Magasin Hammamet Yasmine", "Hammamet", "Zone Touristique Yasmine Hammamet"),
                    ("Megastore Sousse Kantaoui", "Sousse", "Port El Kantaoui, Sousse"),
                    ("Magasin Bizerte Corniche", "Bizerte", "Boulevard de la Corniche, Bizerte"),
                    ("Megastore Sfax Centre Ville", "Sfax", "Avenue Habib Bourguiba, Sfax")
                };

                foreach (var store in stores)
                {
                    const string seedPdv = @"
                        IF NOT EXISTS (SELECT 1 FROM PointDeVente WHERE Nom = @Nom)
                        BEGIN
                            INSERT INTO PointDeVente (Nom, Ville, Adresse) VALUES (@Nom, @Ville, @Adresse);
                        END";
                    using var cmdSeedPdv = new SqlCommand(seedPdv, connection);
                    cmdSeedPdv.Parameters.AddWithValue("@Nom", store.Nom);
                    cmdSeedPdv.Parameters.AddWithValue("@Ville", store.Ville);
                    cmdSeedPdv.Parameters.AddWithValue("@Adresse", store.Adresse);
                    cmdSeedPdv.ExecuteNonQuery();
                }

                const string checkEmpty = "SELECT COUNT(*) FROM ProduitPointDeVente";
                using var cmdCheck = new SqlCommand(checkEmpty, connection);
                int count = (int)cmdCheck.ExecuteScalar();

                if (count == 0)
                {
                    const string seedMap = @"
                        INSERT INTO ProduitPointDeVente (ReferenceExterne, PointDeVenteId, QuantiteStock, StatutDisponibilite)
                        SELECT 
                            pe.ReferenceExterne, 
                            pdv.PointDeVenteId,
                            calc.Qty AS QuantiteStock,
                            CASE 
                                WHEN pdv.Nom LIKE '%En Ligne%' THEN 'Expédition 24h'
                                WHEN calc.Qty > 5 THEN 'En Stock'
                                WHEN calc.Qty BETWEEN 1 AND 5 THEN 'En Arrivage'
                                ELSE 'Commande 48h'
                            END AS StatutDisponibilite
                        FROM ProduitExterne pe
                        CROSS JOIN PointDeVente pdv
                        CROSS APPLY (
                            SELECT CASE 
                                WHEN pdv.Nom LIKE '%En Ligne%' THEN ((ABS(CHECKSUM(COALESCE(pe.ReferenceExterne, pe.Nom)) + 3) % 35) + 15)
                                WHEN pdv.Nom LIKE '%Charguia%' THEN ((ABS(CHECKSUM(COALESCE(pe.ReferenceExterne, pe.Nom)) + 7) % 25))
                                WHEN pdv.Nom LIKE '%Kantaoui%' THEN ((ABS(CHECKSUM(COALESCE(pe.ReferenceExterne, pe.Nom)) + 11) % 20))
                                WHEN pdv.Nom LIKE '%Sfax%' THEN ((ABS(CHECKSUM(COALESCE(pe.ReferenceExterne, pe.Nom)) + 13) % 22))
                                ELSE ((ABS(CHECKSUM(COALESCE(pe.ReferenceExterne, pe.Nom)) + pdv.PointDeVenteId * 19) % 15))
                            END AS Qty
                        ) calc;";
                    using var cmdSeedMap = new SqlCommand(seedMap, connection);
                    cmdSeedMap.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }
    }
}
