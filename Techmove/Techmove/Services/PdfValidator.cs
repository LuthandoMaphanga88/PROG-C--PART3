namespace Techmove.Services;

/// <summary>
/// Validates PDF files based on content and metadata.
/// </summary>
public class PdfValidator
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private const string PdfMimeType = "application/pdf";
    private static readonly byte[] PdfHeader = { 0x25, 0x50, 0x44, 0x46 }; // %PDF

    /// <summary>
    /// Validates if a file is a valid PDF.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <returns>A tuple indicating if the file is valid and an error message if invalid.</returns>
    public (bool IsValid, string? ErrorMessage) ValidatePdfFile(IFormFile? file)
    {
        if (file is null)
        {
            return (false, "File is required.");
        }

        if (file.Length == 0)
        {
            return (false, "File cannot be empty.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, $"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "File must have a .pdf extension.");
        }

        if (!file.ContentType?.Equals(PdfMimeType, StringComparison.OrdinalIgnoreCase) ?? true)
        {
            return (false, $"File must have the correct MIME type ({PdfMimeType}).");
        }

        if (!ValidatePdfSignature(file))
        {
            return (false, "File does not have a valid PDF signature.");
        }

        return (true, null);
    }

    /// <summary>
    /// Validates if a file stream has a valid PDF signature.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <returns>True if the file has a valid PDF signature; otherwise, false.</returns>
    private static bool ValidatePdfSignature(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[PdfHeader.Length];
            var bytesRead = stream.Read(buffer, 0, PdfHeader.Length);

            if (bytesRead < PdfHeader.Length)
            {
                return false;
            }

            return buffer.SequenceEqual(PdfHeader);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates a file name to ensure it's safe and contains a .pdf extension.
    /// </summary>
    /// <param name="fileName">The file name to validate.</param>
    /// <returns>A tuple indicating if the file name is valid and an error message if invalid.</returns>
    public (bool IsValid, string? ErrorMessage) ValidatePdfFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return (false, "File name is required.");
        }

        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "File name must have a .pdf extension.");
        }

        if (fileName.Length > 255)
        {
            return (false, "File name must not exceed 255 characters.");
        }

        // Check for path traversal attempts
        if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
        {
            return (false, "File name contains invalid characters.");
        }

        return (true, null);
    }
}
