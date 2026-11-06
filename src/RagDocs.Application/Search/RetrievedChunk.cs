namespace RagDocs.Application.Search;

// One search hit: where it came from, the text, and how similar it is (0–1, higher is better).
public sealed record RetrievedChunk(
    Guid ChunkId,
    Guid DocumentId,
    string FileName,
    int ChunkIndex,
    int? PageNumber,
    string Content,
    double Score);
    