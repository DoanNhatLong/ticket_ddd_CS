import type { BookingDetail } from '../api/bookings'
import { useCountdown } from '../hooks/useCountdown'
import { formatPrice } from '../utils/format'

interface BookingConfirmationProps {
  booking: BookingDetail
  onClose: () => void
}

/** Shown right after a booking is created: booking code, tickets and the seat hold countdown. */
export function BookingConfirmation({ booking, onClose }: BookingConfirmationProps) {
  const secondsLeft = useCountdown(new Date(booking.expiresAt))
  const minutes = Math.floor(secondsLeft / 60)
  const seconds = String(secondsLeft % 60).padStart(2, '0')
  const holdOver = secondsLeft === 0

  return (
    <section className="confirmation" aria-live="polite">
      <div className="confirmation-head">
        <div>
          <p className="confirmation-label">Booking created</p>
          <h2 className="booking-code">{booking.bookingCode}</h2>
        </div>
        <span className="status-badge">{booking.status}</span>
      </div>

      <div className={holdOver ? 'hold-timer over' : 'hold-timer'}>
        {holdOver ? (
          'The seat hold has run out. The seats may be released.'
        ) : (
          <>
            Seats held for <strong>{minutes}:{seconds}</strong> — pay before the hold runs out.
          </>
        )}
      </div>

      <ul className="confirmation-tickets">
        {booking.tickets.map((ticket) => (
          <li key={ticket.ticketId}>
            <span>
              Trip #{ticket.trainTripId} · seat #{ticket.seatId} · {ticket.passengerName}
            </span>
            <span>{formatPrice(ticket.price)}</span>
          </li>
        ))}
      </ul>

      <div className="confirmation-footer">
        <span>
          Booking total <strong>{formatPrice(booking.totalAmount)}</strong>
        </span>
        <button type="button" className="secondary-button" onClick={onClose}>
          Book more seats
        </button>
      </div>
    </section>
  )
}
