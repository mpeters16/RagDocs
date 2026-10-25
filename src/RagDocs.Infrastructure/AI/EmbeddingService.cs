using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using RagDocs.Application.Abstractions;

namespace RagDocs.Infrastructure.AI;

internal sealed class EmbeddingService(
    IEmbeddingGenerator<string, Embedding<float>> generator,
    IOptions<AiOptions> options) : IEmbeddingService
{
    public async Task<IReadOnlyList<float[]>> EmbedAsync(
        IReadOnlyList<string> texts,
        EmbeddingPurpose purpose,
        CancellationToken cancellationToken)
    {
        if (texts.Count == 0)
        {
            return [];
        }

        var prefix = purpose == EmbeddingPurpose.Query
            ? options.Value.QueryPrefix
            : options.Value.DocumentPrefix;

        var inputs = texts.Select(text => prefix + text).ToList();

        var embeddings = await generator.GenerateAsync(inputs, cancellationToken: cancellationToken);

        var vectors = embeddings.Select(e => e.Vector.ToArray()).ToList();

        if (vectors.Count != texts.Count)
        {
            throw new InvalidOperationException(
                $"Asked for {texts.Count} embeddings but received {vectors.Count}.");
        }

        var wrongSize = vectors.FirstOrDefault(v => v.Length != EmbeddingSettings.Dimensions);

        if (wrongSize is not null)
        {
            throw new InvalidOperationException(
                $"The embedding model returned {wrongSize.Length} dimensions, but the database expects " +
                $"{EmbeddingSettings.Dimensions}. Changing models needs a migration and re-embedding.");
        }

        return vectors;
    }
}