#nullable enable

using ExcelToPdf.Core.Exceptions;
using ExcelToPdf.Core.Interfaces;
using ExcelToPdf.Core.Models;
using Microsoft.Extensions.Logging;

namespace ExcelToPdf.Rendering;

/// <summary>
/// High-level converter that orchestrates Excel parsing and PDF rendering.
/// </summary>
public class ExcelToPdfConverter : IExcelToPdfConverter
{
    private readonly IExcelParser _excelParser;
    private readonly IPdfRenderer _pdfRenderer;
    private readonly ILogger<ExcelToPdfConverter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ExcelToPdfConverter"/>.
    /// </summary>
    /// <param name="excelParser">The Excel parser.</param>
    /// <param name="pdfRenderer">The PDF renderer.</param>
    /// <param name="logger">The logger instance.</param>
    public ExcelToPdfConverter(
        IExcelParser excelParser,
        IPdfRenderer pdfRenderer,
        ILogger<ExcelToPdfConverter> logger)
    {
        ArgumentNullException.ThrowIfNull(excelParser);
        ArgumentNullException.ThrowIfNull(pdfRenderer);
        ArgumentNullException.ThrowIfNull(logger);

        _excelParser = excelParser;
        _pdfRenderer = pdfRenderer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ConvertAsync(
        Stream excelStream,
        Stream pdfStream,
        ConversionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelStream);
        ArgumentNullException.ThrowIfNull(pdfStream);

        options ??= new ConversionOptions();

        _logger.LogInformation("Starting Excel-to-PDF conversion");

        try
        {
            // Step 1: Parse Excel
            var worksheets = await _excelParser.ParseAsync(excelStream, cancellationToken);
            _logger.LogInformation("Parsed {WorksheetCount} worksheets", worksheets.Count);

            // Step 2: Render PDF
            await _pdfRenderer.RenderAsync(worksheets, pdfStream, options, cancellationToken);
            _logger.LogInformation("PDF conversion completed successfully");
        }
        catch (Exception ex) when (ex is not OperationCanceledException
                                    and not InvalidFileFormatException
                                    and not RenderingException)
        {
            _logger.LogError(ex, "Unexpected error during conversion");
            throw new ConversionException("An unexpected error occurred during Excel-to-PDF conversion.", ex);
        }
    }
}
