using LoginRegisterApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegisterApp.PortailApi.Controllers
{
    [ApiController]
    [Route("api/points-de-vente")]
    public class PointsDeVenteController : ControllerBase
    {
        // GET /api/points-de-vente
        // Physical stores only - "Achat En Ligne" is the online fulfillment
        // warehouse (used automatically for Livraison à Domicile / Expédition
        // Express), never something a client picks for in-store pickup.
        [HttpGet]
        public IActionResult GetPointsDeVente()
        {
            var stores = PointDeVenteRepository.GetAllPointsDeVente()
                .Where(pdv => !pdv.Nom.Contains("En Ligne", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(stores);
        }
    }
}
