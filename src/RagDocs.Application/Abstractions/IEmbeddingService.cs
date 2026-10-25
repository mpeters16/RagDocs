namespace RagDocs.Application.Abstractions;

public enum EmbeddingPurpose
{
    Document,
    Query
}

public interface IEmbeddingService
{
    Task<IReadOnlyList<float[]>> EmbedAsync(
        IReadOnlyList<string> texts,
        EmbeddingPurpose purpose,
        CancellationToken cancellationToken);
}