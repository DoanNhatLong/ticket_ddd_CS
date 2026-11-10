import type { SeatState } from './seatState'

const ITEMS: { state: SeatState; label: string }[] = [
  { state: 'free', label: 'Free' },
  { state: 'taken', label: 'Taken' },
  { state: 'not-sold', label: 'Not sold on this trip' },
]

export function SeatLegend() {
  return (
    <ul className="legend">
      {ITEMS.map(({ state, label }) => (
        <li key={state}>
          <span className={`swatch ${state}`} /> {label}
        </li>
      ))}
    </ul>
  )
}
