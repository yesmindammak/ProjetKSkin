import React, { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useCart } from '../context/CartContext.jsx'
import { creerDemande, getPointsDeVente, getContactByPhone } from '../services/api.js'
import { IconMinus, IconPlus } from '../components/Icons.jsx'

function formatPrix(prix) {
  return `${Number(prix).toFixed(2)} TND`
}

const GOUVERNORATS = [
  'Tunis', 'Ariana', 'Ben Arous', 'Manouba', 'Nabeul', 'Bizerte', 'Sousse',
  'Monastir', 'Sfax', 'Kairouan', 'Béja', 'Autre / International',
]

const MODES_LIVRAISON = [
  'Livraison à Domicile',
  'Retrait en Magasin (Click & Collect)',
  'Expédition Express 24h',
]

const MODES_PAIEMENT = [
  'Paiement à la Livraison',
  'Carte Bancaire en Ligne',
  'Virement Bancaire',
  'Chèque à la Livraison',
]

const emptyForm = {
  nom: '',
  prenom: '',
  telephone: '',
  email: '',
  gouvernorat: 'Tunis',
  ville: '',
  adresse: '',
  modeLivraison: MODES_LIVRAISON[0],
  modePaiement: MODES_PAIEMENT[0],
  pointDeVenteId: '',
}

