using LoginRegisterApp.Data;
using LoginRegisterApp.Models;

namespace LoginRegisterApp.Services
{
    // One method per event listed in 5.3, so Views/other services never call
    // NotificationRepository.Create directly with hand-written strings - the
    // wording and the "who receives what" logic live in exactly one place.
    public static class NotificationService
    {
        // 1) Auto-inscription : notifie tous les admins qu'un compte attend validation.
        public static void NotifierNouveauCompteEnAttente(string usernameCree)
        {
            foreach (int adminId in UserRepository.GetAdminUserIds())
            {
                NotificationRepository.Create(
                    expediteur: "Système",
                    destinataireUserId: adminId,
                    destinataireContactId: null,
                    objet: "Nouveau compte en attente de validation",
                    contenu: $"Le compte '{usernameCree}' s'est inscrit et attend votre validation.");
            }
        }

        // 2) L'admin valide le compte : notifie l'utilisateur concerné.
        public static void NotifierCompteValide(int userId)
        {
            NotificationRepository.Create(
                expediteur: "Administrateur",
                destinataireUserId: userId,
                destinataireContactId: null,
                objet: "Votre compte a été validé",
                contenu: "Votre compte a été validé par un administrateur. Vous avez maintenant accès à toutes les fonctionnalités.");
        }

        // 3) Mot de passe régénéré automatiquement à l'expiration.
        public static void NotifierNouveauMotDePasse(int userId, string nouveauMotDePasseClair)
        {
            NotificationRepository.Create(
                expediteur: "Système",
                destinataireUserId: userId,
                destinataireContactId: null,
                objet: "Votre mot de passe a été renouvelé",
                contenu: $"Votre mot de passe a expiré. Votre nouveau mot de passe est : {nouveauMotDePasseClair}");
        }

        // 4) Création d'une demande d'achat : notifie le client final ET le(s) superviseur(s)
        //    d'achat - c'est leur rôle dédié depuis 5.3 ("responsable/superviseur des achats").
        public static void NotifierDemandeCreee(DemandeAchat demande, string nomUtilisateur)
        {
            NotificationRepository.Create(
                expediteur: nomUtilisateur,
                destinataireUserId: null,
                destinataireContactId: demande.ContactId,
                objet: "Votre demande d'achat a été enregistrée",
                contenu: $"Votre demande pour {demande.Quantite} unité(s) a bien été enregistrée. Nous revenons vers vous rapidement.");

            foreach (int superviseurId in UserRepository.GetSuperviseurAchatUserIds())
            {
                NotificationRepository.Create(
                    expediteur: nomUtilisateur,
                    destinataireUserId: superviseurId,
                    destinataireContactId: null,
                    objet: "Nouvelle demande d'achat créée",
                    contenu: $"L'utilisateur commercial '{nomUtilisateur}' a créé une demande d'achat pour le produit #{demande.ProduitId} (quantité : {demande.Quantite}).");
            }
        }

        // 5) Clôture d'une demande d'achat : message de remerciement au client, info au superviseur d'achat.
        public static void NotifierDemandeCloturee(DemandeAchat demande, string nomUtilisateur)
        {
            NotificationRepository.Create(
                expediteur: nomUtilisateur,
                destinataireUserId: null,
                destinataireContactId: demande.ContactId,
                objet: "Merci pour votre confiance",
                contenu: "Votre demande d'achat a été clôturée. Merci pour votre confiance !");

            foreach (int superviseurId in UserRepository.GetSuperviseurAchatUserIds())
            {
                NotificationRepository.Create(
                    expediteur: nomUtilisateur,
                    destinataireUserId: superviseurId,
                    destinataireContactId: null,
                    objet: "Demande d'achat clôturée",
                    contenu: $"La demande créée par {nomUtilisateur} a été clôturée.");
            }
        }

        // 6) Nouveau produit (pas juste un changement de stock) : notifie tous les utilisateurs actifs (y compris superviseurs).
        public static void NotifierNouveauProduit(Produit produit)
        {
            var activeUserIds = UserRepository.GetActiveUserIds();
            foreach (int userId in activeUserIds)
            {
                NotificationRepository.Create(
                    expediteur: "Système",
                    destinataireUserId: userId,
                    destinataireContactId: null,
                    objet: "Nouveau produit disponible",
                    contenu: $"Le produit '{produit.Nom}' vient d'être ajouté au catalogue.");
            }
        }
    }
}
