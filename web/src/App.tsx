import { useMemo, useState } from 'react'
import { createBooking, type BookingDetail } from './api/bookings'
import { ApiError } from './api/http'
import type { TripSeat } from './api/trainTrips'
import { selectionKey, toSelectedSeat, type SelectedSeat } from './booking/selection'
import { BookingConfirmation } from './components/BookingConfirmation'
import { BookingPanel } from './components/BookingPanel'
import { CustomerPicker } from './components/CustomerPicker'
import { SeatLegend } from './components/SeatLegend'
import { SeatMap } from './components/SeatMap'
import { TopBar } from './components/TopBar'
import { TripSummary } from './components/TripSummary'
import { TripTabs } from './components/TripTabs'
import { useTripSeats } from './hooks/useTripSeats'
import './App.css'

const TRIP_IDS = [1, 2, 3, 4, 5]
// Seed customers (init.sql): users 1-3 have the Customer role.
const CUSTOMER_IDS = [1, 2, 3]

function App() {
  const [customerId, setCustomerId] = useState(CUSTOMER_IDS[0])
  const [tripId, setTripId] = useState(TRIP_IDS[0])
  const { trip, loading, error, reload } = useTripSeats(tripId)

  // Picked seats keyed by "tripId:seatId", in the order they were picked.
  const [selection, setSelection] = useState<Map<string, SelectedSeat>>(new Map())
  const [submitting, setSubmitting] = useState(false)
  const [bookingError, setBookingError] = useState<string | null>(null)
  const [confirmed, setConfirmed] = useState<BookingDetail | null>(null)

  const selectedSeats = useMemo(() => [...selection.values()], [selection])

  const selectedSeatIdsOnTrip = useMemo(
    () => new Set(selectedSeats.filter((s) => s.tripId === tripId).map((s) => s.seatId)),
    [selectedSeats, tripId],
  )

  const toggleSeat = (seat: TripSeat) => {
    setBookingError(null)
    setSelection((current) => {
      const next = new Map(current)
      const key = selectionKey(tripId, seat.seatId)
      if (next.has(key)) next.delete(key)
      else next.set(key, toSelectedSeat(tripId, seat))
      return next
    })
  }

  const updateSeat = (seat: SelectedSeat, changes: Partial<SelectedSeat>) => {
    setSelection((current) => {
      const next = new Map(current)
      next.set(selectionKey(seat.tripId, seat.seatId), { ...seat, ...changes })
      return next
    })
  }

  const removeSeat = (seat: SelectedSeat) => {
    setSelection((current) => {
      const next = new Map(current)
      next.delete(selectionKey(seat.tripId, seat.seatId))
      return next
    })
  }

  const clearSelection = () => {
    setSelection(new Map())
    setBookingError(null)
  }

  const submitBooking = async () => {
    setSubmitting(true)
    setBookingError(null)

    try {
      const booking = await createBooking(
        customerId,
        selectedSeats.map((s) => ({
          trainTripId: s.tripId,
          seatId: s.seatId,
          passengerName: s.passengerName.trim(),
          passengerIdNumber: s.passengerIdNumber.trim(),
        })),
      )
      setConfirmed(booking)
      setSelection(new Map())
      reload()
    } catch (err) {
      if (err instanceof ApiError && err.code === 'Booking.SeatTaken') {
        setBookingError('Someone else has just taken one of these seats. The seat map has been refreshed; please pick again.')
        reload()
      } else {
        setBookingError(err instanceof Error ? err.message : 'Booking failed.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  const changeCustomer = (id: number) => {
    setCustomerId(id)
    setConfirmed(null)
    setBookingError(null)
  }

  return (
    <div className="app">
      <TopBar>
        <CustomerPicker customerIds={CUSTOMER_IDS} customerId={customerId} onChange={changeCustomer} />
      </TopBar>

      <main className="content">
        <TripTabs tripIds={TRIP_IDS} activeId={tripId} onChange={setTripId} />

        {confirmed && <BookingConfirmation booking={confirmed} onClose={() => setConfirmed(null)} />}

        <div className="booking-layout">
          <div className="booking-main">
            {error && <div className="alert">{error}</div>}
            {loading && !trip && <div className="placeholder">Loading seats…</div>}

            {trip && (
              <>
                <TripSummary trip={trip} dimmed={loading} />
                <SeatLegend />
                <SeatMap
                  seats={trip.seats}
                  dimmed={loading}
                  selectedSeatIds={selectedSeatIdsOnTrip}
                  onSelectSeat={submitting ? undefined : toggleSeat}
                />
              </>
            )}
          </div>

          <BookingPanel
            seats={selectedSeats}
            submitting={submitting}
            error={bookingError}
            onChangeSeat={updateSeat}
            onRemoveSeat={removeSeat}
            onClear={clearSelection}
            onSubmit={submitBooking}
          />
        </div>
      </main>
    </div>
  )
}

export default App