export default function Checkout() {
  const { items, total, updateQuantite, clearCart } = useCart()
  const navigate = useNavigate()
  const [form, setForm] = useState(emptyForm)
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)
  const [serverError, setServerError] = useState('')
  const [pointsDeVente, setPointsDeVente] = useState([])
  const [contactFound, setContactFound] = useState(false)

  const isRetrait = form.modeLivraison.startsWith('Retrait')

  useEffect(() => {
    getPointsDeVente().then((list) => {
      setPointsDeVente(list)
      if (list.length > 0) {
        setForm((f) => (f.pointDeVenteId ? f : { ...f, pointDeVenteId: String(list[0].pointDeVenteId) }))
      }
    })
  }, [])

  async function checkExistingContact(phone) {
    if (!phone || phone.trim().length < 6) return
    try {
      const contact = await getContactByPhone(phone.trim())
      if (contact) {
        setForm((f) => ({
          ...f,
          nom: contact.nom || f.nom,
          prenom: contact.prenom || f.prenom,
          email: contact.email || f.email,
          gouvernorat: contact.gouvernorat || f.gouvernorat,
          ville: contact.ville || f.ville,
          adresse: contact.adresse || f.adresse,
        }))
        setContactFound(true)
      }
    } catch (err) {
      // Ignore lookup errors
    }
  }

  function validate() {
    const e = {}
    if (!form.nom.trim()) e.nom = 'Le nom est requis.'
    if (!form.prenom.trim()) e.prenom = 'Le prénom est requis.'
    if (!/^[0-9+\s]{6,}$/.test(form.telephone.trim())) e.telephone = 'Numéro de téléphone invalide.'
    if (form.email && !/^\S+@\S+\.\S+$/.test(form.email)) e.email = 'Adresse email invalide.'
    if (!isRetrait && !form.ville.trim()) e.ville = 'La ville est requise pour la livraison.'
    if (isRetrait && !form.pointDeVenteId) e.pointDeVenteId = 'Choisissez un magasin pour le retrait.'
    setErrors(e)
    return Object.keys(e).length === 0
  }

  async function handleSubmit(ev) {
    ev.preventDefault()
    setServerError('')
    if (items.length === 0 || !validate()) return

    setSubmitting(true)
    try {
      await creerDemande({
        ...form,
        pointDeVenteId: isRetrait ? Number(form.pointDeVenteId) : null,
        items: items.map((i) => ({ produitId: i.produitId, quantite: i.quantite })),
      })
      clearCart()
      navigate('/confirmation')
    } catch (err) {
      setServerError(err.message || "Une erreur est survenue, réessayez dans un instant.")
    } finally {
      setSubmitting(false)
    }
  }

  if (items.length === 0) {
    return (
      <div className="section-pad py-24 text-center">
        <p className="font-display text-2xl text-espresso">Votre panier est vide</p>
        <p className="mt-2 text-sm text-muted">Ajoutez des produits avant de soumettre une demande.</p>
        <Link to="/catalogue" className="btn-primary mt-6 inline-flex">Parcourir le catalogue</Link>
      </div>
    )
  }

  return (
    <div className="section-pad py-12">
      <span className="eyebrow">Dernière étape</span>
      <h1 className="mt-2 font-display text-4xl text-espresso">Votre demande d'achat</h1>
      <p className="mt-2 max-w-lg text-sm text-muted">
        Aucun compte requis. Laissez vos coordonnées et un conseiller vous recontacte pour
        confirmer disponibilité et modalités.
      </p>

      <div className="mt-10 grid gap-10 lg:grid-cols-[1.1fr_0.9fr]">
        <form onSubmit={handleSubmit} noValidate className="space-y-5 rounded-2xl border border-espresso/8 bg-card p-6 shadow-card md:p-8">
          {contactFound && (
            <div className="rounded-xl border border-clay/30 bg-blush/40 px-4 py-3 text-xs font-semibold text-clay flex items-center justify-between">
              <span>✨ Contact existant trouvé ! Vos coordonnées ont été pré-remplies.</span>
              <button type="button" onClick={() => setContactFound(false)} className="text-espresso hover:underline font-bold">×</button>
            </div>
          )}

          <div className="grid gap-5 sm:grid-cols-2">
            <Field label="Téléphone" required error={errors.telephone}>
              <input
                value={form.telephone}
                onChange={(e) => setForm({ ...form, telephone: e.target.value })}
                onBlur={() => checkExistingContact(form.telephone)}
                className="input"
                placeholder="+216 XX XXX XXX"
                autoComplete="tel"
              />
            </Field>
            <Field label="Email (optionnel)" error={errors.email}>
              <input
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                className="input"
                type="email"
                autoComplete="email"
              />
            </Field>
          </div>

          <div className="grid gap-5 sm:grid-cols-2">
            <Field label="Nom" required error={errors.nom}>
              <input
                value={form.nom}
                onChange={(e) => setForm({ ...form, nom: e.target.value })}
                className="input"
                autoComplete="family-name"
              />
            </Field>
            <Field label="Prénom" required error={errors.prenom}>
              <input
                value={form.prenom}
                onChange={(e) => setForm({ ...form, prenom: e.target.value })}
                className="input"
                autoComplete="given-name"
              />
            </Field>
          </div>

          <div className="border-t border-espresso/10 pt-5">
            <h2 className="mb-4 text-sm font-semibold text-espresso">Livraison</h2>
            <div className="grid gap-5 sm:grid-cols-2">
              <Field label="Mode de livraison" required>
                <select
                  value={form.modeLivraison}
                  onChange={(e) => setForm({ ...form, modeLivraison: e.target.value })}
                  className="input"
                >
                  {MODES_LIVRAISON.map((m) => (
                    <option key={m} value={m}>{m}</option>
                  ))}
                </select>
              </Field>
              <Field label="Mode de paiement" required>
                <select
                  value={form.modePaiement}
                  onChange={(e) => setForm({ ...form, modePaiement: e.target.value })}
                  className="input"
                >
                  {MODES_PAIEMENT.map((m) => (
                    <option key={m} value={m}>{m}</option>
                  ))}
                </select>
              </Field>
            </div>

            {isRetrait ? (
              <div className="mt-5">
                <Field label="Magasin de retrait" required error={errors.pointDeVenteId}>
                  <select
                    value={form.pointDeVenteId}
                    onChange={(e) => setForm({ ...form, pointDeVenteId: e.target.value })}
                    className="input"
                  >
                    {pointsDeVente.map((pdv) => (
                      <option key={pdv.pointDeVenteId} value={pdv.pointDeVenteId}>
                        {pdv.nom}{pdv.ville ? ` — ${pdv.ville}` : ''}
                      </option>
                    ))}
                  </select>
                </Field>
              </div>
            ) : (
              <div className="mt-5 grid gap-5 sm:grid-cols-2">
                <Field label="Gouvernorat" required>
                  <select
                    value={form.gouvernorat}
                    onChange={(e) => setForm({ ...form, gouvernorat: e.target.value })}
                    className="input"
                  >
                    {GOUVERNORATS.map((g) => (
                      <option key={g} value={g}>{g}</option>
                    ))}
                  </select>
                </Field>
                <Field label="Ville / Cité" required error={errors.ville}>
                  <input
                    value={form.ville}
                    onChange={(e) => setForm({ ...form, ville: e.target.value })}
                    className="input"
                  />
                </Field>
              </div>
            )}

            {!isRetrait && (
              <div className="mt-5">
                <Field label="Adresse exacte (optionnel)">
                  <textarea
                    value={form.adresse}
                    onChange={(e) => setForm({ ...form, adresse: e.target.value })}
                    className="input min-h-[72px] resize-none"
                    placeholder="Rue, numéro, complément d'adresse…"
                  />
                </Field>
              </div>
            )}
          </div>

          {serverError && (
            <div className="rounded-lg bg-danger/10 px-4 py-3 text-sm text-danger space-y-2">
              <p>{serverError}</p>
              {serverError.toLowerCase().includes('introuvable') && (
                <button
                  type="button"
                  onClick={() => {
                    clearCart()
                    setServerError('')
                  }}
                  className="mt-2 inline-flex items-center rounded-lg border border-danger/30 bg-card px-3 py-1.5 text-xs font-semibold text-danger shadow-sm hover:bg-danger/10 transition-colors"
                >
                  🔄 Vider le panier et sélectionner des produits à jour
                </button>
              )}
            </div>
          )}

          <button type="submit" disabled={submitting} className="btn-primary w-full">
            {submitting ? 'Envoi en cours…' : 'Envoyer ma demande'}
          </button>
        </form>

        <aside className="h-fit rounded-2xl border border-espresso/8 bg-card p-6 shadow-card md:p-8">
          <h2 className="mb-4 font-display text-lg text-espresso">Récapitulatif</h2>
          <ul className="space-y-4">
            {items.map((item) => (
              <li key={item.produitId} className="flex items-center justify-between gap-3 text-sm">
                <div>
                  <p className="font-medium text-espresso">{item.nom}</p>
                  <p className="text-xs text-muted">{item.marque}</p>
                </div>
                <div className="flex items-center gap-2">
                  <div className="flex items-center gap-0.5 rounded-full border border-espresso/15 px-0.5">
                    <button
                      onClick={() => updateQuantite(item.produitId, item.quantite - 1)}
                      className="flex h-6 w-6 items-center justify-center text-espresso hover:text-clay"
                      aria-label="Diminuer"
                      type="button"
                    >
                      <IconMinus className="h-3 w-3" />
                    </button>
                    <span className="w-5 text-center text-xs">{item.quantite}</span>
                    <button
                      onClick={() => updateQuantite(item.produitId, item.quantite + 1)}
                      className="flex h-6 w-6 items-center justify-center text-espresso hover:text-clay"
                      aria-label="Augmenter"
                      type="button"
                    >
                      <IconPlus className="h-3 w-3" />
                    </button>
                  </div>
                  <span className="w-16 text-right font-semibold text-clay">{formatPrix(item.prix * item.quantite)}</span>
                </div>
              </li>
            ))}
          </ul>
          <div className="mt-6 flex items-center justify-between border-t border-espresso/10 pt-4">
            <span className="text-sm text-muted">Total estimé</span>
            <span className="font-display text-2xl text-espresso">{formatPrix(total)}</span>
          </div>
        </aside>
      </div>
    </div>
  )
}

function Field({ label, required, error, children }) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-sm font-medium text-espresso">
        {label} {required && <span className="text-clay">*</span>}
      </span>
      {children}
      {error && <span className="mt-1 block text-xs text-danger">{error}</span>}
    </label>
  )
}
