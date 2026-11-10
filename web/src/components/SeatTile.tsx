import type { TripSeat } from '../api/trainTrips'
import { formatPrice } from '../utils/format'
import { seatState } from './seatState'

interface SeatTileProps {
  seat: TripSeat
  /** Called when a free seat is clicked. Leave it out for a read-only seat map. */
  onSelect?: (seat: TripSeat) => void
}

export function SeatTile({ seat, onSelect }: SeatTileProps) {
  const state = seatState(seat)
  const selectable = state === 'free' && onSelect !== undefined

  return (
    <div
      className={`seat ${state}`}
      title={`${seat.seatClassName} · ${state === 'not-sold' ? 'not sold' : state}`}
      role={selectable ? 'button' : undefined}
      tabIndex={selectable ? 0 : undefined}
      onClick={selectable ? () => onSelect(seat) : undefined}
    >
      <span className="seat-number">{seat.seatNumber}</span>
      <span className="seat-class">{seat.seatClassName}</span>
      <span className="seat-price">{seat.fare === null ? 'Not sold' : formatPrice(seat.fare)}</span>
    </div>
  )
}
