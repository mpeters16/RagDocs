using RagDocs.Domain.Common;

namespace RagDocs.Domain.Chunks;

public static class ChunkErrors
{
    public static readonly Error DocumentRequired = Error.Validation(
        "Chunk.DocumentRequired", "A chunk must belong to a document.");

    public static readonly Error OwnerRequired = Error.Validation(
        "Chunk.OwnerRequired", "A chunk must have an owner.");

    public static readonly Error InvalidIndex = Error.Validation(
        "Chunk.InvalidIndex", "The chunk index can't be negative.");

    public static readonly Error ContentRequired = Error.Validation(
        "Chunk.ContentRequired", "A chunk must have text.");

    public static readonly Error EmbeddingRequired = Error.Validation(
        "Chunk.EmbeddingRequired", "A chunk must have an embedding.");
}