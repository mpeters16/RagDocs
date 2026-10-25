using System.Data.Common;

namespace RagDocs.Infrastructure.AI;

// Aspire passes "Endpoint=http://localhost:1234;Model=nomic-embed-text".
internal sealed record OllamaConnection(Uri Endpoint, string Model)
{
    public static OllamaConnection Parse(string? connectionString, string name)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' is missing. Start the app through the AppHost.");
        }

        var parts = new DbConnectionStringBuilder { ConnectionString = connectionString };

        if (!parts.TryGetValue("Endpoint", out var endpoint) || !parts.TryGetValue("Model", out var model))
        {
            throw new InvalidOperationException(
                $"Connection string '{name}' must contain Endpoint and Model.");
        }

        return new OllamaConnection(new Uri(endpoint.ToString()!), model.ToString()!);
    }
}