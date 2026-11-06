using RagDocs.Application.Search;

namespace RagDocs.Application.Abstractions;

public interface IChunkSearch
{
    // The most similar chunks of READY documents owned by ownerId, best first.
    Task<IReadOnlyList<RetrievedChunk>> SearchAsync(
        float[] queryEmbedding,
        string ownerId,
        int topK,
        CancellationToken cancellationToken);
}