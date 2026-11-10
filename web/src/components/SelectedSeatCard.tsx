import type { SelectedSeat } from '../booking/selection'
import { formatPrice } from '../utils/format'

interface SelectedSeatCardProps {
  seat: SelectedSeat
  index: number
  disabled?: boolean
  onChange: (changes: Partial<Pick<SelectedSeat, 'passengerName' | 'passengerIdNumber'>>) => void
  onRemove: () => void
}

/** One picked seat with the passenger fields for its ticket. */
export function SelectedSeatCard({ seat, index, disabled = false, onChange, onRemove }: SelectedSeatCardProps) {
  const nameId = `passenger-name-${seat.tripId}-${seat.seatId}`
  const idNumberId = `passenger-id-${seat.tripId}-${seat.seatId}`

  return (
    <li className="selected-seat">
      <div className="selected-seat-head">
        <span className="selected-seat-title">
          <span className="ticket-index">{index + 1}</span>
          Trip #{seat.tripId} · Carriage {seat.carriageNumber} · <strong>{seat.seatNumber}</strong>
        </span>
        <button type="button" className="link-button" onClick={onRemove} disabled={disabled}>
          Remove
        </button>
      </div>
      <div className="selected-seat-meta">
        {seat.seatClassName} · {formatPrice(seat.fare)}
      </div>

      <div className="field-row">
        <label className="field" htmlFor={nameId}>
          <span>Passenger name</span>
          <input
            id={nameId}
            value={seat.passengerName}
            placeholder="Nguyen Van A"
            maxLength={100}
            disabled={disabled}
            onChange={(e) => onChange({ passengerName: e.target.value })}
          />
        </label>
        <label className="field" htmlFor={idNumberId}>
          <span>ID document number</span>
          <input
            id={idNumberId}
            value={seat.passengerIdNumber}
            placeholder="001099012345"
            maxLength={20}
            disabled={disabled}
            onChange={(e) => onChange({ passengerIdNumber: e.target.value })}
          />
        </label>
      </div>
    </li>
  )
}
