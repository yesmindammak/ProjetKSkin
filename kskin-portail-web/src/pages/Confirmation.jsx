import React from 'react'
import { Link } from 'react-router-dom'
import { IconInstagram, IconPhone } from '../components/Icons.jsx'

export default function Confirmation() {
  return (
    <div className="section-pad flex flex-col items-center py-24 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-blush text-clay">
        <svg viewBox="0 0 24 24" className="h-8 w-8" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
          <path d="M5 13l4 4L19 7" />
        </svg>
      </div>
      <h1 className="mt-6 font-display text-4xl text-espresso">Demande envoyée</h1>
      <p className="mt-3 max-w-md text-sm leading-relaxed text-muted">
        Merci pour votre confiance. Votre demande d'achat a bien été enregistrée — un membre de
        l'équipe KSkin vous contacte sous 24 à 48h pour confirmer la disponibilité et les
        modalités de livraison.
      </p>

      <div className="mt-8 flex flex-wrap justify-center gap-3">
        <Link to="/catalogue" className="btn-primary">Continuer mes achats</Link>
        <Link to="/" className="btn-outline">Retour à l'accueil</Link>
      </div>

      <div className="mt-12 flex items-center gap-6 text-sm text-muted">
        <a href="tel:+21628134234" className="flex items-center gap-2 hover:text-clay">
          <IconPhone className="h-4 w-4" /> +216 28 134 234
        </a>
        <a href="https://www.instagram.com/kskin_tn/" target="_blank" rel="noreferrer" className="flex items-center gap-2 hover:text-clay">
          <IconInstagram className="h-4 w-4" /> @kskin_tn
        </a>
      </div>
    </div>
  )
}
