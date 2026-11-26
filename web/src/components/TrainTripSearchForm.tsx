import { useState, type FormEvent } from 'react'
import type { StationSuggestion } from '../api/stations'
import type { TrainTripSearchParams } from '../api/trainTripSearch'
import { StationSearch } from './StationSearch'

interface TrainTripSearchFormProps {
  /** Called with the new criteria when the customer presses Search or Clear. */
  onSearch: (params: TrainTripSearchParams) => void
}

/** Departure and arrival station (with suggestions), departure date and free text. */
export function TrainTripSearchForm({ onSearch }: TrainTripSearchFormProps) {
  const [from, setFrom] = useState<StationSuggestion | null>(null)
  const [to, setTo] = useState<StationSuggestion | null>(null)
  const [date, setDate] = useState('')
  const [text, setText] = useState('')
  // Remounts the station boxes on Clear so their typed text is cleared too.
  const [resetKey, setResetKey] = useState(0)

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault()
    onSearch({
      from: from?.code,
      to: to?.code,
      date: date || undefined,
      text: text.trim() || undefined,
      page: 1,
    })
  }

  const clear = () => {
    setFrom(null)
    setTo(null)
    setDate('')
    setText('')
    setResetKey((k) => k + 1)
    onSearch({ page: 1 })
  }

  return (
    <form className="trip-search" onSubmit={handleSubmit}>
      <h2 className="trip-search-title">Find a train trip</h2>

      <div className="trip-search-stations" key={resetKey}>
        <StationSearch label="From" selected={from} onSelect={setFrom} />
        <StationSearch label="To" selected={to} onSelect={setTo} />
      </div>

      <div className="trip-search-row">
        <label className="field">
          Departure date (UTC)
          <input type="date" value={date} onChange={(e) => setDate(e.target.value)} />
        </label>

        <label className="field trip-search-text">
          Station, city or train
          <input
            type="text"
            value={text}
            maxLength={100}
            placeholder="e.g. sai gom da nag, SE2"
            onChange={(e) => setText(e.target.value)}
          />
        </label>
      </div>

      <div className="trip-search-actions">
        <button type="submit" className="primary-button">
          Search
        </button>
        <button type="button" className="secondary-button" onClick={clear}>
          Clear
        </button>
        <span className="trip-search-hint">
          A station only counts once it is picked from the suggestions. Empty fields mean "any".
        </span>
      </div>
    </form>
  )
}
