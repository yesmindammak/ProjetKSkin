import React, { createContext, useContext, useEffect, useMemo, useState } from 'react'

const CartContext = createContext(null)
const STORAGE_KEY = 'kskin_cart_v1'

export function CartProvider({ children }) {
  const [items, setItems] = useState(() => {
    try {
      const raw = localStorage.getItem(STORAGE_KEY)
      return raw ? JSON.parse(raw) : []
    } catch {
      return []
    }
  })
  const [isOpen, setIsOpen] = useState(false)

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items))
  }, [items])

  function addItem(produit, quantite = 1) {
    setItems((prev) => {
      const existing = prev.find((i) => i.produitId === produit.produitId)
      if (existing) {
        return prev.map((i) =>
          i.produitId === produit.produitId
            ? { ...i, quantite: Math.min(i.quantite + quantite, produit.stock ?? 99) }
            : i
        )
      }
      return [
        ...prev,
        {
          produitId: produit.produitId,
          nom: produit.nom,
          marque: produit.marque,
          prix: produit.prix,
          imageUrl: produit.imageUrl,
          stock: produit.stock,
          quantite,
        },
      ]
    })
    setIsOpen(true)
  }

  function updateQuantite(produitId, quantite) {
    setItems((prev) =>
      quantite <= 0
        ? prev.filter((i) => i.produitId !== produitId)
        : prev.map((i) => (i.produitId === produitId ? { ...i, quantite } : i))
    )
  }

  function removeItem(produitId) {
    setItems((prev) => prev.filter((i) => i.produitId !== produitId))
  }

  function clearCart() {
    setItems([])
  }

  const count = useMemo(() => items.reduce((sum, i) => sum + i.quantite, 0), [items])
  const total = useMemo(() => items.reduce((sum, i) => sum + i.quantite * i.prix, 0), [items])

  const value = { items, addItem, updateQuantite, removeItem, clearCart, count, total, isOpen, setIsOpen }
  return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}

export function useCart() {
  const ctx = useContext(CartContext)
  if (!ctx) throw new Error('useCart must be used within CartProvider')
  return ctx
}
