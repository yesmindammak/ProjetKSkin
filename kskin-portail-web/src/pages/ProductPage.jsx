import React, { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useCart } from '../context/CartContext.jsx'
import { getProduitById } from '../services/api.js'
import { IconMinus, IconPlus } from '../components/Icons.jsx'

function formatPrix(prix) {
  return `${Number(prix).toFixed(2)} TND`
}

export default function ProductPage() {
  const { id } = useParams()
  const { addItem } = useCart()
  const [produit, setProduit] = useState(null)
  const [quantite, setQuantite] = useState(1)
  const [loading, setLoading] = useState(true)
  const [added, setAdded] = useState(false)

  useEffect(() => {
    setLoading(true)
    setAdded(false)
    getProduitById(id).then((data) => {
      setProduit(data)
      setQuantite(1)
      setLoading(false)
    })
  }, [id])

  if (loading) {
    return <div className="section-pad py-24 text-center text-muted">Chargement…</div>
  }

  if (!produit) {
    return (
      <div className="section-pad py-24 text-center">
        <p className="font-display text-2xl text-espresso">Produit introuvable</p>
        <Link to="/catalogue" className="btn-outline mt-6 inline-flex">Retour au catalogue</Link>
      </div>
    )
  }

  const outOfStock = (produit.stock ?? 0) <= 0
  const maxQty = produit.stock ?? 99

  return (
    <div className="section-pad py-12">
      <nav className="mb-8 text-xs text-muted">
        <Link to="/catalogue" className="hover:text-clay">Catalogue</Link>
        <span className="mx-2">/</span>
        <span className="text-espresso">{produit.nom}</span>
      </nav>

      <div className="grid gap-10 md:grid-cols-2">
        <div className="aspect-square overflow-hidden rounded-[2rem] bg-gradient-to-br from-blush via-cream to-blushdark shadow-card">
          {produit.imageUrl ? (
            <img src={produit.imageUrl} alt={produit.nom} className="h-full w-full object-cover" />
          ) : (
            <div className="flex h-full w-full items-center justify-center text-clay/50">
              <svg viewBox="0 0 24 24" className="h-24 w-24" fill="none" stroke="currentColor" strokeWidth="1">
                <path d="M9 3h6l1 3h3v14a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1V6h3l1-3Z" />
                <circle cx="12" cy="13" r="3.5" />
              </svg>
            </div>
          )}
        </div>

        <div>
          <span className="text-xs font-semibold uppercase tracking-wide text-muted">{produit.marque}</span>
          <h1 className="mt-2 font-display text-3xl text-espresso md:text-4xl">{produit.nom}</h1>
          <p className="mt-4 text-2xl font-semibold text-clay">{formatPrix(produit.prix)}</p>

          {produit.description && (
            <p className="mt-6 max-w-md text-sm leading-relaxed text-muted">{produit.description}</p>
          )}

          <div className="mt-8 flex items-center gap-4">
            <div className="flex items-center gap-1 rounded-full border border-espresso/15 px-1">
              <button
                onClick={() => setQuantite((q) => Math.max(1, q - 1))}
                className="flex h-10 w-10 items-center justify-center text-espresso hover:text-clay"
                aria-label="Diminuer la quantité"
              >
                <IconMinus className="h-4 w-4" />
              </button>
              <span className="w-8 text-center">{quantite}</span>
              <button
                onClick={() => setQuantite((q) => Math.min(maxQty, q + 1))}
                className="flex h-10 w-10 items-center justify-center text-espresso hover:text-clay"
                aria-label="Augmenter la quantité"
              >
                <IconPlus className="h-4 w-4" />
              </button>
            </div>

            <button
              onClick={() => {
                addItem(produit, quantite)
                setAdded(true)
              }}
              disabled={outOfStock}
              className="btn-primary flex-1 disabled:opacity-50"
            >
              {outOfStock ? 'Rupture de stock' : 'Ajouter au panier'}
            </button>
          </div>

          {added && (
            <p className="mt-3 text-sm text-clay">Ajouté au panier — <Link to="/commande" className="underline">finaliser ma demande</Link></p>
          )}

          {produit.ingredients && (
            <div className="mt-6 border-t border-espresso/10 pt-6">
              <h3 className="text-sm font-semibold text-espresso">Ingrédients & Actifs</h3>
              <p className="mt-2 text-sm leading-relaxed text-muted">{produit.ingredients}</p>
            </div>
          )}

          <dl className="mt-8 grid grid-cols-2 gap-4 border-t border-espresso/10 pt-6 text-sm">
            <div>
              <dt className="text-muted">Statut de disponibilité</dt>
              <dd className="font-semibold text-clay">{outOfStock ? 'Rupture de stock' : 'En stock'}</dd>
            </div>
            <div>
              <dt className="text-muted">Marque</dt>
              <dd className="font-medium text-espresso">{produit.marque || '—'}</dd>
            </div>
          </dl>

          {produit.disponibilites && produit.disponibilites.length > 0 && (
            <div className="mt-6 border-t border-espresso/10 pt-6">
              <h3 className="text-sm font-semibold text-espresso mb-3">Disponibilité par point de vente</h3>
              <div className="grid gap-2 sm:grid-cols-2">
                {produit.disponibilites.map((disp, idx) => (
                  <div key={idx} className="flex items-center justify-between rounded-xl border border-espresso/8 bg-card px-3 py-2 text-xs">
                    <span className="font-medium text-espresso truncate">{disp.pointDeVenteNom}</span>
                    <span className="ml-2 font-semibold text-clay shrink-0">{disp.statutDisponibilite}</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
