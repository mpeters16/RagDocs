using RagDocs.Application.Abstractions;
using RagDocs.Domain.Documents;

namespace RagDocs.UnitTests.Application.Fakes;

internal sealed class InMemoryDocumentRepository : IDocumentRepository
{
    // Public so tests can look inside.
    public List<Document> Documents { get; } = new();

    public Task<Document?> GetByIdAsync(
        Guid id, string ownerId, CancellationToken cancellationToken) =>
        Task.FromResult(Documents.FirstOrDefault(d => d.Id == id && d.OwnerId == ownerId));

    public Task<Document?> GetByContentHashAsync(
        string ownerId, string contentHash, CancellationToken cancellationToken) =>
        Task.FromResult(Documents.FirstOrDefault(d => d.OwnerId == ownerId && d.ContentHash == contentHash));

    public Task<IReadOnlyList<Document>> ListAsync(
        string ownerId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Document>>(
            Documents
                .Where(d => d.OwnerId == ownerId)
                .OrderByDescending(d => d.CreatedAt)
                .ToList());

    public void Add(Document document) => Documents.Add(document);
}