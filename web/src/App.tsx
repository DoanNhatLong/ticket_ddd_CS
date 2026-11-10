import { useState } from 'react'
import { SeatLegend } from './components/SeatLegend'
import { SeatMap } from './components/SeatMap'
import { TopBar } from './components/TopBar'
import { TripSummary } from './components/TripSummary'
import { TripTabs } from './components/TripTabs'
import { useTripSeats } from './hooks/useTripSeats'
import './App.css'

const TRIP_IDS = [1, 2, 3, 4, 5]

function App() {
  const [tripId, setTripId] = useState(TRIP_IDS[0])
  const { trip, loading, error } = useTripSeats(tripId)

  return (
    <div className="app">
      <TopBar note="Seat map" />

      <main className="content">
        <TripTabs tripIds={TRIP_IDS} activeId={tripId} onChange={setTripId} />

        {error && <div className="alert">{error}</div>}
        {loading && !trip && <div className="placeholder">Loading seats…</div>}

        {trip && (
          <>
            <TripSummary trip={trip} dimmed={loading} />
            <SeatLegend />
            <SeatMap seats={trip.seats} dimmed={loading} />
          </>
        )}
      </main>
    </div>
  )
}

export default App
