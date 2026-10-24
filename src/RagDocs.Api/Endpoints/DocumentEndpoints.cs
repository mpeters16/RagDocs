using Microsoft.AspNetCore.Http.HttpResults;
using RagDocs.Api.Common;
using RagDocs.Application.Documents;
using RagDocs.Application.Documents.Get;
using RagDocs.Application.Documents.List;
using RagDocs.Application.Documents.Upload;

namespace RagDocs.Api.Endpoints;

internal static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/documents").WithTags("Documents");

        group.MapPost("/", UploadAsync)
            .DisableAntiforgery();

        group.MapGet("/", ListAsync);

        group.MapGet("/{id:guid}", GetAsync);

        return app;
    }

    private static async Task<Results<Accepted<UploadDocumentResult>, Ok<UploadDocumentResult>, ProblemHttpResult>> UploadAsync(
        IFormFile file,
        UploadDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        await using var content = file.OpenReadStream();

        var result = await handler.HandleAsync(
            new UploadDocumentCommand(file.FileName, content),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        if (result.Value.AlreadyExisted)
        {
            return TypedResults.Ok(result.Value);
        }

        return TypedResults.Accepted($"/documents/{result.Value.DocumentId}", result.Value);
    }

    private static async Task<Results<Ok<IReadOnlyList<DocumentResponse>>, ProblemHttpResult>> ListAsync(
        ListDocumentsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }

    private static async Task<Results<Ok<DocumentResponse>, ProblemHttpResult>> GetAsync(
        Guid id,
        GetDocumentHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(result.Value);
    }
}