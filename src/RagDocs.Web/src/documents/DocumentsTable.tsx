import { useState } from 'react'
import { api } from '../api/client'
import type { DocumentDto } from '../api/types'
import { StatusBadge } from './StatusBadge'

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

interface DocumentsTableProps {
  documents: DocumentDto[]
  onDeleted: () => void
}

export function DocumentsTable({ documents, onDeleted }: DocumentsTableProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  if (documents.length === 0) {
    return <p className="muted">No documents yet. Upload one above.</p>
  }

  async function handleDelete(document: DocumentDto) {
    if (!window.confirm(`Delete ${document.fileName}?`)) return

    setDeletingId(document.id)
    setError(null)

    try {
      await api.deleteDocument(document.id)
      onDeleted()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Delete failed')
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <>
      {error && <p className="error">{error}</p>}

      <table>
        <thead>
          <tr>
            <th>File</th>
            <th>Size</th>
            <th>Status</th>
            <th>Chunks</th>
            <th>Uploaded</th>
            <th aria-label="Actions" />
          </tr>
        </thead>
        <tbody>
          {documents.map((document) => (
            <tr key={document.id}>
              <td>{document.fileName}</td>
              <td className="nowrap">{formatSize(document.sizeBytes)}</td>
              <td>
                <StatusBadge status={document.status} />
                {document.failureReason && <div className="muted small">{document.failureReason}</div>}
              </td>
              <td>{document.status === 'Ready' ? document.chunkCount : '–'}</td>
              <td className="nowrap">{new Date(document.createdAt).toLocaleString()}</td>
              <td>
                <button
                  className="danger"
                  disabled={document.status === 'Processing' || deletingId !== null}
                  onClick={() => handleDelete(document)}
                >
                  {deletingId === document.id ? 'Deleting…' : 'Delete'}
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  )
}