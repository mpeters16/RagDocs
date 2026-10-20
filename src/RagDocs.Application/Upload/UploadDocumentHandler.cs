using System.Security.Cryptography;
using RagDocs.Application.Abstractions;
using RagDocs.Domain.Common;
using RagDocs.Domain.Documents;

namespace RagDocs.Application.Documents.Upload;

public sealed class UploadDocumentHandler(
    IDocumentRepository documentRepository,
    IFileStore fileStore,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<Result<UploadDocumentResult>> HandleAsync(
        UploadDocumentCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Step 1a: keep only the file name, never a path like "../../secret.txt".
        var fileName = Path.GetFileName(command.FileName);

        // Step 1b: we read the stream 3 times, so it must be rewindable.
        if (command.Content.CanSeek)
        {
            return await UploadAsync(command.Content, fileName, cancellationToken);
        }

        await using var copy = new MemoryStream();
        await command.Content.CopyToAsync(copy, cancellationToken);

        return await UploadAsync(copy, fileName, cancellationToken);
    }

    private async Task<Result<UploadDocumentResult>> UploadAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken)
    {
        // Step 2: size.
        if (stream.Length == 0)
        {
            return DocumentErrors.EmptyFile;
        }

        if (stream.Length > UploadLimits.MaxFileSizeBytes)
        {
            return UploadErrors.FileTooLarge;
        }

        // Step 3: real file type, from the bytes.
        var contentType = await DetectContentTypeAsync(stream, fileName, cancellationToken);

        if (contentType is null)
        {
            return UploadErrors.UnsupportedFileType;
        }

        // Step 4: fingerprint of the file content.
        stream.Position = 0;
        var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken);
        var contentHash = Convert.ToHexStringLower(hashBytes);

        // Step 5: same user, same file → return what they already have.
        var ownerId = currentUser.UserId;

        var existing = await documentRepository.GetByContentHashAsync(
            ownerId, contentHash, cancellationToken);

        if (existing is not null)
        {
            return new UploadDocumentResult(existing.Id, AlreadyExisted: true);
        }

        // Step 6: store the file.
        stream.Position = 0;
        var storagePath = await fileStore.SaveAsync(stream, fileName, cancellationToken);

        // Step 7: create the document; clean up the file if the Domain says no.
        var documentResult = Document.Create(
            ownerId,
            fileName,
            contentType,
            storagePath,
            contentHash,
            stream.Length,
            timeProvider.GetUtcNow());

        if (documentResult.IsFailure)
        {
            await fileStore.DeleteAsync(storagePath, CancellationToken.None);
            return documentResult.Error;
        }

        // Step 8: persist; clean up the file if the database write fails.
        var document = documentResult.Value;
        documentRepository.Add(document);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await fileStore.DeleteAsync(storagePath, CancellationToken.None);
            throw;
        }

        // Step 9.
        return new UploadDocumentResult(document.Id, AlreadyExisted: false);
    }

    private static async Task<string?> DetectContentTypeAsync(
        Stream stream,
        string fileName,
        CancellationToken cancellationToken)
    {
        stream.Position = 0;

        var buffer = new byte[FileTypeDetector.HeaderSize];
        var read = await stream.ReadAtLeastAsync(
            buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken);

        return FileTypeDetector.Detect(buffer.AsSpan(0, read), fileName);
    }
}