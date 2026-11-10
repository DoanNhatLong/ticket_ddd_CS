// Client for the train trip endpoints of TrainBooking.Api.

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

/** Error body returned by the API (RFC 7807 problem details with a stable `code`). */
export interface ApiProblem {
  code: string
  detail: string
}

export async function getTripSeats(tripId: number): Promise<TripSeats> {
  const response = await fetch(`/api/train-trips/${tripId}/seats`)

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ApiProblem | null
    throw new Error(problem?.detail ?? `Request failed with status ${response.status}`)
  }

  return (await response.json()) as TripSeats
}
