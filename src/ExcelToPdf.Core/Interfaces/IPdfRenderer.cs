#nullable enable

namespace ExcelToPdf.Core.Interfaces;

using ExcelToPdf.Core.Models;

/// <summary>
/// Interface for rendering worksheet data to PDF.
/// </summary>
public interface IPdfRenderer
{
    /// <summary>
    /// Renders worksheet data to a PDF and writes it to the output stream.
    /// </summary>
    /// <param name="worksheets">The worksheet data to render.</param>
    /// <param name="outputStream">The stream to write the PDF to.</param>
    /// <param name="options">Conversion options for page layout and formatting.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task RenderAsync(
        IReadOnlyList<WorksheetData> worksheets,
        Stream outputStream,
        ConversionOptions options,
        CancellationToken cancellationToken = default);
}
