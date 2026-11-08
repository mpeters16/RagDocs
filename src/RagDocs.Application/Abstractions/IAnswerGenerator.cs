using RagDocs.Application.Ask;

namespace RagDocs.Application.Abstractions;

public interface IAnswerGenerator
{
    // Streams the answer piece by piece, as the model writes it.
    IAsyncEnumerable<string> StreamAnswerAsync(Prompt prompt, CancellationToken cancellationToken);
}