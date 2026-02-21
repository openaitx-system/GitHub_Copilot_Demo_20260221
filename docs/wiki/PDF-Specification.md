# PDF Specification

> Reference: Issue #1 — Phase 1: GitHub Wiki as RAG Knowledge Base

This document defines the standards for PDF output generation in the Excel-to-PDF conversion library.

---

## 1. PDF Version and Compliance

| Property | Value |
|----------|-------|
| PDF Version | 1.7 (ISO 32000-1:2008) |
| Compliance | PDF/A-1b recommended for archival |
| Color Space | RGB |
| Compression | Flate (zlib) for text streams |

---

## 2. Page Layout

### 2.1 Page Sizes

| Size | Width (pt) | Height (pt) | Width (mm) | Height (mm) |
|------|-----------|-------------|-----------|-------------|
| A4 (default) | 595.28 | 841.89 | 210 | 297 |
| Letter | 612 | 792 | 215.9 | 279.4 |
| A3 | 841.89 | 1190.55 | 297 | 420 |
| Legal | 612 | 1008 | 215.9 | 355.6 |

### 2.2 Page Orientation

| Setting | Rule |
|---------|------|
| Portrait | Default orientation |
| Landscape | Swap width and height |
| Auto-detect | Use landscape if content width > content height |

### 2.3 Margins

| Margin | Default (pt) | Default (mm) | Minimum (pt) |
|--------|-------------|-------------|--------------|
| Top | 72 | 25.4 | 18 |
| Bottom | 72 | 25.4 | 18 |
| Left | 72 | 25.4 | 18 |
| Right | 72 | 25.4 | 18 |

---

## 3. Font Standards

### 3.1 Default Fonts

| Usage | Font | Fallback |
|-------|------|----------|
| Body Text | Arial | Helvetica → Liberation Sans |
| Monospace | Courier New | Courier → Liberation Mono |
| Header | Arial Bold | Helvetica-Bold |

### 3.2 Font Embedding Rules

| Rule | Description |
|------|-------------|
| Always embed | All non-standard fonts must be embedded |
| Subset embedding | Only embed glyphs actually used |
| CJK support | Embed CJK fonts when CJK characters detected |
| Fallback chain | Try: Original → System font → Default font |

### 3.3 Font Size Mapping

| Excel Size (pt) | PDF Size (pt) | Notes |
|-----------------|---------------|-------|
| 8 | 8 | Minimum readable |
| 10 | 10 | Small text |
| 11 | 11 | Default body |
| 12 | 12 | Standard |
| 14 | 14 | Sub-heading |
| 18 | 18 | Heading |
| 24+ | 24+ | Title |

---

## 4. Text Rendering

### 4.1 Basic Text Rules

| Rule | Description |
|------|-------------|
| Encoding | UTF-8 → PDF Unicode mapping (ToUnicode CMap) |
| Kerning | Apply if font supports it |
| Leading | 120% of font size (default line spacing) |
| Character spacing | 0 (default), adjustable per cell |

### 4.2 Line Break Rendering

| Source | PDF Behavior |
|--------|-------------|
| Explicit `\n` | Start new line within text block |
| Explicit `\r\n` | Normalize to `\n`, start new line |
| Word wrap | Calculate wrap points based on cell width |
| Overflow | Clip to cell boundary |

### 4.3 Text Positioning

```
Cell Bounding Box
┌─────────────────────────┐
│ padding-top              │
│  ┌───────────────────┐  │
│  │ Text content here │  │ ← Positioned by alignment
│  │ Second line       │  │
│  └───────────────────┘  │
│ padding-bottom           │
└─────────────────────────┘
```

| Property | Calculation |
|----------|-------------|
| X position | Cell X + padding + alignment offset |
| Y position | Cell Y + padding + vertical alignment offset |
| Text width | Cell width - (2 × padding) |
| Text height | Line count × line height |

---

## 5. Cell Rendering

### 5.1 Render Order (Back to Front)

1. **Background fill** — Cell background color
2. **Borders** — Cell border lines
3. **Text content** — Cell text with formatting

### 5.2 Border Rendering

| Property | PDF Equivalent |
|----------|---------------|
| Thin (1px) | 0.5 pt line |
| Medium (2px) | 1.0 pt line |
| Thick (3px) | 1.5 pt line |
| Dashed | Dash pattern `[3 3]` |
| Dotted | Dash pattern `[1 2]` |
| Double | Two 0.5pt lines with 1pt gap |

### 5.3 Color Mapping

| Excel Color | PDF Color |
|-------------|-----------|
| RGB(r, g, b) | Direct mapping to PDF RGB |
| Theme color | Resolve theme → RGB → PDF RGB |
| Indexed color | Map from Excel palette → RGB |
| Auto/Default | Black (#000000) for text |

---

## 6. Multi-Page Handling

| Scenario | Rule |
|----------|------|
| Content exceeds page height | Auto page break |
| Row split across pages | Keep row together if possible; split if row > page height |
| Repeat rows (header) | Repeat defined header rows on each page |
| Repeat columns | Repeat defined header columns on each page |
| Page break (explicit) | Honor Excel-defined page breaks |
| Sheet-per-page | Each worksheet starts on a new page (configurable) |

---

## 7. Header and Footer

| Element | Position | Content Support |
|---------|----------|-----------------|
| Header Left | Top-left | Text, page number, date |
| Header Center | Top-center | Text, page number, date |
| Header Right | Top-right | Text, page number, date |
| Footer Left | Bottom-left | Text, page number, total pages |
| Footer Center | Bottom-center | Text, page number, total pages |
| Footer Right | Bottom-right | Text, page number, total pages |

### 7.1 Special Tokens

| Token | Replacement |
|-------|-------------|
| `&P` | Current page number |
| `&N` | Total page count |
| `&D` | Current date |
| `&T` | Current time |
| `&F` | File name |
| `&A` | Sheet name |

---

## 8. Performance Requirements

| Metric | Target |
|--------|--------|
| Small file (< 100 rows) | < 500ms |
| Medium file (100-1000 rows) | < 2s |
| Large file (1000-10000 rows) | < 10s |
| Memory usage | < 2× input file size |

---

## 9. Output Quality Standards

| Standard | Requirement |
|----------|-------------|
| Text clarity | All text must be searchable/selectable |
| Resolution | 300 DPI equivalent for any rasterized content |
| File size | Optimize with compression; target < 3× content data |
| Accessibility | Include document structure tags when possible |

---

## 10. Error Handling

| Error Scenario | Behavior |
|----------------|----------|
| Invalid Excel file | Throw `InvalidFileFormatException` with details |
| Corrupted cell data | Skip cell, log warning, continue |
| Font not found | Use fallback font, log warning |
| Out of memory | Throw `OutOfMemoryException` with file size info |
| Empty worksheet | Generate single blank page with sheet name |

---

> **AI Context Note**: When generating PDF rendering code, always reference this specification for layout calculations and quality standards. Use `// Ref: Wiki/PDF-Specification#section` comments in code.
