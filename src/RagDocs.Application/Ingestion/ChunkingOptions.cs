namespace RagDocs.Application.Ingestion;

public sealed record ChunkingOptions
{
    public static ChunkingOptions Default { get; } = new();

    // ~500 tokens ≈ 375 words: big enough for context, small enough for precise matches.
    public int MaxTokens { get; init; } = 500;

    // ~20% overlap so boundary sentences appear whole in at least one chunk.
    public int OverlapTokens { get; init; } = 100;
}