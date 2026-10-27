using RagDocs.Domain.Chunks;

public interface IChunkRepository
{
    //throw away document’s old chunks
    Task DeleteForDocumentAsync(Guid documentId, CancellationToken cancellationToken);

    //add the new chunks;
    void AddRange(IEnumerable<Chunk> chunks);
}