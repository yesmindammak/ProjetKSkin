import React from 'react'
import { useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext.jsx'
import { IconClose, IconMinus, IconPlus, IconBag } from './Icons.jsx'

function formatPrix(prix) {
  return `${Number(prix).toFixed(2)} TND`
}

export default function CartDrawer() {
  const { items, isOpen, setIsOpen, updateQuantite, removeItem, total } = useCart()
  const navigate = useNavigate()

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50">
      <button
        aria-label="Fermer le panier"
        onClick={() => setIsOpen(false)}
        className="absolute inset-0 bg-espresso/40 backdrop-blur-sm"
      />
      <aside className="absolute right-0 top-0 flex h-full w-full max-w-md flex-col bg-cream shadow-2xl">
        <div className="flex items-center justify-between border-b border-espresso/10 px-6 py-5">
          <h2 className="font-display text-xl text-espresso">Votre panier</h2>
          <button
            onClick={() => setIsOpen(false)}
            className="flex h-9 w-9 items-center justify-center rounded-full hover:bg-blush"
            aria-label="Fermer"
          >
            <IconClose className="h-5 w-5" />
          </button>
        </div>

        {items.length === 0 ? (
          <div className="flex flex-1 flex-col items-center justify-center gap-3 px-6 text-center text-muted">
            <IconBag className="h-10 w-10 opacity-40" />
            <p>Votre panier est vide pour le moment.</p>
            <button onClick={() => setIsOpen(false)} className="btn-outline mt-2">
              Parcourir le catalogue
            </button>
          </div>
        ) : (
          <>
            <ul className="flex-1 overflow-y-auto px-6 py-4">
              {items.map((item) => (
                <li key={item.produitId} className="flex gap-3 border-b border-espresso/8 py-4 last:border-none">
                  <div className="h-16 w-16 shrink-0 rounded-xl bg-blush" />
                  <div className="flex flex-1 flex-col">
                    <div className="flex items-start justify-between gap-2">
                      <div>
                        <p className="text-[11px] font-semibold uppercase tracking-wide text-muted">{item.marque}</p>
                        <p className="font-display text-base leading-tight text-espresso">{item.nom}</p>
                      </div>
                      <button
                        onClick={() => removeItem(item.produitId)}
                        className="text-xs text-muted hover:text-danger"
                      >
                        Retirer
                      </button>
                    </div>
                    <div className="mt-2 flex items-center justify-between">
                      <div className="flex items-center gap-1 rounded-full border border-espresso/15">
                        <button
                          onClick={() => updateQuantite(item.produitId, item.quantite - 1)}
                          className="flex h-7 w-7 items-center justify-center text-espresso hover:text-clay"
                          aria-label="Diminuer la quantité"
                        >
                          <IconMinus className="h-3.5 w-3.5" />
                        </button>
                        <span className="w-6 text-center text-sm">{item.quantite}</span>
                        <button
                          onClick={() => updateQuantite(item.produitId, item.quantite + 1)}
                          className="flex h-7 w-7 items-center justify-center text-espresso hover:text-clay"
                          aria-label="Augmenter la quantité"
                        >
                          <IconPlus className="h-3.5 w-3.5" />
                        </button>
                      </div>
                      <span className="text-sm font-semibold text-clay">
                        {formatPrix(item.prix * item.quantite)}
                      </span>
                    </div>
                  </div>
                </li>
              ))}
            </ul>

            <div className="border-t border-espresso/10 px-6 py-5">
              <div className="mb-4 flex items-center justify-between text-sm">
                <span className="text-muted">Total estimé</span>
                <span className="font-display text-xl text-espresso">{formatPrix(total)}</span>
              </div>
              <button
                onClick={() => {
                  setIsOpen(false)
                  navigate('/commande')
                }}
                className="btn-primary w-full"
              >
                Soumettre ma demande
              </button>
              <p className="mt-3 text-center text-xs text-muted">
                Aucun compte requis. Un conseiller vous contacte pour confirmer.
              </p>
            </div>
          </>
        )}
      </aside>
    </div>
  )
}
