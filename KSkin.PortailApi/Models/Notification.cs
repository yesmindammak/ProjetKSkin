namespace LoginRegisterApp.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public string Expediteur { get; set; } = "";

        // Exactly one of these two is set - the DB enforces this with a CHECK constraint.
        public int? DestinataireUserId { get; set; }
        public int? DestinataireContactId { get; set; }

        public string Objet { get; set; } = "";
        public string Contenu { get; set; } = "";
        public string? PieceJointe { get; set; }
        public DateTime DateEnvoi { get; set; }
        public bool Lu { get; set; }
    }
}
