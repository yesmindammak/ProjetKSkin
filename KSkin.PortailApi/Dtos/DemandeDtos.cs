using System.ComponentModel.DataAnnotations;

namespace LoginRegisterApp.PortailApi.Dtos
{
    public class DemandeItemRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "produitId invalide.")]
        public int ProduitId { get; set; }

        [Range(1, 10_000, ErrorMessage = "La quantité doit être supérieure à 0.")]
        public int Quantite { get; set; }
    }

    // Matches exactly the JSON body sent by src/services/api.js -> creerDemande().
    // Mirrors the fields MainWindow.xaml.cs's "Nouvelle Demande d'Achat" modal
    // collects: client identity, Gouvernorat/Ville/Adresse for the Contact,
    // and ModeLivraison/ModePaiement/PointDeVenteId for the DemandeAchat.
    public class CreerDemandeRequest
    {
        [Required, StringLength(100)]
        public string Nom { get; set; } = "";

        [Required, StringLength(100)]
        public string Prenom { get; set; } = "";

        [Required, StringLength(30)]
        public string Telephone { get; set; } = "";

        [StringLength(150)]
        public string? Email { get; set; }

        // Only required when ModeLivraison isn't a store pickup - validated
        // in DemandesController rather than with [Required] here, since the
        // rule depends on ModeLivraison.
        [StringLength(100)]
        public string? Gouvernorat { get; set; }

        [StringLength(100)]
        public string? Ville { get; set; }

        [StringLength(250)]
        public string? Adresse { get; set; }

        [Required, StringLength(100)]
        public string ModeLivraison { get; set; } = "Livraison à Domicile";

        [Required, StringLength(100)]
        public string ModePaiement { get; set; } = "Paiement à la Livraison";

        // Required only for "Retrait en Magasin" - the chosen store. For the
        // other delivery modes the API resolves the best store itself
        // (same PointDeVenteRepository.ResolveBestPointDeVente the desktop
        // app uses), so this is left null from the frontend in that case.
        public int? PointDeVenteId { get; set; }

        [Required, MinLength(1, ErrorMessage = "Le panier ne peut pas être vide.")]
        public List<DemandeItemRequest> Items { get; set; } = new();

        // Always "Portail" coming from this API - kept as a field (rather than
        // hardcoded server-side) only so the same DTO could later serve a
        // second public channel without changing its shape.
        public string Origine { get; set; } = "Portail";
    }

    public class CreerDemandeResponse
    {
        public bool Success { get; set; }
        public List<int> DemandeIds { get; set; } = new();
    }
}
