namespace RagDocs.Application.Ingestion;

// One chunk ready to embed. PageNumber = page where the chunk's new content starts.
public sealed record TextChunk(int Index, string Text, int? PageNumber, int TokenCount);