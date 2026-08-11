namespace LoginRegisterApp.Models
{
    public class FamilleProduit
    {
        public int FamilleId { get; set; }
        public string Nom { get; set; } = ""; // ex. "Korean Skincare"
        public string? Description { get; set; }
    }
}
