namespace LoginRegisterApp.Models
{
    public class CategorieProduit
    {
        public int CategorieId { get; set; }
        public int FamilleId { get; set; }
        public string Nom { get; set; } = "";
        public string? Description { get; set; }
    }
}
