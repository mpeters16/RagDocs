using RagDocs.Domain.Common;
using RagDocs.Domain.Documents;

namespace RagDocs.UnitTests.Domain;

internal static class DocumentFactory
{
    public static readonly DateTimeOffset Now = new(2026, 10, 19, 9, 0, 0, TimeSpan.Zero);

    public const string ValidOwnerId = "dev-user";
    public const string ValidFileName = "handbook.pdf";
    public const string ValidContentType = "application/pdf";
    public const string ValidStoragePath = "uploads/dev-user/handbook.pdf";
    public const long ValidSizeBytes = 1024;
    public static readonly string ValidHash = new('a', 64);

    // Calls Document.Create with valid values; override only the one you're testing.
    // Example: DocumentFactory.Create(fileName: "")
    public static Result<Document> Create(
        string ownerId = ValidOwnerId,
        string fileName = ValidFileName,
        string contentType = ValidContentType,
        string storagePath = ValidStoragePath,
        string? contentHash = null,
        long sizeBytes = ValidSizeBytes,
        DateTimeOffset? now = null) =>
        Document.Create(
            ownerId,
            fileName,
            contentType,
            storagePath,
            contentHash ?? ValidHash,
            sizeBytes,
            now ?? Now);

    public static Document CreateValid() => Create().Value;

    public static Document CreateInStatus(DocumentStatus status)
    {
        var document = CreateValid();

        switch (status)
        {
            case DocumentStatus.Pending:
                break;

            case DocumentStatus.Processing:
                EnsureSuccess(document.StartProcessing(Now));
                break;

            case DocumentStatus.Ready:
                EnsureSuccess(document.StartProcessing(Now));
                EnsureSuccess(document.MarkReady(3, Now));
                break;

            case DocumentStatus.Failed:
                EnsureSuccess(document.StartProcessing(Now));
                EnsureSuccess(document.MarkFailed("boom", Now));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown status.");
        }

        return document;
    }

    // If setup fails, stop with a clear message instead of a confusing failure later.
    private static void EnsureSuccess(Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Test setup failed: {result.Error.Code} - {result.Error.Description}");
        }
    }
}