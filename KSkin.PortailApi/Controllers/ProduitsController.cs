using LoginRegisterApp.PortailApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace LoginRegisterApp.PortailApi.Controllers
{
    [ApiController]
    [Route("api/produits")]
    public class ProduitsController : ControllerBase
    {
        // GET /api/produits?marque=Innisfree&categorie=S%C3%A9rums&q=creme
        // All three query params are optional and combine as AND filters.
        [HttpGet]
        public IActionResult GetProduits([FromQuery] string? marque, [FromQuery] string? categorie, [FromQuery] string? q)
        {
            var produits = ProduitPortailRepository.GetProduits(marque, categorie, q);
            return Ok(produits);
        }

        // GET /api/produits/12
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var produit = ProduitPortailRepository.GetById(id);
            if (produit is null) return NotFound();
            return Ok(produit);
        }

        // GET /api/produits/marques -> distinct list, used to build the filter pills
        [HttpGet("marques")]
        public IActionResult GetMarques() => Ok(ProduitPortailRepository.GetDistinctMarques());

        // GET /api/produits/marques-details -> distinct list of brands with logos
        [HttpGet("marques-details")]
        public IActionResult GetMarquesDetails() => Ok(ProduitPortailRepository.GetDistinctMarquesDetails());

        // GET /api/produits/categories -> distinct list, used to build the filter pills
        [HttpGet("categories")]
        public IActionResult GetCategories() => Ok(ProduitPortailRepository.GetDistinctCategories());
    }
}
