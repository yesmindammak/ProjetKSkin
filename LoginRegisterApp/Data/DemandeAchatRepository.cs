using System;
using System.Collections.Generic;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Models;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    public static class DemandeAchatRepository
    {
        public static int Create(DemandeAchat demande)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                INSERT INTO DemandeAchat (UtilisateurId, ContactId, ProduitId, Quantite, Origine, Statut, ModeLivraison, ModePaiement, PointDeVenteId)
                OUTPUT INSERTED.DemandeId
                VALUES (@UtilisateurId, @ContactId, @ProduitId, @Quantite, @Origine, 'EnAttente', @ModeLivraison, @ModePaiement, @PointDeVenteId)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UtilisateurId", demande.UtilisateurId);
            command.Parameters.AddWithValue("@ContactId", demande.ContactId);
            command.Parameters.AddWithValue("@ProduitId", demande.ProduitId);
            command.Parameters.AddWithValue("@Quantite", demande.Quantite);
            command.Parameters.AddWithValue("@Origine", demande.Origine ?? "Desktop");
            command.Parameters.AddWithValue("@ModeLivraison", (object?)demande.ModeLivraison ?? "Livraison à Domicile");
            command.Parameters.AddWithValue("@ModePaiement", (object?)demande.ModePaiement ?? "Paiement à la Livraison");
            command.Parameters.AddWithValue("@PointDeVenteId", (object?)demande.PointDeVenteId ?? System.DBNull.Value);

            return (int)command.ExecuteScalar();
        }

        public static List<DemandeAchat> GetByUser(int userId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT DemandeId, UtilisateurId, ContactId, ProduitId, Quantite, Origine, Statut, DateDemande, DateCloture,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'ModeLivraison') IS NOT NULL THEN ModeLivraison ELSE 'Livraison à Domicile' END AS ModeLivraison,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'ModePaiement') IS NOT NULL THEN ModePaiement ELSE 'Paiement à la Livraison' END AS ModePaiement,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'PointDeVenteId') IS NOT NULL THEN PointDeVenteId ELSE NULL END AS PointDeVenteId
                FROM DemandeAchat WHERE UtilisateurId = @UserId ORDER BY DateDemande DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            using SqlDataReader reader = command.ExecuteReader();

            var demandes = new List<DemandeAchat>();
            while (reader.Read())
            {
                demandes.Add(new DemandeAchat
                {
                    DemandeId = reader.GetInt32(0),
                    UtilisateurId = reader.GetInt32(1),
                    ContactId = reader.GetInt32(2),
                    ProduitId = reader.GetInt32(3),
                    Quantite = reader.GetInt32(4),
                    Origine = reader.GetString(5),
                    Statut = reader.GetString(6),
                    DateDemande = reader.GetDateTime(7),
                    DateCloture = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    ModeLivraison = reader.IsDBNull(9) ? "Livraison à Domicile" : reader.GetString(9),
                    ModePaiement = reader.IsDBNull(10) ? "Paiement à la Livraison" : reader.GetString(10),
                    PointDeVenteId = reader.IsDBNull(11) ? null : reader.GetInt32(11)
                });
            }
            return demandes;
        }

        public static void SetStatut(int demandeId, string nouveauStatut)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "UPDATE DemandeAchat SET Statut = @Statut WHERE DemandeId = @Id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Statut", nouveauStatut);
            command.Parameters.AddWithValue("@Id", demandeId);
            command.ExecuteNonQuery();
        }

        public static DemandeAchat? GetById(int demandeId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT DemandeId, UtilisateurId, ContactId, ProduitId, Quantite, Origine, Statut, DateDemande, DateCloture,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'ModeLivraison') IS NOT NULL THEN ModeLivraison ELSE 'Livraison à Domicile' END AS ModeLivraison,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'ModePaiement') IS NOT NULL THEN ModePaiement ELSE 'Paiement à la Livraison' END AS ModePaiement,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'PointDeVenteId') IS NOT NULL THEN PointDeVenteId ELSE NULL END AS PointDeVenteId
                FROM DemandeAchat WHERE DemandeId = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", demandeId);
            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read()) return null;

            return new DemandeAchat
            {
                DemandeId = reader.GetInt32(0),
                UtilisateurId = reader.GetInt32(1),
                ContactId = reader.GetInt32(2),
                ProduitId = reader.GetInt32(3),
                Quantite = reader.GetInt32(4),
                Origine = reader.GetString(5),
                Statut = reader.GetString(6),
                DateDemande = reader.GetDateTime(7),
                DateCloture = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                ModeLivraison = reader.IsDBNull(9) ? "Livraison à Domicile" : reader.GetString(9),
                ModePaiement = reader.IsDBNull(10) ? "Paiement à la Livraison" : reader.GetString(10),
                PointDeVenteId = reader.IsDBNull(11) ? null : reader.GetInt32(11)
            };
        }

        public static List<DemandeAchatDisplay> GetAllDisplay()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT d.DemandeId, d.UtilisateurId, d.ContactId, d.ProduitId, d.Quantite, d.Origine, d.Statut, d.DateDemande, d.DateCloture,
                       COALESCE(u.Name, u.Username, 'Portail Client') AS UtilisateurNom,
                       COALESCE(c.Nom + ' ' + c.Prenom + ' (' + c.Telephone + ')', 'Client #' + CAST(d.ContactId AS VARCHAR)) AS ContactNom,
                       COALESCE(p.Nom, 'Produit #' + CAST(d.ProduitId AS VARCHAR)) AS ProduitNom,
                       COALESCE(p.Prix, 0) AS ProduitPrix,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'ModeLivraison') IS NOT NULL THEN d.ModeLivraison ELSE 'Livraison à Domicile' END AS ModeLivraison,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'ModePaiement') IS NOT NULL THEN d.ModePaiement ELSE 'Paiement à la Livraison' END AS ModePaiement,
                       COALESCE(pdv.Nom, 'Achat En Ligne') AS PointDeVenteNom,
                       d.PointDeVenteId
                FROM DemandeAchat d
                LEFT JOIN Users u ON d.UtilisateurId = u.UserId
                LEFT JOIN Contact c ON d.ContactId = c.ContactId
                LEFT JOIN Produit p ON d.ProduitId = p.ProduitId
                LEFT JOIN PointDeVente pdv ON d.PointDeVenteId = pdv.PointDeVenteId
                ORDER BY d.DateDemande DESC";

            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var list = new List<DemandeAchatDisplay>();
            while (reader.Read())
            {
                list.Add(new DemandeAchatDisplay
                {
                    DemandeId = reader.GetInt32(0),
                    UtilisateurId = reader.GetInt32(1),
                    ContactId = reader.GetInt32(2),
                    ProduitId = reader.GetInt32(3),
                    Quantite = reader.GetInt32(4),
                    Origine = reader.GetString(5),
                    Statut = reader.GetString(6),
                    DateDemande = reader.GetDateTime(7),
                    DateCloture = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    UtilisateurNom = reader.GetString(9),
                    ContactNom = reader.GetString(10),
                    ProduitNom = reader.GetString(11),
                    ProduitPrix = reader.GetDecimal(12),
                    ModeLivraison = reader.IsDBNull(13) ? "Livraison à Domicile" : reader.GetString(13),
                    ModePaiement = reader.IsDBNull(14) ? "Paiement à la Livraison" : reader.GetString(14),
                    PointDeVenteNom = reader.GetString(15),
                    PointDeVenteId = reader.IsDBNull(16) ? null : reader.GetInt32(16)
                });
            }
            return list;
        }

        public static List<DemandeAchatDisplay> GetByUserDisplay(int userId)
        {
            return GetAllDisplayForUserAndPortal(userId);
        }

        public static List<DemandeAchatDisplay> GetAllDisplayForUserAndPortal(int userId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT d.DemandeId, d.UtilisateurId, d.ContactId, d.ProduitId, d.Quantite, d.Origine, d.Statut, d.DateDemande, d.DateCloture,
                       COALESCE(u.Name, u.Username, 'Portail Client') AS UtilisateurNom,
                       COALESCE(c.Nom + ' ' + c.Prenom + ' (' + c.Telephone + ')', 'Client #' + CAST(d.ContactId AS VARCHAR)) AS ContactNom,
                       COALESCE(p.Nom, 'Produit #' + CAST(d.ProduitId AS VARCHAR)) AS ProduitNom,
                       COALESCE(p.Prix, 0) AS ProduitPrix,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'ModeLivraison') IS NOT NULL THEN d.ModeLivraison ELSE 'Livraison à Domicile' END AS ModeLivraison,
                       CASE WHEN COL_LENGTH('DemandeAchat', 'ModePaiement') IS NOT NULL THEN d.ModePaiement ELSE 'Paiement à la Livraison' END AS ModePaiement,
                       COALESCE(pdv.Nom, 'Achat En Ligne') AS PointDeVenteNom,
                       d.PointDeVenteId
                FROM DemandeAchat d
                LEFT JOIN Users u ON d.UtilisateurId = u.UserId
                LEFT JOIN Contact c ON d.ContactId = c.ContactId
                LEFT JOIN Produit p ON d.ProduitId = p.ProduitId
                LEFT JOIN PointDeVente pdv ON d.PointDeVenteId = pdv.PointDeVenteId
                WHERE d.UtilisateurId = @UserId OR d.Origine = 'Portail'
                ORDER BY d.DateDemande DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            using SqlDataReader reader = command.ExecuteReader();

            var list = new List<DemandeAchatDisplay>();
            while (reader.Read())
            {
                list.Add(new DemandeAchatDisplay
                {
                    DemandeId = reader.GetInt32(0),
                    UtilisateurId = reader.GetInt32(1),
                    ContactId = reader.GetInt32(2),
                    ProduitId = reader.GetInt32(3),
                    Quantite = reader.GetInt32(4),
                    Origine = reader.GetString(5),
                    Statut = reader.GetString(6),
                    DateDemande = reader.GetDateTime(7),
                    DateCloture = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                    UtilisateurNom = reader.GetString(9),
                    ContactNom = reader.GetString(10),
                    ProduitNom = reader.GetString(11),
                    ProduitPrix = reader.GetDecimal(12),
                    ModeLivraison = reader.IsDBNull(13) ? "Livraison à Domicile" : reader.GetString(13),
                    ModePaiement = reader.IsDBNull(14) ? "Paiement à la Livraison" : reader.GetString(14),
                    PointDeVenteNom = reader.GetString(15),
                    PointDeVenteId = reader.IsDBNull(16) ? null : reader.GetInt32(16)
                });
            }
            return list;
        }

        public static void Cloturer(int demandeId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                UPDATE DemandeAchat
                SET Statut = 'Cloturee', DateCloture = @DateCloture
                WHERE DemandeId = @Id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@DateCloture", DateTime.Now);
            command.Parameters.AddWithValue("@Id", demandeId);
            command.ExecuteNonQuery();
        }
    }
}
