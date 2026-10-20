using RagDocs.Application.Documents.Get;
using RagDocs.Domain.Documents;
using RagDocs.UnitTests.Application.Fakes;
using RagDocs.UnitTests.Domain;

namespace RagDocs.UnitTests.Application;

public class GetDocumentHandlerTests
{
    private readonly InMemoryDocumentRepository _repository = new();
    private readonly FakeCurrentUser _currentUser = new() { UserId = "user-a" };

    private GetDocumentHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task Get_OwnDocument_ReturnsResponse()
    {
        // Arrange
        var document = DocumentFactory.Create(ownerId: "user-a").Value;
        _repository.Add(document);

        // Act
        var result = await CreateHandler().HandleAsync(document.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(document.Id, result.Value.Id);
        Assert.Equal(document.FileName, result.Value.FileName);
        Assert.Equal("Pending", result.Value.Status);
    }

    [Fact]
    public async Task Get_UnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().HandleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(DocumentErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Get_OtherUsersDocument_ReturnsNotFound()
    {
        var othersDocument = DocumentFactory.Create(ownerId: "user-b").Value;
        _repository.Add(othersDocument);

        var result = await CreateHandler().HandleAsync(othersDocument.Id, CancellationToken.None);

        Assert.Equal(DocumentErrors.NotFound, result.Error);
    }
}