#nullable enable

// Ref: Wiki/PDF-Specification (all sections)

using ExcelToPdf.Core.Exceptions;
using ExcelToPdf.Core.Interfaces;
using ExcelToPdf.Core.Models;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ExcelToPdf.Rendering;

/// <summary>
/// Renders worksheet data to PDF using QuestPDF.
/// Ref: Wiki/PDF-Specification
/// </summary>
public class PdfRenderer : IPdfRenderer
{
    private readonly ILogger<PdfRenderer> _logger;

    /// <summary>
    /// Character-to-point conversion factor for column widths.
    /// Excel default: 1 character ≈ 7.5 points.
    /// </summary>
    private const float CharacterWidthInPoints = 7.5f;

    /// <summary>
    /// Cell padding in points.
    /// </summary>
    private const float CellPadding = 2f;

    /// <summary>
    /// Initializes a new instance of <see cref="PdfRenderer"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public PdfRenderer(ILogger<PdfRenderer> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task RenderAsync(
        IReadOnlyList<WorksheetData> worksheets,
        Stream outputStream,
        ConversionOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(worksheets);
        ArgumentNullException.ThrowIfNull(outputStream);
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            // Set QuestPDF license type (Community for open source)
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                foreach (var worksheet in worksheets)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    RenderWorksheet(container, worksheet, options);
                }

                // Handle empty workbook
                if (worksheets.Count == 0)
                {
                    container.Page(page =>
                    {
                        ConfigurePage(page, options);
                        page.Content().Text("Empty workbook");
                    });
                }
            });

            document.GeneratePdf(outputStream);

            _logger.LogInformation(
                "PDF generated successfully with {SheetCount} worksheets",
                worksheets.Count);

            return Task.CompletedTask;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to render PDF");
            throw new RenderingException("Failed to generate PDF output.", ex);
        }
    }

    /// <summary>
    /// Renders a single worksheet as a PDF page.
    /// </summary>
    private void RenderWorksheet(
        IDocumentContainer container,
        WorksheetData worksheet,
        ConversionOptions options)
    {
        _logger.LogInformation(
            "Rendering worksheet {SheetName} ({RowCount}x{ColumnCount})",
            worksheet.Name, worksheet.RowCount, worksheet.ColumnCount);
        Console.WriteLine($"Render debug: {worksheet.Name}");

        container.Page(page =>
        {
            ConfigurePage(page, options);

            // Header with sheet name
            page.Header()
                .PaddingBottom(5)
                .Text(worksheet.Name)
                .FontSize(10)
                .FontColor(Colors.Grey.Darken1);

            // Content - render the table
            page.Content().Element(contentContainer =>
            {
                if (worksheet.Cells.Count == 0)
                {
                    contentContainer.Text("(empty sheet)").FontColor(Colors.Grey.Medium);
                    return;
                }

                RenderTable(contentContainer, worksheet, options);
            });

            // Footer with page number
            page.Footer()
                .AlignCenter()
                .Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
        });
    }

    /// <summary>
    /// Configures page size, orientation, and margins.
    /// Ref: Wiki/PDF-Specification#Page-Layout
    /// </summary>
    private static void ConfigurePage(PageDescriptor page, ConversionOptions options)
    {
        // Ref: Wiki/PDF-Specification#Page-Sizes
        var size = options.PageSize switch
        {
            Core.Models.PageSize.A4 => PageSizes.A4,
            Core.Models.PageSize.Letter => PageSizes.Letter,
            Core.Models.PageSize.A3 => PageSizes.A3,
            Core.Models.PageSize.Legal => PageSizes.Legal,
            _ => PageSizes.A4
        };

        if (options.Orientation == PageOrientation.Landscape)
        {
            size = new QuestPDF.Helpers.PageSize(size.Height, size.Width);
        }

        page.Size(size);
        page.MarginTop(options.MarginTop);
        page.MarginBottom(options.MarginBottom);
        page.MarginLeft(options.MarginLeft);
        page.MarginRight(options.MarginRight);
        page.DefaultTextStyle(x => x.FontSize(options.DefaultFontSize));
    }

    /// <summary>
    /// Renders the worksheet data as a table.
    /// Ref: Wiki/PDF-Specification#Cell-Rendering
    /// </summary>
    private void RenderTable(
        IContainer container,
        WorksheetData worksheet,
        ConversionOptions options)
    {
        var totalTableWidth = GetTotalTableWidth(worksheet);
        var availableWidth = GetAvailableContentWidth(options);
        var shouldFitToPage = totalTableWidth > availableWidth && totalTableWidth > 0;

        container.Table(table =>
        {
            // Define columns
            // Ref: Wiki/Excel-Specification#Row-and-Column-Sizing
            table.ColumnsDefinition(columns =>
            {
                for (int col = 0; col < worksheet.ColumnCount; col++)
                {
                    var widthInChars = worksheet.GetColumnWidth(col);
                    var widthInPoints = (float)(widthInChars * CharacterWidthInPoints);

                    if (shouldFitToPage)
                    {
                        columns.RelativeColumn(Math.Max(widthInPoints, 1f));
                    }
                    else
                    {
                        columns.ConstantColumn(widthInPoints);
                    }
                }
            });

            // Render cells
            for (int row = 0; row < worksheet.RowCount; row++)
            {
                for (int col = 0; col < worksheet.ColumnCount; col++)
                {
                    var hasCell = worksheet.Cells.TryGetValue((row, col), out var cellValue);

                    table.Cell()
                        .Row((uint)(row + 1))
                        .Column((uint)(col + 1))
                        .MinHeight((float)worksheet.GetRowHeight(row))
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(CellPadding)
                        .Element(cellContainer =>
                        {
                            if (hasCell && cellValue is not null)
                            {
                                RenderCellContent(cellContainer, cellValue, options);
                            }
                        });
                }
            }
        });
    }

    /// <summary>
    /// Renders the content of a single cell.
    /// Handles text formatting and line breaks.
    /// Ref: Wiki/PDF-Specification#Text-Rendering
    /// </summary>
    private void RenderCellContent(
        IContainer container,
        CellValue cellValue,
        ConversionOptions options)
    {
        // Apply background color
        // Ref: Wiki/PDF-Specification#Cell-Rendering — Render Order
        if (cellValue.BackgroundColor is not null)
        {
            container = container.Background(cellValue.BackgroundColor);
        }

        // Apply alignment
        container = ApplyAlignment(container, cellValue);

        // Render text with formatting
        // Ref: Wiki/PDF-Specification#Text-Rendering — Line Break Rendering
        container.Text(text =>
        {
            var displayValue = cellValue.DisplayValue;

            // Handle line breaks
            // Ref: Wiki/Excel-Specification#Line-Break-Handling
            var lines = displayValue.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var span = text.Span(lines[i]);

                // Apply font styling
                ApplyFontStyle(span, cellValue, options);

                // Add line break between lines (not after last)
                if (i < lines.Length - 1)
                {
                    text.EmptyLine();
                }
            }
        });
    }

    /// <summary>
    /// Applies horizontal and vertical alignment to a container.
    /// Ref: Wiki/Excel-Specification#Cell-Alignment
    /// </summary>
    private static IContainer ApplyAlignment(IContainer container, CellValue cellValue)
    {
        // Horizontal alignment
        container = cellValue.HorizontalAlignment switch
        {
            Core.Models.HorizontalAlignment.Left => container.AlignLeft(),
            Core.Models.HorizontalAlignment.Center => container.AlignCenter(),
            Core.Models.HorizontalAlignment.Right => container.AlignRight(),
            Core.Models.HorizontalAlignment.Justify => container.AlignLeft(),
            _ => container.AlignLeft()
        };

        // Vertical alignment
        container = cellValue.VerticalAlignment switch
        {
            Core.Models.VerticalAlignment.Top => container.AlignTop(),
            Core.Models.VerticalAlignment.Middle => container.AlignMiddle(),
            Core.Models.VerticalAlignment.Bottom => container.AlignBottom(),
            _ => container.AlignBottom()
        };

        return container;
    }

    /// <summary>
    /// Calculates total table width in points from worksheet column widths.
    /// </summary>
    private static float GetTotalTableWidth(WorksheetData worksheet)
    {
        float totalWidth = 0;
        for (int col = 0; col < worksheet.ColumnCount; col++)
        {
            var widthInChars = worksheet.GetColumnWidth(col);
            totalWidth += (float)(widthInChars * CharacterWidthInPoints);
        }

        return totalWidth;
    }

    /// <summary>
    /// Gets available content width in points based on page size and margins.
    /// </summary>
    private static float GetAvailableContentWidth(ConversionOptions options)
    {
        var (pageWidth, pageHeight) = options.PageSize switch
        {
            Core.Models.PageSize.A4 => (595.28f, 841.89f),
            Core.Models.PageSize.Letter => (612f, 792f),
            Core.Models.PageSize.A3 => (841.89f, 1190.55f),
            Core.Models.PageSize.Legal => (612f, 1008f),
            _ => (595.28f, 841.89f)
        };

        var effectivePageWidth = options.Orientation == PageOrientation.Landscape
            ? pageHeight
            : pageWidth;

        var contentWidth = effectivePageWidth - options.MarginLeft - options.MarginRight;
        return Math.Max(contentWidth, 50f);
    }

    /// <summary>
    /// Applies font styling to a text span.
    /// Ref: Wiki/PDF-Specification#Font-Standards
    /// </summary>
    private static void ApplyFontStyle(
        TextSpanDescriptor span,
        CellValue cellValue,
        ConversionOptions options)
    {
        // Font family with fallback
        // Ref: Wiki/PDF-Specification#Font-Embedding-Rules
        span.FontFamily(cellValue.FontFamily);
        span.FontSize((float)cellValue.FontSize);
        span.FontColor(cellValue.FontColor);

        if (cellValue.IsBold)
        {
            span.Bold();
        }

        if (cellValue.IsItalic)
        {
            span.Italic();
        }

        if (cellValue.IsUnderline)
        {
            span.Underline();
        }

        if (cellValue.IsStrikethrough)
        {
            span.Strikethrough();
        }

        // Error cells in red
        // Ref: Wiki/Excel-Specification#Cell-Data-Types — Error type
        if (cellValue.DataType == CellDataType.Error)
        {
            span.FontColor(Colors.Red.Medium);
        }
    }
}
