using Microsoft.AspNetCore.Http.HttpResults;
using RagDocs.Domain.Common;

namespace RagDocs.Api.Common;

internal static class ResultExtensions
{
    public static ProblemHttpResult ToProblem(this Error error)
    {
        var (statusCode, title) = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation error"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Server error")
        };

        return TypedResults.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Description,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}