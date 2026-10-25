using RagDocs.Application.Abstractions;

namespace RagDocs.Api.Endpoints;

// Development-only tools. Not part of the real API.
internal static class DevEndpoints
{
    public static IEndpointRouteBuilder MapDevEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/dev/similarity", SimilarityAsync)
            .ExcludeFromDescription();

        return app;
    }

    private static async Task<IResult> SimilarityAsync(
        SimilarityRequest request,
        IEmbeddingService embeddings,
        CancellationToken cancellationToken)
    {
        var question = (await embeddings.EmbedAsync([request.Question], EmbeddingPurpose.Query, cancellationToken))[0];
        var texts = await embeddings.EmbedAsync(request.Texts, EmbeddingPurpose.Document, cancellationToken);

        var ranked = request.Texts
            .Select((text, i) => new { text, score = Math.Round(CosineSimilarity(question, texts[i]), 3) })
            .OrderByDescending(r => r.score);

        return TypedResults.Ok(ranked);
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, lengthA = 0, lengthB = 0;

        //similarity = (a · b) / (|a| × |b|)
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            lengthA += a[i] * a[i];
            lengthB += b[i] * b[i];
        }

        return dot / (Math.Sqrt(lengthA) * Math.Sqrt(lengthB));
    }

    internal sealed record SimilarityRequest(string Question, string[] Texts);
}