using Microsoft.Extensions.Logging.Abstractions;
using RagDocs.Application.Documents;
using RagDocs.Application.Ingestion;
using RagDocs.Infrastructure.Extraction;
using Xunit.Abstractions;   // xUnit v3: delete this line (ITestOutputHelper is in the Xunit namespace)

namespace RagDocs.UnitTests.Infrastructure;

public class ChunkPreviewTests(ITestOutputHelper output)
{
    [Fact]
    [Trait("Category", "Preview")]
    public async Task Preview_SamplePdfChunks()
    {
        var extractor = new TextExtractor(NullLogger<TextExtractor>.Instance);
        var chunker = new Chunker(new CharacterTokenCounter());

        await using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "TestData", "sample.pdf"));
        var pages = await extractor.ExtractAsync(stream, AllowedContentTypes.Pdf, CancellationToken.None);

        var chunks = chunker.Split(pages.Value, new ChunkingOptions { MaxTokens = 120, OverlapTokens = 25 });

        foreach (var chunk in chunks)
        {
            output.WriteLine($"── Chunk {chunk.Index} · page {chunk.PageNumber} · ~{chunk.TokenCount} tokens ──");
            output.WriteLine(chunk.Text);
            output.WriteLine(string.Empty);
        }

        Assert.NotEmpty(chunks);
    }
}