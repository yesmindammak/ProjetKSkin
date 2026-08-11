import React, { useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import { useCart } from '../context/CartContext.jsx'
import { IconBag, IconMenu, IconClose } from './Icons.jsx'

const links = [
  { to: '/', label: 'Accueil' },
  { to: '/catalogue', label: 'Catalogue' },
  { to: '/a-propos', label: 'À propos' },
  { to: '/contact', label: 'Contact' },
]

export default function Navbar() {
  const { count, setIsOpen } = useCart()
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <header className="sticky top-0 z-40 border-b border-espresso/10 bg-cream/90 backdrop-blur">
      <div className="section-pad flex h-18 items-center justify-between py-4">
        <Link to="/" className="flex items-center gap-2" onClick={() => setMobileOpen(false)}>
          <span className="flex h-9 w-9 items-center justify-center rounded-full bg-espresso text-gold">
            <svg viewBox="0 0 24 24" className="h-5 w-5" fill="currentColor">
              <path d="M12 2c4 5 6.5 8.5 6.5 12.2A6.5 6.5 0 1 1 5.5 14.2C5.5 10.5 8 7 12 2Z" />
            </svg>
          </span>
          <span className="font-display text-2xl tracking-tight text-espresso">KSkin</span>
        </Link>

        <nav className="hidden items-center gap-8 md:flex">
          {links.map((l) => (
            <NavLink
              key={l.to}
              to={l.to}
              className={({ isActive }) =>
                `text-sm font-medium tracking-wide transition-colors hover:text-clay ${
                  isActive ? 'text-clay' : 'text-espresso/80'
                }`
              }
            >
              {l.label}
            </NavLink>
          ))}
        </nav>

        <div className="flex items-center gap-3">
          <button
            onClick={() => setIsOpen(true)}
            className="relative flex h-10 w-10 items-center justify-center rounded-full text-espresso transition-colors hover:bg-blush"
            aria-label="Voir le panier"
          >
            <IconBag className="h-5 w-5" />
            {count > 0 && (
              <span className="absolute -right-1 -top-1 flex h-5 min-w-[20px] items-center justify-center rounded-full bg-clay px-1 text-[11px] font-semibold text-white">
                {count}
              </span>
            )}
          </button>
          <button
            className="flex h-10 w-10 items-center justify-center rounded-full text-espresso hover:bg-blush md:hidden"
            onClick={() => setMobileOpen((v) => !v)}
            aria-label="Ouvrir le menu"
          >
            {mobileOpen ? <IconClose className="h-5 w-5" /> : <IconMenu className="h-5 w-5" />}
          </button>
        </div>
      </div>

      {mobileOpen && (
        <nav className="border-t border-espresso/10 bg-cream md:hidden">
          <div className="section-pad flex flex-col gap-1 py-3">
            {links.map((l) => (
              <NavLink
                key={l.to}
                to={l.to}
                onClick={() => setMobileOpen(false)}
                className={({ isActive }) =>
                  `rounded-lg px-3 py-2.5 text-sm font-medium ${
                    isActive ? 'bg-blush text-clay' : 'text-espresso/80 hover:bg-blush/60'
                  }`
                }
              >
                {l.label}
              </NavLink>
            ))}
          </div>
        </nav>
      )}
    </header>
  )
}
