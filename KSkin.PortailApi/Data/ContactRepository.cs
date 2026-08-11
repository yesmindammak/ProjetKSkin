using System.Collections.Generic;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.Models;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    public static class ContactRepository
    {
        public static void EnsureColumnsExist()
        {
            try
            {
                using SqlConnection connection = DatabaseHelper.GetConnection();
                const string query = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Contact') AND name = 'Gouvernorat')
                    BEGIN
                        ALTER TABLE Contact ADD Gouvernorat NVARCHAR(100) NULL;
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Contact') AND name = 'Ville')
                    BEGIN
                        ALTER TABLE Contact ADD Ville NVARCHAR(100) NULL;
                    END";
                using var cmd = new SqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        public static List<Contact> GetByUser(int userId)
        {
            EnsureColumnsExist();
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT ContactId, Nom, Prenom, Telephone, Email, Adresse, CreePar, DateCreation,
                       CASE WHEN COL_LENGTH('Contact', 'Gouvernorat') IS NOT NULL THEN Gouvernorat ELSE NULL END AS Gouvernorat,
                       CASE WHEN COL_LENGTH('Contact', 'Ville') IS NOT NULL THEN Ville ELSE NULL END AS Ville
                FROM Contact WHERE CreePar = @UserId ORDER BY Nom";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            using SqlDataReader reader = command.ExecuteReader();

            var contacts = new List<Contact>();
            while (reader.Read())
            {
                contacts.Add(new Contact
                {
                    ContactId = reader.GetInt32(0),
                    Nom = reader.GetString(1),
                    Prenom = reader.GetString(2),
                    Telephone = reader.GetString(3),
                    Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Adresse = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CreePar = reader.GetInt32(6),
                    DateCreation = reader.GetDateTime(7),
                    Gouvernorat = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Ville = reader.IsDBNull(9) ? null : reader.GetString(9),
                });
            }
            return contacts;
        }

        public static Contact? FindByPhone(string telephone)
        {
            EnsureColumnsExist();
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT ContactId, Nom, Prenom, Telephone, Email, Adresse, CreePar, DateCreation,
                       CASE WHEN COL_LENGTH('Contact', 'Gouvernorat') IS NOT NULL THEN Gouvernorat ELSE NULL END AS Gouvernorat,
                       CASE WHEN COL_LENGTH('Contact', 'Ville') IS NOT NULL THEN Ville ELSE NULL END AS Ville
                FROM Contact WHERE Telephone = @Telephone";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Telephone", telephone);
            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read()) return null;

            return new Contact
            {
                ContactId = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Prenom = reader.GetString(2),
                Telephone = reader.GetString(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                Adresse = reader.IsDBNull(5) ? null : reader.GetString(5),
                CreePar = reader.GetInt32(6),
                DateCreation = reader.GetDateTime(7),
                Gouvernorat = reader.IsDBNull(8) ? null : reader.GetString(8),
                Ville = reader.IsDBNull(9) ? null : reader.GetString(9),
            };
        }

        public static Contact? FindByName(string nom, string prenom)
        {
            EnsureColumnsExist();
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT ContactId, Nom, Prenom, Telephone, Email, Adresse, CreePar, DateCreation,
                       CASE WHEN COL_LENGTH('Contact', 'Gouvernorat') IS NOT NULL THEN Gouvernorat ELSE NULL END AS Gouvernorat,
                       CASE WHEN COL_LENGTH('Contact', 'Ville') IS NOT NULL THEN Ville ELSE NULL END AS Ville
                FROM Contact WHERE LOWER(Nom) = LOWER(@Nom) AND LOWER(Prenom) = LOWER(@Prenom)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nom", nom);
            command.Parameters.AddWithValue("@Prenom", prenom);
            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read()) return null;

            return new Contact
            {
                ContactId = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Prenom = reader.GetString(2),
                Telephone = reader.GetString(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                Adresse = reader.IsDBNull(5) ? null : reader.GetString(5),
                CreePar = reader.GetInt32(6),
                DateCreation = reader.GetDateTime(7),
                Gouvernorat = reader.IsDBNull(8) ? null : reader.GetString(8),
                Ville = reader.IsDBNull(9) ? null : reader.GetString(9),
            };
        }

        public static int Create(Contact contact)
        {
            EnsureColumnsExist();
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                INSERT INTO Contact (Nom, Prenom, Telephone, Email, Adresse, Gouvernorat, Ville, CreePar)
                OUTPUT INSERTED.ContactId
                VALUES (@Nom, @Prenom, @Telephone, @Email, @Adresse, @Gouvernorat, @Ville, @CreePar)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Nom", contact.Nom);
            command.Parameters.AddWithValue("@Prenom", contact.Prenom);
            command.Parameters.AddWithValue("@Telephone", contact.Telephone);
            command.Parameters.AddWithValue("@Email", (object?)contact.Email ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Adresse", (object?)contact.Adresse ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Gouvernorat", (object?)contact.Gouvernorat ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@Ville", (object?)contact.Ville ?? System.DBNull.Value);
            command.Parameters.AddWithValue("@CreePar", contact.CreePar);

            return (int)command.ExecuteScalar();
        }
    }
}
