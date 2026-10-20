using RagDocs.Application.Documents.Upload;

namespace RagDocs.UnitTests.Application;

internal static class TestFiles
{
    public static byte[] Pdf => "%PDF-1.7\nfake pdf body"u8.ToArray();

    public static byte[] Docx => [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00];

    public static byte[] Text => "Hello, RagDocs!"u8.ToArray();

    // "MZ" header of a Windows program, followed by zero bytes.
    public static byte[] Exe => [0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00];

    // A "PDF" one byte over the limit (20 MB is fine for a test).
    public static byte[] TooLargePdf()
    {
        var bytes = new byte[UploadLimits.MaxFileSizeBytes + 1];
        "%PDF-"u8.CopyTo(bytes);
        return bytes;
    }
}