// Shared fetch helper for TrainBooking.Api: JSON in/out, customer header, problem-details errors.

/** Error body returned by the API (RFC 7807 problem details with a stable `code`). */
export interface ApiProblem {
  code: string
  detail: string
  status?: number
}

/** Thrown for any non-2xx response; carries the API's error code and HTTP status. */
export class ApiError extends Error {
  readonly code: string
  readonly status: number

  constructor(status: number, problem: ApiProblem | null) {
    super(problem?.detail ?? `Request failed with status ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.code = problem?.code ?? 'Http.' + status
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST'
  body?: unknown
  /** Sent as X-Customer-Id until real authentication exists. */
  customerId?: number
}

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' }
  if (options.body !== undefined) headers['Content-Type'] = 'application/json'
  if (options.customerId !== undefined) headers['X-Customer-Id'] = String(options.customerId)

  const response = await fetch(path, {
    method: options.method ?? 'GET',
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ApiProblem | null
    throw new ApiError(response.status, problem)
  }

  return (await response.json()) as T
}
