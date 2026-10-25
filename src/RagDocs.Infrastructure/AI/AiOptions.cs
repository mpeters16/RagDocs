namespace RagDocs.Infrastructure.AI;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public string DocumentPrefix { get; set; } = string.Empty;

    public string QueryPrefix { get; set; } = string.Empty;
}