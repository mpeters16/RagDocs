using RagDocs.Application.Abstractions;
using RagDocs.Domain.Common;

namespace RagDocs.Application.Search;

public sealed class SearchChunksHandler(
    IEmbeddingService embeddings,
    IChunkSearch chunkSearch,
    ICurrentUser currentUser)
{
    public async Task<Result<IReadOnlyList<RetrievedChunk>>> HandleAsync(
        SearchChunksQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Question))
        {
            return SearchErrors.EmptyQuestion;
        }

        var question = query.Question.Trim();

        if (question.Length > SearchLimits.MaxQuestionLength)
        {
            return SearchErrors.QuestionTooLong;
        }

        if (query.TopK is < 1 or > SearchLimits.MaxTopK)
        {
            return SearchErrors.InvalidTopK;
        }

        var vectors = await embeddings.EmbedAsync([question], EmbeddingPurpose.Query, cancellationToken);

        var results = await chunkSearch.SearchAsync(vectors[0], currentUser.UserId, query.TopK, cancellationToken);

        return Result<IReadOnlyList<RetrievedChunk>>.Success(results);
    }
}