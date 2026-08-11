import React, { useState } from 'react'
import { IconInstagram, IconPhone, IconClose } from './Icons.jsx'

const INSTAGRAM_URL = 'https://www.instagram.com/kskin_tn/'
const PHONE_TEL = '+21628134234'
const WHATSAPP_URL = `https://wa.me/${PHONE_TEL.replace('+', '')}`

export default function QuickContact() {
  const [open, setOpen] = useState(false)

  return (
    <div className="fixed bottom-6 right-5 z-50 flex flex-col items-end gap-3">
      <div
        className={`flex flex-col items-end gap-3 transition-all duration-300 ${
          open ? 'translate-y-0 opacity-100' : 'pointer-events-none translate-y-3 opacity-0'
        }`}
      >
        <a
          href={WHATSAPP_URL}
          target="_blank"
          rel="noreferrer"
          className="flex items-center gap-2 rounded-full bg-white py-2.5 pl-4 pr-2.5 text-sm font-medium text-espresso shadow-soft transition-transform hover:-translate-y-0.5"
        >
          Discuter sur WhatsApp
          <span className="flex h-8 w-8 items-center justify-center rounded-full bg-clay text-white">
            <IconPhone className="h-4 w-4" />
          </span>
        </a>
        <a
          href={INSTAGRAM_URL}
          target="_blank"
          rel="noreferrer"
          className="flex items-center gap-2 rounded-full bg-white py-2.5 pl-4 pr-2.5 text-sm font-medium text-espresso shadow-soft transition-transform hover:-translate-y-0.5"
        >
          @kskin_tn
          <span className="flex h-8 w-8 items-center justify-center rounded-full bg-gold text-espresso">
            <IconInstagram className="h-4 w-4" />
          </span>
        </a>
      </div>

      <button
        onClick={() => setOpen((v) => !v)}
        aria-label={open ? 'Fermer le menu de contact' : 'Ouvrir le menu de contact'}
        aria-expanded={open}
        className="flex h-14 w-14 items-center justify-center rounded-full bg-espresso text-gold shadow-soft transition-transform hover:scale-105 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-clay"
      >
        {open ? <IconClose className="h-6 w-6" /> : <IconInstagram className="h-6 w-6" />}
      </button>
    </div>
  )
}
