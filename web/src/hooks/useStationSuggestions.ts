import { useEffect, useState } from 'react'
import { ApiError } from '../api/http'
import { suggestStations, type StationSuggestion } from '../api/stations'

const DEBOUNCE_MS = 250

interface StationSuggestionsState {
  suggestions: StationSuggestion[]
  /** The text the suggestions belong to, or null when nothing has been searched yet. */
  searchedText: string | null
  /** How long the last search took, in milliseconds. */
  elapsedMs: number | null
  error: string | null
}

const EMPTY: StationSuggestionsState = { suggestions: [], searchedText: null, elapsedMs: null, error: null }

function toMessage(err: unknown): string {
  if (err instanceof ApiError && err.code === 'Search.Unavailable') {
    return 'Station search is unavailable right now. Check that Elasticsearch is running.'
  }
  return err instanceof Error ? err.message : 'Station search failed.'
}

/**
 * Suggests stations for the text being typed. Waits until typing pauses, and drops answers to
 * older texts so a slow response can never replace a newer one.
 */
export function useStationSuggestions(text: string): StationSuggestionsState & { loading: boolean } {
  const [state, setState] = useState<StationSuggestionsState>(EMPTY)
  const query = text.trim()

  useEffect(() => {
    if (query === '') {
      setState(EMPTY)
      return
    }

    const controller = new AbortController()
    const timer = setTimeout(() => {
      const startedAt = performance.now()

      suggestStations(query, controller.signal)
        .then((suggestions) => {
          if (controller.signal.aborted) return
          const elapsedMs = Math.round(performance.now() - startedAt)
          setState({ suggestions, searchedText: query, elapsedMs, error: null })
        })
        .catch((err: unknown) => {
          if (controller.signal.aborted) return
          setState({ suggestions: [], searchedText: query, elapsedMs: null, error: toMessage(err) })
        })
    }, DEBOUNCE_MS)

    return () => {
      clearTimeout(timer)
      controller.abort()
    }
  }, [query])

  return { ...state, loading: query !== '' && state.searchedText !== query }
}
