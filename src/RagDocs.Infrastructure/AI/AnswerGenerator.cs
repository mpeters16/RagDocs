using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;
using RagDocs.Application.Abstractions;
using RagDocs.Application.Ask;

namespace RagDocs.Infrastructure.AI;

internal sealed class AnswerGenerator(IChatClient chatClient) : IAnswerGenerator
{
    public async IAsyncEnumerable<string> StreamAnswerAsync(
        Prompt prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        List<ChatMessage> messages =
        [
            new(ChatRole.System, prompt.System),
            new(ChatRole.User, prompt.User),
        ];

        // Low temperature: stick to the sources, little creativity.
        var options = new ChatOptions { Temperature = 0.1f };

        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                yield return update.Text;
            }
        }
    }
}