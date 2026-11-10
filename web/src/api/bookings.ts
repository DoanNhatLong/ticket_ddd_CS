// Client for the booking endpoints of TrainBooking.Api.
import { apiFetch } from './http'

export interface CreateBookingTicket {
  trainTripId: number
  seatId: number
  passengerName: string
  passengerIdNumber: string
}

export interface Ticket {
  ticketId: number
  trainTripId: number
  seatId: number
  passengerName: string
  passengerIdNumber: string
  price: number
  status: string
}

export interface BookingDetail {
  bookingCode: string
  status: string
  totalAmount: number
  createdAt: string | null
  /** When the seat hold of an unpaid booking runs out (UTC). */
  expiresAt: string
  tickets: Ticket[]
}

export function createBooking(customerId: number, tickets: CreateBookingTicket[]): Promise<BookingDetail> {
  return apiFetch<BookingDetail>('/api/bookings', { method: 'POST', body: { tickets }, customerId })
}

export function getBooking(customerId: number, bookingCode: string): Promise<BookingDetail> {
  return apiFetch<BookingDetail>(`/api/bookings/${encodeURIComponent(bookingCode)}`, { customerId })
}
