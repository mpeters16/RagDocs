using RagDocs.Domain.Documents;

namespace RagDocs.Application.Documents;

public sealed record DocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Status,
    string? FailureReason,
    int ChunkCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt)
{
    public static DocumentResponse From(Document document) =>
        new(
            document.Id,
            document.FileName,
            document.ContentType,
            document.SizeBytes,
            document.Status.ToString(),
            document.FailureReason,
            document.ChunkCount,
            document.CreatedAt,
            document.ProcessedAt);
}