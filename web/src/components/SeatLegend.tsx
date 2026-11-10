const ITEMS: { className: string; label: string }[] = [
  { className: 'free', label: 'Free' },
  { className: 'selected', label: 'Selected' },
  { className: 'taken', label: 'Taken' },
  { className: 'not-sold', label: 'Not sold on this trip' },
]

export function SeatLegend() {
  return (
    <ul className="legend">
      {ITEMS.map(({ className, label }) => (
        <li key={className}>
          <span className={`swatch ${className}`} /> {label}
        </li>
      ))}
    </ul>
  )
}
