using System;
using System.Collections.Generic;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Models;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    // All direct SQL for the Users table lives here. Views call this instead
    // of writing SqlCommand/SqlConnection code themselves.
    public static class UserRepository
    {
        public static bool ExistsByUsernameEmailOrPhone(string username, string email, string phone)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT COUNT(*) FROM Users
                WHERE Username = @Username OR Email = @Email OR PhoneNumber = @Phone";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Phone", phone);
            return (int)command.ExecuteScalar() > 0;
        }

        // Admin-created accounts (5.1): Actif + Valide directly, unlike self-registration
        // which lands as Actif + NonValide. role must be "Admin", "SuperviseurAchat" or "Client".
        public static void Create(string username, string name, string email, string phone,
            string role, string hashedPassword)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                INSERT INTO Users
                    (Username, Name, Password, PhoneNumber, GeneratedPassword, Email, Role, StatutActivation, StatutValidation)
                VALUES
                    (@Username, @Name, @Password, @Phone, @GeneratedPassword, @Email, @Role, 'Actif', 'Valide')";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Password", hashedPassword);
            command.Parameters.AddWithValue("@Phone", phone);
            command.Parameters.AddWithValue("@GeneratedPassword", hashedPassword); // always system-generated here
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@Role", role);
            command.ExecuteNonQuery();
        }

        public static List<UserRow> GetAll()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();

            const string query = @"
                SELECT UserId, Username, Name, Email, PhoneNumber, Role, StatutActivation, StatutValidation
                FROM Users
                ORDER BY Name";

            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var rows = new List<UserRow>();
            while (reader.Read())
            {
                rows.Add(new UserRow
                {
                    UserId = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Name = reader.GetString(2),
                    Email = reader.GetString(3),
                    PhoneNumber = reader.GetString(4),
                    Role = reader.GetString(5),
                    StatutActivation = reader.GetString(6),
                    StatutValidation = reader.GetString(7),
                });
            }
            return rows;
        }

        // newStatut must be "Actif" or "Desactive".
        public static void SetStatutActivation(string username, string newStatut)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "UPDATE Users SET StatutActivation = @Statut WHERE Username = @Username";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Statut", newStatut);
            command.Parameters.AddWithValue("@Username", username);
            command.ExecuteNonQuery();
        }

        // newStatut must be "Valide" or "NonValide".
        public static void SetStatutValidation(string username, string newStatut)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "UPDATE Users SET StatutValidation = @Statut WHERE Username = @Username";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Statut", newStatut);
            command.Parameters.AddWithValue("@Username", username);
            command.ExecuteNonQuery();
        }

        // Used by UserRepository/AuthService to look up the internal UserId
        // when a repository elsewhere (Notification, Contact...) needs to
        // reference "who did this" by id rather than by username.
        // Used by NotificationService for demande-d'achat events - the "responsable/
        // superviseur des achats" from 5.3 is now a real role, distinct from Admin.
        public static List<int> GetSuperviseurAchatUserIds()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "SELECT UserId FROM Users WHERE (Role = 'SuperviseurAchat' OR Role LIKE '%Superviseur%' OR Role = 'Admin') AND LOWER(StatutActivation) = 'actif'";
            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var ids = new List<int>();
            while (reader.Read()) ids.Add(reader.GetInt32(0));
            return ids;
        }

        // Used by NotificationService to fan out "nouveau compte en attente"
        // to every admin (5.3).
        public static List<int> GetAdminUserIds()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "SELECT UserId FROM Users WHERE Role = 'Admin' AND StatutActivation = 'Actif'";
            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var ids = new List<int>();
            while (reader.Read()) ids.Add(reader.GetInt32(0));
            return ids;
        }

        // Used by NotificationService when a brand new product is added -
        // "l'ensemble des utilisateurs actifs" (5.3).
        public static List<int> GetActiveUserIds()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "SELECT UserId FROM Users WHERE LOWER(StatutActivation) = 'actif'";
            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var ids = new List<int>();
            while (reader.Read()) ids.Add(reader.GetInt32(0));
            return ids;
        }

        // Used when an admin creates a user directly, and when a password is
        // regenerated after expiration.
        public static void UpdatePassword(string username, string hashedPassword, string? hashedGeneratedPassword,
            DateTime? nouvelleDateExpiration)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                UPDATE Users
                SET Password = @Password, GeneratedPassword = @GeneratedPassword, DateExpirationMotDePasse = @Expiration
                WHERE Username = @Username";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Password", hashedPassword);
            command.Parameters.AddWithValue("@GeneratedPassword", (object?)hashedGeneratedPassword ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Expiration", (object?)nouvelleDateExpiration ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Username", username);
            command.ExecuteNonQuery();
        }

        // Every user whose expiration date has passed - PasswordExpirationService
        // regenerates a password for each of them.
        public static List<(int UserId, string Username, DateTime DateCreation)> GetUsersWithExpiredPassword()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT UserId, Username, DateCreation FROM Users
                WHERE DateExpirationMotDePasse IS NOT NULL AND DateExpirationMotDePasse <= GETDATE()";
            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var results = new List<(int, string, DateTime)>();
            while (reader.Read())
                results.Add((reader.GetInt32(0), reader.GetString(1), reader.GetDateTime(2)));
            return results;
        }

        public static int GetUserId(string username)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "SELECT UserId FROM Users WHERE Username = @Username";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            return (int)command.ExecuteScalar();
        }
    }
}
