import type { ReactNode } from 'react'

interface TopBarProps {
  /** Content shown on the right, e.g. the customer picker. */
  children?: ReactNode
}

export function TopBar({ children }: TopBarProps) {
  return (
    <header className="topbar">
      <div className="topbar-inner">
        <span className="brand">
          <span className="brand-mark" aria-hidden="true">🚆</span>
          TrainBooking
        </span>
        {children}
      </div>
    </header>
  )
}
