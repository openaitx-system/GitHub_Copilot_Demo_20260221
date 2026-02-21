#nullable enable

// Ref: Wiki/Excel-Specification#Cell-Formatting-Rules

namespace ExcelToPdf.Core.Models;

/// <summary>
/// Represents the value and formatting of a single Excel cell.
/// </summary>
public class CellValue
{
    /// <summary>
    /// Gets or sets the data type of the cell.
    /// </summary>
    public CellDataType DataType { get; set; }

    /// <summary>
    /// Gets or sets the raw value of the cell.
    /// </summary>
    public object? RawValue { get; set; }

    /// <summary>
    /// Gets or sets the formatted display string.
    /// </summary>
    public string DisplayValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the font family name.
    /// </summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>
    /// Gets or sets the font size in points.
    /// </summary>
    public double FontSize { get; set; } = 11;

    /// <summary>
    /// Gets or sets whether the text is bold.
    /// </summary>
    public bool IsBold { get; set; }

    /// <summary>
    /// Gets or sets whether the text is italic.
    /// </summary>
    public bool IsItalic { get; set; }

    /// <summary>
    /// Gets or sets whether the text has underline.
    /// </summary>
    public bool IsUnderline { get; set; }

    /// <summary>
    /// Gets or sets whether the text has strikethrough.
    /// </summary>
    public bool IsStrikethrough { get; set; }

    /// <summary>
    /// Gets or sets the font color as an RGB hex string (e.g., "#FF0000").
    /// </summary>
    public string FontColor { get; set; } = "#000000";

    /// <summary>
    /// Gets or sets the background fill color as an RGB hex string.
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets whether text wrapping is enabled.
    /// </summary>
    public bool WrapText { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;

    /// <summary>
    /// Gets or sets the vertical alignment.
    /// </summary>
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Bottom;

    /// <summary>
    /// Gets or sets the row index (0-based).
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the column index (0-based).
    /// </summary>
    public int Column { get; set; }
}

/// <summary>
/// Horizontal text alignment options.
/// Ref: Wiki/Excel-Specification#Cell-Alignment
/// </summary>
public enum HorizontalAlignment
{
    /// <summary>Left-aligned (default for text).</summary>
    Left,

    /// <summary>Center-aligned.</summary>
    Center,

    /// <summary>Right-aligned (default for numbers).</summary>
    Right,

    /// <summary>Justified text.</summary>
    Justify
}

/// <summary>
/// Vertical text alignment options.
/// Ref: Wiki/Excel-Specification#Cell-Alignment
/// </summary>
public enum VerticalAlignment
{
    /// <summary>Top-aligned.</summary>
    Top,

    /// <summary>Middle-aligned.</summary>
    Middle,

    /// <summary>Bottom-aligned (default).</summary>
    Bottom
}
