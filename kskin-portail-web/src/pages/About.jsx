import React from 'react'
import { Link } from 'react-router-dom'
import { IconLeaf } from '../components/Icons.jsx'

const values = [
  {
    title: '100% Authentique & Importation Légale',
    text: "Nous sélectionnons et importons légalement en Tunisie les plus grandes références K-Beauty authentiques directement depuis la Corée du Sud pour garantir leur efficacité et leur qualité exceptionnelle."
  },
  {
    title: 'Prix Accessibles & Transparents',
    text: "Notre engagement est de rendre le meilleur du soin coréen accessible à tous en Tunisie, avec une tarification juste, honnête et sans surcoût inutile."
  },
  {
    title: '+500 Clients Satisfaits',
    text: "Déjà plus de 500 passionnés de skincare à travers toute la Tunisie font confiance à KSkin pour recevoir leurs soins favoris en toute sérénité."
  },
]

export default function About() {
  return (
    <div className="section-pad py-16">
      <div className="grid gap-12 md:grid-cols-2 md:items-center">
        <div>
          <span className="eyebrow">À propos de KSkin</span>
          <h1 className="mt-3 font-display text-4xl text-espresso md:text-5xl">
            L'authenticité K-Beauty, <span className="italic text-clay">au meilleur prix</span>.
          </h1>
          <p className="mt-6 text-sm leading-relaxed text-muted">
            Chez KSkin, nous croyons que chacun mérite d'accéder à la fameuse routine coréenne (<em>glass skin</em>) grâce à des soins performants et de grande qualité.
            Nous dénichons, vérifions et importons légalement en Tunisie les marques coréennes les plus réputées (Anua, Beauty of Joseon, Centella, Dr. Althea, Purito...) afin de vous offrir des formules reconnues pour leurs bienfaits, certifiées d'origine et proposées à des prix transparents et accessibles.
          </p>
          <div className="mt-8 flex flex-wrap items-center gap-6">
            <Link to="/catalogue" className="btn-primary">Découvrir le catalogue</Link>
            <div className="flex items-center gap-2 text-sm font-semibold text-clay">
              <span className="rounded-full bg-clay/10 px-3 py-1 text-clay">+500 Clients Satisfaits ✨</span>
            </div>
          </div>
        </div>
        <div className="relative mx-auto aspect-square w-full max-w-sm overflow-hidden rounded-[2rem] bg-gradient-to-br from-blush via-cream to-blushdark shadow-soft">
          <div className="absolute inset-0 flex items-center justify-center text-clay/60">
            <IconLeaf className="h-24 w-24" />
          </div>
        </div>
      </div>

      <div className="mt-20 grid gap-6 md:grid-cols-3">
        {values.map((v) => (
          <div key={v.title} className="rounded-2xl border border-espresso/8 bg-card p-6 shadow-card transition-all hover:border-clay/40 hover:-translate-y-1">
            <h3 className="font-display text-lg font-semibold text-espresso">{v.title}</h3>
            <p className="mt-2 text-sm leading-relaxed text-muted">{v.text}</p>
          </div>
        ))}
      </div>
    </div>
  )
}
