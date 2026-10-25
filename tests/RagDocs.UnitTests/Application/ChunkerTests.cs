using RagDocs.Application.Ingestion;
using RagDocs.UnitTests.Application.Fakes;

namespace RagDocs.UnitTests.Application;

public class ChunkerTests
{
    private static readonly ChunkingOptions SmallChunks = new() { MaxTokens = 50, OverlapTokens = 10 };

    private readonly Chunker _chunker = new(new WordTokenCounter());

    // 100 sentences of exactly 7 words each.
    private static ExtractedPage LongPage() =>
        new(1, string.Join(" ", Enumerable.Range(1, 100)
            .Select(i => $"Sentence number {i} has exactly seven words.")));

    [Fact]
    public void Split_LongText_ChunksStayWithinLimitAndAreNumbered()
    {
        // Act
        var chunks = _chunker.Split([LongPage()], SmallChunks);

        // Assert
        Assert.True(chunks.Count > 1);
        Assert.All(chunks, chunk =>
            Assert.True(chunk.TokenCount <= 50, $"Chunk {chunk.Index} has {chunk.TokenCount} tokens"));
        Assert.Equal(Enumerable.Range(0, chunks.Count), chunks.Select(c => c.Index));
        Assert.All(chunks, chunk => Assert.Equal(1, chunk.PageNumber));
    }

    [Fact]
    public void Split_ConsecutiveChunks_ShareOverlappingSentence()
    {
        // Act
        var chunks = _chunker.Split([LongPage()], SmallChunks);

        // Assert: each chunk starts with the previous chunk's last sentence.
        for (var i = 0; i < chunks.Count - 1; i++)
        {
            var lastSentence = LastSentence(chunks[i].Text);
            Assert.StartsWith(lastSentence, chunks[i + 1].Text);
        }
    }

    private static string LastSentence(string text) =>
        text[(text.LastIndexOf(". ", StringComparison.Ordinal) + 2)..];
}