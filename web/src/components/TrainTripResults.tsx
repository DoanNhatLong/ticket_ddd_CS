import type { TrainTripSearchItem, TrainTripSearchPage } from '../api/trainTripSearch'
import { formatDate, formatDuration, formatPrice, formatTime } from '../utils/format'

interface TrainTripResultsProps {
  result: TrainTripSearchPage | null
  loading: boolean
  error: string | null
  selectedTripId: number
  onSelect: (trainTripId: number) => void
  onPageChange: (page: number) => void
}

/** Search results: one card per train trip, with paging. */
export function TrainTripResults({
  result,
  loading,
  error,
  selectedTripId,
  onSelect,
  onPageChange,
}: TrainTripResultsProps) {
  if (error) {
    return (
      <div className="alert" role="alert">
        {error}
      </div>
    )
  }

  if (!result) {
    return <div className="placeholder">Searching…</div>
  }

  const pageCount = Math.max(1, Math.ceil(result.total / result.pageSize))

  return (
    <section className={loading ? 'trip-results dimmed' : 'trip-results'} aria-live="polite" aria-busy={loading}>
      <p className="trip-results-count">
        {result.total === 0
          ? 'No train trip matches. Try fewer criteria.'
          : `${result.total} ${result.total === 1 ? 'train trip' : 'train trips'} found`}
      </p>

      <ul className="trip-results-list">
        {result.items.map((trip) => (
          <TrainTripResult
            key={trip.trainTripId}
            trip={trip}
            selected={trip.trainTripId === selectedTripId}
            onSelect={() => onSelect(trip.trainTripId)}
          />
        ))}
      </ul>

      {pageCount > 1 && (
        <nav className="trip-results-pages" aria-label="Result pages">
          <button
            type="button"
            className="secondary-button"
            disabled={loading || result.page <= 1}
            onClick={() => onPageChange(result.page - 1)}
          >
            Previous
          </button>
          <span>
            Page {result.page} of {pageCount}
          </span>
          <button
            type="button"
            className="secondary-button"
            disabled={loading || result.page >= pageCount}
            onClick={() => onPageChange(result.page + 1)}
          >
            Next
          </button>
        </nav>
      )}
    </section>
  )
}

interface TrainTripResultProps {
  trip: TrainTripSearchItem
  selected: boolean
  onSelect: () => void
}

function TrainTripResult({ trip, selected, onSelect }: TrainTripResultProps) {
  const departure = new Date(trip.departureTime)
  const arrival = new Date(trip.arrivalTime)

  return (
    <li className={selected ? 'trip-result selected' : 'trip-result'}>
      <div className="trip-result-train">
        <strong>{trip.trainCode}</strong>
        <small>{trip.trainName}</small>
      </div>

      <div className="trip-result-route">
        <div>
          <strong>{formatTime(departure)}</strong>
          <span>{trip.departure.name}</span>
          <small>{formatDate(departure)}</small>
        </div>
        <span className="trip-result-duration">{formatDuration(departure, arrival)} →</span>
        <div>
          <strong>{formatTime(arrival)}</strong>
          <span>{trip.arrival.name}</span>
          <small>{formatDate(arrival)}</small>
        </div>
      </div>

      <div className="trip-result-side">
        <span className="trip-result-fare">
          {trip.lowestFare === null ? 'No fares yet' : `From ${formatPrice(trip.lowestFare)}`}
        </span>
        {trip.status !== 'Scheduled' && <span className="status-badge">{trip.status}</span>}
        <button
          type="button"
          className={selected ? 'secondary-button' : 'primary-button'}
          aria-pressed={selected}
          onClick={onSelect}
        >
          {selected ? 'Selected' : 'Choose seats'}
        </button>
      </div>
    </li>
  )
}
