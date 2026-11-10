interface TripTabsProps {
  tripIds: number[]
  activeId: number
  onChange: (tripId: number) => void
}

export function TripTabs({ tripIds, activeId, onChange }: TripTabsProps) {
  return (
    <nav className="trip-tabs" aria-label="Train trips">
      {tripIds.map((id) => (
        <button
          key={id}
          type="button"
          className={id === activeId ? 'trip-tab active' : 'trip-tab'}
          aria-pressed={id === activeId}
          onClick={() => onChange(id)}
        >
          Trip #{id}
        </button>
      ))}
    </nav>
  )
}
