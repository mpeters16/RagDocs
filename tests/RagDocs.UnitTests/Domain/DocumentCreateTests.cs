using RagDocs.Domain.Documents;

namespace RagDocs.UnitTests.Domain;

public class DocumentCreateTests
{
    // ---------- Happy path ----------

    [Fact]
    public void Create_WithValidInput_ReturnsPendingDocument()
    {
        // Act
        var result = DocumentFactory.Create();

        // Assert
        Assert.True(result.IsSuccess);

        var document = result.Value;
        Assert.NotEqual(Guid.Empty, document.Id);
        Assert.Equal(DocumentFactory.ValidOwnerId, document.OwnerId);
        Assert.Equal(DocumentFactory.ValidFileName, document.FileName);
        Assert.Equal(DocumentFactory.ValidContentType, document.ContentType);
        Assert.Equal(DocumentFactory.ValidStoragePath, document.StoragePath);
        Assert.Equal(DocumentFactory.ValidHash, document.ContentHash);
        Assert.Equal(DocumentFactory.ValidSizeBytes, document.SizeBytes);
        Assert.Equal(DocumentStatus.Pending, document.Status);
        Assert.Equal(DocumentFactory.Now, document.CreatedAt);
        Assert.Null(document.FailureReason);
        Assert.Null(document.ProcessedAt);
        Assert.Equal(0, document.ChunkCount);
    }

    [Fact]
    public void Create_CalledTwice_GivesDifferentIds()
    {
        // Act
        var first = DocumentFactory.CreateValid();
        var second = DocumentFactory.CreateValid();

        // Assert
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Create_WithUpperCaseHash_StoresHashLowerCase()
    {
        // Arrange
        var upperCaseHash = new string('A', 64);

        // Act
        var result = DocumentFactory.Create(contentHash: upperCaseHash);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(new string('a', 64), result.Value.ContentHash);
    }

    // ---------- Rule 1: owner ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankOwner_ReturnsOwnerRequired(string ownerId)
    {
        var result = DocumentFactory.Create(ownerId: ownerId);

        Assert.True(result.IsFailure);
        Assert.Equal(DocumentErrors.OwnerRequired, result.Error);
    }

    // ---------- Rule 2: file name required ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankFileName_ReturnsFileNameRequired(string fileName)
    {
        var result = DocumentFactory.Create(fileName: fileName);

        Assert.True(result.IsFailure);
        Assert.Equal(DocumentErrors.FileNameRequired, result.Error);
    }

    // ---------- Rule 3: file name length (boundary) ----------

    [Fact]
    public void Create_WithFileNameAtMaxLength_Succeeds()
    {
        // Arrange: exactly 255 characters
        var fileName = new string('x', Document.MaxFileNameLength);

        // Act
        var result = DocumentFactory.Create(fileName: fileName);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithFileNameOverMaxLength_ReturnsFileNameTooLong()
    {
        // Arrange: 256 characters
        var fileName = new string('x', Document.MaxFileNameLength + 1);

        // Act
        var result = DocumentFactory.Create(fileName: fileName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DocumentErrors.FileNameTooLong, result.Error);
    }

    // ---------- Rule 4: content type ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankContentType_ReturnsContentTypeRequired(string contentType)
    {
        var result = DocumentFactory.Create(contentType: contentType);

        Assert.True(result.IsFailure);
        Assert.Equal(DocumentErrors.ContentTypeRequired, result.Error);
    }

    // ---------- Rule 5: storage path ----------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankStoragePath_ReturnsStoragePathRequired(string storagePath)
    {
        var result = DocumentFactory.Create(storagePath: storagePath);

        Assert.True(result.IsFailure);
        Assert.Equal(DocumentErrors.StoragePathRequired, result.Error);
    }

    // ---------- Rule 6: size ----------

    [Theory]
    [InlineData(0L)]
    [InlineData(-1L)]
    public void Create_WithZeroOrNegativeSize_ReturnsEmptyFile(long sizeBytes)
    {
        var result = DocumentFactory.Create(sizeBytes: sizeBytes);

        Assert.True(result.IsFailure);
        Assert.Equal(DocumentErrors.EmptyFile, result.Error);
    }

    // ---------- Rule 7: content hash ----------

    [Theory]
    [InlineData(0, 'a')]    // empty
    [InlineData(63, 'a')]   // one too short
    [InlineData(65, 'a')]   // one too long
    [InlineData(64, 'g')]   // right length, but 'g' is not hex
    public void Create_WithInvalidHash_ReturnsInvalidContentHash(int length, char character)
    {
        // Arrange
        var hash = new string(character, length);

        // Act
        var result = DocumentFactory.Create(contentHash: hash);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(DocumentErrors.InvalidContentHash, result.Error);
    }

    // ---------- Rule order ----------

    [Fact]
    public void Create_WithSeveralInvalidValues_ReturnsFirstRuleError()
    {
        // Owner (rule 1) and file name (rule 2) are both blank.
        var result = DocumentFactory.Create(ownerId: "", fileName: "");

        Assert.Equal(DocumentErrors.OwnerRequired, result.Error);
    }
}