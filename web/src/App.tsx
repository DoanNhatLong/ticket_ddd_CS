import { useEffect, useState } from 'react'
import { getTripSeats, type TripSeats } from './api/trainTrips'
import './App.css'

const priceFormat = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })
const timeFormat = new Intl.DateTimeFormat('vi-VN', { dateStyle: 'short', timeStyle: 'short', timeZone: 'UTC' })

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

  return (
    <main className="page">
      <h1>TrainBooking</h1>

      <label className="trip-picker">
        Train trip
        <select value={tripId} onChange={(e) => setTripId(Number(e.target.value))}>
          {[1, 2, 3, 4, 5].map((id) => (
            <option key={id} value={id}>
              #{id}
            </option>
          ))}
        </select>
      </label>

      {loading && <p>Loading seats…</p>}
      {error && <p className="error">{error}</p>}

      {trip && !loading && (
        <>
          <p className="trip-times">
            Departs {timeFormat.format(new Date(trip.departureTime))} UTC · arrives{' '}
            {timeFormat.format(new Date(trip.arrivalTime))} UTC
          </p>

          {trip.seats.length === 0 ? (
            <p>This train has no seats yet.</p>
          ) : (
            <table className="seats">
              <thead>
                <tr>
                  <th>Carriage</th>
                  <th>Seat</th>
                  <th>Seat class</th>
                  <th>Fare</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {trip.seats.map((seat) => (
                  <tr key={seat.seatId}>
                    <td>{seat.carriageNumber}</td>
                    <td>{seat.seatNumber}</td>
                    <td>{seat.seatClassName}</td>
                    <td>{seat.fare === null ? 'Not sold' : priceFormat.format(seat.fare)}</td>
                    <td className={seat.isFree ? 'free' : 'taken'}>{seat.isFree ? 'Free' : 'Taken'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </>
      )}
    </main>
  )
}

export default App
