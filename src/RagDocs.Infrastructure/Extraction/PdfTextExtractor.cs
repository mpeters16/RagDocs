using RagDocs.Application.Ingestion;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace RagDocs.Infrastructure.Extraction;

internal static class PdfTextExtractor
{
    public static IReadOnlyList<ExtractedPage> Extract(Stream content)
    {
        using var document = PdfDocument.Open(content);

        var pages = new List<ExtractedPage>(document.NumberOfPages);

        foreach (var page in document.GetPages())
        {
            // Reads text in visual order (better than page.Text for multi-column layouts).
            pages.Add(new ExtractedPage(page.Number, ContentOrderTextExtractor.GetText(page)));
        }

        return pages;
    }
}