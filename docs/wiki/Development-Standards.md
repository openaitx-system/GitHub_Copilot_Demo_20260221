# Development Standards

> Reference: Issue #2 — Phase 2: Development Standards and Conventions

This document defines the coding standards for all C# code in the Excel-to-PDF conversion library.

---

## 1. Language and Framework

| Setting | Value |
|---------|-------|
| Language | C# 12 |
| Framework | .NET 8.0 |
| Target | Library (class library) |
| Nullable | Enabled (`#nullable enable`) |
| Implicit Usings | Enabled |

---

## 2. Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Namespace | PascalCase | `ExcelToPdf.Rendering` |
| Class | PascalCase | `PdfRenderer` |
| Interface | IPascalCase | `ICellParser` |
| Public Method | PascalCase | `ConvertToPdf()` |
| Private Method | PascalCase | `CalculateWidth()` |
| Public Property | PascalCase | `PageSize` |
| Private Field | _camelCase | `_logger` |
| Parameter | camelCase | `cellValue` |
| Local Variable | camelCase | `rowHeight` |
| Constant | PascalCase | `DefaultFontSize` |
| Enum | PascalCase | `CellDataType.String` |
| Async Method | PascalCase + Async | `ReadFileAsync()` |
| Test Method | Method_Scenario_Expected | `ParseCell_NullInput_ThrowsException()` |

---

## 3. File Organization

### 3.1 File-Scoped Namespaces (Required)

```csharp
// ✅ Correct - file-scoped namespace
namespace ExcelToPdf.Parsing;

public class CellParser
{
    // ...
}

// ❌ Wrong - block-scoped namespace
namespace ExcelToPdf.Parsing
{
    public class CellParser
    {
        // ...
    }
}
```

### 3.2 File Structure Order

```csharp
// 1. Using directives (sorted)
using System.Text;
using ExcelToPdf.Models;
using Microsoft.Extensions.Logging;

// 2. File-scoped namespace
namespace ExcelToPdf.Parsing;

// 3. Class/Interface declaration
/// <summary>
/// XML documentation for the class.
/// </summary>
public class CellParser : ICellParser
{
    // 4. Constants
    private const int MaxCellLength = 32767;

    // 5. Private fields
    private readonly ILogger<CellParser> _logger;
    private readonly CellFormatOptions _options;

    // 6. Constructor(s)
    public CellParser(ILogger<CellParser> logger, CellFormatOptions options)
    {
        _logger = logger;
        _options = options;
    }

    // 7. Public properties
    public CellFormatOptions Options => _options;

    // 8. Public methods
    /// <summary>
    /// Parses a cell value from the Excel sheet.
    /// </summary>
    public CellValue ParseCell(ICell cell)
    {
        // ...
    }

    // 9. Private methods
    private string FormatNumber(double value)
    {
        // ...
    }
}
```

---

## 4. Code Style Rules

### 4.1 Pattern Matching (Preferred)

```csharp
// ✅ Correct - switch expression
public string GetCellTypeName(CellDataType type) => type switch
{
    CellDataType.String => "Text",
    CellDataType.Number => "Numeric",
    CellDataType.DateTime => "Date/Time",
    CellDataType.Boolean => "Boolean",
    CellDataType.Formula => "Calculated",
    CellDataType.Error => "Error",
    CellDataType.Blank => "Empty",
    _ => throw new ArgumentOutOfRangeException(nameof(type))
};

// ✅ Correct - is pattern
if (value is string text and { Length: > 0 })
{
    ProcessText(text);
}

// ❌ Wrong - traditional switch
switch (type)
{
    case CellDataType.String:
        return "Text";
    // ...
}
```

### 4.2 Null Handling

```csharp
// ✅ Correct - null-conditional and null-coalescing
var displayValue = cell?.StringCellValue ?? string.Empty;
var length = text?.Length ?? 0;

// ✅ Correct - argument null check
public void Render(PdfDocument document)
{
    ArgumentNullException.ThrowIfNull(document);
    // ...
}

// ❌ Wrong - manual null check
if (document == null)
    throw new ArgumentNullException(nameof(document));
```

### 4.3 Async/Await

```csharp
// ✅ Correct - async suffix, cancellation token
public async Task<PdfDocument> ConvertAsync(
    Stream excelStream,
    ConversionOptions options,
    CancellationToken cancellationToken = default)
{
    await using var workbook = await LoadWorkbookAsync(excelStream, cancellationToken);
    // ...
}

// ❌ Wrong - missing Async suffix, no cancellation token
public async Task<PdfDocument> Convert(Stream excelStream)
{
    // ...
}
```

