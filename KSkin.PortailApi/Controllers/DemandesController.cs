using LoginRegisterApp.Data;
using LoginRegisterApp.Models;
using LoginRegisterApp.PortailApi.Data;
using LoginRegisterApp.PortailApi.Dtos;
using LoginRegisterApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegisterApp.PortailApi.Controllers
{
    [ApiController]
    [Route("api/demandes")]
    public class DemandesController : ControllerBase
    {
        // See README - Contact.CreePar / DemandeAchat.UtilisateurId both need a
        // real UserId, but portal visitors aren't logged in, so every portal
        // request is attributed to one seeded "system" user.
        private readonly int _systemUserId;

        public DemandesController(IConfiguration configuration)
        {
            _systemUserId = configuration.GetValue<int>("Portail:SystemUserId");
        }

        [HttpPost]
        public IActionResult CreerDemande([FromBody] CreerDemandeRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            bool isRetrait = request.ModeLivraison.StartsWith("Retrait", StringComparison.OrdinalIgnoreCase);

            // Cross-field rules the [Required] attributes on the DTO can't express
            // on their own, because which fields are mandatory depends on
            // ModeLivraison - same split MainWindow.xaml.cs makes in its modal.
            if (isRetrait && request.PointDeVenteId is null)
                return BadRequest("Veuillez choisir un magasin pour le retrait en magasin.");

            if (!isRetrait && string.IsNullOrWhiteSpace(request.Ville))
                return BadRequest("La ville est requise pour la livraison.");

            // 1) Verify every product in the cart actually exists before writing
            //    anything - fail the whole request rather than create a partial
            //    order the client never asked for.
            foreach (var item in request.Items)
            {
                if (ProduitPortailRepository.GetById(item.ProduitId) is null)
                    return BadRequest($"Produit introuvable (id {item.ProduitId}).");
            }

            // 2) Find or create the Contact (the final client) by phone number,
            //    now carrying Gouvernorat/Ville alongside Adresse - same
            //    "vérifier ou créer contact" step the desktop app's own modal
            //    runs (ClientContactInput_Changed / SubmitDemande_Click).
            Contact? contact = ContactRepository.FindByPhone(request.Telephone.Trim());
            int contactId;
            if (contact is null)
            {
                contactId = ContactRepository.Create(new Contact
                {
                    Nom = request.Nom.Trim(),
                    Prenom = request.Prenom.Trim(),
                    Telephone = request.Telephone.Trim(),
                    Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                    Gouvernorat = string.IsNullOrWhiteSpace(request.Gouvernorat) ? null : request.Gouvernorat.Trim(),
                    Ville = string.IsNullOrWhiteSpace(request.Ville) ? null : request.Ville.Trim(),
                    Adresse = string.IsNullOrWhiteSpace(request.Adresse) ? null : request.Adresse.Trim(),
                    CreePar = _systemUserId,
                });
            }
            else
            {
                contactId = contact.ContactId;
            }

            // 3) One DemandeAchat row per cart line, each with its own resolved
            //    Point de Vente - PointDeVenteRepository.ResolveBestPointDeVente
            //    is the exact same method AutoSelectBestStoreForLocation() and
            //    SubmitDemande_Click() call on the desktop side, so a request
            //    coming from the portal is assigned a store using identical
            //    rules (explicit choice for pickup, online warehouse for
            //    express, gouvernorat/ville/adresse match for home delivery,
            //    highest-stock fallback otherwise).
            var demandeIds = new List<int>();
            foreach (var item in request.Items)
            {
                int targetPdvId = PointDeVenteRepository.ResolveBestPointDeVente(
                    produitId: item.ProduitId,
                    modeLivraison: request.ModeLivraison,
                    gouvernorat: request.Gouvernorat,
                    ville: request.Ville,
                    clientAdresse: request.Adresse,
                    selectedPdvId: request.PointDeVenteId);

                var demande = new DemandeAchat
                {
                    UtilisateurId = _systemUserId,
                    ContactId = contactId,
                    ProduitId = item.ProduitId,
                    PointDeVenteId = targetPdvId,
                    Quantite = item.Quantite,
                    Origine = "Portail",
                    ModeLivraison = request.ModeLivraison,
                    ModePaiement = request.ModePaiement,
                };

                int demandeId = DemandeAchatRepository.Create(demande);
                demande.DemandeId = demandeId;
                demandeIds.Add(demandeId);

                // 4) Same notification path as the desktop app (5.3, event #4):
                //    a thank-you-style confirmation to the client, and a heads-up
                //    to every active superviseur d'achat.
                NotificationService.NotifierDemandeCreee(demande, nomUtilisateur: $"{request.Prenom} {request.Nom}".Trim());
            }

            return Ok(new CreerDemandeResponse { Success = true, DemandeIds = demandeIds });
        }
    }
}
