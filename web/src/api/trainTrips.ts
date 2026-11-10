// Client for the train trip endpoints of TrainBooking.Api.
import { apiFetch } from './http'

export interface TripSeat {
  seatId: number
  carriageNumber: number
  seatNumber: string
  seatClassCode: string
  seatClassName: string
  /** The trip's fare for this seat class, or null when the seat is not sold on this trip. */
  fare: number | null
  isFree: boolean
}

export interface TripSeats {
  trainTripId: number
  departureTime: string
  arrivalTime: string
  seats: TripSeat[]
}

export function getTripSeats(tripId: number): Promise<TripSeats> {
  return apiFetch<TripSeats>(`/api/train-trips/${tripId}/seats`)
}
