// Talks to the KSkin Portail Web API (see /api project). Configure the base
// URL with VITE_API_URL in a .env file — see .env.example.
const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5127'

// Used only if the API can't be reached (e.g. while building the UI before
// the backend is wired up). Shaped exactly like the real /api/produits
// response so swapping to the live API needs no component changes.
const MOCK_PRODUITS = [
  { produitId: 3, nom: 'Centella Oil Cleanser', marque: 'Centella', marqueImageUrl: null, prix: 72.0, stock: 51, categorieNom: 'Nettoyants', imageUrl: null, description: "Huile démaquillante apaisante au centella asiatica." },
  { produitId: 4, nom: 'Anua Heartleaf 77% Soothing Toner 250ml', marque: 'Anua', marqueImageUrl: null, prix: 65.0, stock: 54, categorieNom: 'Toners', imageUrl: null, description: "Toner apaisant au heartleaf 77%." },
  { produitId: 5, nom: 'Anua Heartleaf Pore Control Cleansing Oil 200ml', marque: 'Anua', marqueImageUrl: null, prix: 58.0, stock: 82, categorieNom: 'Nettoyants', imageUrl: null, description: "Huile démaquillante contrôle des pores." },
  { produitId: 6, nom: 'Beauty of Joseon Relief Sun : Rice + Probiotics SPF50+', marque: 'Beauty of Joseon', marqueImageUrl: null, prix: 56.0, stock: 62, categorieNom: 'Protection solaire', imageUrl: null, description: "Écran solaire riz et probiotiques SPF50+." },
  { produitId: 7, nom: 'Beauty of Joseon Dynasty Cream 50ml', marque: 'Beauty of Joseon', marqueImageUrl: null, prix: 69.0, stock: 75, categorieNom: 'Hydratants', imageUrl: null, description: "Crème hydratante dynastie d'exception." },
  { produitId: 8, nom: 'Beauty of Joseon Glow Serum : Propolis + Niacinamide', marque: 'Beauty of Joseon', marqueImageUrl: null, prix: 52.0, stock: 70, categorieNom: 'Sérums', imageUrl: null, description: "Sérum éclat à la propolis et niacinamide." },
]

async function request(path, options = {}) {
  const urlsToTry = [
    API_BASE,
    'http://localhost:5127',
    'https://localhost:7138',
    'http://localhost:5000',
  ]
  const uniqueUrls = [...new Set(urlsToTry)]

  for (const baseUrl of uniqueUrls) {
    try {
      const res = await fetch(`${baseUrl}${path}`, {
        headers: { 'Content-Type': 'application/json' },
        ...options,
      })
      if (res.ok) {
        return await res.json()
      }
    } catch (err) {
      // try next candidate URL
    }
  }

  console.warn(`[api] backend unreachable across candidate ports, falling back to mock for ${path}`)
  return null
}

export async function getProduits({ marque, categorie, search } = {}) {
  const params = new URLSearchParams()
  if (marque) params.set('marque', marque)
  if (categorie) params.set('categorie', categorie)
  if (search) params.set('q', search)
  const qs = params.toString() ? `?${params.toString()}` : ''

  const data = await request(`/api/produits${qs}`)
  if (data) return data

  return MOCK_PRODUITS.filter((p) => {
    if (marque && p.marque !== marque) return false
    if (categorie && p.categorieNom !== categorie) return false
    if (search && !p.nom.toLowerCase().includes(search.toLowerCase())) return false
    return true
  })
}

export async function getProduitById(id) {
  const data = await request(`/api/produits/${id}`)
  if (data) return data
  return MOCK_PRODUITS.find((p) => String(p.produitId) === String(id)) || null
}

export async function getMarques() {
  const data = await request('/api/produits/marques')
  if (data) return data
  return [...new Set(MOCK_PRODUITS.map((p) => p.marque))]
}

export async function getMarquesDetails() {
  const data = await request('/api/produits/marques-details')
  if (data) return data
  const distinct = [...new Set(MOCK_PRODUITS.map((p) => p.marque))]
  return distinct.map((m) => {
    const found = MOCK_PRODUITS.find((p) => p.marque === m)
    return { nom: m, imageUrl: found?.marqueImageUrl || null }
  })
}

export async function getContactByPhone(telephone) {
  if (!telephone) return null
  const data = await request(`/api/contacts/by-phone?telephone=${encodeURIComponent(telephone)}`)
  if (data && data.found) return data.contact
  return null
}

export async function getCategories() {
  const data = await request('/api/produits/categories')
  const rawList = data || MOCK_PRODUITS.map((p) => p.categorieNom)
  const cleaned = rawList.map((c) => (typeof c === 'string' ? c.trim() : (c?.nom || '').trim())).filter(Boolean)
  return Array.from(new Set(cleaned))
}

// Physical stores only (the "Achat En Ligne" warehouse is never a pickup
// point) — used to populate the "Retrait en Magasin" store picker.
const MOCK_POINTS_DE_VENTE = [
  { pointDeVenteId: 2, nom: 'Megastore Tunis Charguia 1', ville: 'Tunis' },
  { pointDeVenteId: 3, nom: 'Magasin Av. Liberté', ville: 'Tunis' },
  { pointDeVenteId: 4, nom: 'Magasin Manar City', ville: 'Tunis' },
  { pointDeVenteId: 5, nom: 'Magasin Hammamet Yasmine', ville: 'Hammamet' },
  { pointDeVenteId: 6, nom: 'Megastore Sousse Kantaoui', ville: 'Sousse' },
  { pointDeVenteId: 7, nom: 'Magasin Bizerte Corniche', ville: 'Bizerte' },
  { pointDeVenteId: 8, nom: 'Megastore Sfax Centre Ville', ville: 'Sfax' },
]

export async function getPointsDeVente() {
  const data = await request('/api/points-de-vente')
  if (data) return data
  return MOCK_POINTS_DE_VENTE
}

// items: [{ produitId, quantite }]. modeLivraison: 'Livraison à Domicile' |
// 'Retrait en Magasin (Click & Collect)' | 'Expédition Express 24h'.
// pointDeVenteId is only meaningful (and required) for "Retrait en Magasin" —
// otherwise the API resolves the best store itself from gouvernorat/ville/adresse.
// Returns { success, demandeIds } or throws.
export async function creerDemande({
  nom,
  prenom,
  telephone,
  email,
  gouvernorat,
  ville,
  adresse,
  modeLivraison,
  modePaiement,
  pointDeVenteId,
  items,
}) {
  const res = await fetch(`${API_BASE}/api/demandes`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      nom,
      prenom,
      telephone,
      email,
      gouvernorat,
      ville,
      adresse,
      modeLivraison,
      modePaiement,
      pointDeVenteId,
      items,
      origine: 'Portail',
    }),
  })
  if (!res.ok) {
    const text = await res.text().catch(() => '')
    throw new Error(text || `Erreur serveur (${res.status})`)
  }
  return res.json()
}
