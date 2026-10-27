using Microsoft.Extensions.Logging.Abstractions;
using RagDocs.Application.Documents.Delete;
using RagDocs.Domain.Documents;
using RagDocs.UnitTests.Application.Fakes;
using RagDocs.UnitTests.Domain;

namespace RagDocs.UnitTests.Application;

public class DeleteDocumentHandlerTests
{
    private readonly InMemoryDocumentRepository _documents = new();
    private readonly InMemoryFileStore _fileStore = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeCurrentUser _currentUser = new(); // UserId = "user-a"

    private DeleteDocumentHandler CreateHandler() =>
        new(_documents, _fileStore, _unitOfWork, _currentUser, NullLogger<DeleteDocumentHandler>.Instance);

    [Fact]
    public async Task Handle_OwnDocument_RemovesDocumentAndFile()
    {
        // Arrange
        var path = await _fileStore.SaveAsync(
            new MemoryStream("hello"u8.ToArray()), "notes.txt", CancellationToken.None);

        var document = DocumentFactory.Create(ownerId: "user-a", storagePath: path).Value;
        _documents.Add(document);

        // Act
        var result = await CreateHandler().HandleAsync(document.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(_documents.Documents);
        Assert.Contains(path, _fileStore.DeletedPaths);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Handle_OtherUsersDocument_ReturnsNotFoundAndKeepsIt()
    {
        // Arrange
        var document = DocumentFactory.Create(ownerId: "user-b").Value;
        _documents.Add(document);

        // Act
        var result = await CreateHandler().HandleAsync(document.Id, CancellationToken.None);

        // Assert
        Assert.Equal(DocumentErrors.NotFound, result.Error);
        Assert.Single(_documents.Documents);
        Assert.Empty(_fileStore.DeletedPaths);
        Assert.Equal(0, _unitOfWork.SaveChangesCalls);
    }
}