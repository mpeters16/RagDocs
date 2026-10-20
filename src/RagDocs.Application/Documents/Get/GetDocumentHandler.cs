using RagDocs.Application.Abstractions;
using RagDocs.Domain.Common;
using RagDocs.Domain.Documents;

namespace RagDocs.Application.Documents.Get;

public sealed class GetDocumentHandler(
    IDocumentRepository documentRepository,
    ICurrentUser currentUser)
{
    public async Task<Result<DocumentResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var document = await documentRepository.GetByIdAsync(
            id, currentUser.UserId, cancellationToken);

        if (document is null)
        {
            return DocumentErrors.NotFound;
        }

        return DocumentResponse.From(document);
    }
}