using System;
using System.Linq;

namespace LoginRegisterApp.Models
{
    public class Contact
    {
        public int ContactId { get; set; }
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public string Telephone { get; set; } = "";
        public string? Email { get; set; }
        public string? Gouvernorat { get; set; }
        public string? Ville { get; set; }
        public string? Adresse { get; set; }

        public string FullAdresseDisplay => string.Join(", ", new[] { Adresse, Ville, Gouvernorat }.Where(s => !string.IsNullOrWhiteSpace(s)));

        // The commercial user who owns/manages this contact.
        public int CreePar { get; set; }
        public DateTime DateCreation { get; set; }
    }
}
