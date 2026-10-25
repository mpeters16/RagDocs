namespace RagDocs.Infrastructure.AI;

internal static class EmbeddingSettings
{
    // Must match the embedding model (nomic-embed-text = 768).
    // Changing it needs a migration AND re-embedding every chunk.
    public const int Dimensions = 768;
}