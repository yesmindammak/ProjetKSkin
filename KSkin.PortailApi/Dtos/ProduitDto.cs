namespace LoginRegisterApp.PortailApi.Dtos
{
    // System.Text.Json (ASP.NET Core's default serializer) automatically
    // camel-cases these property names in the JSON response, so ProduitId
    // becomes "produitId" - which is exactly the shape src/services/api.js
    // expects on the frontend. No manual mapping needed on either side.
    public class ProduitDto
    {
        public int ProduitId { get; set; }
        public string Nom { get; set; } = "";
        public decimal Prix { get; set; }
        public int Stock { get; set; }
        public string? Marque { get; set; }
        public string? MarqueImageUrl { get; set; }
        public string? CategorieNom { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string? Ingredients { get; set; }
        public List<ProduitDisponibiliteDto>? Disponibilites { get; set; }
    }

    public class ProduitDisponibiliteDto
    {
        public string PointDeVenteNom { get; set; } = "";
        public string StatutDisponibilite { get; set; } = "";
    }

    public class MarqueDto
    {
        public string Nom { get; set; } = "";
        public string? ImageUrl { get; set; }
    }
}
