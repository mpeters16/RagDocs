using Microsoft.EntityFrameworkCore;
using RagDocs.Application.Abstractions;
using RagDocs.Domain.Chunks;

namespace RagDocs.Infrastructure.Persistence.Repositories;

internal sealed class ChunkRepository(RagDbContext dbContext) : IChunkRepository
{
    public Task DeleteForDocumentAsync(Guid documentId, CancellationToken cancellationToken) =>
        dbContext.Chunks
            .Where(c => c.DocumentId == documentId)
            .ExecuteDeleteAsync(cancellationToken);

    public void AddRange(IEnumerable<Chunk> chunks) => dbContext.Chunks.AddRange(chunks);
}