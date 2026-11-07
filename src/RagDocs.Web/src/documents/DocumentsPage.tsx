import { DocumentsTable } from './DocumentsTable'
import { UploadForm } from './UploadForm'
import { useDocuments } from './useDocuments'

export function DocumentsPage() {
  const { data, isPending, isError, error, refresh } = useDocuments()

  return (
    <section>
      <h1>Documents</h1>
      <UploadForm onUploaded={refresh} />

      {isPending && <p className="muted">Loading…</p>}
      {isError && <p className="error">Couldn't load documents: {error?.message}</p>}
      {data && <DocumentsTable documents={data} onDeleted={refresh} />}
    </section>
  )
}