using System;

namespace LoginRegisterApp.Models
{
    public class Produit
    {
        public int ProduitId { get; set; }
        public int CategorieId { get; set; }
        public string Nom { get; set; } = "";
        public decimal Prix { get; set; }
        public int Stock { get; set; }
        public string? Couleur { get; set; }
        public string? Type { get; set; }
        public string? Ingredients { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Marque { get; set; }
        public string? MarqueImageUrl { get; set; }
        public string? ReferenceExterne { get; set; }
        public DateTime DateAjout { get; set; }

        public bool EstDisponible(int quantiteDemandee) => Stock >= quantiteDemandee;
    }
}
