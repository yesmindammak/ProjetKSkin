import React from 'react'
import { Link } from 'react-router-dom'
import { IconInstagram, IconPhone, IconLeaf } from './Icons.jsx'

const INSTAGRAM_URL = 'https://www.instagram.com/kskin_tn/'
const PHONE_DISPLAY = '+216 28 134 234'
const PHONE_TEL = '+21628134234'
const WHATSAPP_URL = `https://wa.me/${PHONE_TEL.replace('+', '')}`

export default function Footer() {
  return (
    <footer className="mt-24 bg-espresso text-cream/80">
      <div className="section-pad grid gap-10 py-16 md:grid-cols-[1.3fr_1fr_1fr_1.1fr]">
        <div>
          <div className="mb-4 flex items-center gap-2">
            <span className="flex h-9 w-9 items-center justify-center rounded-full bg-gold text-espresso">
              <IconLeaf className="h-4 w-4" />
            </span>
            <span className="font-display text-2xl text-cream">KSkin</span>
          </div>
          <p className="max-w-xs text-sm leading-relaxed text-cream/60">
            Une sélection pointue de skincare coréen — nettoyants, sérums, hydratants et
            protection solaire, choisis pour leur formulation et leur efficacité réelle.
          </p>
        </div>

        <div>
          <h4 className="eyebrow mb-4 text-gold">Navigation</h4>
          <ul className="space-y-2.5 text-sm">
            <li><Link to="/" className="transition-colors hover:text-gold">Accueil</Link></li>
            <li><Link to="/catalogue" className="transition-colors hover:text-gold">Catalogue</Link></li>
            <li><Link to="/a-propos" className="transition-colors hover:text-gold">À propos</Link></li>
            <li><Link to="/contact" className="transition-colors hover:text-gold">Contact</Link></li>
          </ul>
        </div>

        <div>
          <h4 className="eyebrow mb-4 text-gold">Comment ça marche</h4>
          <ul className="space-y-2.5 text-sm text-cream/70">
            <li>1. Parcourez le catalogue</li>
            <li>2. Ajoutez vos produits</li>
            <li>3. Envoyez votre demande</li>
            <li>4. On vous contacte pour confirmer</li>
          </ul>
        </div>

        <div>
          <h4 className="eyebrow mb-4 text-gold">Contact</h4>
          <a
            href={`tel:${PHONE_TEL}`}
            className="mb-3 flex items-center gap-3 text-sm transition-colors hover:text-gold"
          >
            <span className="flex h-9 w-9 items-center justify-center rounded-full border border-cream/20">
              <IconPhone className="h-4 w-4" />
            </span>
            {PHONE_DISPLAY}
          </a>
          <a
            href={INSTAGRAM_URL}
            target="_blank"
            rel="noreferrer"
            className="mb-3 flex items-center gap-3 text-sm transition-colors hover:text-gold"
          >
            <span className="flex h-9 w-9 items-center justify-center rounded-full border border-cream/20">
              <IconInstagram className="h-4 w-4" />
            </span>
            @kskin_tn
          </a>
          <a
            href={WHATSAPP_URL}
            target="_blank"
            rel="noreferrer"
            className="btn-primary mt-2 w-full !py-2.5 text-xs"
          >
            Écrire sur WhatsApp
          </a>
        </div>
      </div>

      <div className="border-t border-cream/10">
        <div className="section-pad flex flex-col items-center justify-between gap-2 py-5 text-xs text-cream/50 md:flex-row">
          <p>© {new Date().getFullYear()} KSkin. Tous droits réservés.</p>
          <p>Demandes soumises depuis le portail sont traitées sous 24–48h.</p>
        </div>
      </div>
    </footer>
  )
}
