import React from 'react'
import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <div className="section-pad flex flex-col items-center py-32 text-center">
      <p className="eyebrow">Erreur 404</p>
      <h1 className="mt-2 font-display text-4xl text-espresso">Page introuvable</h1>
      <Link to="/" className="btn-primary mt-8">Retour à l'accueil</Link>
    </div>
  )
}
