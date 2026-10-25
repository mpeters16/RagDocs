using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RagDocs.Application.Abstractions;
using RagDocs.Application.Documents;
using RagDocs.Application.Ingestion;
using RagDocs.Domain.Common;

namespace RagDocs.Infrastructure.Extraction;

internal sealed partial class TextExtractor(ILogger<TextExtractor> logger) : ITextExtractor
{
    public async Task<Result<IReadOnlyList<ExtractedPage>>> ExtractAsync(
        Stream content,
        string contentType,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ExtractedPage> pages;

        try
        {
            switch (contentType)
            {
                case AllowedContentTypes.Pdf:
                    pages = PdfTextExtractor.Extract(content);
                    break;

                case AllowedContentTypes.Docx:
                    pages = DocxTextExtractor.Extract(content);
                    break;

                case AllowedContentTypes.PlainText:
                case AllowedContentTypes.Markdown:
                    pages = await PlainTextExtractor.ExtractAsync(content, cancellationToken);
                    break;

                default:
                    return ExtractionErrors.UnsupportedContentType;
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Corrupt, encrypted or fake files: expected, so a Result error, not a crash.
            logger.LogWarning(exception, "Could not extract text from a {ContentType} file", contentType);
            return ExtractionErrors.Unreadable;
        }

        var cleaned = pages
            .Select(page => page with { Text = Normalize(page.Text) })
            .Where(page => page.Text.Length > 0)
            .ToList();

        if (cleaned.Count == 0)
        {
            return ExtractionErrors.NoText;
        }

        return Result<IReadOnlyList<ExtractedPage>>.Success(cleaned);
    }

    private static string Normalize(string text)
    {
        var normalized = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Replace("\0", string.Empty, StringComparison.Ordinal);   // PostgreSQL text can't store \0

        return ExtraBlankLines().Replace(normalized, "\n\n").Trim();
    }

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExtraBlankLines();
}