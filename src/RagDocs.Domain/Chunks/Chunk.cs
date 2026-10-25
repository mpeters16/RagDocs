using RagDocs.Domain.Common;

namespace RagDocs.Domain.Chunks;

public sealed class Chunk
{
    private Chunk(
        Guid id,
        Guid documentId,
        string ownerId,
        int chunkIndex,
        string content,
        int? pageNumber,
        int tokenCount,
        float[] embedding)
    {
        Id = id;
        DocumentId = documentId;
        OwnerId = ownerId;
        ChunkIndex = chunkIndex;
        Content = content;
        PageNumber = pageNumber;
        TokenCount = tokenCount;
        Embedding = embedding;
    }

    private Chunk()
    {
    }

    public Guid Id { get; private set; }

    public Guid DocumentId { get; private set; }

    // Copied from the document so search can filter by owner without a join.
    public string OwnerId { get; private set; } = string.Empty;

    public int ChunkIndex { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public int? PageNumber { get; private set; }

    public int TokenCount { get; private set; }

    public float[] Embedding { get; private set; } = [];

    public static Result<Chunk> Create(
        Guid documentId,
        string ownerId,
        int chunkIndex,
        string content,
        int? pageNumber,
        int tokenCount,
        float[] embedding)
    {
        if (documentId == Guid.Empty)
        {
            return ChunkErrors.DocumentRequired;
        }

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return ChunkErrors.OwnerRequired;
        }

        if (chunkIndex < 0)
        {
            return ChunkErrors.InvalidIndex;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return ChunkErrors.ContentRequired;
        }

        if (embedding is null || embedding.Length == 0)
        {
            return ChunkErrors.EmbeddingRequired;
        }

        return new Chunk(
            Guid.CreateVersion7(),
            documentId,
            ownerId,
            chunkIndex,
            content,
            pageNumber,
            Math.Max(tokenCount, 0),
            embedding.ToArray());   // copy: nobody outside can change our vector
    }
}