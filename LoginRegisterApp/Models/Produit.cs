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
        public string? ImageUrl { get; set; } // chemin ou URL vers la photo du produit, pour le catalogue
        public string? Marque { get; set; } // nom de la marque (ex. Purito, Innisfree, Numbuzin)
        public string? MarqueImageUrl { get; set; } // photo/logo de la marque
        public string? ReferenceExterne { get; set; } // cle de rapprochement avec la base externe

        // Used to detect a "brand new product" (triggers a notification to all active users)
        // rather than a routine stock update on an existing one.
        public DateTime DateAjout { get; set; }

        public bool EstDisponible(int quantiteDemandee) => Stock >= quantiteDemandee;
    }
}
