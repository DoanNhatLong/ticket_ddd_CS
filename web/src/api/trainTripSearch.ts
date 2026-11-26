// Client for the train trip search endpoint of TrainBooking.Api.
import { apiFetch } from './http'

/** What to search for; every field is optional. */
export interface TrainTripSearchParams {
  /** Free text: station, city or train; accents and small typos are fine. */
  text?: string
  /** Departure station code, e.g. "SGN". */
  from?: string
  /** Arrival station code, e.g. "DAD". */
  to?: string
  /** Departure day as yyyy-MM-dd (UTC). */
  date?: string
  page?: number
  pageSize?: number
}

export interface TrainTripSearchStation {
  code: string
  name: string
  city: string
}

export interface TrainTripSearchItem {
  trainTripId: number
  trainCode: string
  trainName: string
  departure: TrainTripSearchStation
  arrival: TrainTripSearchStation
  departureTime: string
  arrivalTime: string
  status: string
  /** Cheapest fare on the train trip, or null when no seat class is sold yet. */
  lowestFare: number | null
}

export interface TrainTripSearchPage {
  items: TrainTripSearchItem[]
  total: number
  page: number
  pageSize: number
}

/** Train trips that have not departed yet, matching the given criteria. */
export function searchTrainTrips(params: TrainTripSearchParams, signal?: AbortSignal): Promise<TrainTripSearchPage> {
  const query = new URLSearchParams()
  if (params.text) query.set('q', params.text)
  if (params.from) query.set('from', params.from)
  if (params.to) query.set('to', params.to)
  if (params.date) query.set('date', params.date)
  if (params.page) query.set('page', String(params.page))
  if (params.pageSize) query.set('pageSize', String(params.pageSize))

  return apiFetch<TrainTripSearchPage>(`/api/train-trips/search?${query}`, { signal })
}
