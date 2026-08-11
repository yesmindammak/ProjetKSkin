using LoginRegisterApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegisterApp.PortailApi.Controllers
{
    [ApiController]
    [Route("api/contacts")]
    public class ContactsController : ControllerBase
    {
        // GET /api/contacts/by-phone?telephone=21698765432
        [HttpGet("by-phone")]
        public IActionResult GetByPhone([FromQuery] string telephone)
        {
            if (string.IsNullOrWhiteSpace(telephone))
                return BadRequest("Numéro de téléphone requis.");

            var contact = ContactRepository.FindByPhone(telephone.Trim());
            if (contact is null)
                return NotFound(new { found = false, message = "Contact non trouvé." });

            return Ok(new
            {
                found = true,
                contact = new
                {
                    contact.Nom,
                    contact.Prenom,
                    contact.Telephone,
                    contact.Email,
                    contact.Gouvernorat,
                    contact.Ville,
                    contact.Adresse
                }
            });
        }
    }
}
