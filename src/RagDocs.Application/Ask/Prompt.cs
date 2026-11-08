namespace RagDocs.Application.Ask;

// What we send to the chat model: fixed rules + this question's sources and question.
public sealed record Prompt(string System, string User);