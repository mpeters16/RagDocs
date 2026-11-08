using Microsoft.Extensions.Logging;
using RagDocs.Application.Abstractions;
using RagDocs.Application.Search;
using RagDocs.Domain.Common;

namespace RagDocs.Application.Ask;

public sealed class AskQuestionHandler(
    SearchChunksHandler search,
    PromptBuilder promptBuilder,
    IAnswerGenerator answerGenerator,
    ILogger<AskQuestionHandler> logger)
{
    private const int ExcerptLength = 200;

    public async Task<Result<AskQuestionResult>> HandleAsync(
        AskQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var found = await search.HandleAsync(new SearchChunksQuery(command.Question), cancellationToken);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var chunks = found.Value;

        if (chunks.Count == 0)
        {
            logger.LogInformation("No chunks found; answering with the no-information response");
            return new AskQuestionResult([], NoInformation());
        }

        logger.LogInformation(
            "Answering with {SourceCount} sources: {ChunkIds}",
            chunks.Count,
            chunks.Select(c => c.ChunkId).ToArray());

        var prompt = promptBuilder.Build(command.Question.Trim(), chunks);

        var sources = chunks
            .Select((chunk, index) => new AnswerSource(
                Number: index + 1,
                DocumentId: chunk.DocumentId,
                FileName: chunk.FileName,
                PageNumber: chunk.PageNumber,
                ChunkIndex: chunk.ChunkIndex,
                Score: Math.Round(chunk.Score, 3),
                Excerpt: Excerpt(chunk.Content)))
            .ToList();

        return new AskQuestionResult(sources, answerGenerator.StreamAnswerAsync(prompt, cancellationToken));
    }

    private static async IAsyncEnumerable<string> NoInformation()
    {
        await Task.CompletedTask; // an async iterator needs at least one await
        yield return PromptBuilder.NoInformationAnswer;
    }

    private static string Excerpt(string content) =>
        content.Length <= ExcerptLength ? content : content[..ExcerptLength] + "…";
}