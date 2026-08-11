using System;
using LoginRegisterApp.Helpers;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.Data
{
    public static class ParametrageExpirationRepository
    {
        // The current setting is simply the most recently modified row (5.2).
        // Returns null if the admin hasn't configured anything yet.
        public static int? GetDureeValiditeJoursActuelle()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "SELECT TOP 1 DureeValiditeJours FROM ParametrageExpiration ORDER BY DateModification DESC";
            using var command = new SqlCommand(query, connection);
            object result = command.ExecuteScalar();
            return result == null ? null : (int)result;
        }

        public static void DefinirDureeExpiration(int dureeJours, int adminUserId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                INSERT INTO ParametrageExpiration (DureeValiditeJours, ModifiePar)
                VALUES (@Duree, @AdminId)";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Duree", dureeJours);
            command.Parameters.AddWithValue("@AdminId", adminUserId);
            command.ExecuteNonQuery();
        }
    }
}
