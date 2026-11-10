import { useEffect, useState } from 'react'
import { getTripSeats, type TripSeats } from '../api/trainTrips'

interface TripSeatsState {
  trip: TripSeats | null
  loading: boolean
  error: string | null
}

/**
 * Loads the seat map of a train trip and reloads it whenever the trip id changes.
 * The previous trip stays visible while the next one loads.
 */
export function useTripSeats(tripId: number): TripSeatsState {
  const [trip, setTrip] = useState<TripSeats | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

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

  return { trip, loading, error }
}
