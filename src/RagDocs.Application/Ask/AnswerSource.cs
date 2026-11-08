namespace RagDocs.Application.Ask;

// One source the answer may cite as [Number]. Sent to the client after the answer.
public sealed record AnswerSource(
    int Number,
    Guid DocumentId,
    string FileName,
    int? PageNumber,
    int ChunkIndex,
    double Score,
    string Excerpt);