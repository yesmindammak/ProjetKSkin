namespace LoginRegisterApp.Models
{
    public class ParametrageExpiration
    {
        public int ParametrageId { get; set; }
        public int DureeValiditeJours { get; set; }
        public DateTime DateModification { get; set; }
        public int ModifiePar { get; set; }

        public DateTime CalculerDateExpiration(DateTime dateCreationUtilisateur) =>
            dateCreationUtilisateur.AddDays(DureeValiditeJours);
    }
}
