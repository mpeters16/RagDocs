using Microsoft.AspNetCore.Http.HttpResults;
using RagDocs.Api.Common;
using RagDocs.Application.Search;

namespace RagDocs.Api.Endpoints;

public sealed record SearchRequest(string Question, int? TopK);

internal static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/search", SearchAsync).WithTags("Search");

        return app;
    }

    private static async Task<Results<Ok<IReadOnlyList<RetrievedChunk>>, ProblemHttpResult>> SearchAsync(
        SearchRequest request,
        SearchChunksHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new SearchChunksQuery(request.Question, request.TopK ?? SearchLimits.DefaultTopK);

        var result = await handler.HandleAsync(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}