using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using RagDocs.Application.Ingestion;

namespace RagDocs.Infrastructure.Extraction;

internal static class DocxTextExtractor
{
    public static IReadOnlyList<ExtractedPage> Extract(Stream content)
    {
        using var document = WordprocessingDocument.Open(content, isEditable: false);

        var body = document.MainDocumentPart?.Document?.Body;

        if (body is null)
        {
            return [];
        }

        var paragraphs = body.Descendants<Paragraph>()
            .Select(ToText)
            .Where(text => !string.IsNullOrWhiteSpace(text));

        // Word files have no fixed pages (pagination depends on the printer), so PageNumber is null.
        return [new ExtractedPage(null, string.Join("\n\n", paragraphs))];
    }

    private static string ToText(Paragraph paragraph)
    {
        var text = paragraph.InnerText;
        var style = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

        var isHeading = style is not null
            && (style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase)
                || style.Equals("Title", StringComparison.OrdinalIgnoreCase));

        // Mark headings Markdown-style so the chunker starts a new chunk there.
        return isHeading ? $"# {text}" : text;
    }
}