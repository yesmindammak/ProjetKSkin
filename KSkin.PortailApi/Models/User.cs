using System;

namespace LoginRegisterApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Password { get; set; } = "";
        public string? GeneratedPassword { get; set; }
        public string Role { get; set; } = "Client";
        public string StatutActivation { get; set; } = "Actif";
        public string StatutValidation { get; set; } = "NonValide";
        public DateTime DateCreation { get; set; }
        public DateTime? DateExpirationMotDePasse { get; set; }

        public bool EstActif => StatutActivation == "Actif";
        public bool EstValide => StatutValidation == "Valide";
        public bool EstAdmin => Role == "Admin";
    }
}
