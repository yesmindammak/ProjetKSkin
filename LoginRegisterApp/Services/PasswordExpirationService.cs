using System;
using LoginRegisterApp.Data;
using LoginRegisterApp.Helpers;

namespace LoginRegisterApp.Services
{
    public static class PasswordExpirationService
    {
        public static DateTime CalculerDateExpiration(DateTime dateCreation)
        {
            int dureeJours = ParametrageExpirationRepository.GetDureeValiditeJoursActuelle() ?? 90; // default if never configured
            return dateCreation.AddDays(dureeJours);
        }

        // Call this once at app startup (e.g. after an admin logs in) to catch
        // every account whose password has expired since the last run, generate
        // a new one for each, and notify them (5.2 + notification #3 from 5.3).
        public static void RegenererMotsDePasseExpires()
        {
            foreach (var (userId, username, dateCreation) in UserRepository.GetUsersWithExpiredPassword())
            {
                string nouveauMotDePasse = PasswordHelper.GenerateRandomPassword();
                string hashed = PasswordHelper.HashPassword(nouveauMotDePasse);
                DateTime nouvelleExpiration = CalculerDateExpiration(DateTime.Now);

                UserRepository.UpdatePassword(username, hashed, hashed, nouvelleExpiration);
                NotificationService.NotifierNouveauMotDePasse(userId, nouveauMotDePasse);
            }
        }
    }
}
