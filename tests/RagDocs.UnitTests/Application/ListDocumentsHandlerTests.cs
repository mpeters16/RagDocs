using RagDocs.Application.Documents.List;
using RagDocs.UnitTests.Application.Fakes;
using RagDocs.UnitTests.Domain;

namespace RagDocs.UnitTests.Application;

public class ListDocumentsHandlerTests
{
    private const string CurrentUserId = "user-a";

    private readonly InMemoryDocumentRepository _repository = new();
    private readonly FakeCurrentUser _currentUser = new() { UserId = CurrentUserId };

    private ListDocumentsHandler CreateHandler() => new(_repository, _currentUser);

    [Fact]
    public async Task List_ReturnsOnlyCurrentUsersDocuments()
    {
        _repository.Add(DocumentFactory.Create(ownerId: CurrentUserId).Value);
        _repository.Add(DocumentFactory.Create(ownerId: CurrentUserId).Value);
        _repository.Add(DocumentFactory.Create(ownerId: "user-b").Value);

        var result = await CreateHandler().HandleAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task List_ReturnsNewestFirst()
    {
        // Arrange: both documents belong to the current user.
        var older = DocumentFactory.Create(ownerId: CurrentUserId, now: DocumentFactory.Now).Value;
        var newer = DocumentFactory.Create(ownerId: CurrentUserId, now: DocumentFactory.Now.AddHours(1)).Value;
        _repository.Add(older);
        _repository.Add(newer);

        // Act
        var result = await CreateHandler().HandleAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Value.Count);   // fails with a clear message if the list is empty
        Assert.Equal(newer.Id, result.Value[0].Id);
        Assert.Equal(older.Id, result.Value[1].Id);
    }

    [Fact]
    public async Task List_NoDocuments_ReturnsEmptySuccess()
    {
        var result = await CreateHandler().HandleAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }
}