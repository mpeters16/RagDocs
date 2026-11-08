using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using RagDocs.Api.Common;
using RagDocs.Application.Ask;

namespace RagDocs.Api.Endpoints;

public sealed record AskRequest(string Question);

internal static class AskEndpoints
{
    public static IEndpointRouteBuilder MapAskEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/ask", AskAsync).WithTags("Ask");

        return app;
    }

    private static async Task<Results<ServerSentEventsResult<object>, ProblemHttpResult>> AskAsync(
        AskRequest request,
        AskQuestionHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new AskQuestionCommand(request.Question), cancellationToken);

        // Errors are returned BEFORE streaming starts, as normal ProblemDetails.
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.ServerSentEvents(ToEvents(result.Value, cancellationToken));
    }

    // token, token, token… then sources, then done.
    private static async IAsyncEnumerable<SseItem<object>> ToEvents(
        AskQuestionResult result,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var text in result.Answer.WithCancellation(cancellationToken))
        {
            yield return new SseItem<object>(new { text }, "token");
        }

        yield return new SseItem<object>(result.Sources, "sources");
        yield return new SseItem<object>(new { }, "done");
    }
}