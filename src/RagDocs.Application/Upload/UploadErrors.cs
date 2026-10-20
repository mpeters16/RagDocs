using RagDocs.Domain.Common;

namespace RagDocs.Application.Documents.Upload;

public static class UploadErrors
{
    public static readonly Error UnsupportedFileType = Error.Validation(
        "Upload.UnsupportedFileType",
        "Only PDF, DOCX, TXT and Markdown files are supported.");

    public static readonly Error FileTooLarge = Error.Validation(
        "Upload.FileTooLarge",
        $"The file cannot be larger than {UploadLimits.MaxFileSizeBytes / (1024 * 1024)} MB.");
}