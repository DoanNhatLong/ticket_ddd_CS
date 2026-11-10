import type { TripSeat } from '../api/trainTrips'
import { formatPrice } from '../utils/format'
import { seatState } from './seatState'

interface SeatTileProps {
  seat: TripSeat
  selected?: boolean
  /** Called when a free seat is clicked. Leave it out for a read-only seat map. */
  onSelect?: (seat: TripSeat) => void
}

export function SeatTile({ seat, selected = false, onSelect }: SeatTileProps) {
  const state = seatState(seat)
  const selectable = state === 'free' && onSelect !== undefined

  const className = ['seat', state, selectable ? 'selectable' : '', selected ? 'selected' : '']
    .filter(Boolean)
    .join(' ')

  return (
    <div
      className={className}
      title={`${seat.seatClassName} · ${state === 'not-sold' ? 'not sold' : state}`}
      role={selectable ? 'button' : undefined}
      tabIndex={selectable ? 0 : undefined}
      aria-pressed={selectable ? selected : undefined}
      onClick={selectable ? () => onSelect(seat) : undefined}
      onKeyDown={
        selectable
          ? (e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault()
                onSelect(seat)
              }
            }
          : undefined
      }
    >
      <span className="seat-number">
        {seat.seatNumber}
        {selected && <span className="seat-check" aria-hidden="true">✓</span>}
      </span>
      <span className="seat-class">{seat.seatClassName}</span>
      <span className="seat-price">{seat.fare === null ? 'Not sold' : formatPrice(seat.fare)}</span>
    </div>
  )
}
