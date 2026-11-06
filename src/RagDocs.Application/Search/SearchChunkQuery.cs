namespace RagDocs.Application.Search;

public sealed record SearchChunksQuery(string Question, int TopK = SearchLimits.DefaultTopK);