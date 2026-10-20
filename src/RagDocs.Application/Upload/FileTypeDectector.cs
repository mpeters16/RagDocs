namespace RagDocs.Application.Documents.Upload;

internal static class FileTypeDetector
{
    // How many bytes from the start of the file the caller should read and pass in.
    public const int HeaderSize = 8 * 1024;

    private static ReadOnlySpan<byte> PdfSignature => "%PDF-"u8;

    private static ReadOnlySpan<byte> ZipSignature => [0x50, 0x4B, 0x03, 0x04];

    // Returns the content type to store, or null if the file type isn't allowed.
    public static string? Detect(ReadOnlySpan<byte> header, string fileName)
    {
        // 1. PDF: trust the bytes, whatever the file is called.
        if (header.StartsWith(PdfSignature))
        {
            return AllowedContentTypes.Pdf;
        }

        var extension = Path.GetExtension(fileName);

        // 2. ZIP: only accepted if it claims to be a Word document.
        if (header.StartsWith(ZipSignature))
        {
            return HasExtension(extension, ".docx")
                ? AllowedContentTypes.Docx
                : null;
        }

        // 3. Text formats have no magic bytes, so the bytes must look like text
        //    AND the extension must say which kind.
        if (!LooksLikeText(header))
        {
            return null;
        }

        if (HasExtension(extension, ".md"))
        {
            return AllowedContentTypes.Markdown;
        }

        if (HasExtension(extension, ".txt"))
        {
            return AllowedContentTypes.PlainText;
        }

        // 4. Anything else is rejected.
        return null;
    }

    private static bool HasExtension(string extension, string expected) =>
        extension.Equals(expected, StringComparison.OrdinalIgnoreCase);

    // Binary files almost always contain zero bytes; plain text never does.
    private static bool LooksLikeText(ReadOnlySpan<byte> header) =>
        !header.IsEmpty && !header.Contains((byte)0);
}