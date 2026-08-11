namespace LoginRegisterApp.Models
{
    public class PointDeVente
    {
        public int PointDeVenteId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Ville { get; set; }
        public string? Adresse { get; set; }
    }

    public class ProduitDisponibilite
    {
        public string PointDeVenteNom { get; set; } = string.Empty;
        public string? Ville { get; set; }
        public string? Adresse { get; set; }
        public string StatutDisponibilite { get; set; } = "En Stock"; // En Stock, En Arrivage, Commande 48h, Expédition 24h
        public int QuantiteStock { get; set; } = 0;

        public string BadgeColorHex => StatutDisponibilite switch
        {
            "En Stock" => "#2E7D32",         // Soft Green
            "En Arrivage" => "#1976D2",      // Soft Blue
            "Expédition 24h" => "#2E7D32",   // Green
            "Commande 48h" => "#E65100",     // Orange
            _ => "#757575"
        };
    }
}
