using Microsoft.EntityFrameworkCore;
using RagDocs.Application.Abstractions;
using RagDocs.Domain.Documents;

namespace RagDocs.Infrastructure.Persistence.Repositories;

internal sealed class DocumentRepository(RagDbContext dbContext) : IDocumentRepository
{
    public Task<Document?> GetByIdAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken) =>
        dbContext.Documents
            .FirstOrDefaultAsync(
                d => d.Id == id && d.OwnerId == ownerId,
                cancellationToken);

    public Task<Document?> GetByContentHashAsync(
        string ownerId,
        string contentHash,
        CancellationToken cancellationToken) =>
        dbContext.Documents
            .FirstOrDefaultAsync(
                d => d.OwnerId == ownerId && d.ContentHash == contentHash,
                cancellationToken);

    public async Task<IReadOnlyList<Document>> ListAsync(
        string ownerId,
        CancellationToken cancellationToken) =>
        await dbContext.Documents
            .AsNoTracking()
            .Where(d => d.OwnerId == ownerId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

    public void Add(Document document) => dbContext.Documents.Add(document);
}
