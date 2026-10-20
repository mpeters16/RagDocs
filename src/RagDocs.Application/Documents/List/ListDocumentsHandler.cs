using RagDocs.Application.Abstractions;
using RagDocs.Domain.Common;

namespace RagDocs.Application.Documents.List;

public sealed class ListDocumentsHandler(
    IDocumentRepository documentRepository,
    ICurrentUser currentUser)
{
    public async Task<Result<IReadOnlyList<DocumentResponse>>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var documents = await documentRepository.ListAsync(
            currentUser.UserId, cancellationToken);

        var responses = documents
            .Select(DocumentResponse.From)
            .ToList();

        return Result<IReadOnlyList<DocumentResponse>>.Success(responses);
    }
}