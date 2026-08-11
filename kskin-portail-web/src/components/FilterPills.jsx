import React from 'react'

export default function FilterPills({ options, active, onSelect, allLabel = 'Tout' }) {
  return (
    <div className="flex gap-2 overflow-x-auto pb-1 [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
      <button
        onClick={() => onSelect(null)}
        className={`shrink-0 rounded-full border px-4 py-2 text-sm font-medium transition-colors ${
          !active
            ? 'border-espresso bg-espresso text-cream'
            : 'border-espresso/15 text-espresso/70 hover:border-espresso/40'
        }`}
      >
        {allLabel}
      </button>
      {options.map((opt) => (
        <button
          key={opt}
          onClick={() => onSelect(opt)}
          className={`shrink-0 rounded-full border px-4 py-2 text-sm font-medium transition-colors ${
            active === opt
              ? 'border-espresso bg-espresso text-cream'
              : 'border-espresso/15 text-espresso/70 hover:border-espresso/40'
          }`}
        >
          {opt}
        </button>
      ))}
    </div>
  )
}
