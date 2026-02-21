# Excel Specification

> Reference: Issue #1 — Phase 1: GitHub Wiki as RAG Knowledge Base

This document defines the standards for Excel file handling in the Excel-to-PDF conversion library.

---

## 1. Supported File Formats

| Format | Extension | Support Level |
|--------|-----------|---------------|
| Excel 2007+ | `.xlsx` | Full |
| Excel 97-2003 | `.xls` | Read-only |
| CSV | `.csv` | Import only |

---

## 2. Cell Data Types

| Data Type | Description | Conversion Behavior |
|-----------|-------------|---------------------|
| `String` | Plain text content | Rendered as-is in PDF |
| `Number` | Integer or decimal | Right-aligned, formatted per culture |
| `DateTime` | Date and/or time | Formatted using specified date format |
| `Boolean` | TRUE/FALSE | Rendered as "TRUE" or "FALSE" |
| `Formula` | Calculated cell | Evaluate and render the **result value** |
| `Error` | Error values (#N/A, etc.) | Render error string in red |
| `Blank` | Empty cell | Render as empty space |

---

## 3. Cell Formatting Rules

### 3.1 Text Formatting

| Property | Supported | PDF Mapping |
|----------|-----------|-------------|
| Font Family | ✅ | Map to closest PDF-embeddable font |
| Font Size | ✅ | Direct point-size mapping |
| Bold | ✅ | PDF bold variant |
| Italic | ✅ | PDF italic variant |
| Underline | ✅ | PDF text decoration |
| Strikethrough | ✅ | PDF line-through decoration |
| Font Color | ✅ | Direct RGB mapping |
| Text Wrap | ✅ | Enable multi-line cell rendering |

### 3.2 Cell Alignment

| Alignment | Values | Default |
|-----------|--------|---------|
| Horizontal | Left, Center, Right, Justify | Left (text), Right (numbers) |
| Vertical | Top, Middle, Bottom | Bottom |
| Text Rotation | 0-180 degrees | 0 |
| Indent Level | 0-15 | 0 |

### 3.3 Cell Borders

| Property | Values |
|----------|--------|
| Style | None, Thin, Medium, Thick, Dashed, Dotted, Double |
| Color | RGB hex value |
| Position | Top, Bottom, Left, Right, Diagonal |

### 3.4 Cell Background

| Property | Values |
|----------|--------|
| Fill Type | None, Solid, Pattern |
| Fill Color | RGB hex value |
| Pattern Style | (if Pattern) Various pattern types |

---

## 4. Worksheet Rules

### 4.1 Sheet Structure

```
Workbook
├── Sheet 1
│   ├── Merged Cells (track merge ranges)
│   ├── Row Heights (custom or auto-fit)
│   ├── Column Widths (custom or auto-fit)
│   └── Print Area (if defined, use as PDF boundary)
├── Sheet 2
│   └── ...
└── Sheet N
```

### 4.2 Row and Column Sizing

| Property | Rule |
|----------|------|
| Default Row Height | 15 points |
| Default Column Width | 8.43 characters (64 pixels) |
| Auto-fit | Calculate based on content + padding |
| Hidden Rows/Columns | Skip in PDF output |
| Max Row Height | 409 points |
| Max Column Width | 255 characters |

### 4.3 Merged Cells

- Merged cells span across the defined range in PDF
- Content is placed in the **top-left cell** of the merge range
- Alignment applies to the entire merged area
- Borders apply to the outer edge of the merged range

---

## 5. Line Break Handling

| Scenario | Rule |
|----------|------|
| Explicit line break (`\n` or `Alt+Enter`) | Insert PDF line break |
| Word wrap enabled | Auto-wrap at cell boundary |
| Word wrap disabled | Truncate or overflow based on setting |
| Overflow to adjacent empty cells | Render only within cell bounds in PDF |

### 5.1 Line Break Priority

1. Explicit line breaks (`\n`, `\r\n`) — always honored
2. Word wrap setting — if enabled, wrap at cell width
3. Text overflow — if adjacent cell is empty and wrap is off, text may overflow visually

---

## 6. Number Formatting

| Format Code | Example | PDF Output |
|-------------|---------|------------|
| `General` | 1234.5 | `1234.5` |
| `#,##0` | 1234 | `1,234` |
| `#,##0.00` | 1234.5 | `1,234.50` |
| `0%` | 0.75 | `75%` |
| `0.00%` | 0.756 | `75.60%` |
| `$#,##0` | 1234 | `$1,234` |
| `yyyy-MM-dd` | 45678 | `2025-01-15` |

---

## 7. Limitations and Edge Cases

| Scenario | Handling |
|----------|----------|
| Cells exceeding max content length | Truncate with ellipsis (`...`) |
| Unsupported fonts | Fallback to default sans-serif |
| Complex conditional formatting | Apply first matching rule only |
| Pivot tables | Render as static data (no interactivity) |
| Charts/Images | Phase 2 feature — skip in Phase 1 |
| VBA Macros | Ignore — not relevant to rendering |
| External links | Render as plain text |

---

## 8. Encoding and Culture

| Setting | Rule |
|---------|------|
| Text Encoding | UTF-8 |
| Default Culture | `en-US` |
| Number Decimal Separator | Culture-dependent (default `.`) |
| Number Group Separator | Culture-dependent (default `,`) |
| Date Format | Culture-dependent (default `yyyy-MM-dd`) |

---

> **AI Context Note**: When generating code for Excel parsing, always reference this specification for data type handling and formatting rules. Use `// Ref: Wiki/Excel-Specification#section` comments in code.
