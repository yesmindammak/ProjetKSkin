import React from 'react'
import { IconInstagram } from './Icons.jsx'

const INSTAGRAM_URL = 'https://www.instagram.com/kskin_tn/'

const tiles = [
  'from-clay to-gold',
  'from-blush to-clay',
  'from-gold to-claydark',
  'from-espresso to-clay',
  'from-blushdark to-gold',
  'from-clay to-espresso',
]

export default function InstagramStrip() {
  return (
    <section className="section-pad py-20">
      <div className="mb-8 flex flex-col items-center gap-2 text-center">
        <span className="eyebrow">Suivez-nous</span>
        <h2 className="font-display text-3xl text-espresso md:text-4xl">@kskin_tn sur Instagram</h2>
        <p className="max-w-md text-sm text-muted">
          Routines, nouveautés et conseils skincare coréens, chaque semaine.
        </p>
      </div>

      <div className="grid grid-cols-3 gap-2 md:grid-cols-6 md:gap-3">
        {tiles.map((gradient, i) => (
          <a
            key={i}
            href={INSTAGRAM_URL}
            target="_blank"
            rel="noreferrer"
            className={`group relative aspect-square overflow-hidden rounded-xl bg-gradient-to-br ${gradient}`}
          >
            <span className="absolute inset-0 flex items-center justify-center bg-espresso/0 text-white opacity-0 transition-all group-hover:bg-espresso/30 group-hover:opacity-100">
              <IconInstagram className="h-6 w-6" />
            </span>
          </a>
        ))}
      </div>

      <div className="mt-6 flex justify-center">
        <a href={INSTAGRAM_URL} target="_blank" rel="noreferrer" className="btn-outline">
          <IconInstagram className="h-4 w-4" />
          Voir le profil
        </a>
      </div>
    </section>
  )
}
