#nullable enable

// Ref: Wiki/PDF-Specification#Page-Layout

namespace ExcelToPdf.Core.Models;

/// <summary>
/// Configuration options for Excel-to-PDF conversion.
/// </summary>
public class ConversionOptions
{
    /// <summary>
    /// Gets or sets the page size. Default is A4.
    /// Ref: Wiki/PDF-Specification#Page-Sizes
    /// </summary>
    public PageSize PageSize { get; set; } = PageSize.A4;

    /// <summary>
    /// Gets or sets the page orientation.
    /// </summary>
    public PageOrientation Orientation { get; set; } = PageOrientation.Portrait;

    /// <summary>
    /// Gets or sets the top margin in points. Default is 72pt (1 inch).
    /// </summary>
    public float MarginTop { get; set; } = 72f;

    /// <summary>
    /// Gets or sets the bottom margin in points.
    /// </summary>
    public float MarginBottom { get; set; } = 72f;

    /// <summary>
    /// Gets or sets the left margin in points.
    /// </summary>
    public float MarginLeft { get; set; } = 72f;

    /// <summary>
    /// Gets or sets the right margin in points.
    /// </summary>
    public float MarginRight { get; set; } = 72f;

    /// <summary>
    /// Gets or sets the default font family.
    /// Ref: Wiki/PDF-Specification#Default-Fonts
    /// </summary>
    public string DefaultFontFamily { get; set; } = "Arial";

    /// <summary>
    /// Gets or sets the default font size in points.
    /// </summary>
    public float DefaultFontSize { get; set; } = 11f;

    /// <summary>
    /// Gets or sets whether each worksheet starts on a new page.
    /// </summary>
    public bool SheetPerPage { get; set; } = true;
}

/// <summary>
/// Supported page sizes.
/// Ref: Wiki/PDF-Specification#Page-Sizes
/// </summary>
public enum PageSize
{
    /// <summary>A4 (210mm × 297mm, 595.28pt × 841.89pt).</summary>
    A4,

    /// <summary>Letter (8.5in × 11in, 612pt × 792pt).</summary>
    Letter,

    /// <summary>A3 (297mm × 420mm, 841.89pt × 1190.55pt).</summary>
    A3,

    /// <summary>Legal (8.5in × 14in, 612pt × 1008pt).</summary>
    Legal
}

/// <summary>
/// Page orientation options.
/// </summary>
public enum PageOrientation
{
    /// <summary>Portrait (default).</summary>
    Portrait,

    /// <summary>Landscape.</summary>
    Landscape
}
