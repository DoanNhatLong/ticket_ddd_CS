import { useEffect, useState } from 'react'
import { ApiError } from '../api/http'
import { searchTrainTrips, type TrainTripSearchPage, type TrainTripSearchParams } from '../api/trainTripSearch'

interface TrainTripSearchState {
  result: TrainTripSearchPage | null
  loading: boolean
  error: string | null
}

function toMessage(err: unknown): string {
  if (err instanceof ApiError && err.code === 'Search.Unavailable') {
    return 'Train trip search is unavailable right now. Check that Elasticsearch is running.'
  }
  return err instanceof Error ? err.message : 'Train trip search failed.'
}

/**
 * Runs a train trip search every time a new params object is passed in.
 * The previous result stays visible while the next one loads; a slower, older answer is dropped.
 */
export function useTrainTripSearch(params: TrainTripSearchParams): TrainTripSearchState {
  const [state, setState] = useState<TrainTripSearchState>({ result: null, loading: true, error: null })

  useEffect(() => {
    const controller = new AbortController()
    setState((current) => ({ ...current, loading: true, error: null }))

    searchTrainTrips(params, controller.signal)
      .then((result) => {
        if (!controller.signal.aborted) setState({ result, loading: false, error: null })
      })
      .catch((err: unknown) => {
        if (!controller.signal.aborted) setState({ result: null, loading: false, error: toMessage(err) })
      })

    return () => controller.abort()
  }, [params])

  return state
}
