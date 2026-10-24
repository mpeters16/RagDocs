using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RagDocs.Application.Abstractions;

namespace RagDocs.Infrastructure.Storage;

internal sealed class LocalFileStore : IFileStore
{
    private const int BufferSize = 81920;

    private readonly string _rootPath;

    public LocalFileStore(IOptions<StorageOptions> options, IHostEnvironment environment)
    {
        _rootPath = Path.GetFullPath(
            Path.Combine(environment.ContentRootPath, options.Value.RootPath));

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        CancellationToken cancellationToken)
    {
        var safeFileName = Path.GetFileName(fileName);

        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = "file";
        }

        // Relative, with forward slashes: this is what gets stored on the Document.
        var storagePath = $"{Guid.NewGuid():N}/{safeFileName}";
        var fullPath = GetFullPath(storagePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        try
        {
            await using var file = new FileStream(
                fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                BufferSize, useAsync: true);

            await content.CopyToAsync(file, cancellationToken);
        }
        catch
        {
            // Don't leave half-written files behind (e.g. upload cancelled).
            DeleteFileAndFolder(fullPath);
            throw;
        }

        return storagePath;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        Stream stream = new FileStream(
            GetFullPath(storagePath), FileMode.Open, FileAccess.Read, FileShare.Read,
            BufferSize, useAsync: true);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        DeleteFileAndFolder(GetFullPath(storagePath));
        return Task.CompletedTask;
    }

    // Turns a stored relative path into a full path, refusing anything outside the root.
    private string GetFullPath(string storagePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, storagePath));

        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The storage path points outside the storage folder.");
        }

        return fullPath;
    }

    private void DeleteFileAndFolder(string fullPath)
    {
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        var folder = Path.GetDirectoryName(fullPath);

        if (folder is not null
            && !string.Equals(folder, _rootPath, StringComparison.OrdinalIgnoreCase)
            && Directory.Exists(folder)
            && !Directory.EnumerateFileSystemEntries(folder).Any())
        {
            Directory.Delete(folder);
        }
    }
}