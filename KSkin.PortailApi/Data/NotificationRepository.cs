using System.Collections.Generic;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Models;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    public static class NotificationRepository
    {
        // Exactly one of destinataireUserId / destinataireContactId must be set -
        // matches the CHECK constraint on the Notification table. Prefer calling
        // this through NotificationService rather than directly, so the six
        // trigger points from 5.3 stay in one place.
        public static void Create(string expediteur, int? destinataireUserId, int? destinataireContactId,
            string objet, string contenu, string? pieceJointe = null)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                INSERT INTO Notification (Expediteur, DestinataireUserId, DestinataireContactId, Objet, Contenu, PieceJointe)
                VALUES (@Expediteur, @DestinataireUserId, @DestinataireContactId, @Objet, @Contenu, @PieceJointe)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Expediteur", expediteur);
            command.Parameters.AddWithValue("@DestinataireUserId", (object?)destinataireUserId ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@DestinataireContactId", (object?)destinataireContactId ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Objet", objet);
            command.Parameters.AddWithValue("@Contenu", contenu);
            command.Parameters.AddWithValue("@PieceJointe", (object?)pieceJointe ?? System.DBNull.Value);
            command.ExecuteNonQuery();
        }

        public static List<Notification> GetForUser(int userId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT NotificationId, Expediteur, DestinataireUserId, DestinataireContactId, Objet, Contenu, PieceJointe, DateEnvoi, Lu
                FROM Notification WHERE DestinataireUserId = @UserId ORDER BY DateEnvoi DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            using SqlDataReader reader = command.ExecuteReader();

            var notifications = new List<Notification>();
            while (reader.Read())
            {
                notifications.Add(new Notification
                {
                    NotificationId = reader.GetInt32(0),
                    Expediteur = reader.GetString(1),
                    DestinataireUserId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    DestinataireContactId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Objet = reader.GetString(4),
                    Contenu = reader.GetString(5),
                    PieceJointe = reader.IsDBNull(6) ? null : reader.GetString(6),
                    DateEnvoi = reader.GetDateTime(7),
                    Lu = reader.GetBoolean(8),
                });
            }
            return notifications;
        }

        public class NotificationDisplay
        {
            public int NotificationId { get; set; }
            public string Expediteur { get; set; } = "";
            public string DestinataireNom { get; set; } = "";
            public string Objet { get; set; } = "";
            public string Contenu { get; set; } = "";
            public DateTime DateEnvoi { get; set; }
            public bool Lu { get; set; }
        }

        public static List<NotificationDisplay> GetForUserDisplay(int userId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT n.NotificationId, n.Expediteur,
                       COALESCE(u.Name, u.Username, CONCAT(c.Nom, ' ', c.Prenom), 'Destinataire') AS DestinataireNom,
                       n.Objet, n.Contenu, n.DateEnvoi, n.Lu
                FROM Notification n
                LEFT JOIN Users u ON n.DestinataireUserId = u.UserId
                LEFT JOIN Contact c ON n.DestinataireContactId = c.ContactId
                WHERE n.DestinataireUserId = @UserId OR @UserId IS NULL
                ORDER BY n.DateEnvoi DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            using SqlDataReader reader = command.ExecuteReader();

            var list = new List<NotificationDisplay>();
            while (reader.Read())
            {
                list.Add(new NotificationDisplay
                {
                    NotificationId = reader.GetInt32(0),
                    Expediteur = reader.GetString(1),
                    DestinataireNom = reader.GetString(2),
                    Objet = reader.GetString(3),
                    Contenu = reader.GetString(4),
                    DateEnvoi = reader.GetDateTime(5),
                    Lu = reader.GetBoolean(6),
                });
            }
            return list;
        }

        public static void MarkAsRead(int notificationId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "UPDATE Notification SET Lu = 1 WHERE NotificationId = @Id";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", notificationId);
            command.ExecuteNonQuery();
        }
    }
}
