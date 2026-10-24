namespace RagDocs.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    // Relative paths are resolved against the Api's content root.
    public string RootPath { get; set; } = "data/uploads";
}