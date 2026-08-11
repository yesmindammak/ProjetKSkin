using System.Collections.Generic;
using LoginRegisterApp.Data;
using LoginRegisterApp.Models;

namespace LoginRegisterApp.Services
{
    // The only piece of code that talks to BOTH databases. Reads everything from
    // the external stock database and reconciles it into the app's Produit table:
    //   - ReferenceExterne already known here  -> UPDATE (price/stock/description...)
    //   - ReferenceExterne unknown here        -> INSERT, then notify all active users
    //   - Missing Store/Product in external DB -> DELETE from local app DB
    public static class ProductSyncService
    {
        public static List<Produit> Synchroniser()
        {
            // Ensure local DB tables exist
            PointDeVenteRepository.EnsureTablesCreated();

            // Reconcile deleted stores from KSkinStockExterne to local WPF DB
            var activeExternalStoreNames = StockExterneRepository.GetAllExternalStoreNames();
            PointDeVenteRepository.SyncStoreDeletionsFromExternal(activeExternalStoreNames);

            var nouveauxProduits = new List<Produit>();
            var validExternalRefs = new HashSet<string>();

            foreach (ProduitExterne externe in StockExterneRepository.GetAll())
            {
                if (!string.IsNullOrWhiteSpace(externe.ReferenceExterne))
                {
                    validExternalRefs.Add(externe.ReferenceExterne);
                }

                Produit? existant = ProduitRepository.GetByReferenceExterne(externe.ReferenceExterne);
                int targetProduitId;

                if (existant != null)
                {
                    // Known product: refresh data
                    existant.Nom = externe.Nom;
                    existant.Prix = externe.Prix;
                    existant.Stock = externe.Stock;
                    existant.Couleur = externe.Couleur;
                    existant.Type = externe.Type;
                    existant.Ingredients = externe.Ingredients;
                    existant.Description = externe.Description;
                    existant.ImageUrl = externe.ImageUrl;
                    existant.Marque = externe.Marque;
                    existant.MarqueImageUrl = externe.MarqueImageUrl;
                    ProduitRepository.UpdateFromSync(existant);
                    targetProduitId = existant.ProduitId;
                }
                else
                {
                    int familleId = CategorieRepository.FindOrCreateFamille(externe.FamilleNom);
                    int categorieId = CategorieRepository.FindOrCreateCategorie(familleId, externe.CategorieNom);

                    var nouveauProduit = new Produit
                    {
                        CategorieId = categorieId,
                        Nom = externe.Nom,
                        Prix = externe.Prix,
                        Stock = externe.Stock,
                        Couleur = externe.Couleur,
                        Type = externe.Type,
                        Ingredients = externe.Ingredients,
                        Description = externe.Description,
                        ImageUrl = externe.ImageUrl,
                        Marque = externe.Marque,
                        MarqueImageUrl = externe.MarqueImageUrl,
                        ReferenceExterne = externe.ReferenceExterne,
                    };

                    nouveauProduit = ProduitRepository.CreateFromSync(nouveauProduit);
                    nouveauxProduits.Add(nouveauProduit);
                    targetProduitId = nouveauProduit.ProduitId;

                    NotificationService.NotifierNouveauProduit(nouveauProduit);
                }

                // Sync store availability per product & enforce overall Product Stock = SUM(Store Quantities)
                var disponibilites = StockExterneRepository.GetDisponibilitesForExternalProduct(externe.ReferenceExterne, externe.Nom);
                if (disponibilites != null && disponibilites.Count > 0)
                {
                    PointDeVenteRepository.SaveDisponibilitesForProduit(targetProduitId, disponibilites);

                    int totalStoreStock = 0;
                    foreach (var d in disponibilites) totalStoreStock += d.QuantiteStock;

                    ProduitRepository.SetExactStock(targetProduitId, totalStoreStock);
                    StockExterneRepository.SetExactStock(externe.ReferenceExterne, externe.Nom, totalStoreStock);
                }
            }

            // Remove any local products whose ReferenceExterne no longer exists in external stock DB
            ProduitRepository.DeleteMissingFromSync(validExternalRefs);

            return nouveauxProduits;
        }
    }
}
