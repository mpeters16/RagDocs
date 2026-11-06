using RagDocs.Domain.Common;

namespace RagDocs.Application.Search;

public static class SearchErrors
{
    public static readonly Error EmptyQuestion = Error.Validation(
        "Search.EmptyQuestion",
        "Enter a question.");

    public static readonly Error QuestionTooLong = Error.Validation(
        "Search.QuestionTooLong",
        $"A question can be at most {SearchLimits.MaxQuestionLength} characters.");

    public static readonly Error InvalidTopK = Error.Validation(
        "Search.InvalidTopK",
        $"TopK must be between 1 and {SearchLimits.MaxTopK}.");
}