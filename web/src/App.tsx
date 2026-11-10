import { useEffect, useMemo, useState } from 'react'
import { getTripSeats, type TripSeat, type TripSeats } from './api/trainTrips'
import './App.css'

const TRIP_IDS = [1, 2, 3, 4, 5]

const priceFormat = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
const dateFormat = new Intl.DateTimeFormat('vi-VN', { weekday: 'short', day: '2-digit', month: '2-digit', year: 'numeric', timeZone: 'UTC' })
const timeFormat = new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit', timeZone: 'UTC' })

type SeatState = 'free' | 'taken' | 'not-sold'

function seatState(seat: TripSeat): SeatState {
  if (seat.fare === null) return 'not-sold'
  return seat.isFree ? 'free' : 'taken'
}

function formatDuration(from: Date, to: Date): string {
  const minutes = Math.round((to.getTime() - from.getTime()) / 60000)
  const hours = Math.floor(minutes / 60)
  const rest = minutes % 60
  return rest === 0 ? `${hours}h` : `${hours}h ${rest}m`
}

function App() {
  const [tripId, setTripId] = useState(1)
  const [trip, setTrip] = useState<TripSeats | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)

    getTripSeats(tripId)
      .then((data) => {
        if (!cancelled) setTrip(data)
      })
      .catch((err: Error) => {
        if (!cancelled) {
          setTrip(null)
          setError(err.message)
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [tripId])

  const carriages = useMemo(() => {
    const groups = new Map<number, TripSeat[]>()
    for (const seat of trip?.seats ?? []) {
      const seats = groups.get(seat.carriageNumber) ?? []
      seats.push(seat)
      groups.set(seat.carriageNumber, seats)
    }
    return [...groups.entries()].sort(([a], [b]) => a - b)
  }, [trip])

  const stats = useMemo(() => {
    const seats = trip?.seats ?? []
    const sellable = seats.filter((s) => s.fare !== null)
    const fares = sellable.map((s) => s.fare as number)
    return {
      total: seats.length,
      free: sellable.filter((s) => s.isFree).length,
      lowestFare: fares.length > 0 ? Math.min(...fares) : null,
    }
  }, [trip])

  const departure = trip ? new Date(trip.departureTime) : null
  const arrival = trip ? new Date(trip.arrivalTime) : null

  return (
    <div className="app">
      <header className="topbar">
        <div className="topbar-inner">
          <span className="brand">
            <span className="brand-mark" aria-hidden="true">🚆</span>
            TrainBooking
          </span>
          <span className="topbar-note">Seat map</span>
        </div>
      </header>

      <main className="content">
        <nav className="trip-tabs" aria-label="Train trips">
          {TRIP_IDS.map((id) => (
            <button
              key={id}
              type="button"
              className={id === tripId ? 'trip-tab active' : 'trip-tab'}
              onClick={() => setTripId(id)}
            >
              Trip #{id}
            </button>
          ))}
        </nav>

        {error && <div className="alert">{error}</div>}

        {trip && departure && arrival && (
          <section className={loading ? 'trip-card dimmed' : 'trip-card'}>
            <div className="trip-timeline">
              <div className="trip-point">
                <span className="trip-time">{timeFormat.format(departure)}</span>
                <span className="trip-date">{dateFormat.format(departure)}</span>
              </div>
              <div className="trip-line">
                <span className="trip-duration">{formatDuration(departure, arrival)}</span>
              </div>
              <div className="trip-point end">
                <span className="trip-time">{timeFormat.format(arrival)}</span>
                <span className="trip-date">{dateFormat.format(arrival)}</span>
              </div>
            </div>

            <dl className="trip-stats">
              <div>
                <dt>Free seats</dt>
                <dd>
                  {stats.free}
                  <small> / {stats.total}</small>
                </dd>
              </div>
              <div>
                <dt>From</dt>
                <dd>{stats.lowestFare === null ? '—' : priceFormat.format(stats.lowestFare)}</dd>
              </div>
            </dl>
            <p className="trip-footnote">Times shown in UTC.</p>
          </section>
        )}

        {loading && !trip && <div className="placeholder">Loading seats…</div>}

        {trip && (
          <>
            <ul className="legend">
              <li><span className="swatch free" /> Free</li>
              <li><span className="swatch taken" /> Taken</li>
              <li><span className="swatch not-sold" /> Not sold on this trip</li>
            </ul>

            {carriages.length === 0 ? (
              <div className="placeholder">This train has no seats yet.</div>
            ) : (
              <div className={loading ? 'carriages dimmed' : 'carriages'}>
                {carriages.map(([carriageNumber, seats]) => (
                  <section key={carriageNumber} className="carriage">
                    <h2 className="carriage-title">
                      Carriage {carriageNumber}
                      <span className="carriage-count">
                        {seats.filter((s) => seatState(s) === 'free').length} free
                      </span>
                    </h2>
                    <div className="seat-grid">
                      {seats.map((seat) => {
                        const state = seatState(seat)
                        return (
                          <div
                            key={seat.seatId}
                            className={`seat ${state}`}
                            title={`${seat.seatClassName} · ${state === 'not-sold' ? 'not sold' : state}`}
                          >
                            <span className="seat-number">{seat.seatNumber}</span>
                            <span className="seat-class">{seat.seatClassName}</span>
                            <span className="seat-price">
                              {seat.fare === null ? 'Not sold' : priceFormat.format(seat.fare)}
                            </span>
                          </div>
                        )
                      })}
                    </div>
                  </section>
                ))}
              </div>
            )}
          </>
        )}
      </main>
    </div>
  )
}

export default App
