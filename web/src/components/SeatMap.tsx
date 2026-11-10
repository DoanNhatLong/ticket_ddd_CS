import { useMemo } from 'react'
import type { TripSeat } from '../api/trainTrips'
import { CarriageCard } from './CarriageCard'

interface SeatMapProps {
  seats: TripSeat[]
  dimmed?: boolean
  onSelectSeat?: (seat: TripSeat) => void
}

/** All seats of a train trip, grouped by carriage. */
export function SeatMap({ seats, dimmed = false, onSelectSeat }: SeatMapProps) {
  const carriages = useMemo(() => {
    const groups = new Map<number, TripSeat[]>()
    for (const seat of seats) {
      const group = groups.get(seat.carriageNumber) ?? []
      group.push(seat)
      groups.set(seat.carriageNumber, group)
    }
    return [...groups.entries()].sort(([a], [b]) => a - b)
  }, [seats])

  if (carriages.length === 0) {
    return <div className="placeholder">This train has no seats yet.</div>
  }

  return (
    <div className={dimmed ? 'carriages dimmed' : 'carriages'}>
      {carriages.map(([carriageNumber, carriageSeats]) => (
        <CarriageCard
          key={carriageNumber}
          carriageNumber={carriageNumber}
          seats={carriageSeats}
          onSelectSeat={onSelectSeat}
        />
      ))}
    </div>
  )
}
