namespace RagDocs.Application.Ingestion;

// One page of extracted text. PageNumber is null for formats without pages (DOCX, TXT, MD).
public sealed record ExtractedPage(int? PageNumber, string Text);