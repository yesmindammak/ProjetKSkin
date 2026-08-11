import React from 'react'
import { Link } from 'react-router-dom'
import { useCart } from '../context/CartContext.jsx'
import { IconBag } from './Icons.jsx'

function formatPrix(prix) {
  return `${Number(prix).toFixed(2)} TND`
}

export default function ProductCard({ produit }) {
  const { addItem } = useCart()
  const outOfStock = (produit.stock ?? 0) <= 0

  return (
    <div className="group relative flex flex-col overflow-hidden rounded-2xl border border-espresso/8 bg-card shadow-card transition-all hover:-translate-y-1 hover:shadow-soft">
      <Link to={`/produit/${produit.produitId}`} className="relative block aspect-[4/5] overflow-hidden bg-blush">
        {produit.imageUrl ? (
          <img
            src={produit.imageUrl}
            alt={produit.nom}
            className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-blush via-cream to-blushdark text-clay/50">
            <svg viewBox="0 0 24 24" className="h-14 w-14" fill="none" stroke="currentColor" strokeWidth="1.2">
              <path d="M9 3h6l1 3h3v14a1 1 0 0 1-1 1H6a1 1 0 0 1-1-1V6h3l1-3Z" />
              <circle cx="12" cy="13" r="3.5" />
            </svg>
          </div>
        )}
        {outOfStock && (
          <span className="absolute left-3 top-3 rounded-full bg-espresso/90 px-3 py-1 text-[11px] font-semibold text-cream">
            Rupture de stock
          </span>
        )}
      </Link>

      <div className="flex flex-1 flex-col gap-1 p-4">
        <span className="text-[11px] font-semibold uppercase tracking-wide text-muted">{produit.marque}</span>
        <Link to={`/produit/${produit.produitId}`} className="font-display text-lg leading-snug text-espresso hover:text-clay">
          {produit.nom}
        </Link>
        <div className="mt-auto flex items-center justify-between pt-3">
          <span className="text-base font-semibold text-clay">{formatPrix(produit.prix)}</span>
          <button
            onClick={() => addItem(produit, 1)}
            disabled={outOfStock}
            className="flex h-9 w-9 items-center justify-center rounded-full bg-blush text-espresso transition-colors hover:bg-clay hover:text-white disabled:cursor-not-allowed disabled:opacity-40"
            aria-label={`Ajouter ${produit.nom} au panier`}
          >
            <IconBag className="h-4 w-4" />
          </button>
        </div>
      </div>
    </div>
  )
}
