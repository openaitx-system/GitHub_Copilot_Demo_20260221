#nullable enable

// Ref: Wiki/Excel-Specification#Worksheet-Rules

namespace ExcelToPdf.Core.Models;

/// <summary>
/// Represents a parsed Excel worksheet with all cell values and dimensions.
/// </summary>
public class WorksheetData
{
    /// <summary>
    /// Gets or sets the worksheet name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cells in this worksheet, keyed by (row, column).
    /// </summary>
    public Dictionary<(int Row, int Column), CellValue> Cells { get; set; } = new();

    /// <summary>
    /// Gets or sets custom row heights in points, keyed by row index.
    /// </summary>
    public Dictionary<int, double> RowHeights { get; set; } = new();

    /// <summary>
    /// Gets or sets custom column widths in character units, keyed by column index.
    /// </summary>
    public Dictionary<int, double> ColumnWidths { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of rows with data.
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of columns with data.
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// Gets the default row height in points.
    /// Ref: Wiki/Excel-Specification#Row-and-Column-Sizing
    /// </summary>
    public double DefaultRowHeight { get; set; } = 15.0;

    /// <summary>
    /// Gets the default column width in character units.
    /// Ref: Wiki/Excel-Specification#Row-and-Column-Sizing
    /// </summary>
    public double DefaultColumnWidth { get; set; } = 8.43;

    /// <summary>
    /// Gets the row height for a specific row, falling back to default.
    /// </summary>
    /// <param name="row">The 0-based row index.</param>
    /// <returns>The row height in points.</returns>
    public double GetRowHeight(int row)
    {
        return RowHeights.TryGetValue(row, out var height) ? height : DefaultRowHeight;
    }

    /// <summary>
    /// Gets the column width for a specific column, falling back to default.
    /// </summary>
    /// <param name="column">The 0-based column index.</param>
    /// <returns>The column width in character units.</returns>
    public double GetColumnWidth(int column)
    {
        return ColumnWidths.TryGetValue(column, out var width) ? width : DefaultColumnWidth;
    }
}