---

## 5. XML Documentation Requirements

### 5.1 Required On

| Element | Required |
|---------|----------|
| Public classes | ✅ Yes |
| Public interfaces | ✅ Yes |
| Public methods | ✅ Yes |
| Public properties | ✅ Yes |
| Public constructors | ✅ Yes |
| Private methods | ❌ Optional |
| Private fields | ❌ Optional |

### 5.2 Documentation Template

```csharp
/// <summary>
/// Converts an Excel worksheet to a PDF page.
/// </summary>
/// <param name="worksheet">The source Excel worksheet. Must not be null.</param>
/// <param name="options">Conversion options including page size and margins.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>A rendered PDF page ready for output.</returns>
/// <exception cref="InvalidFileFormatException">
/// Thrown when the worksheet contains unsupported features.
/// </exception>
/// <remarks>
/// Ref: Wiki/Excel-Specification#Worksheet-Rules
/// </remarks>
public async Task<PdfPage> ConvertWorksheetAsync(
    IWorksheet worksheet,
    ConversionOptions options,
    CancellationToken cancellationToken = default)
```

---

## 6. Dependency Injection Rules

```csharp
// ✅ Correct - constructor injection with interfaces
public class PdfRenderer : IPdfRenderer
{
    private readonly ILogger<PdfRenderer> _logger;
    private readonly ICellParser _cellParser;
    private readonly IFontResolver _fontResolver;

    public PdfRenderer(
        ILogger<PdfRenderer> logger,
        ICellParser cellParser,
        IFontResolver fontResolver)
    {
        _logger = logger;
        _cellParser = cellParser;
        _fontResolver = fontResolver;
    }
}

// ✅ Correct - service registration
services.AddScoped<ICellParser, CellParser>();
services.AddScoped<IPdfRenderer, PdfRenderer>();
services.AddScoped<IFontResolver, FontResolver>();

// ❌ Wrong - static dependencies
public class PdfRenderer
{
    private static readonly CellParser _parser = new(); // NO static mutable state
}

// ❌ Wrong - service locator pattern
public class PdfRenderer
{
    public void Render(IServiceProvider provider) // NO service locator
    {
        var parser = provider.GetService<ICellParser>();
    }
}
```

---

## 7. Logging Standards

```csharp
// ✅ Correct - ILogger<T> with structured logging
_logger.LogInformation("Converting worksheet {SheetName} with {RowCount} rows", 
    worksheet.Name, worksheet.RowCount);

_logger.LogWarning("Font {FontName} not found, using fallback {FallbackFont}", 
    requestedFont, fallbackFont);

_logger.LogError(ex, "Failed to render cell at ({Row}, {Column})", row, column);

// ❌ Wrong - Console.WriteLine
Console.WriteLine($"Converting {worksheet.Name}"); // NEVER in library code

// ❌ Wrong - string interpolation in log
_logger.LogInformation($"Converting {worksheet.Name}"); // Use structured logging
```

---

## 8. Error Handling

### 8.1 Custom Exception Types

| Exception | Usage |
|-----------|-------|
| `InvalidFileFormatException` | Invalid or corrupted Excel file |
| `UnsupportedFeatureException` | Feature not supported in current version |
| `FontNotFoundException` | Required font not available |
| `RenderingException` | Error during PDF rendering |
| `ConversionException` | General conversion failure |

### 8.2 Exception Pattern

```csharp
// ✅ Correct - specific exception with context
public class InvalidFileFormatException : Exception
{
    public string? FilePath { get; }
    public string? ExpectedFormat { get; }

    public InvalidFileFormatException(string message, string? filePath = null)
        : base(message)
    {
        FilePath = filePath;
    }
}

// Usage
throw new InvalidFileFormatException(
    $"File is not a valid Excel format: {Path.GetFileName(filePath)}",
    filePath);
```

---

## 9. Performance Guidelines

| Rule | Description |
|------|-------------|
| Use `Span<T>` / `Memory<T>` | For buffer operations |
| Use `StringBuilder` | For string concatenation in loops |
| Avoid LINQ in hot paths | Use explicit loops for performance-critical code |
| Use `ArrayPool<T>` | For temporary array allocations |
| Cache compiled regex | Use `[GeneratedRegex]` attribute |
| Use `ValueTask` | When result is often synchronous |

---

> **AI Context Note**: When generating C# code for this project, enforce all rules in this document. Flag violations in code reviews. Reference specific sections using `// Ref: Wiki/Development-Standards#Section-N`.
