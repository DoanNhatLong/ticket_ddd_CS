import type { TripSeat } from '../api/trainTrips'
import { SeatTile } from './SeatTile'
import { seatState } from './seatState'

interface CarriageCardProps {
  carriageNumber: number
  seats: TripSeat[]
  selectedSeatIds?: ReadonlySet<number>
  onSelectSeat?: (seat: TripSeat) => void
}

export function CarriageCard({ carriageNumber, seats, selectedSeatIds, onSelectSeat }: CarriageCardProps) {
  const freeCount = seats.filter((s) => seatState(s) === 'free').length

  return (
    <section className="carriage">
      <h2 className="carriage-title">
        Carriage {carriageNumber}
        <span className="carriage-count">{freeCount} free</span>
      </h2>
      <div className="seat-grid">
        {seats.map((seat) => (
          <SeatTile
            key={seat.seatId}
            seat={seat}
            selected={selectedSeatIds?.has(seat.seatId) ?? false}
            onSelect={onSelectSeat}
          />
        ))}
      </div>
    </section>
  )
}
