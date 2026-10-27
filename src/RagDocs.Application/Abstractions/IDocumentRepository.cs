using RagDocs.Domain.Documents;

namespace RagDocs.Application.Abstractions;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken);

    Task<Document?> GetByContentHashAsync(
        string ownerId,
        string contentHash,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Document>> ListAsync(
        string ownerId,
        CancellationToken cancellationToken);

    void Add(Document document);

    // Takes the oldest Pending document and marks it Processing. Null when there's nothing to do.
    Task<Document?> ClaimNextPendingAsync(CancellationToken cancellationToken);

    // On startup: documents left in Processing by a crash go back to Pending.
    Task<int> ResetStuckProcessingAsync(CancellationToken cancellationToken);
}