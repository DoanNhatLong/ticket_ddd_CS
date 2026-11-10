import type { TripSeat } from '../api/trainTrips'

/** How a seat is shown on the seat map. */
export type SeatState = 'free' | 'taken' | 'not-sold'

export function seatState(seat: TripSeat): SeatState {
  if (seat.fare === null) return 'not-sold'
  return seat.isFree ? 'free' : 'taken'
}
