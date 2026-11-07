import { useCallback, useEffect, useState } from 'react'
import { api } from '../api/client'
import type { DocumentDto } from '../api/types'

export function useDocuments() {
  const [data, setData] = useState<DocumentDto[]>()
  const [error, setError] = useState<Error | null>(null)
  const [version, setVersion] = useState(0)

  // Changing `version` re-runs the effect below, i.e. loads the list again.
  const refresh = useCallback(() => setVersion((v) => v + 1), [])

  // Load the list (on mount and on every refresh).
  useEffect(() => {
    const controller = new AbortController()

    api
      .listDocuments(controller.signal)
      .then((documents) => {
        setData(documents)
        setError(null)
      })
      .catch((reason: unknown) => {
        if (controller.signal.aborted) return // the page closed or a newer load started
        setError(reason instanceof Error ? reason : new Error(String(reason)))
      })

    return () => controller.abort()
  }, [version])

  return {
    data,
    error,
    isPending: data === undefined && error === null,
    isError: error !== null,
    refresh,
  }
}