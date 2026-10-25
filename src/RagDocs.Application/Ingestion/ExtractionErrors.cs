using RagDocs.Domain.Common;

namespace RagDocs.Application.Ingestion;

public static class ExtractionErrors
{
    public static readonly Error UnsupportedContentType = Error.Validation(
        "Extraction.UnsupportedContentType",
        "Text can't be extracted from this file type.");

    public static readonly Error Unreadable = Error.Validation(
        "Extraction.Unreadable",
        "The file could not be read. It may be corrupt or password-protected.");

    public static readonly Error NoText = Error.Validation(
        "Extraction.NoText",
        "No text was found. Scanned documents (images of text) aren't supported yet.");
}