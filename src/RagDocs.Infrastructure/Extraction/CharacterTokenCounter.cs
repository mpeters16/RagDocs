using RagDocs.Application.Abstractions;

namespace RagDocs.Infrastructure.Extraction;

// Rough estimate: ~4 characters per token for English. Good enough for chunk sizing.
internal sealed class CharacterTokenCounter : ITokenCounter
{
    public int CountTokens(string text) =>
        string.IsNullOrEmpty(text) ? 0 : (text.Length + 3) / 4;
}