import { useId, useState, type KeyboardEvent } from 'react'
import type { StationSuggestion } from '../api/stations'
import { useStationSuggestions } from '../hooks/useStationSuggestions'

interface StationSearchProps {
  label: string
  selected: StationSuggestion | null
  onSelect: (station: StationSuggestion | null) => void
  /** Sample texts offered as one-click searches. */
  examples?: string[]
}

/** Search box that suggests stations while typing (arrow keys, Enter and Escape are supported). */
export function StationSearch({ label, selected, onSelect, examples = [] }: StationSearchProps) {
  const id = useId()
  const listId = `${id}-list`
  const optionId = (index: number) => `${id}-option-${index}`

  const [text, setText] = useState('')
  const [open, setOpen] = useState(false)
  const [activeIndex, setActiveIndex] = useState(-1)

  // After a station is picked the box shows its name; that text is not a new search.
  const { suggestions, searchedText, elapsedMs, error, loading } = useStationSuggestions(selected ? '' : text)
  const showList = open && !selected && suggestions.length > 0
  const noMatch = !loading && !error && searchedText !== null && suggestions.length === 0

  const search = (value: string) => {
    setText(value)
    setActiveIndex(-1)
    setOpen(true)
    if (selected) onSelect(null)
  }

  const pick = (station: StationSuggestion) => {
    setText(station.name)
    setActiveIndex(-1)
    setOpen(false)
    onSelect(station)
  }

  const handleKeyDown = (e: KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Escape') {
      setOpen(false)
      setActiveIndex(-1)
      return
    }
    if (suggestions.length === 0) return

    if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
      e.preventDefault()
      const step = e.key === 'ArrowDown' ? 1 : -1
      const start = activeIndex === -1 && step === -1 ? 0 : activeIndex
      setOpen(true)
      setActiveIndex((start + step + suggestions.length) % suggestions.length)
    } else if (e.key === 'Enter' && showList && !loading && activeIndex >= 0) {
      e.preventDefault()
      pick(suggestions[activeIndex])
    }
  }

  return (
    <section className="station-search">
      <label className="station-label" htmlFor={id}>
        {label}
      </label>

      <div className="station-box">
        <input
          id={id}
          className="station-input"
          type="text"
          role="combobox"
          aria-autocomplete="list"
          aria-expanded={showList}
          aria-controls={listId}
          aria-activedescendant={showList && activeIndex >= 0 ? optionId(activeIndex) : undefined}
          autoComplete="off"
          spellCheck={false}
          maxLength={50}
          placeholder="Station, city or code"
          value={text}
          onChange={(e) => search(e.target.value)}
          onFocus={() => setOpen(true)}
          onBlur={() => setOpen(false)}
          onKeyDown={handleKeyDown}
        />

        <ul id={listId} className="station-list" role="listbox" aria-label={label} hidden={!showList}>
          {suggestions.map((station, index) => (
            <li
              key={station.stationId}
              id={optionId(index)}
              role="option"
              aria-selected={index === activeIndex}
              className={index === activeIndex ? 'station-option active' : 'station-option'}
              // Keep the focus in the input so the list does not close before the click.
              onMouseDown={(e) => e.preventDefault()}
              onMouseEnter={() => setActiveIndex(index)}
              onClick={() => pick(station)}
            >
              <span>
                <strong>{station.name}</strong>
                <small>{station.city}</small>
              </span>
              <code className="station-code">{station.code}</code>
            </li>
          ))}
        </ul>
      </div>

      {error && (
        <div className="alert" role="alert">
          {error}
        </div>
      )}

      <p className="station-status" aria-live="polite">
        {selected
          ? `Selected: ${selected.name} (${selected.code}), ${selected.city}`
          : loading
            ? 'Searching…'
            : noMatch
              ? `No station matches "${searchedText}".`
              : elapsedMs !== null
                ? `${suggestions.length} ${suggestions.length === 1 ? 'station' : 'stations'} for "${searchedText}" in ${elapsedMs} ms`
                : 'Accents and small typos are fine.'}
      </p>

      {examples.length > 0 && (
        <div className="station-examples">
          <span>Try:</span>
          {examples.map((example) => (
            <button
              key={example}
              type="button"
              className="example-chip"
              onClick={() => {
                search(example)
                document.getElementById(id)?.focus()
              }}
            >
              {example}
            </button>
          ))}
        </div>
      )}
    </section>
  )
}
