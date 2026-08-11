import React from 'react'
import { IconInstagram, IconPhone } from '../components/Icons.jsx'

export default function Contact() {
  return (
    <div className="section-pad py-16">
      <span className="eyebrow">On vous répond vite</span>
      <h1 className="mt-2 font-display text-4xl text-espresso">Contactez KSkin</h1>
      <p className="mt-3 max-w-md text-sm text-muted">
        Une question sur un produit, une commande en cours ? Écrivez-nous directement.
      </p>

      <div className="mt-10 grid gap-5 sm:grid-cols-2 max-w-xl">
        <a
          href="tel:+21628134234"
          className="flex flex-col gap-4 rounded-2xl border border-espresso/8 bg-card p-6 shadow-card transition-all hover:-translate-y-1 hover:shadow-soft"
        >
          <span className="flex h-11 w-11 items-center justify-center rounded-full bg-blush text-clay">
            <IconPhone className="h-5 w-5" />
          </span>
          <div>
            <p className="font-display text-lg text-espresso">Téléphone / WhatsApp</p>
            <p className="mt-1 text-sm text-muted">+216 28 134 234</p>
          </div>
        </a>

        <a
          href="https://www.instagram.com/kskin_tn/"
          target="_blank"
          rel="noreferrer"
          className="flex flex-col gap-4 rounded-2xl border border-espresso/8 bg-card p-6 shadow-card transition-all hover:-translate-y-1 hover:shadow-soft"
        >
          <span className="flex h-11 w-11 items-center justify-center rounded-full bg-blush text-clay">
            <IconInstagram className="h-5 w-5" />
          </span>
          <div>
            <p className="font-display text-lg text-espresso">Instagram</p>
            <p className="mt-1 text-sm text-muted">@kskin_tn</p>
          </div>
        </a>
      </div>
    </div>
  )
}
