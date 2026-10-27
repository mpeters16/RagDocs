using Microsoft.Extensions.Logging;
using RagDocs.Application.Abstractions;
using RagDocs.Domain.Common;
using RagDocs.Domain.Documents;

namespace RagDocs.Application.Documents.Delete;

public sealed class DeleteDocumentHandler(
    IDocumentRepository documents,
    IFileStore fileStore,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<DeleteDocumentHandler> logger)
{
    public async Task<Result> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await documents.GetByIdAsync(id, currentUser.UserId, cancellationToken);
        if (document is null)
        {
            return DocumentErrors.NotFound;
        }

        var canDelete = document.EnsureCanBeDeleted();
        if (canDelete.IsFailure)
        {
            return canDelete;
        }

        documents.Remove(document);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // The row (and its chunks) is gone; now remove the file.
        // If that fails, the user's delete still succeeded: log it and move on.
        try
        {
            await fileStore.DeleteAsync(document.StoragePath, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Document {DocumentId} was deleted but its file could not be removed", document.Id);
        }

        return Result.Success();
    }
}