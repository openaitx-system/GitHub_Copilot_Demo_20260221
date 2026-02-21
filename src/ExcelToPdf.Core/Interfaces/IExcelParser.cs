#nullable enable

namespace ExcelToPdf.Core.Interfaces;

using ExcelToPdf.Core.Models;

/// <summary>
/// Interface for parsing Excel files into worksheet data.
/// </summary>
public interface IExcelParser
{
    /// <summary>
    /// Parses an Excel file from a stream and returns worksheet data.
    /// </summary>
    /// <param name="excelStream">The stream containing the Excel file.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of parsed worksheet data.</returns>
    /// <exception cref="Exceptions.InvalidFileFormatException">
    /// Thrown when the stream does not contain a valid Excel file.
    /// </exception>
    Task<IReadOnlyList<WorksheetData>> ParseAsync(
        Stream excelStream,
        CancellationToken cancellationToken = default);
}
