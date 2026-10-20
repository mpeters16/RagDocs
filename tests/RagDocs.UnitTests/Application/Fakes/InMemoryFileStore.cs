using RagDocs.Application.Abstractions;

namespace RagDocs.UnitTests.Application.Fakes;

internal sealed class InMemoryFileStore : IFileStore
{
    public Dictionary<string, byte[]> Files { get; } = new();

    public List<string> DeletedPaths { get; } = new();

    public async Task<string> SaveAsync(
        Stream content, string fileName, CancellationToken cancellationToken)
    {
        using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken);

        var path = $"files/{Guid.NewGuid()}/{fileName}";
        Files[path] = copy.ToArray();

        return path;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken) =>
        Task.FromResult<Stream>(new MemoryStream(Files[storagePath]));

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        Files.Remove(storagePath);      // no error if already gone
        DeletedPaths.Add(storagePath);  // remember it for assertions
        return Task.CompletedTask;
    }
}