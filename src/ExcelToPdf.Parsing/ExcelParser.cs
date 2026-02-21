#nullable enable

// Ref: Wiki/Excel-Specification (all sections)

using ClosedXML.Excel;
using ExcelToPdf.Core.Exceptions;
using ExcelToPdf.Core.Models;
using ExcelToPdf.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExcelToPdf.Parsing;

/// <summary>
/// Parses Excel (.xlsx) files using ClosedXML and converts them to worksheet data models.
/// Handles all 7 cell data types per Excel Specification Section 2.
/// </summary>
public class ExcelParser : IExcelParser
{
    private readonly ILogger<ExcelParser> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ExcelParser"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public ExcelParser(ILogger<ExcelParser> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WorksheetData>> ParseAsync(
        Stream excelStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelStream);

        try
        {
            var workbook = new XLWorkbook(excelStream);
            var worksheets = new List<WorksheetData>();

            foreach (var sheet in workbook.Worksheets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogInformation(
                    "Parsing worksheet {SheetName} with {RowCount} rows and {ColumnCount} columns",
                    sheet.Name,
                    sheet.LastRowUsed()?.RowNumber() ?? 0,
                    sheet.LastColumnUsed()?.ColumnNumber() ?? 0);

                var worksheetData = ParseWorksheet(sheet);
                worksheets.Add(worksheetData);
            }

            return Task.FromResult<IReadOnlyList<WorksheetData>>(worksheets);
        }
        catch (Exception ex) when (ex is not OperationCanceledException
                                    and not InvalidFileFormatException)
        {
            _logger.LogError(ex, "Failed to parse Excel file");
            throw new InvalidFileFormatException(
                "The stream does not contain a valid Excel file.",
                ex);
        }
    }

    /// <summary>
    /// Parses a single worksheet into a <see cref="WorksheetData"/> model.
    /// </summary>
    /// <param name="worksheet">The ClosedXML worksheet to parse.</param>
    /// <returns>Parsed worksheet data.</returns>
    private WorksheetData ParseWorksheet(IXLWorksheet worksheet)
    {
        var data = new WorksheetData
        {
            Name = worksheet.Name
        };

        var lastRow = worksheet.LastRowUsed();
        var lastColumn = worksheet.LastColumnUsed();

        if (lastRow is null || lastColumn is null)
        {
            _logger.LogWarning("Worksheet {SheetName} is empty", worksheet.Name);
            return data;
        }

        data.RowCount = lastRow.RowNumber();
        data.ColumnCount = lastColumn.ColumnNumber();

        // Parse row heights
        for (int row = 1; row <= data.RowCount; row++)
        {
            var xlRow = worksheet.Row(row);
            if (xlRow.Height != worksheet.RowHeight)
            {
                data.RowHeights[row - 1] = xlRow.Height;
            }
        }

        // Parse column widths
        for (int col = 1; col <= data.ColumnCount; col++)
        {
            var xlCol = worksheet.Column(col);
            if (xlCol.Width != worksheet.ColumnWidth)
            {
                data.ColumnWidths[col - 1] = xlCol.Width;
            }
        }

        // Parse cells
        // Ref: Wiki/Excel-Specification#Cell-Data-Types
        for (int row = 1; row <= data.RowCount; row++)
        {
            for (int col = 1; col <= data.ColumnCount; col++)
            {
                var cell = worksheet.Cell(row, col);

                if (cell.IsEmpty())
                {
                    continue;
                }

                var cellValue = ParseCell(cell, row - 1, col - 1);
                data.Cells[(row - 1, col - 1)] = cellValue;
            }
        }

        _logger.LogInformation(
            "Parsed worksheet {SheetName}: {CellCount} cells, {RowCount}x{ColumnCount}",
            worksheet.Name, data.Cells.Count, data.RowCount, data.ColumnCount);

        return data;
    }

    /// <summary>
    /// Parses a single cell into a <see cref="CellValue"/> model.
    /// Handles all 7 data types per Excel Specification Section 2.
    /// </summary>
    /// <param name="cell">The ClosedXML cell to parse.</param>
    /// <param name="row">The 0-based row index.</param>
    /// <param name="column">The 0-based column index.</param>
    /// <returns>Parsed cell value with formatting.</returns>
    private CellValue ParseCell(IXLCell cell, int row, int column)
    {
        var cellValue = new CellValue
        {
            Row = row,
            Column = column,
        };

        // Parse data type and value
        // Ref: Wiki/Excel-Specification#Cell-Data-Types
        ParseCellValue(cell, cellValue);

        // Parse formatting
        // Ref: Wiki/Excel-Specification#Cell-Formatting-Rules
        ParseCellFormatting(cell, cellValue);

        return cellValue;
    }

    /// <summary>
    /// Parses the cell value and determines the data type.
    /// </summary>
    private void ParseCellValue(IXLCell cell, CellValue cellValue)
    {
        // Ref: Wiki/Excel-Specification#Cell-Data-Types — all 7 types
        cellValue.DataType = cell.DataType switch
        {
            XLDataType.Text => CellDataType.String,
            XLDataType.Number => CellDataType.Number,
            XLDataType.DateTime => CellDataType.DateTime,
            XLDataType.Boolean => CellDataType.Boolean,
            XLDataType.TimeSpan => CellDataType.DateTime,
            XLDataType.Error => CellDataType.Error,
            XLDataType.Blank => CellDataType.Blank,
            _ => CellDataType.String
        };

        // Handle formula cells — evaluate and use result
        if (cell.HasFormula)
        {
            cellValue.DataType = CellDataType.Formula;
        }

        // Get display value with line break handling
        // Ref: Wiki/Excel-Specification#Line-Break-Handling
        cellValue.RawValue = GetRawValue(cell);
        cellValue.DisplayValue = GetDisplayValue(cell);
    }

    /// <summary>
    /// Gets the raw value from a cell based on its data type.
    /// </summary>
    private static object? GetRawValue(IXLCell cell)
    {
        return cell.DataType switch
        {
            XLDataType.Text => cell.GetString(),
            XLDataType.Number => cell.GetDouble(),
            XLDataType.DateTime => cell.GetDateTime(),
            XLDataType.Boolean => cell.GetBoolean(),
            XLDataType.TimeSpan => cell.GetTimeSpan(),
            XLDataType.Error => cell.GetError().ToString(),
            XLDataType.Blank => null,
            _ => cell.GetString()
        };
    }

    /// <summary>
    /// Gets the display value of a cell, handling line breaks.
    /// Ref: Wiki/Excel-Specification#Line-Break-Handling (Section 5)
    /// </summary>
    private static string GetDisplayValue(IXLCell cell)
    {
        if (cell.DataType == XLDataType.Blank)
        {
            return string.Empty;
        }

        if (cell.DataType == XLDataType.Error)
        {
            return cell.GetError().ToString();
        }

        var formatted = cell.GetFormattedString();

        // Normalize line breaks: \r\n → \n
        // Ref: Wiki/Excel-Specification#Line-Break-Handling — explicit line breaks
        formatted = formatted.Replace("\r\n", "\n").Replace("\r", "\n");

        return formatted;
    }

    /// <summary>
    /// Parses cell formatting properties (font, alignment, colors).
    /// Ref: Wiki/Excel-Specification#Cell-Formatting-Rules (Section 3)
    /// </summary>
    private static void ParseCellFormatting(IXLCell cell, CellValue cellValue)
    {
        var style = cell.Style;

        // Font properties
        cellValue.FontFamily = style.Font.FontName;
        cellValue.FontSize = style.Font.FontSize;
        cellValue.IsBold = style.Font.Bold;
        cellValue.IsItalic = style.Font.Italic;
        cellValue.IsUnderline = style.Font.Underline != XLFontUnderlineValues.None;
        cellValue.IsStrikethrough = style.Font.Strikethrough;

        // Font color
        if (style.Font.FontColor.ColorType == XLColorType.Color)
        {
            cellValue.FontColor = $"#{style.Font.FontColor.Color.R:X2}{style.Font.FontColor.Color.G:X2}{style.Font.FontColor.Color.B:X2}";
        }

        // Background color
        if (style.Fill.BackgroundColor.ColorType == XLColorType.Color)
        {
            var bgColor = style.Fill.BackgroundColor.Color;
            cellValue.BackgroundColor = $"#{bgColor.R:X2}{bgColor.G:X2}{bgColor.B:X2}";
        }

        // Text wrap
        cellValue.WrapText = style.Alignment.WrapText;

        // Alignment
        cellValue.HorizontalAlignment = style.Alignment.Horizontal switch
        {
            XLAlignmentHorizontalValues.Left => HorizontalAlignment.Left,
            XLAlignmentHorizontalValues.Center => HorizontalAlignment.Center,
            XLAlignmentHorizontalValues.Right => HorizontalAlignment.Right,
            XLAlignmentHorizontalValues.Justify => HorizontalAlignment.Justify,
            _ => cellValue.DataType is CellDataType.Number or CellDataType.DateTime
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left
        };

        cellValue.VerticalAlignment = style.Alignment.Vertical switch
        {
            XLAlignmentVerticalValues.Top => VerticalAlignment.Top,
            XLAlignmentVerticalValues.Center => VerticalAlignment.Middle,
            XLAlignmentVerticalValues.Bottom => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Bottom
        };
    }
}
