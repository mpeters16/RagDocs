using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using RagDocs.Application.Documents;
using RagDocs.Application.Ingestion;
using RagDocs.Infrastructure.Extraction;

namespace RagDocs.UnitTests.Infrastructure;

public class TextExtractorTests
{
    private readonly TextExtractor _extractor = new(NullLogger<TextExtractor>.Instance);

    private static string SamplePdfPath => Path.Combine(AppContext.BaseDirectory, "TestData", "sample.pdf");

    [Fact]
    public async Task Extract_Pdf_ReturnsTextWithPageNumbers()
    {
        // Arrange
        await using var stream = File.OpenRead(SamplePdfPath);

        // Act
        var result = await _extractor.ExtractAsync(stream, AllowedContentTypes.Pdf, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.Code : null);
        Assert.Contains("25 days", result.Value.Single(p => p.PageNumber == 1).Text);
        Assert.Contains("two days per week", result.Value.Single(p => p.PageNumber == 2).Text);
    }

    [Fact]
    public async Task Extract_EmptyText_ReturnsNoText()
    {
        // Arrange
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes("   \n\n  "));

        // Act
        var result = await _extractor.ExtractAsync(stream, AllowedContentTypes.PlainText, CancellationToken.None);

        // Assert
        Assert.Equal(ExtractionErrors.NoText, result.Error);
    }
}