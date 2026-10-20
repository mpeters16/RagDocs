using RagDocs.Domain.Documents;

public interface IDocumentRepository
{
    // Returns null if the document doesn't exist OR belongs to another owner.
    Task<Document?> GetByIdAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken);

    // Used to spot duplicate uploads (same file, same owner).
    Task<Document?> GetByContentHashAsync(
        string ownerId,
        string contentHash,
        CancellationToken cancellationToken);

    // All documents of one owner, newest first.
    Task<IReadOnlyList<Document>> ListAsync(
        string ownerId,
        CancellationToken cancellationToken);

    // Starts tracking a new document. Nothing is written until IUnitOfWork.SaveChangesAsync.
    void Add(Document document);
}