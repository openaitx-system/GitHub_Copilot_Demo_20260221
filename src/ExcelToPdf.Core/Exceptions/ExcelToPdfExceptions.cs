#nullable enable

// Ref: Wiki/PDF-Specification#Error-Handling

namespace ExcelToPdf.Core.Exceptions;

/// <summary>
/// Thrown when the input file is not a valid Excel format.
/// Ref: Wiki/PDF-Specification#Error-Handling
/// </summary>
public class InvalidFileFormatException : Exception
{
    /// <summary>
    /// Gets the file path that caused the error, if available.
    /// </summary>
    public string? FilePath { get; }

    /// <summary>
    /// Gets the expected file format.
    /// </summary>
    public string? ExpectedFormat { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="InvalidFileFormatException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="filePath">The file path that caused the error.</param>
    /// <param name="expectedFormat">The expected file format.</param>
    public InvalidFileFormatException(string message, string? filePath = null, string? expectedFormat = null)
        : base(message)
    {
        FilePath = filePath;
        ExpectedFormat = expectedFormat;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InvalidFileFormatException"/> with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidFileFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when a rendering error occurs during PDF generation.
/// </summary>
public class RenderingException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="RenderingException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public RenderingException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Thrown when a general conversion error occurs.
/// </summary>
public class ConversionException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="ConversionException"/>.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConversionException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
