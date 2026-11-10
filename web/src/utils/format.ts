// Display formatters shared by the UI. Times are shown in UTC, matching how the API stores them.

const priceFormat = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' })

const dateFormat = new Intl.DateTimeFormat('vi-VN', {
  weekday: 'short',
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  timeZone: 'UTC',
})

const timeFormat = new Intl.DateTimeFormat('vi-VN', { hour: '2-digit', minute: '2-digit', timeZone: 'UTC' })

export function formatPrice(amount: number): string {
  return priceFormat.format(amount)
}

export function formatDate(date: Date): string {
  return dateFormat.format(date)
}

export function formatTime(date: Date): string {
  return timeFormat.format(date)
}

export function formatDuration(from: Date, to: Date): string {
  const minutes = Math.round((to.getTime() - from.getTime()) / 60000)
  const hours = Math.floor(minutes / 60)
  const rest = minutes % 60
  return rest === 0 ? `${hours}h` : `${hours}h ${rest}m`
}
