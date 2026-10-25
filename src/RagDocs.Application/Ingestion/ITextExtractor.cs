using RagDocs.Application.Ingestion;
using RagDocs.Domain.Common;

namespace RagDocs.Application.Abstractions;

public interface ITextExtractor
{
    Task<Result<IReadOnlyList<ExtractedPage>>> ExtractAsync(
        Stream content,
        string contentType,
        CancellationToken cancellationToken);
}