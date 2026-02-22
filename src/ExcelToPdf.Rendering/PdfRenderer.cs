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
    /// Page dimension constants in points (portrait width, portrait height).
    /// Values match the standard ISO and ANSI paper sizes.
    /// Ref: Wiki/PDF-Specification#Page-Sizes
    /// </summary>
    private static readonly Dictionary<Core.Models.PageSize, (float Width, float Height)> PageDimensions = new()
    {
        // A4  = 210 mm × 297 mm  → 595.28 pt × 841.89 pt
        { Core.Models.PageSize.A4,     (595.28f, 841.89f) },
        // Letter = 8.5 in × 11 in → 612 pt × 792 pt
        { Core.Models.PageSize.Letter, (612f,    792f)    },
        // A3  = 297 mm × 420 mm  → 841.89 pt × 1190.55 pt
        { Core.Models.PageSize.A3,     (841.89f, 1190.55f) },
        // Legal = 8.5 in × 14 in → 612 pt × 1008 pt
        { Core.Models.PageSize.Legal,  (612f,    1008f)   },
    };

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
    /// Applies wide-table overflow protection and custom row heights.
    /// Ref: Wiki/PDF-Specification#Cell-Rendering
    /// </summary>
    private void RenderTable(
        IContainer container,
        WorksheetData worksheet,
        ConversionOptions options)
    {
        // Calculate total column width in points
        // Ref: Wiki/Excel-Specification#Row-and-Column-Sizing
        var totalWidthInPoints = 0f;
        for (int col = 0; col < worksheet.ColumnCount; col++)
        {
            totalWidthInPoints += (float)(worksheet.GetColumnWidth(col) * CharacterWidthInPoints);
        }

        // Wide-table overflow protection: scale the container to fit the available area
        var availableWidth = GetAvailableContentWidth(options);
        IContainer tableContainer = totalWidthInPoints > availableWidth
            ? container.ScaleToFit()
            : container;

        tableContainer.Table(table =>
        {
            // Define columns (original widths; ScaleToFit handles the resize when needed)
            table.ColumnsDefinition(columns =>
            {
                for (int col = 0; col < worksheet.ColumnCount; col++)
                {
                    var widthInPoints = (float)(worksheet.GetColumnWidth(col) * CharacterWidthInPoints);
                    columns.ConstantColumn(widthInPoints);
                }
            });

            // Render cells
            for (int row = 0; row < worksheet.RowCount; row++)
            {
                var hasCustomRowHeight = worksheet.RowHeights.TryGetValue(row, out var rowHeight);

                for (int col = 0; col < worksheet.ColumnCount; col++)
                {
                    var hasCell = worksheet.Cells.TryGetValue((row, col), out var cellValue);

                    var cell = table.Cell()
                        .Row((uint)(row + 1))
                        .Column((uint)(col + 1))
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(CellPadding);

                    // Apply custom row height via the first cell of each row.
                    // QuestPDF propagates MinHeight to all cells in the same row automatically,
                    // so setting it once on col 0 is sufficient and avoids redundant constraints.
                    // Ref: Wiki/Excel-Specification#Row-and-Column-Sizing
                    IContainer cellElement = col == 0 && hasCustomRowHeight
                        ? cell.MinHeight((float)rowHeight)
                        : cell;

                    cellElement.Element(cellContainer =>
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
    /// Returns the available content width in points for the given page size, orientation, and margins.
    /// Ref: Wiki/PDF-Specification#Page-Layout
    /// </summary>
    private static float GetAvailableContentWidth(ConversionOptions options)
    {
        if (!PageDimensions.TryGetValue(options.PageSize, out var dims))
        {
            dims = PageDimensions[Core.Models.PageSize.A4];
        }

        var pageWidth = options.Orientation == PageOrientation.Landscape ? dims.Height : dims.Width;
        return pageWidth - options.MarginLeft - options.MarginRight;
    }

    /// <summary>
    /// Renders the content of a single cell.
    /// Handles text formatting, alignment, and line breaks.
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
            // Apply text alignment within the block
            // Ref: Wiki/Excel-Specification#Cell-Alignment
            switch (cellValue.HorizontalAlignment)
            {
                case Core.Models.HorizontalAlignment.Center:
                    text.AlignCenter();
                    break;
                case Core.Models.HorizontalAlignment.Right:
                    text.AlignRight();
                    break;
                case Core.Models.HorizontalAlignment.Justify:
                    text.Justify();
                    break;
                default:
                    text.AlignLeft();
                    break;
            }

            var displayValue = cellValue.DisplayValue;

            // Handle line breaks
            // Ref: Wiki/Excel-Specification#Line-Break-Handling
            var lines = displayValue.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                // Use Line() for all but the last segment to emit a newline without an empty paragraph
                if (i < lines.Length - 1)
                {
                    var span = text.Line(lines[i]);
                    ApplyFontStyle(span, cellValue, options);
                }
                else
                {
                    var span = text.Span(lines[i]);
                    ApplyFontStyle(span, cellValue, options);
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
        // Vertical alignment
        container = cellValue.VerticalAlignment switch
        {
            Core.Models.VerticalAlignment.Top => container.AlignTop(),
            Core.Models.VerticalAlignment.Middle => container.AlignMiddle(),
            Core.Models.VerticalAlignment.Bottom => container.AlignBottom(),
            _ => container.AlignBottom()
        };

        // Horizontal alignment
        // Ref: Wiki/PDF-Specification#Cell-Rendering
        container = cellValue.HorizontalAlignment switch
        {
            Core.Models.HorizontalAlignment.Left => container.AlignLeft(),
            Core.Models.HorizontalAlignment.Center => container.AlignCenter(),
            Core.Models.HorizontalAlignment.Right => container.AlignRight(),
            Core.Models.HorizontalAlignment.Justify => container.AlignLeft(),
            _ => container.AlignLeft()
        };

        return container;
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
