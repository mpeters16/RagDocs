import { useState, type FormEvent } from 'react'
import { api } from '../api/client'

export function UploadForm({ onUploaded }: { onUploaded: () => void }) {
  const [file, setFile] = useState<File | null>(null)
  const [inputKey, setInputKey] = useState(0) // changing the key clears the file input
  const [isUploading, setIsUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [alreadyExisted, setAlreadyExisted] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!file) return

    setIsUploading(true)
    setError(null)
    setAlreadyExisted(false)

    try {
      const result = await api.uploadDocument(file)
      setAlreadyExisted(result.alreadyExisted)
      setFile(null)
      setInputKey((key) => key + 1)
      onUploaded()
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'Upload failed')
    } finally {
      setIsUploading(false)
    }
  }

  return (
    <form className="upload" onSubmit={handleSubmit}>
      <input
        key={inputKey}
        type="file"
        accept=".pdf,.docx,.txt,.md"
        onChange={(event) => setFile(event.target.files?.[0] ?? null)}
      />
      <button type="submit" disabled={!file || isUploading}>
        {isUploading ? 'Uploading…' : 'Upload'}
      </button>

      {error && <p className="error">{error}</p>}
      {alreadyExisted && <p className="info">You've already uploaded this file.</p>}
    </form>
  )
}