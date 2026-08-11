using System;

namespace LoginRegisterApp.Models
{
    public class DemandeAchat
    {
        public int DemandeId { get; set; }
        public int UtilisateurId { get; set; }
        public int ContactId { get; set; }
        public int ProduitId { get; set; }
        public int? PointDeVenteId { get; set; }
        public int Quantite { get; set; }
        public string Origine { get; set; } = "Desktop";
        public string Statut { get; set; } = "EnAttenteValidation";
        public string? ModeLivraison { get; set; } = "Livraison à Domicile";
        public string? ModePaiement { get; set; } = "Paiement à la Livraison";
        public DateTime DateDemande { get; set; } = DateTime.Now;
        public DateTime? DateCloture { get; set; }
    }
}