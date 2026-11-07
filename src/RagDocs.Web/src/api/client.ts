import type { DocumentDto, ProblemDetails, UploadResult } from './types'

// Thrown for any non-2xx response; carries the API's ProblemDetails when there is one.
export class ApiError extends Error {
  readonly status: number
  readonly problem?: ProblemDetails

  constructor(status: number, problem?: ProblemDetails) {
    super(problem?.detail ?? problem?.title ?? `Request failed (${status})`)
    this.status = status
    this.problem = problem
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`/api${path}`, init)

  if (!response.ok) {
    // Not every error has a JSON body (e.g. 413 from the server), so don't fail on parsing.
    const problem = (await response.json().catch(() => undefined)) as ProblemDetails | undefined
    throw new ApiError(response.status, problem)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export const api = {
  listDocuments: (signal?: AbortSignal) => request<DocumentDto[]>('/documents', { signal }),

  uploadDocument: (file: File) => {
    const form = new FormData()
    form.append('file', file) // must match the endpoint's IFormFile parameter name
    return request<UploadResult>('/documents', { method: 'POST', body: form })
  },

  deleteDocument: (id: string) => request<void>(`/documents/${id}`, { method: 'DELETE' }),
}