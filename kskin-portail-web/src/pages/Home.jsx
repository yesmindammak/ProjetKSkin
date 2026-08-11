import React, { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import Hero from '../components/Hero.jsx'
import ProductCard from '../components/ProductCard.jsx'
import InstagramStrip from '../components/InstagramStrip.jsx'
import { getProduits, getMarquesDetails } from '../services/api.js'

const ROUTINE_STEPS = [
  {
    num: '1',
    title: 'Huile démaquillante',
    desc: 'Dissout le maquillage et le sébum sans agresser la peau.',
    icon: (
      <svg className="h-8 w-8 text-clay" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.5">
        <path strokeLinecap="round" strokeLinejoin="round" d="M9.75 3.104v5.714a2.25 2.25 0 0 1-.659 1.591L5 14.5M9.75 3.104c-.251.023-.501.05-.75.082m.75-.082a24.301 24.301 0 0 1 4.5 0m0 0v5.714c0 .597.237 1.17.659 1.591L19 14.5M14.25 3.104c.251.023.501.05.75.082M19 14.5a3.75 3.75 0 0 1-3.75 3.75H8.75A3.75 3.75 0 0 1 5 14.5m14 0V9a2.25 2.25 0 0 0-2.25-2.25H7.25A2.25 2.25 0 0 0 5 9v5.5" />
      </svg>
    ),
  },
  {
    num: '2',
    title: 'Nettoyant mousse',
    desc: 'Nettoie en douceur et laisse la peau fraîche.',
    icon: (
      <svg className="h-8 w-8 text-clay" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.5">
        <path strokeLinecap="round" strokeLinejoin="round" d="M12 21a9 9 0 0 0 6-15.683A8.966 8.966 0 0 0 12 3a8.966 8.966 0 0 0-6 2.317A9 9 0 0 0 12 21Z" />
      </svg>
    ),
  },
  {
    num: '3',
    title: 'Toner',
    desc: 'Rééquilibre le pH et prépare la peau à recevoir les soins.',
    icon: (
      <svg className="h-8 w-8 text-clay" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.5">
        <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v18m-6-6 6 6 6-6" />
      </svg>
    ),
  },
  {
    num: '4',
    title: 'Sérum',
    desc: "Apporte une concentration d'actifs pour une peau éclatante.",
    icon: (
      <svg className="h-8 w-8 text-clay" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.5">
        <path strokeLinecap="round" strokeLinejoin="round" d="m11.25 11.25.041-.02a.75.75 0 0 1 1.063.852l-.708 2.836a.75.75 0 0 0 1.063.853l.041-.021M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9-3.75h.008v.008H12V8.25Z" />
      </svg>
    ),
  },
  {
    num: '5',
    title: 'Crème hydratante',
    desc: 'Nourrit et protège la barrière cutanée.',
    icon: (
      <svg className="h-8 w-8 text-clay" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.5">
        <path strokeLinecap="round" strokeLinejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12Z" />
      </svg>
    ),
  },
  {
    num: '6',
    title: 'Écran solaire',
    desc: 'Protège la peau des rayons UV, dernière étape essentielle.',
    icon: (
      <svg className="h-8 w-8 text-clay" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.5">
        <path strokeLinecap="round" strokeLinejoin="round" d="M12 3v2.25m6.364.386-1.591 1.591M21 12h-2.25m-.386 6.364-1.591-1.591M12 18.75V21m-4.773-4.227-1.591 1.591M5.25 12H3m4.227-4.773L5.636 5.636M15.75 12a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0Z" />
      </svg>
    ),
  },
]

const FAQ_ITEMS = [
  {
    question: 'Comment savoir si vos produits sont authentiques ?',
    answer:
      'Chez KSkin, nous sommes engagés à vous offrir 100% de produits coréens authentiques. Tous nos soins sont directement issus des circuits officiels des marques (Purito, Innisfree, Numbuzin, I\'m From, Round Lab, etc.), garantissant une fraîcheur et une qualité optimales sans contrefaçon.',
  },
  {
    question: 'Comment assurez-vous la qualité et la sécurité de vos produits ?',
    answer:
      'Tous nos produits sont stockés dans des locaux sécurisés et spécialement adaptés à la conservation des cosmétiques , avec un cntrôle strict de la température et de l\'humidité. Nous nous engageons à respecter les normes de sécurité et d\'hygiène les plus strictes pour garantir que chaque produit que vous recevez est sûr et efficace.',
  },
  {
    question: 'Comment soumettre une demande d\'achat ?',
    answer:
      'Parcourez notre catalogue, ajoutez vos soins au panier, puis remplissez votre formulaire de coordonnées lors de la validation. Notre système transmet automatiquement votre demande à nos équipes pour confirmation instantanée.',
  },
  {
    question: 'Quels sont les délais de livraison et points de retrait ?',
    answer:
      'Nous proposons la livraison à domicile dans toute la Tunisie ainsi que le retrait Click & Collect dans notre réseau de magasins partenaires (Tunis Charguia, Manar, Sousse, Hammamet, Sfax, Bizerte).',
  },
]

export default function Home() {
  const [produits, setProduits] = useState([])
  const [marques, setMarques] = useState([])
  const [loading, setLoading] = useState(true)
  const [openFaq, setOpenFaq] = useState(null)

  useEffect(() => {
    Promise.all([getProduits(), getMarquesDetails()]).then(([prodsData, marquesData]) => {
      setProduits(prodsData.slice(0, 4))
      setMarques(marquesData)
      setLoading(false)
    })
  }, [])

  return (
    <>
      <Hero />

      {/* Marques Section */}
      <section className="section-pad py-14">
        <div className="mb-8 flex items-end justify-between">
          <div>
            <span className="eyebrow">Nos marques partenaires</span>
            <h2 className="mt-2 font-display text-3xl text-espresso md:text-4xl">Parcourir par marque</h2>
          </div>
          <Link to="/catalogue" className="text-sm font-semibold text-clay hover:underline">
            Voir tous les produits &rarr;
          </Link>
        </div>
        <div className="grid grid-cols-2 gap-5 sm:grid-cols-3 md:grid-cols-5">
          {marques.map((marque) => {
            const nomMarque = typeof marque === 'string' ? marque : marque.nom
            const imageUrl = typeof marque === 'object' ? marque.imageUrl : null
            return (
              <Link
                key={nomMarque}
                to={`/catalogue?marque=${encodeURIComponent(nomMarque)}`}
                className="group flex flex-col items-center justify-center rounded-2xl border border-espresso/8 bg-card p-6 shadow-card transition-all hover:-translate-y-1.5 hover:border-clay/40 hover:shadow-soft text-center min-h-[170px]"
              >
                {imageUrl ? (
                  <img
                    src={imageUrl}
                    alt={nomMarque}
                    className="mb-5 h-40 max-h-80 w-auto max-w-full object-contain transition-transform duration-300 group-hover:scale-105"
                  />
                ) : null}
                <span className="font-display text-lg font-semibold text-espresso group-hover:text-clay">
                  {nomMarque}
                </span>
              </Link>
            )
          })}
        </div>
      </section>

      {/* Routine Skincare Steps Section */}
      <section className="section-pad py-16 bg-cream/50">
        <div className="mx-auto max-w-3xl text-center">
          <h2 className="font-display text-3xl text-espresso md:text-4xl">
            Les étapes de votre routine skincare ✨
          </h2>
          <p className="mt-4 text-sm leading-relaxed text-muted">
            Une belle peau ne se construit pas en un seul geste mais dans la régularité de chaque soin.
            Chacune de ces étapes se complète, entre purification, hydratation et protection pour révéler jour après jour l'éclat naturel de votre peau.
          </p>
        </div>

        <div className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {ROUTINE_STEPS.map((step) => (
            <div
              key={step.num}
              className="flex flex-col items-center rounded-2xl border border-espresso/8 bg-card p-6 text-center shadow-card transition-all hover:-translate-y-1 hover:shadow-soft"
            >
              <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-blush">
                {step.icon}
              </div>
              <h3 className="font-display text-lg text-espresso">
                {step.num}. {step.title}
              </h3>
              <p className="mt-2 text-xs leading-relaxed text-muted">{step.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* Incontournables Products Section */}
      <section className="section-pad py-14">
        <div className="mb-8 flex flex-wrap items-end justify-between gap-3">
          <div>
            <span className="eyebrow">Sélection du moment</span>
            <h2 className="mt-2 font-display text-3xl text-espresso md:text-4xl">Nos incontournables</h2>
          </div>
          <Link to="/catalogue" className="btn-outline">
            Voir tout le catalogue
          </Link>
        </div>

        {loading ? (
          <div className="grid grid-cols-2 gap-5 md:grid-cols-4">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="aspect-[4/5] animate-pulse rounded-2xl bg-blush/60" />
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-5 md:grid-cols-4">
            {produits.map((p) => (
              <ProductCard key={p.produitId} produit={p} />
            ))}
          </div>
        )}
      </section>

      {/* FAQ Accordion Section */}
      <section className="section-pad py-16">
        <div className="mx-auto max-w-3xl">
          <h2 className="mb-8 font-display text-3xl text-espresso md:text-4xl text-center">
            Les questions que vous nous posez le plus
          </h2>
          <div className="space-y-4">
            {FAQ_ITEMS.map((item, idx) => {
              const isOpen = openFaq === idx
              return (
                <div
                  key={idx}
                  className="rounded-2xl border border-espresso/8 bg-card shadow-card transition-all"
                >
                  <button
                    onClick={() => setOpenFaq(isOpen ? null : idx)}
                    className="flex w-full items-center justify-between p-6 text-left font-display text-lg text-espresso hover:text-clay"
                  >
                    <span>{item.question}</span>
                    <span className="ml-4 font-sans text-xl font-bold">{isOpen ? '−' : '+'}</span>
                  </button>
                  {isOpen && (
                    <div className="px-6 pb-6 pt-0 text-sm leading-relaxed text-muted border-t border-espresso/5">
                      <p className="mt-3">{item.answer}</p>
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        </div>
      </section>

      <InstagramStrip />
    </>
  )
}
