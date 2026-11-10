import type { TripSeats } from '../api/trainTrips'
import { formatDate, formatDuration, formatPrice, formatTime } from '../utils/format'

interface TripSummaryProps {
  trip: TripSeats
  /** Dims the card while a different trip is loading. */
  dimmed?: boolean
}

export function TripSummary({ trip, dimmed = false }: TripSummaryProps) {
  const departure = new Date(trip.departureTime)
  const arrival = new Date(trip.arrivalTime)

  const sellable = trip.seats.filter((s) => s.fare !== null)
  const freeCount = sellable.filter((s) => s.isFree).length
  const fares = sellable.map((s) => s.fare as number)
  const lowestFare = fares.length > 0 ? Math.min(...fares) : null

  return (
    <section className={dimmed ? 'trip-card dimmed' : 'trip-card'}>
      <div className="trip-timeline">
        <TripPoint date={departure} />
        <div className="trip-line">
          <span className="trip-duration">{formatDuration(departure, arrival)}</span>
        </div>
        <TripPoint date={arrival} alignEnd />
      </div>

      <dl className="trip-stats">
        <div>
          <dt>Free seats</dt>
          <dd>
            {freeCount}
            <small> / {trip.seats.length}</small>
          </dd>
        </div>
        <div>
          <dt>From</dt>
          <dd>{lowestFare === null ? '—' : formatPrice(lowestFare)}</dd>
        </div>
      </dl>

      <p className="trip-footnote">Times shown in UTC.</p>
    </section>
  )
}

interface TripPointProps {
  date: Date
  alignEnd?: boolean
}

function TripPoint({ date, alignEnd = false }: TripPointProps) {
  return (
    <div className={alignEnd ? 'trip-point end' : 'trip-point'}>
      <span className="trip-time">{formatTime(date)}</span>
      <span className="trip-date">{formatDate(date)}</span>
    </div>
  )
}
