using System.Text;
using RagDocs.Api.Common;
using RagDocs.Application.Abstractions;
using RagDocs.Application.Search;

namespace RagDocs.Api.Endpoints;

// Development-only tools. Not part of the real API.
internal static class DevEndpoints
{
    public static IEndpointRouteBuilder MapDevEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/dev/similarity", SimilarityAsync)
            .ExcludeFromDescription();

        app.MapPost("/dev/ask-preview", async Task<IResult> (
            DevAskRequest request,
            SearchChunksHandler search,
            Application.Ask.PromptBuilder prompts,
            IAnswerGenerator answers,
            CancellationToken cancellationToken) =>
        {
            var found = await search.HandleAsync(new SearchChunksQuery(request.Question), cancellationToken);
            if (found.IsFailure)
            {
                return found.Error.ToProblem();
            }

            var prompt = prompts.Build(request.Question, found.Value);

            var answer = new StringBuilder();
            await foreach (var piece in answers.StreamAnswerAsync(prompt, cancellationToken))
            {
                answer.Append(piece);
            }

            return TypedResults.Ok(new
            {
                Answer = answer.ToString(),
                Sources = found.Value.Select((chunk, index) => new
                {
                    Number = index + 1,
                    chunk.FileName,
                    chunk.PageNumber,
                    Score = Math.Round(chunk.Score, 3),
                }),
                prompt.System,
                prompt.User,
            });
        });

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

    public sealed record DevAskRequest(string Question);

    
}