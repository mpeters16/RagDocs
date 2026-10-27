using Microsoft.EntityFrameworkCore;
using RagDocs.Application.Abstractions;
using RagDocs.Domain.Documents;

namespace RagDocs.Infrastructure.Persistence.Repositories;

internal sealed class DocumentRepository(RagDbContext dbContext, TimeProvider timeProvider) : IDocumentRepository
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

    public async Task<Document?> ClaimNextPendingAsync(CancellationToken cancellationToken)
    {
        // 1. Find the oldest Pending document
        var document = await dbContext.Documents
            .Where(d => d.Status == DocumentStatus.Pending)
            .OrderBy(d => d.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            return null;   // nothing to do
        }

        // 2. Mark it Processing (using the Domain rule from Day 2)
        document.StartProcessing(timeProvider.GetUtcNow());

        // 3. Save, so it's no longer Pending
        await dbContext.SaveChangesAsync(cancellationToken);

        return document;
    }

    public Task<int> ResetStuckProcessingAsync(CancellationToken cancellationToken) =>
        dbContext.Documents
            .Where(d => d.Status == DocumentStatus.Processing)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(d => d.Status, DocumentStatus.Pending),
                cancellationToken);
}
