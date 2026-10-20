namespace RagDocs.Application.Documents.Upload;

public sealed record UploadDocumentCommand(string FileName, Stream Content);