import { useEffect, useState } from 'react'

/** Seconds left until `deadline` (never negative), updated every second. */
export function useCountdown(deadline: Date): number {
  const [now, setNow] = useState(() => Date.now())

  useEffect(() => {
    const timer = window.setInterval(() => setNow(Date.now()), 1000)
    return () => window.clearInterval(timer)
  }, [])

  return Math.max(0, Math.floor((deadline.getTime() - now) / 1000))
}
