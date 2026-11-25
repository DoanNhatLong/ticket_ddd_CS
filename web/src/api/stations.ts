// Client for the station endpoints of TrainBooking.Api.
import { apiFetch } from './http'

export interface StationSuggestion {
  stationId: number
  code: string
  name: string
  city: string
}

/** Stations matching the text typed so far; tolerates missing accents and small typos. */
export function suggestStations(text: string, signal?: AbortSignal): Promise<StationSuggestion[]> {
  const params = new URLSearchParams({ q: text })
  return apiFetch<StationSuggestion[]>(`/api/stations/suggest?${params}`, { signal })
}
