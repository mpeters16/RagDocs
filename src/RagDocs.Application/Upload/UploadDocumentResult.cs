namespace RagDocs.Application.Documents.Upload;

public sealed record UploadDocumentResult(Guid DocumentId, bool AlreadyExisted);