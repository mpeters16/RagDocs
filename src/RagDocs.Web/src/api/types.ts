export type DocumentStatus = 'Pending' | 'Processing' | 'Ready' | 'Failed'

export interface DocumentDto {
    id: string
    fileName: string
    contentType: string
    sizeBytes: number
    status: DocumentStatus
    failureReason: string | null
    chunkCount: number
    createdAt: string
    processedAt: string | null
  }

  export interface UploadResult {
    documentId: string
    alreadyExisted: boolean
  }
  
  export interface ProblemDetails {
    title?: string
    status?: number
    detail?: string
    code?: string
    traceId?: string
  }