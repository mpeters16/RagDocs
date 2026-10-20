namespace RagDocs.Application.Abstractions;

public interface IFileStore
{
    // Saves the file and returns where it was stored (kept on Document.StoragePath).
    Task<string> SaveAsync(
        Stream content,
        string fileName,
        CancellationToken cancellationToken);

    // Opens a stored file for reading (used by ingestion on Day 10).
    Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken);

    // Removes a stored file. Should not fail if the file is already gone.
    Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken);
}