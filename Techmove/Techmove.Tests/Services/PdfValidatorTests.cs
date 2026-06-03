using Microsoft.AspNetCore.Http;
using Moq;
using Techmove.Services;
using Xunit;

namespace Techmove.Tests.Services;

public class PdfValidatorTests
{
    private readonly PdfValidator _pdfValidator = new();

    #region ValidatePdfFile Tests

    [Fact]
    public void ValidatePdfFile_WithNullFile_ReturnsFalseAndErrorMessage()
    {
        // Arrange
        IFormFile? file = null;

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFile(file);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File is required.", errorMessage);
    }

    [Fact]
    public void ValidatePdfFile_WithEmptyFile_ReturnsFalseAndErrorMessage()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFile(mockFile.Object);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File cannot be empty.", errorMessage);
    }

    [Fact]
    public void ValidatePdfFile_WithFileTooLarge_ReturnsFalseAndErrorMessage()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6 MB

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFile(mockFile.Object);

        // Assert
        Assert.False(isValid);
        Assert.Contains("must not exceed 5 MB", errorMessage);
    }

    [Fact]
    public void ValidatePdfFile_WithInvalidExtension_ReturnsFalseAndErrorMessage()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns("document.txt");

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFile(mockFile.Object);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File must have a .pdf extension.", errorMessage);
    }

    [Fact]
    public void ValidatePdfFile_WithInvalidMimeType_ReturnsFalseAndErrorMessage()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns("document.pdf");
        mockFile.Setup(f => f.ContentType).Returns("text/plain");

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFile(mockFile.Object);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File must have the correct MIME type (application/pdf).", errorMessage);
    }

    [Fact]
    public void ValidatePdfFile_WithInvalidPdfSignature_ReturnsFalseAndErrorMessage()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns("document.pdf");
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        var memoryStream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 }); // Invalid PDF header
        mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFile(mockFile.Object);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File does not have a valid PDF signature.", errorMessage);
    }

    [Fact]
    public void ValidatePdfFile_WithValidPdf_ReturnsTrue()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(2048);
        mockFile.Setup(f => f.FileName).Returns("document.pdf");
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        // PDF signature: %PDF
        var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x0D, 0x0A };
        var memoryStream = new MemoryStream(pdfContent);
        mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFile(mockFile.Object);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Theory]
    [InlineData("Document.PDF")]
    [InlineData("test-file.pdf")]
    [InlineData("2024-01-contract.PDF")]
    public void ValidatePdfFile_WithValidPdfVariations_ReturnsTrue(string fileName)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");

        var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var memoryStream = new MemoryStream(pdfContent);
        mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFile(mockFile.Object);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    #endregion

    #region ValidatePdfFileName Tests

    [Fact]
    public void ValidatePdfFileName_WithNullFileName_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFileName(null);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File name is required.", errorMessage);
    }

    [Fact]
    public void ValidatePdfFileName_WithEmptyFileName_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFileName(string.Empty);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File name is required.", errorMessage);
    }

    [Fact]
    public void ValidatePdfFileName_WithoutPdfExtension_ReturnsFalseAndErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFileName("document.txt");

        // Assert
        Assert.False(isValid);
        Assert.Equal("File name must have a .pdf extension.", errorMessage);
    }

    [Fact]
    public void ValidatePdfFileName_WithTooLongFileName_ReturnsFalseAndErrorMessage()
    {
        // Arrange
        var longFileName = new string('a', 252) + ".pdf"; // 256 characters

        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFileName(longFileName);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File name must not exceed 255 characters.", errorMessage);
    }

    [Theory]
    [InlineData("../../../etc/passwd.pdf")]
    [InlineData("..\\..\\..\\windows\\system32.pdf")]
    [InlineData("folder/document.pdf")]
    [InlineData("folder\\document.pdf")]
    public void ValidatePdfFileName_WithPathTraversalAttempt_ReturnsFalseAndErrorMessage(string fileName)
    {
        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFileName(fileName);

        // Assert
        Assert.False(isValid);
        Assert.Equal("File name contains invalid characters.", errorMessage);
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("CONTRACT-2024.PDF")]
    [InlineData("123-agreement.pdf")]
    [InlineData("client_agreement_final.pdf")]
    public void ValidatePdfFileName_WithValidFileName_ReturnsTrue(string fileName)
    {
        // Act
        var (isValid, errorMessage) = _pdfValidator.ValidatePdfFileName(fileName);

        // Assert
        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    #endregion
}
