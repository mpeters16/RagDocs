namespace RagDocs.Application.Ask;

// Sources are known straight away; the answer arrives as a stream.
public sealed record AskQuestionResult(
    IReadOnlyList<AnswerSource> Sources,
    IAsyncEnumerable<string> Answer);