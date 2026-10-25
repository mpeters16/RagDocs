using System.Text;
using RagDocs.Application.Ingestion;

namespace RagDocs.Infrastructure.Extraction;

internal static class PlainTextExtractor
{
    public static async Task<IReadOnlyList<ExtractedPage>> ExtractAsync(
        Stream content,
        CancellationToken cancellationToken)
    {

        //if the file begins with a BOM (a few bytes some editors add to mark UTF-8 or UTF-16), it’s detected, otherwise UTF-8 is assumed
        using var reader = new StreamReader(
            content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

        var text = await reader.ReadToEndAsync(cancellationToken);

        return [new ExtractedPage(null, text)];
    }
}