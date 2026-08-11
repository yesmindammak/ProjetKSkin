using System;

namespace LoginRegisterApp.Models
{
    public class DemandeAchatDisplay
    {
        public int DemandeId { get; set; }
        public int UtilisateurId { get; set; }
        public int ContactId { get; set; }
        public int ProduitId { get; set; }
        public int? PointDeVenteId { get; set; }
        public int Quantite { get; set; }
        public string Origine { get; set; } = "Desktop";
        public string Statut { get; set; } = "EnAttente";
        public string ModeLivraison { get; set; } = "Livraison à Domicile";
        public string ModePaiement { get; set; } = "Paiement à la Livraison";
        public string PointDeVenteNom { get; set; } = "Magasin Principal";
        public DateTime DateDemande { get; set; }
        public DateTime? DateCloture { get; set; }

        // Display properties for UI DataGrid
        public string UtilisateurNom { get; set; } = "";
        public string ContactNom { get; set; } = "";
        public string ProduitNom { get; set; } = "";
        public decimal ProduitPrix { get; set; }
        public decimal TotalPrix => ProduitPrix * Quantite;

        // Visual helper properties (matches EnAttente & EnAttenteValidation case-insensitively)
        public bool CanBeValidatedOrRefused => string.Equals(Statut, "EnAttente", StringComparison.OrdinalIgnoreCase) || string.Equals(Statut, "EnAttenteValidation", StringComparison.OrdinalIgnoreCase);
        public bool CanBeClosed => string.Equals(Statut, "Validee", StringComparison.OrdinalIgnoreCase) || string.Equals(Statut, "EnAttente", StringComparison.OrdinalIgnoreCase) || string.Equals(Statut, "EnAttenteValidation", StringComparison.OrdinalIgnoreCase);
    }
}
