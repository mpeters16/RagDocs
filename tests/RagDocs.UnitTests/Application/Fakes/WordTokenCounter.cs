using RagDocs.Application.Abstractions;

namespace RagDocs.UnitTests.Application.Fakes;

// 1 word = 1 token: easy to reason about in tests.
internal sealed class WordTokenCounter : ITokenCounter
{
    public int CountTokens(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
}