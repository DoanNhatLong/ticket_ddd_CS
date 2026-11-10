interface TopBarProps {
  /** Short label shown on the right, e.g. the current page. */
  note?: string
}

export function TopBar({ note }: TopBarProps) {
  return (
    <header className="topbar">
      <div className="topbar-inner">
        <span className="brand">
          <span className="brand-mark" aria-hidden="true">🚆</span>
          TrainBooking
        </span>
        {note && <span className="topbar-note">{note}</span>}
      </div>
    </header>
  )
}
