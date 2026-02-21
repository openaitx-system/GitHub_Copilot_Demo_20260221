#nullable enable

namespace ExcelToPdf.Core.Interfaces;

using ExcelToPdf.Core.Models;

/// <summary>
/// High-level interface for converting Excel files to PDF.
/// </summary>
public interface IExcelToPdfConverter
{
    /// <summary>
    /// Converts an Excel file to PDF.
    /// </summary>
    /// <param name="excelStream">The input stream containing the Excel file.</param>
    /// <param name="pdfStream">The output stream to write the PDF to.</param>
    /// <param name="options">Optional conversion options. Uses defaults if null.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task ConvertAsync(
        Stream excelStream,
        Stream pdfStream,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default);
}
