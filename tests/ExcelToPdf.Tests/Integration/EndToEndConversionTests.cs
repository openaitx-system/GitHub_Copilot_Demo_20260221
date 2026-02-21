#nullable enable

using ClosedXML.Excel;
using ExcelToPdf.Core.Models;
using ExcelToPdf.Parsing;
using ExcelToPdf.Rendering;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ExcelToPdf.Tests.Integration;

/// <summary>
/// End-to-end integration tests for Excel-to-PDF conversion.
/// </summary>
public class EndToEndConversionTests
{
    private readonly ExcelToPdfConverter _converter;

    public EndToEndConversionTests()
    {
        var parserLogger = Substitute.For<ILogger<ExcelParser>>();
        var rendererLogger = Substitute.For<ILogger<PdfRenderer>>();
        var converterLogger = Substitute.For<ILogger<ExcelToPdfConverter>>();

        var parser = new ExcelParser(parserLogger);
        var renderer = new PdfRenderer(rendererLogger);
        _converter = new ExcelToPdfConverter(parser, renderer, converterLogger);
    }

    [Fact]
    public async Task ConvertAsync_SimpleWorkbook_ProducesValidPdf()
    {
        // Arrange
        using var excelStream = CreateSampleExcel();
        using var pdfStream = new MemoryStream();

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        pdfStream.Length.Should().BeGreaterThan(0);
        pdfStream.Position = 0;

        var header = new byte[5];
        await pdfStream.ReadAsync(header);
        var headerStr = System.Text.Encoding.ASCII.GetString(header);
        headerStr.Should().Be("%PDF-");
    }

    [Fact]
    public async Task ConvertAsync_AllDataTypes_ProducesValidPdf()
    {
        // Arrange
        using var excelStream = CreateAllDataTypesExcel();
        using var pdfStream = new MemoryStream();

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        pdfStream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertAsync_FormattedWorkbook_ProducesValidPdf()
    {
        // Arrange
        using var excelStream = CreateFormattedExcel();
        using var pdfStream = new MemoryStream();

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        pdfStream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertAsync_MultipleSheets_ProducesValidPdf()
    {
        // Arrange
        using var excelStream = CreateMultiSheetExcel();
        using var pdfStream = new MemoryStream();

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        pdfStream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertAsync_LandscapeLetter_ProducesValidPdf()
    {
        // Arrange
        using var excelStream = CreateSampleExcel();
        using var pdfStream = new MemoryStream();
        var options = new ConversionOptions
        {
            PageSize = PageSize.Letter,
            Orientation = PageOrientation.Landscape,
            MarginTop = 36f,
            MarginBottom = 36f
        };

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream, options);

        // Assert
        pdfStream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertAsync_WithLineBreaks_ProducesValidPdf()
    {
        // Arrange
        using var excelStream = CreateExcelWithLineBreaks();
        using var pdfStream = new MemoryStream();

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        pdfStream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertAsync_EmptyWorkbook_ProducesValidPdf()
    {
        // Arrange
        var wb = new XLWorkbook();
        wb.AddWorksheet("Empty");
        using var excelStream = new MemoryStream();
        wb.SaveAs(excelStream);
        excelStream.Position = 0;

        using var pdfStream = new MemoryStream();

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        pdfStream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ConvertAsync_LargeDataset_ProducesValidPdf()
    {
        // Arrange: 100 rows × 5 columns (fits in landscape A3)
        var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("LargeData");
        for (int row = 1; row <= 100; row++)
        {
            for (int col = 1; col <= 5; col++)
            {
                ws.Cell(row, col).Value = $"R{row}C{col}";
            }
        }

        using var excelStream = new MemoryStream();
        wb.SaveAs(excelStream);
        excelStream.Position = 0;

        using var pdfStream = new MemoryStream();
        var options = new ConversionOptions
        {
            PageSize = PageSize.A3,
            Orientation = PageOrientation.Landscape,
            MarginLeft = 36f,
            MarginRight = 36f
        };

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream, options);

        // Assert
        pdfStream.Length.Should().BeGreaterThan(0);
    }

    private static MemoryStream CreateSampleExcel()
    {
        var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Sales Report");

        // Header row
        ws.Cell(1, 1).Value = "Product";
        ws.Cell(1, 2).Value = "Quarter";
        ws.Cell(1, 3).Value = "Revenue";
        ws.Cell(1, 4).Value = "Units Sold";

        // Style header
        var headerRange = ws.Range(1, 1, 1, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        // Data rows
        ws.Cell(2, 1).Value = "Widget A";
        ws.Cell(2, 2).Value = "Q1 2024";
        ws.Cell(2, 3).Value = 15000.50;
        ws.Cell(2, 4).Value = 150;

        ws.Cell(3, 1).Value = "Widget B";
        ws.Cell(3, 2).Value = "Q1 2024";
        ws.Cell(3, 3).Value = 22000.75;
        ws.Cell(3, 4).Value = 220;

        var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateAllDataTypesExcel()
    {
        var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("DataTypes");

        ws.Cell(1, 1).Value = "String Value";          // String
        ws.Cell(1, 2).Value = 42.5;                    // Number
        ws.Cell(1, 3).Value = new DateTime(2024, 1, 15); // DateTime
        ws.Cell(1, 4).Value = true;                     // Boolean
        ws.Cell(1, 5).FormulaA1 = "=B1*2";             // Formula
        // Row 2: Blank cell (just skip it)

        var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateFormattedExcel()
    {
        var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Formatted");

        ws.Cell(1, 1).Value = "Bold";
        ws.Cell(1, 1).Style.Font.Bold = true;

        ws.Cell(1, 2).Value = "Italic";
        ws.Cell(1, 2).Style.Font.Italic = true;

        ws.Cell(1, 3).Value = "Red Text";
        ws.Cell(1, 3).Style.Font.FontColor = XLColor.Red;

        ws.Cell(2, 1).Value = "Center";
        ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        ws.Cell(2, 2).Value = "Wrapped Text With a Very Long Content Here";
        ws.Cell(2, 2).Style.Alignment.WrapText = true;

        ws.Cell(2, 3).Value = "Highlight";
        ws.Cell(2, 3).Style.Fill.BackgroundColor = XLColor.Yellow;

        var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateMultiSheetExcel()
    {
        var wb = new XLWorkbook();

        var ws1 = wb.AddWorksheet("Summary");
        ws1.Cell(1, 1).Value = "Summary Page";

        var ws2 = wb.AddWorksheet("Details");
        ws2.Cell(1, 1).Value = "Detail Data";
        ws2.Cell(2, 1).Value = 100;

        var ws3 = wb.AddWorksheet("Appendix");
        ws3.Cell(1, 1).Value = "Appendix Content";

        var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream CreateExcelWithLineBreaks()
    {
        var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("LineBreaks");

        ws.Cell(1, 1).Value = "Line1\nLine2\nLine3";
        ws.Cell(1, 1).Style.Alignment.WrapText = true;

        ws.Cell(1, 2).Value = "Normal Text";

        var stream = new MemoryStream();
        wb.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
