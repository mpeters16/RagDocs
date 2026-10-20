using System.Security.Cryptography;
using RagDocs.Application.Documents;
using RagDocs.Application.Documents.Upload;
using RagDocs.Domain.Common;
using RagDocs.Domain.Documents;
using RagDocs.UnitTests.Application.Fakes;

namespace RagDocs.UnitTests.Application;

public class UploadDocumentHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 20, 9, 0, 0, TimeSpan.Zero);

    // xUnit creates a new instance of this class for EVERY test,
    // so each test starts with empty fakes.
    private readonly InMemoryDocumentRepository _repository = new();
    private readonly InMemoryFileStore _fileStore = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeCurrentUser _currentUser = new();

    private UploadDocumentHandler CreateHandler() =>
        new(_repository, _fileStore, _unitOfWork, _currentUser, new FixedTimeProvider(Now));

    private Task<Result<UploadDocumentResult>> UploadAsync(byte[] bytes, string fileName = "handbook.pdf") =>
        UploadAsync(new MemoryStream(bytes), fileName);

    private Task<Result<UploadDocumentResult>> UploadAsync(Stream stream, string fileName) =>
        CreateHandler().HandleAsync(new UploadDocumentCommand(fileName, stream), CancellationToken.None);

    // ---------- Happy path ----------

    [Fact]
    public async Task Upload_ValidPdf_CreatesPendingDocument()
    {
        // Arrange
        var bytes = TestFiles.Pdf;

        // Act
        var result = await UploadAsync(bytes);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.AlreadyExisted);

        var document = Assert.Single(_repository.Documents);
        Assert.Equal(result.Value.DocumentId, document.Id);
        Assert.Equal("user-a", document.OwnerId);
        Assert.Equal("handbook.pdf", document.FileName);
        Assert.Equal(DocumentStatus.Pending, document.Status);
        Assert.Equal(bytes.Length, document.SizeBytes);
        Assert.Equal(Now, document.CreatedAt);

        var savedFile = Assert.Single(_fileStore.Files);
        Assert.Equal(document.StoragePath, savedFile.Key);
        Assert.Equal(bytes, savedFile.Value);

        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Upload_StoresDetectedContentType_NotTheFileExtension()
    {
        // PDF bytes, but named .txt
        var result = await UploadAsync(TestFiles.Pdf, "cv.txt");

        Assert.True(result.IsSuccess);
        Assert.Equal(AllowedContentTypes.Pdf, Assert.Single(_repository.Documents).ContentType);
    }

    [Fact]
    public async Task Upload_StoresSha256HashOfContent()
    {
        var bytes = TestFiles.Pdf;
        var expectedHash = Convert.ToHexStringLower(SHA256.HashData(bytes));

        await UploadAsync(bytes);

        Assert.Equal(expectedHash, Assert.Single(_repository.Documents).ContentHash);
    }

    [Fact]
    public async Task Upload_FileNameWithPath_KeepsOnlyTheName()
    {
        var result = await UploadAsync(TestFiles.Pdf, "../../secret/handbook.pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal("handbook.pdf", Assert.Single(_repository.Documents).FileName);
    }

    [Fact]
    public async Task Upload_NonSeekableStream_Works()
    {
        var bytes = TestFiles.Pdf;

        var result = await UploadAsync(new NonSeekableStream(bytes), "handbook.pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal(bytes, Assert.Single(_fileStore.Files).Value);
    }

    // ---------- Duplicates ----------

    [Fact]
    public async Task Upload_SameFileTwice_ReturnsExistingDocument()
    {
        // Act
        var first = await UploadAsync(TestFiles.Pdf);
        var second = await UploadAsync(TestFiles.Pdf);

        // Assert
        Assert.True(second.IsSuccess);
        Assert.True(second.Value.AlreadyExisted);
        Assert.Equal(first.Value.DocumentId, second.Value.DocumentId);

        Assert.Single(_repository.Documents);
        Assert.Single(_fileStore.Files);
        Assert.Equal(1, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Upload_SameFileDifferentUsers_CreatesTwoDocuments()
    {
        var first = await UploadAsync(TestFiles.Pdf);

        _currentUser.UserId = "user-b";
        var second = await UploadAsync(TestFiles.Pdf);

        Assert.False(second.Value.AlreadyExisted);
        Assert.NotEqual(first.Value.DocumentId, second.Value.DocumentId);
        Assert.Equal(2, _repository.Documents.Count);
    }

    // ---------- Rejected before saving ----------

    [Fact]
    public async Task Upload_UnsupportedType_ReturnsErrorAndSavesNothing()
    {
        var result = await UploadAsync(TestFiles.Exe, "virus.pdf");

        Assert.Equal(UploadErrors.UnsupportedFileType, result.Error);
        Assert.Empty(_fileStore.Files);
        Assert.Empty(_repository.Documents);
        Assert.Equal(0, _unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsEmptyFile()
    {
        var result = await UploadAsync([]);

        Assert.Equal(DocumentErrors.EmptyFile, result.Error);
        Assert.Empty(_fileStore.Files);
    }

    [Fact]
    public async Task Upload_TooLarge_ReturnsFileTooLarge()
    {
        var result = await UploadAsync(TestFiles.TooLargePdf());

        Assert.Equal(UploadErrors.FileTooLarge, result.Error);
        Assert.Empty(_fileStore.Files);
    }

    // ---------- Clean-up after saving ----------

    [Fact]
    public async Task Upload_DomainRejectsDocument_DeletesSavedFile()
    {
        // 252 + ".pdf" = 256 characters, one over the limit
        var tooLongName = new string('x', 252) + ".pdf";

        var result = await UploadAsync(TestFiles.Pdf, tooLongName);

        Assert.Equal(DocumentErrors.FileNameTooLong, result.Error);
        Assert.Single(_fileStore.DeletedPaths);
        Assert.Empty(_fileStore.Files);
        Assert.Empty(_repository.Documents);
    }

    [Fact]
    public async Task Upload_SaveChangesThrows_DeletesSavedFileAndRethrows()
    {
        _unitOfWork.ThrowOnSave = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => UploadAsync(TestFiles.Pdf));

        Assert.Single(_fileStore.DeletedPaths);
        Assert.Empty(_fileStore.Files);
    }
}