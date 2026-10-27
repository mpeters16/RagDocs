using RagDocs.Domain.Common;

namespace RagDocs.Domain.Documents;

public sealed class Document
{
    public const int MaxFileNameLength = 255;
    private const int Sha256HexLength = 64;

    private Document(
        Guid id,
        string ownerId,
        string fileName,
        string contentType,
        string storagePath,
        string contentHash,
        long sizeBytes,
        DateTimeOffset createdAt)
    {
        Id = id;
        OwnerId = ownerId;
        FileName = fileName;
        ContentType = contentType;
        StoragePath = storagePath;
        ContentHash = contentHash;
        SizeBytes = sizeBytes;
        Status = DocumentStatus.Pending;
        CreatedAt = createdAt;
    }

    private Document()
    {
    }

    public Guid Id { get; private set; }

    public string OwnerId { get; private set; } = string.Empty;

    public string FileName { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public string StoragePath { get; private set; } = string.Empty;

    public string ContentHash { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }

    public DocumentStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    public int ChunkCount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public static Result<Document> Create(
        string ownerId,
        string fileName,
        string contentType,
        string storagePath,
        string contentHash,
        long sizeBytes,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return DocumentErrors.OwnerRequired;
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return DocumentErrors.FileNameRequired;
        }

        if (fileName.Length > MaxFileNameLength)
        {
            return DocumentErrors.FileNameTooLong;
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return DocumentErrors.ContentTypeRequired;
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return DocumentErrors.StoragePathRequired;
        }

        if (sizeBytes <= 0)
        {
            return DocumentErrors.EmptyFile;
        }

        if (!IsValidSha256Hex(contentHash))
        {
            return DocumentErrors.InvalidContentHash;
        }

        return new Document(
            Guid.CreateVersion7(),
            ownerId,
            fileName,
            contentType,
            storagePath,
            contentHash.ToLowerInvariant(),
            sizeBytes,
            now);
    }

    public Result StartProcessing(DateTimeOffset now)
    {
        if (Status != DocumentStatus.Pending)
        {
            return DocumentErrors.InvalidStatusTransition;
        }

        Status = DocumentStatus.Processing;
        return Result.Success();
    }

    public Result MarkReady(int chunkCount, DateTimeOffset now)
    {
        if (Status != DocumentStatus.Processing)
        {
            return DocumentErrors.InvalidStatusTransition;
        }

        if (chunkCount <= 0)
        {
            return DocumentErrors.InvalidChunkCount;
        }

        Status = DocumentStatus.Ready;
        ChunkCount = chunkCount;
        ProcessedAt = now;
        return Result.Success();
    }

    public Result MarkFailed(string reason, DateTimeOffset now)
    {
        if (Status != DocumentStatus.Processing)
        {
            return DocumentErrors.InvalidStatusTransition;
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return DocumentErrors.FailureReasonRequired;
        }

        Status = DocumentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = now;
        return Result.Success();
    }

    private static bool IsValidSha256Hex(string? value) =>
        value is { Length: Sha256HexLength } && value.All(char.IsAsciiHexDigit);

    public Result EnsureCanBeDeleted()
    {
        if (Status == DocumentStatus.Processing)
        {
            return DocumentErrors.StillProcessing;
        }

        return Result.Success();
    }
}