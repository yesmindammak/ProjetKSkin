import React from 'react'
import { Link } from 'react-router-dom'

export default function Hero() {
  return (
    <section className="relative overflow-hidden">
      <div className="pointer-events-none absolute inset-0 bg-glow" />
      <div className="section-pad relative grid gap-10 py-16 md:grid-cols-2 md:items-center md:py-24">
        <div>
          <span className="eyebrow">Korean skincare, Tunisie</span>
          <h1 className="mt-4 font-display text-4xl leading-[1.08] text-espresso md:text-6xl">
            La routine <span className="italic text-clay">glass&nbsp;skin</span>,
            <br /> sans le détour.
          </h1>
          <p className="mt-6 max-w-lg text-base leading-relaxed text-muted">
            Nettoyants, sérums, hydratants et protections solaires sélectionnés pour leur formulation.
            KSkin est la destination beauté coréenne (K-Beauty) par excellence en Tunisie : nous vous
            apportons les soins les plus innovants aux actifs apaisants et efficaces
            pour révéler une peau saine et éclatante effet <em>glass skin</em>.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link to="/catalogue" className="btn-primary">
              Découvrir le catalogue
            </Link>
            <Link to="/a-propos" className="btn-outline">
              Notre sélection
            </Link>
          </div>
        </div>

        <div className="relative">
          <div className="relative mx-auto aspect-[4/5] w-full max-w-sm overflow-hidden rounded-[2rem] bg-gradient-to-br from-blush via-cream to-blushdark shadow-soft">
            <div className="absolute inset-8 rounded-[1.5rem] border border-white/60" />
            <div className="absolute inset-0 flex items-center justify-center">
              <svg viewBox="0 0 200 200" className="h-40 w-40 text-clay/70" fill="none" stroke="currentColor" strokeWidth="1.2">
                <path d="M100 20c30 40 55 70 55 100a55 55 0 1 1-110 0c0-30 25-60 55-100Z" />
              </svg>
            </div>
          </div>
          <div className="absolute -bottom-4 -left-4 hidden rounded-2xl bg-card px-5 py-4 shadow-card md:block">
            <p className="font-display text-sm text-espresso">"Formulations honnêtes, résultats visibles."</p>
            <p className="mt-1 text-xs text-muted">— L'esprit KSkin</p>
          </div>
        </div>
      </div>
    </section>
  )
}
