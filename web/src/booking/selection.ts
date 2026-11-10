import type { TripSeat } from '../api/trainTrips'

/** A seat the customer picked for the next booking, with the passenger who will use it. */
export interface SelectedSeat {
  tripId: number
  seatId: number
  seatNumber: string
  carriageNumber: number
  seatClassName: string
  fare: number
  passengerName: string
  passengerIdNumber: string
}

/** Seats are unique per train trip, so the key combines both. */
export function selectionKey(tripId: number, seatId: number): string {
  return `${tripId}:${seatId}`
}

export function toSelectedSeat(tripId: number, seat: TripSeat): SelectedSeat {
  return {
    tripId,
    seatId: seat.seatId,
    seatNumber: seat.seatNumber,
    carriageNumber: seat.carriageNumber,
    seatClassName: seat.seatClassName,
    fare: seat.fare ?? 0,
    passengerName: '',
    passengerIdNumber: '',
  }
}

export function isPassengerComplete(seat: SelectedSeat): boolean {
  return seat.passengerName.trim() !== '' && seat.passengerIdNumber.trim() !== ''
}
