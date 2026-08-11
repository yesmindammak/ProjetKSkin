import React, { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import ProductCard from '../components/ProductCard.jsx'
import FilterPills from '../components/FilterPills.jsx'
import { IconSearch } from '../components/Icons.jsx'
import { getProduits, getMarques, getCategories } from '../services/api.js'

export default function Catalogue() {
  const [searchParams, setSearchParams] = useSearchParams()
  const marque = searchParams.get('marque')
  const categorie = searchParams.get('categorie')

  const [produits, setProduits] = useState([])
  const [marques, setMarques] = useState([])
  const [categories, setCategories] = useState([])
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getMarques().then(setMarques)
    getCategories().then(setCategories)
  }, [])

  useEffect(() => {
    setLoading(true)
    getProduits({ marque, categorie, search }).then((data) => {
      setProduits(data)
      setLoading(false)
    })
  }, [marque, categorie, search])

  function setParam(key, value) {
    const next = new URLSearchParams(searchParams)
    if (value) next.set(key, value)
    else next.delete(key)
    setSearchParams(next)
  }

  return (
    <div className="section-pad py-12">
      <div className="mb-8">
        <span className="eyebrow">Catalogue complet</span>
        <h1 className="mt-2 font-display text-4xl text-espresso">Tous nos produits</h1>
        <p className="mt-2 max-w-lg text-sm text-muted">
          Sélectionnez vos produits, ajustez les quantités, puis soumettez votre demande — un
          conseiller vous recontacte pour confirmer.
        </p>
      </div>

      <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="relative w-full md:max-w-xs">
          <IconSearch className="pointer-events-none absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
          <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Rechercher un produit…"
            className="w-full rounded-full border border-espresso/15 bg-card py-2.5 pl-10 pr-4 text-sm outline-none transition-colors focus:border-clay"
          />
        </div>
      </div>

      <div className="mb-4 space-y-3">
        <FilterPills options={categories} active={categorie} onSelect={(v) => setParam('categorie', v)} allLabel="Toutes les catégories" />
        <FilterPills options={marques} active={marque} onSelect={(v) => setParam('marque', v)} allLabel="Toutes les marques" />
      </div>

      {loading ? (
        <div className="mt-8 grid grid-cols-2 gap-5 md:grid-cols-3 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="aspect-[4/5] animate-pulse rounded-2xl bg-blush/60" />
          ))}
        </div>
      ) : produits.length === 0 ? (
        <div className="mt-16 flex flex-col items-center gap-2 text-center text-muted">
          <p className="font-display text-xl text-espresso">Aucun produit trouvé</p>
          <p className="text-sm">Essayez un autre filtre ou une autre recherche.</p>
        </div>
      ) : (
        <div className="mt-8 grid grid-cols-2 gap-5 md:grid-cols-3 lg:grid-cols-4">
          {produits.map((p) => (
            <ProductCard key={p.produitId} produit={p} />
          ))}
        </div>
      )}
    </div>
  )
}
