type Props = {
  iconKey: string
  label?: string
  size?: 'small' | 'large'
}

export function VehicleIcon({ iconKey, label, size = 'small' }: Props) {
  const isTruck = iconKey === 'heavy' || iconKey === 'truck'
  const isVan = iconKey === 'medium' || iconKey === 'van'

  return (
    <span className={`vehicle-icon vehicle-icon--${size}`} title={label} aria-label={label}>
      <svg viewBox="0 0 64 40" role="img" aria-hidden="true">
        {isTruck ? (
          <>
            <path d="M5 8h32v22H5zM37 16h12l9 9v5H37z" />
            <path className="icon-window" d="M41 19h7l5 6H41z" />
          </>
        ) : isVan ? (
          <>
            <path d="M6 11h39l12 13v7H6z" />
            <path className="icon-window" d="M36 15h8l7 9H36z" />
          </>
        ) : (
          <>
            <path d="M8 23l7-11h28l10 11 5 2v6H5v-6z" />
            <path className="icon-window" d="M18 15h11v8H13zM32 15h9l7 8H32z" />
          </>
        )}
        <circle cx="17" cy="31" r="5" />
        <circle cx="48" cy="31" r="5" />
      </svg>
    </span>
  )
}
