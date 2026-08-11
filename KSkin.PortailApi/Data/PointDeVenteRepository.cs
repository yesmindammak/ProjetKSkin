using System;
using System.Collections.Generic;
using System.Linq;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Models;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    public static class PointDeVenteRepository
    {
        public static void EnsureTablesCreated()
        {
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();

                // 1. PointDeVente Table
                const string createPdvTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PointDeVente')
                    BEGIN
                        CREATE TABLE PointDeVente (
                            PointDeVenteId INT IDENTITY(1,1) PRIMARY KEY,
                            Nom NVARCHAR(100) NOT NULL,
                            Ville NVARCHAR(50) NULL,
                            Adresse NVARCHAR(255) NULL
                        );
                    END";
                using var cmd1 = new SqlCommand(createPdvTable, connection);
                cmd1.ExecuteNonQuery();

                // 2. ProduitPointDeVente Junction Table
                const string createProduitPdvTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProduitPointDeVente')
                    BEGIN
                        CREATE TABLE ProduitPointDeVente (
                            ProduitPointDeVenteId INT IDENTITY(1,1) PRIMARY KEY,
                            ProduitId INT NOT NULL,
                            PointDeVenteId INT NOT NULL,
                            StatutDisponibilite NVARCHAR(50) NOT NULL DEFAULT 'En Stock',
                            QuantiteStock INT NOT NULL DEFAULT 10,
                            CONSTRAINT FK_ProduitPdv_Produit FOREIGN KEY (ProduitId) REFERENCES Produit(ProduitId) ON DELETE CASCADE,
                            CONSTRAINT FK_ProduitPdv_Pdv FOREIGN KEY (PointDeVenteId) REFERENCES PointDeVente(PointDeVenteId) ON DELETE CASCADE
                        );
                    END";
                using var cmd2 = new SqlCommand(createProduitPdvTable, connection);
                cmd2.ExecuteNonQuery();

                // 3. Ensure DemandeAchat columns exist & drop restrictive legacy CHECK constraints
                const string addDemandeColumns = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DemandeAchat') AND name = 'ModeLivraison')
                    BEGIN
                        ALTER TABLE DemandeAchat ADD ModeLivraison NVARCHAR(100) NULL DEFAULT 'Livraison à Domicile';
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DemandeAchat') AND name = 'ModePaiement')
                    BEGIN
                        ALTER TABLE DemandeAchat ADD ModePaiement NVARCHAR(100) NULL DEFAULT 'Paiement à la Livraison';
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DemandeAchat') AND name = 'PointDeVenteId')
                    BEGIN
                        ALTER TABLE DemandeAchat ADD PointDeVenteId INT NULL;
                    END
                    IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_DemandeAchat_Statut')
                    BEGIN
                        ALTER TABLE DemandeAchat DROP CONSTRAINT CK_DemandeAchat_Statut;
                    END";
                using var cmd3 = new SqlCommand(addDemandeColumns, connection);
                cmd3.ExecuteNonQuery();
            }
            catch (Exception)
            {
            }
        }

        public static List<PointDeVente> GetAllPointsDeVente()
        {
            EnsureTablesCreated();
            var list = new List<PointDeVente>();

            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                const string query = "SELECT PointDeVenteId, Nom, Ville, Adresse FROM PointDeVente ORDER BY PointDeVenteId ASC";
                using var command = new SqlCommand(query, connection);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new PointDeVente
                    {
                        PointDeVenteId = reader.GetInt32(0),
                        Nom = reader.GetString(1),
                        Ville = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Adresse = reader.IsDBNull(3) ? null : reader.GetString(3)
                    });
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        public static List<ProduitDisponibilite> GetDisponibilitesForProduit(int produitId)
        {
            EnsureTablesCreated();
            var list = new List<ProduitDisponibilite>();

            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                const string query = @"
                    SELECT pdv.Nom AS PointDeVenteNom, ppdv.StatutDisponibilite, ppdv.QuantiteStock
                    FROM ProduitPointDeVente ppdv
                    JOIN PointDeVente pdv ON ppdv.PointDeVenteId = pdv.PointDeVenteId
                    WHERE ppdv.ProduitId = @ProduitId
                    ORDER BY pdv.PointDeVenteId ASC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProduitId", produitId);
                using SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new ProduitDisponibilite
                    {
                        PointDeVenteNom = reader.GetString(0),
                        StatutDisponibilite = reader.GetString(1),
                        QuantiteStock = reader.GetInt32(2)
                    });
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        public static void SaveDisponibilitesForProduit(int produitId, List<ProduitDisponibilite> disponibilites)
        {
            EnsureTablesCreated();
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();

                foreach (var item in disponibilites)
                {
                    const string pdvQuery = @"
                        IF NOT EXISTS (SELECT 1 FROM PointDeVente WHERE Nom = @Nom)
                        BEGIN
                            INSERT INTO PointDeVente (Nom, Ville, Adresse) VALUES (@Nom, @Ville, @Adresse);
                        END
                        ELSE
                        BEGIN
                            UPDATE PointDeVente SET Ville = @Ville, Adresse = @Adresse WHERE Nom = @Nom;
                        END
                        SELECT PointDeVenteId FROM PointDeVente WHERE Nom = @Nom;";

                    using var cmdPdv = new SqlCommand(pdvQuery, connection);
                    cmdPdv.Parameters.AddWithValue("@Nom", item.PointDeVenteNom);
                    cmdPdv.Parameters.AddWithValue("@Ville", (object?)item.Ville ?? System.DBNull.Value);
                    cmdPdv.Parameters.AddWithValue("@Adresse", (object?)item.Adresse ?? System.DBNull.Value);
                    int pdvId = Convert.ToInt32(cmdPdv.ExecuteScalar());

                    string computedStatut = item.PointDeVenteNom.Contains("En Ligne", StringComparison.OrdinalIgnoreCase) 
                        ? "Expédition 24h" 
                        : (item.QuantiteStock <= 0 ? "Commande 48h" 
                        : (item.QuantiteStock <= 5 ? "En Arrivage" : "En Stock"));

                    const string upsertQuery = @"
                        IF EXISTS (SELECT 1 FROM ProduitPointDeVente WHERE ProduitId = @ProduitId AND PointDeVenteId = @PdvId)
                        BEGIN
                            UPDATE ProduitPointDeVente
                            SET StatutDisponibilite = @Statut, QuantiteStock = @Quantite
                            WHERE ProduitId = @ProduitId AND PointDeVenteId = @PdvId;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO ProduitPointDeVente (ProduitId, PointDeVenteId, StatutDisponibilite, QuantiteStock)
                            VALUES (@ProduitId, @PdvId, @Statut, @Quantite);
                        END";

                    using var cmdUpsert = new SqlCommand(upsertQuery, connection);
                    cmdUpsert.Parameters.AddWithValue("@ProduitId", produitId);
                    cmdUpsert.Parameters.AddWithValue("@PdvId", pdvId);
                    cmdUpsert.Parameters.AddWithValue("@Statut", computedStatut);
                    cmdUpsert.Parameters.AddWithValue("@Quantite", item.QuantiteStock);
                    cmdUpsert.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
            }
        }

        // Synchronizes store deletions: if a store is deleted from external DB, delete it locally as well!
        public static void SyncStoreDeletionsFromExternal(HashSet<string> validExternalStoreNames)
        {
            EnsureTablesCreated();
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                var localStores = GetAllPointsDeVente();

                foreach (var localStore in localStores)
                {
                    if (!validExternalStoreNames.Contains(localStore.Nom))
                    {
                        const string deleteQuery = "DELETE FROM PointDeVente WHERE PointDeVenteId = @PdvId";
                        using var cmd = new SqlCommand(deleteQuery, connection);
                        cmd.Parameters.AddWithValue("@PdvId", localStore.PointDeVenteId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
            }
        }

        // Resolves the optimal store to fulfill the order based on delivery mode, gouvernorat, ville, and client address
        public static int ResolveBestPointDeVente(int produitId, string modeLivraison, string? gouvernorat, string? ville, string? clientAdresse, int? selectedPdvId)
        {
            EnsureTablesCreated();

            // 1. Click & Collect (Retrait en Magasin)
            if (modeLivraison.Contains("Retrait", StringComparison.OrdinalIgnoreCase) && selectedPdvId.HasValue && selectedPdvId.Value > 0)
            {
                return selectedPdvId.Value;
            }

            // 2. Express Delivery (Achat En Ligne warehouse)
            if (modeLivraison.Contains("Express", StringComparison.OrdinalIgnoreCase))
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                const string query = "SELECT TOP 1 PointDeVenteId FROM PointDeVente WHERE Nom LIKE '%En Ligne%'";
                using var cmd = new SqlCommand(query, connection);
                object? res = cmd.ExecuteScalar();
                if (res != null && res != DBNull.Value) return Convert.ToInt32(res);
            }

            // 3. Home Delivery (Livraison à Domicile): Match physical retail stores by Gouvernorat, Ville, or Street Address (Excluding "Achat En Ligne" warehouse!)
            string fullSearchText = $"{gouvernorat} {ville} {clientAdresse}".ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(fullSearchText))
            {
                var stores = GetAllPointsDeVente();
                foreach (var store in stores)
                {
                    // Skip central online warehouse for Home Delivery matching
                    if (store.Nom.Contains("En Ligne", StringComparison.OrdinalIgnoreCase)) continue;

                    string storeName = store.Nom.ToLowerInvariant();
                    string storeCity = (store.Ville ?? "").ToLowerInvariant();

                    if ((!string.IsNullOrEmpty(storeCity) && fullSearchText.Contains(storeCity)) ||
                        (storeName.Contains("tunis") && fullSearchText.Contains("tunis")) ||
                        (storeName.Contains("sousse") && fullSearchText.Contains("sousse")) ||
                        (storeName.Contains("nabeul") && fullSearchText.Contains("nabeul")) ||
                        (storeName.Contains("bizerte") && fullSearchText.Contains("bizerte")) ||
                        (storeName.Contains("sfax") && fullSearchText.Contains("sfax")) ||
                        (storeName.Contains("hammamet") && fullSearchText.Contains("hammamet")) ||
                        (storeName.Contains("kairouan") && fullSearchText.Contains("kairouan")) ||
                        (storeName.Contains("béja") && fullSearchText.Contains("béja")))
                    {
                        return store.PointDeVenteId;
                    }
                }
            }

            // 4. Fallback: Pick store with highest available stock for this product
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                const string fallbackQuery = @"
                    SELECT TOP 1 PointDeVenteId
                    FROM ProduitPointDeVente
                    WHERE ProduitId = @ProduitId AND QuantiteStock > 0
                    ORDER BY QuantiteStock DESC";

                using var cmdFallback = new SqlCommand(fallbackQuery, connection);
                cmdFallback.Parameters.AddWithValue("@ProduitId", produitId);
                object? result = cmdFallback.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    return Convert.ToInt32(result);
            }
            catch
            {
            }

            // 5. Ultimate Fallback: Select the first valid existing PointDeVenteId in the database
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                const string firstPdvQuery = "SELECT TOP 1 PointDeVenteId FROM PointDeVente ORDER BY PointDeVenteId ASC";
                using var cmdFirst = new SqlCommand(firstPdvQuery, connection);
                object? firstResult = cmdFirst.ExecuteScalar();
                if (firstResult != null && firstResult != DBNull.Value)
                    return Convert.ToInt32(firstResult);
            }
            catch
            {
            }

            return 1;
        }

        // Deducts stock atomically for a specific Point de Vente in the local DB
        public static bool DeductStoreStockAtomic(int produitId, int pointDeVenteId, int quantite)
        {
            EnsureTablesCreated();
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();

                const string updateQuery = @"
                    UPDATE ppdv
                    SET QuantiteStock = QuantiteStock - @Quantite,
                        StatutDisponibilite = CASE 
                            WHEN pdv.Nom LIKE '%En Ligne%' THEN 'Expédition 24h'
                            WHEN (QuantiteStock - @Quantite) <= 0 THEN 'Commande 48h'
                            WHEN (QuantiteStock - @Quantite) BETWEEN 1 AND 5 THEN 'En Arrivage'
                            ELSE 'En Stock'
                        END
                    FROM ProduitPointDeVente ppdv
                    JOIN PointDeVente pdv ON ppdv.PointDeVenteId = pdv.PointDeVenteId
                    WHERE ppdv.ProduitId = @ProduitId AND ppdv.PointDeVenteId = @PdvId AND ppdv.QuantiteStock >= @Quantite";

                using var cmd = new SqlCommand(updateQuery, connection);
                cmd.Parameters.AddWithValue("@Quantite", quantite);
                cmd.Parameters.AddWithValue("@ProduitId", produitId);
                cmd.Parameters.AddWithValue("@PdvId", pointDeVenteId);

                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
            catch
            {
                return false;
            }
        }

        public static string? GetPointDeVenteNomById(int pdvId)
        {
            EnsureTablesCreated();
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                const string query = "SELECT Nom FROM PointDeVente WHERE PointDeVenteId = @PdvId";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@PdvId", pdvId);
                object? res = cmd.ExecuteScalar();
                return res?.ToString();
            }
            catch
            {
                return null;
            }
        }

        // Deducts stock atomically for a specific Point de Vente in the external DB by Store Name
        public static void DeductExternalStoreStockAtomic(string? referenceExterne, string nom, string storeNom, int quantite)
        {
            try
            {
                using SqlConnection connection = DatabaseHelper.GetStockConnection();
                const string updateQuery = @"
                    UPDATE ppdv
                    SET ppdv.QuantiteStock = ppdv.QuantiteStock - @Quantite,
                        ppdv.StatutDisponibilite = CASE 
                            WHEN pdv.Nom LIKE '%En Ligne%' THEN 'Expédition 24h'
                            WHEN (ppdv.QuantiteStock - @Quantite) <= 0 THEN 'Commande 48h'
                            WHEN (ppdv.QuantiteStock - @Quantite) BETWEEN 1 AND 5 THEN 'En Arrivage'
                            ELSE 'En Stock'
                        END
                    FROM ProduitPointDeVente ppdv
                    JOIN PointDeVente pdv ON ppdv.PointDeVenteId = pdv.PointDeVenteId
                    JOIN ProduitExterne pe ON (ppdv.ReferenceExterne = pe.ReferenceExterne OR pe.Nom = @Nom)
                    WHERE pdv.Nom = @StoreNom 
                      AND ((pe.ReferenceExterne = @Ref AND @Ref IS NOT NULL AND @Ref <> '') OR pe.Nom = @Nom)
                      AND ppdv.QuantiteStock >= @Quantite";

                using var cmd = new SqlCommand(updateQuery, connection);
                cmd.Parameters.AddWithValue("@Quantite", quantite);
                cmd.Parameters.AddWithValue("@StoreNom", storeNom);
                cmd.Parameters.AddWithValue("@Ref", (object?)referenceExterne ?? System.DBNull.Value);
                cmd.Parameters.AddWithValue("@Nom", nom);
                cmd.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        public static void RecalculateTotalStockForProduit(int produitId, string? refExt, string nom)
        {
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                const string query = "SELECT ISNULL(SUM(QuantiteStock), 0) FROM ProduitPointDeVente WHERE ProduitId = @ProduitId";
                using var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ProduitId", produitId);
                int localTotal = Convert.ToInt32(cmd.ExecuteScalar());
                ProduitRepository.SetExactStock(produitId, localTotal);
            }
            catch { }

            try
            {
                using SqlConnection extConn = DatabaseHelper.GetStockConnection();
                const string extQuery = @"
                    SELECT ISNULL(SUM(ppdv.QuantiteStock), 0)
                    FROM ProduitPointDeVente ppdv
                    WHERE (@Ref IS NOT NULL AND @Ref <> '' AND ppdv.ReferenceExterne = @Ref)
                       OR ((@Ref IS NULL OR @Ref = '') AND EXISTS (SELECT 1 FROM ProduitExterne pe WHERE pe.ReferenceExterne = ppdv.ReferenceExterne AND pe.Nom = @Nom))";
                using var extCmd = new SqlCommand(extQuery, extConn);
                extCmd.Parameters.AddWithValue("@Ref", (object?)refExt ?? System.DBNull.Value);
                extCmd.Parameters.AddWithValue("@Nom", nom);
                int extTotal = Convert.ToInt32(extCmd.ExecuteScalar());

                StockExterneRepository.SetExactStock(refExt, nom, extTotal);
            }
            catch { }
        }
    }
}
