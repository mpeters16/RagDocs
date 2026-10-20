using RagDocs.Application.Documents;
using RagDocs.Application.Documents.Upload;

namespace RagDocs.UnitTests.Application;

public class FileTypeDetectorTests
{
    [Theory]
    [InlineData("report.pdf")]
    [InlineData("notes.txt")]
    [InlineData("no-extension")]
    public void Detect_PdfBytes_ReturnsPdfWhateverTheName(string fileName)
    {
        var result = FileTypeDetector.Detect(TestFiles.Pdf, fileName);

        Assert.Equal(AllowedContentTypes.Pdf, result);
    }

    [Theory]
    [InlineData("report.docx", AllowedContentTypes.Docx)]
    [InlineData("REPORT.DOCX", AllowedContentTypes.Docx)]
    [InlineData("report.pdf", null)]      // ZIP pretending to be a PDF
    [InlineData("photos.zip", null)]      // ordinary ZIP
    public void Detect_ZipBytes_OnlyAcceptsDocx(string fileName, string? expected)
    {
        var result = FileTypeDetector.Detect(TestFiles.Docx, fileName);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("notes.txt", AllowedContentTypes.PlainText)]
    [InlineData("NOTES.TXT", AllowedContentTypes.PlainText)]
    [InlineData("README.md", AllowedContentTypes.Markdown)]
    [InlineData("script.exe", null)]
    [InlineData("no-extension", null)]
    public void Detect_TextBytes_UsesExtension(string fileName, string? expected)
    {
        var result = FileTypeDetector.Detect(TestFiles.Text, fileName);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Detect_ProgramRenamedToTxt_ReturnsNull()
    {
        var result = FileTypeDetector.Detect(TestFiles.Exe, "innocent.txt");

        Assert.Null(result);
    }

    [Fact]
    public void Detect_EmptyHeader_ReturnsNull()
    {
        var result = FileTypeDetector.Detect(ReadOnlySpan<byte>.Empty, "empty.txt");

        Assert.Null(result);
    }
}