using LoginRegisterApp.Data;
using LoginRegisterApp.Helpers;
using LoginRegisterApp.PortailApi.Dtos;
using Microsoft.Data.SqlClient;

namespace LoginRegisterApp.PortailApi.Data
{
    public static class ProduitPortailRepository
    {
        // marque / categorieNom / q are all optional filters (AND-ed together).
        // Passing null/empty for a filter means "don't filter on this".
        public static List<ProduitDto> GetProduits(string? marque, string? categorieNom, string? q)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();

            const string query = @"
                SELECT p.ProduitId, p.Nom, p.Prix, p.Stock, p.ImageUrl, p.Description,
                       CASE WHEN COL_LENGTH('Produit', 'Marque') IS NOT NULL THEN p.Marque ELSE NULL END AS Marque,
                       CASE WHEN COL_LENGTH('Produit', 'MarqueImageUrl') IS NOT NULL THEN p.MarqueImageUrl ELSE NULL END AS MarqueImageUrl,
                       c.Nom AS CategorieNom,
                       CASE WHEN COL_LENGTH('Produit', 'Ingredients') IS NOT NULL THEN p.Ingredients ELSE NULL END AS Ingredients
                FROM Produit p
                LEFT JOIN CategorieProduit c ON p.CategorieId = c.CategorieId
                WHERE (@Marque IS NULL OR p.Marque = @Marque)
                  AND (@CategorieNom IS NULL OR c.Nom = @CategorieNom)
                  AND (@Q IS NULL OR p.Nom LIKE '%' + @Q + '%')
                ORDER BY p.Nom";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Marque", (object?)marque ?? DBNull.Value);
            command.Parameters.AddWithValue("@CategorieNom", (object?)categorieNom ?? DBNull.Value);
            command.Parameters.AddWithValue("@Q", (object?)q ?? DBNull.Value);

            using SqlDataReader reader = command.ExecuteReader();
            var list = new List<ProduitDto>();
            while (reader.Read())
            {
                list.Add(new ProduitDto
                {
                    ProduitId = reader.GetInt32(0),
                    Nom = reader.GetString(1),
                    Prix = reader.GetDecimal(2),
                    Stock = reader.GetInt32(3),
                    ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Description = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Marque = reader.IsDBNull(6) ? null : reader.GetString(6),
                    MarqueImageUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
                    CategorieNom = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Ingredients = reader.IsDBNull(9) ? null : reader.GetString(9),
                });
            }
            return list;
        }

        public static ProduitDto? GetById(int produitId)
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();

            const string query = @"
                SELECT p.ProduitId, p.Nom, p.Prix, p.Stock, p.ImageUrl, p.Description,
                       CASE WHEN COL_LENGTH('Produit', 'Marque') IS NOT NULL THEN p.Marque ELSE NULL END AS Marque,
                       CASE WHEN COL_LENGTH('Produit', 'MarqueImageUrl') IS NOT NULL THEN p.MarqueImageUrl ELSE NULL END AS MarqueImageUrl,
                       c.Nom AS CategorieNom,
                       CASE WHEN COL_LENGTH('Produit', 'Ingredients') IS NOT NULL THEN p.Ingredients ELSE NULL END AS Ingredients
                FROM Produit p
                LEFT JOIN CategorieProduit c ON p.CategorieId = c.CategorieId
                WHERE p.ProduitId = @Id";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", produitId);
            using SqlDataReader reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            var dto = new ProduitDto
            {
                ProduitId = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Prix = reader.GetDecimal(2),
                Stock = reader.GetInt32(3),
                ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                Description = reader.IsDBNull(5) ? null : reader.GetString(5),
                Marque = reader.IsDBNull(6) ? null : reader.GetString(6),
                MarqueImageUrl = reader.IsDBNull(7) ? null : reader.GetString(7),
                CategorieNom = reader.IsDBNull(8) ? null : reader.GetString(8),
                Ingredients = reader.IsDBNull(9) ? null : reader.GetString(9),
            };
            reader.Close();

            try
            {
                var disps = PointDeVenteRepository.GetDisponibilitesForProduit(produitId);
                if (disps.Any())
                {
                    dto.Disponibilites = disps.Select(d => new ProduitDisponibiliteDto
                    {
                        PointDeVenteNom = d.PointDeVenteNom,
                        StatutDisponibilite = d.StatutDisponibilite
                    }).ToList();
                }
            }
            catch
            {
            }

            return dto;
        }

        public static List<string> GetDistinctMarques()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT DISTINCT Marque FROM Produit
                WHERE Marque IS NOT NULL AND Marque <> '' ORDER BY Marque";
            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var list = new List<string>();
            while (reader.Read()) list.Add(reader.GetString(0));
            return list;
        }

        public static List<MarqueDto> GetDistinctMarquesDetails()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = @"
                SELECT DISTINCT 
                       CASE WHEN COL_LENGTH('Produit', 'Marque') IS NOT NULL THEN Marque ELSE NULL END AS Marque,
                       MAX(CASE WHEN COL_LENGTH('Produit', 'MarqueImageUrl') IS NOT NULL THEN MarqueImageUrl ELSE NULL END) AS MarqueImageUrl
                FROM Produit
                WHERE Marque IS NOT NULL AND Marque <> ''
                GROUP BY Marque
                ORDER BY Marque";
            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var list = new List<MarqueDto>();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    list.Add(new MarqueDto
                    {
                        Nom = reader.GetString(0),
                        ImageUrl = reader.IsDBNull(1) ? null : reader.GetString(1)
                    });
                }
            }
            return list;
        }

        public static List<string> GetDistinctCategories()
        {
            using SqlConnection connection = DatabaseHelper.GetConnection();
            const string query = "SELECT DISTINCT Nom FROM CategorieProduit WHERE Nom IS NOT NULL AND Nom <> '' ORDER BY Nom";
            using var command = new SqlCommand(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            var list = new List<string>();
            while (reader.Read())
            {
                string cat = reader.GetString(0).Trim();
                if (!list.Contains(cat, StringComparer.OrdinalIgnoreCase))
                {
                    list.Add(cat);
                }
            }
            return list;
        }
    }
}
