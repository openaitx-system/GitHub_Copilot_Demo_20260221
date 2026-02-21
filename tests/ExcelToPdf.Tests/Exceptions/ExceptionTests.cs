#nullable enable

using ExcelToPdf.Core.Exceptions;
using FluentAssertions;

namespace ExcelToPdf.Tests.Exceptions;

/// <summary>
/// Unit tests for custom exception types.
/// </summary>
public class ExceptionTests
{
    [Fact]
    public void InvalidFileFormatException_WithMessage_SetsMessage()
    {
        // Arrange & Act
        var ex = new InvalidFileFormatException("Invalid file");

        // Assert
        ex.Message.Should().Be("Invalid file");
        ex.FilePath.Should().BeNull();
        ex.ExpectedFormat.Should().BeNull();
    }

    [Fact]
    public void InvalidFileFormatException_WithAllProperties_SetsCorrectly()
    {
        // Arrange & Act
        var ex = new InvalidFileFormatException(
            "Invalid file", filePath: "/path/to/file.xlsx", expectedFormat: ".xlsx");

        // Assert
        ex.Message.Should().Be("Invalid file");
        ex.FilePath.Should().Be("/path/to/file.xlsx");
        ex.ExpectedFormat.Should().Be(".xlsx");
    }

    [Fact]
    public void InvalidFileFormatException_WithInnerException_SetsInnerException()
    {
        // Arrange
        var inner = new IOException("File not found");

        // Act
        var ex = new InvalidFileFormatException("Invalid file", inner);

        // Assert
        ex.Message.Should().Be("Invalid file");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void RenderingException_WithMessage_SetsMessage()
    {
        // Arrange & Act
        var ex = new RenderingException("Render failed");

        // Assert
        ex.Message.Should().Be("Render failed");
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void RenderingException_WithInnerException_SetsInnerException()
    {
        // Arrange
        var inner = new OutOfMemoryException();

        // Act
        var ex = new RenderingException("Render failed", inner);

        // Assert
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void ConversionException_WithMessage_SetsMessage()
    {
        // Arrange & Act
        var ex = new ConversionException("Conversion failed");

        // Assert
        ex.Message.Should().Be("Conversion failed");
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void ConversionException_WithInnerException_SetsInnerException()
    {
        // Arrange
        var inner = new InvalidOperationException("Bad state");

        // Act
        var ex = new ConversionException("Conversion failed", inner);

        // Assert
        ex.InnerException.Should().BeSameAs(inner);
    }
}
