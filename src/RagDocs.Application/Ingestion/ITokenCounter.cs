namespace RagDocs.Application.Abstractions;

public interface ITokenCounter
{
    int CountTokens(string text);
}