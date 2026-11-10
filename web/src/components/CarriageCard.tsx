import type { TripSeat } from '../api/trainTrips'
import { SeatTile } from './SeatTile'
import { seatState } from './seatState'

interface CarriageCardProps {
  carriageNumber: number
  seats: TripSeat[]
  onSelectSeat?: (seat: TripSeat) => void
}

export function CarriageCard({ carriageNumber, seats, onSelectSeat }: CarriageCardProps) {
  const freeCount = seats.filter((s) => seatState(s) === 'free').length

  return (
    <section className="carriage">
      <h2 className="carriage-title">
        Carriage {carriageNumber}
        <span className="carriage-count">{freeCount} free</span>
      </h2>
      <div className="seat-grid">
        {seats.map((seat) => (
          <SeatTile key={seat.seatId} seat={seat} onSelect={onSelectSeat} />
        ))}
      </div>
    </section>
  )
}
