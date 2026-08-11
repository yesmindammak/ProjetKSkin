namespace LoginRegisterApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        // Hash only - never store or pass around a plain-text password.
        public string Password { get; set; } = "";
        public string? GeneratedPassword { get; set; }

        public string Role { get; set; } = "Client"; // "Admin" or "Client"

        // Two independent statuses - see cahier des charges 5.1.
        public string StatutActivation { get; set; } = "Actif";   // "Actif" / "Desactive"
        public string StatutValidation { get; set; } = "NonValide"; // "Valide" / "NonValide"

        public DateTime DateCreation { get; set; }
        public DateTime? DateExpirationMotDePasse { get; set; }

        public bool EstActif => StatutActivation == "Actif";
        public bool EstValide => StatutValidation == "Valide";
        public bool EstAdmin => Role == "Admin";
    }
}
