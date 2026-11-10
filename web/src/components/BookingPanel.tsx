import type { FormEvent } from 'react'
import { isPassengerComplete, type SelectedSeat } from '../booking/selection'
import { formatPrice } from '../utils/format'
import { SelectedSeatCard } from './SelectedSeatCard'

interface BookingPanelProps {
  seats: SelectedSeat[]
  submitting: boolean
  error: string | null
  onChangeSeat: (seat: SelectedSeat, changes: Partial<SelectedSeat>) => void
  onRemoveSeat: (seat: SelectedSeat) => void
  onClear: () => void
  onSubmit: () => void
}

/** The customer's picked seats, the passenger details for each, and the "Book" action. */
export function BookingPanel({
  seats,
  submitting,
  error,
  onChangeSeat,
  onRemoveSeat,
  onClear,
  onSubmit,
}: BookingPanelProps) {
  const total = seats.reduce((sum, s) => sum + s.fare, 0)
  const ready = seats.length > 0 && seats.every(isPassengerComplete)

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    if (ready && !submitting) onSubmit()
  }

  return (
    <aside className="booking-panel">
      <form onSubmit={handleSubmit}>
        <div className="panel-head">
          <h2>Your booking</h2>
          {seats.length > 0 && (
            <button type="button" className="link-button" onClick={onClear} disabled={submitting}>
              Clear
            </button>
          )}
        </div>

        {seats.length === 0 ? (
          <p className="panel-empty">Pick one or more free seats on the seat map. You can mix seats from different trips.</p>
        ) : (
          <ul className="selected-seats">
            {seats.map((seat, index) => (
              <SelectedSeatCard
                key={`${seat.tripId}:${seat.seatId}`}
                seat={seat}
                index={index}
                disabled={submitting}
                onChange={(changes) => onChangeSeat(seat, changes)}
                onRemove={() => onRemoveSeat(seat)}
              />
            ))}
          </ul>
        )}

        {error && <div className="alert">{error}</div>}

        <div className="panel-footer">
          <div className="panel-total">
            <span>
              Total · {seats.length} {seats.length === 1 ? 'ticket' : 'tickets'}
            </span>
            <strong>{formatPrice(total)}</strong>
          </div>
          <button type="submit" className="primary-button" disabled={!ready || submitting}>
            {submitting ? 'Booking…' : 'Book seats'}
          </button>
          {seats.length > 0 && !ready && (
            <p className="panel-hint">Fill in a passenger name and ID document number for every seat.</p>
          )}
          <p className="panel-hint">Seats are held for 5 minutes until you pay.</p>
        </div>
      </form>
    </aside>
  )
}
