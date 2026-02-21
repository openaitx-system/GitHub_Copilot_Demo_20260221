#nullable enable

// Ref: Wiki/Excel-Specification#Cell-Data-Types

namespace ExcelToPdf.Core.Models;

/// <summary>
/// Represents the data type of an Excel cell.
/// Ref: Wiki/Excel-Specification#Cell-Data-Types (Section 2)
/// </summary>
public enum CellDataType
{
    /// <summary>Plain text content.</summary>
    String,

    /// <summary>Integer or decimal number.</summary>
    Number,

    /// <summary>Date and/or time value.</summary>
    DateTime,

    /// <summary>TRUE/FALSE value.</summary>
    Boolean,

    /// <summary>Calculated cell (formula result).</summary>
    Formula,

    /// <summary>Error value (#N/A, #REF!, etc.).</summary>
    Error,

    /// <summary>Empty cell.</summary>
    Blank
}
