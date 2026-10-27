using RagDocs.Domain.Common;

namespace RagDocs.Domain.Documents;

public static class DocumentErrors
{
    // Validation errors (Document.Create)

    public static readonly Error OwnerRequired = Error.Validation(
        "Document.OwnerRequired",
        "The document must belong to an owner.");

    public static readonly Error FileNameRequired = Error.Validation(
        "Document.FileNameRequired",
        "The file name is required.");

    public static readonly Error FileNameTooLong = Error.Validation(
        "Document.FileNameTooLong",
        $"The file name cannot be longer than {Document.MaxFileNameLength} characters.");

    public static readonly Error ContentTypeRequired = Error.Validation(
        "Document.ContentTypeRequired",
        "The content type is required.");

    public static readonly Error StoragePathRequired = Error.Validation(
        "Document.StoragePathRequired",
        "The storage path is required.");

    public static readonly Error EmptyFile = Error.Validation(
        "Document.EmptyFile",
        "The uploaded file is empty.");

    public static readonly Error InvalidContentHash = Error.Validation(
        "Document.InvalidContentHash",
        "The content hash must be a 64-character hexadecimal SHA-256 value.");

    // Validation errors (status transitions)

    public static readonly Error InvalidChunkCount = Error.Validation(
        "Document.InvalidChunkCount",
        "A ready document must have at least one chunk.");

    public static readonly Error FailureReasonRequired = Error.Validation(
        "Document.FailureReasonRequired",
        "A failure reason is required when marking a document as failed.");

    // Conflict errors

    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "Document.InvalidStatusTransition",
        "The document cannot move to that status from its current status.");

    // Not found errors

    public static readonly Error NotFound = Error.NotFound(
        "Document.NotFound",
        "The document was not found.");

    public static readonly Error StillProcessing = Error.Conflict(
        "Document.StillProcessing",
        "The document is being processed. Try again in a moment.");
}