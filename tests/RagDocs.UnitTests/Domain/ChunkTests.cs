using RagDocs.Domain.Chunks;

namespace RagDocs.UnitTests.Domain;

public class ChunkTests
{
    private static readonly Guid DocumentId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInput_ReturnsChunk()
    {
        // Act
        var result = Chunk.Create(DocumentId, "user-a", 0, "Some text.", 1, 3, [0.1f, 0.2f, 0.3f]);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentId, result.Value.DocumentId);
        Assert.Equal("user-a", result.Value.OwnerId);
        Assert.Equal(3, result.Value.Embedding.Length);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public void Create_WithEmptyEmbedding_ReturnsEmbeddingRequired()
    {
        // Act
        var result = Chunk.Create(DocumentId, "user-a", 0, "Some text.", 1, 3, []);

        // Assert
        Assert.Equal(ChunkErrors.EmbeddingRequired, result.Error);
    }
}