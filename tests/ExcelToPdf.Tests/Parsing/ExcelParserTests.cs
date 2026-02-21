#nullable enable

using ClosedXML.Excel;
using ExcelToPdf.Core.Exceptions;
using ExcelToPdf.Core.Models;
using ExcelToPdf.Parsing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ExcelToPdf.Tests.Parsing;

/// <summary>
/// Unit tests for <see cref="ExcelParser"/>.
/// </summary>
public class ExcelParserTests
{
    private readonly ILogger<ExcelParser> _logger;
    private readonly ExcelParser _parser;

    public ExcelParserTests()
    {
        _logger = Substitute.For<ILogger<ExcelParser>>();
        _parser = new ExcelParser(_logger);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ExcelParser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task ParseAsync_NullStream_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _parser.ParseAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("excelStream");
    }

    [Fact]
    public async Task ParseAsync_InvalidStream_ThrowsInvalidFileFormatException()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02, 0x03 });

        // Act
        var act = () => _parser.ParseAsync(stream);

        // Assert
        await act.Should().ThrowAsync<InvalidFileFormatException>();
    }

    [Fact]
    public async Task ParseAsync_EmptyWorkbook_ReturnsEmptyWorksheet()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            wb.AddWorksheet("Empty");
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Empty");
        result[0].Cells.Should().BeEmpty();
        result[0].RowCount.Should().Be(0);
        result[0].ColumnCount.Should().Be(0);
    }

    [Fact]
    public async Task ParseAsync_StringCell_ParsesCorrectly()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Hello World";
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().HaveCount(1);
        result[0].Cells.Should().ContainKey((0, 0));
        var cell = result[0].Cells[(0, 0)];
        cell.DataType.Should().Be(CellDataType.String);
        cell.DisplayValue.Should().Be("Hello World");
        cell.Row.Should().Be(0);
        cell.Column.Should().Be(0);
    }

    [Fact]
    public async Task ParseAsync_NumberCell_ParsesCorrectly()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = 42.5;
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].Cells[(0, 0)].DataType.Should().Be(CellDataType.Number);
        result[0].Cells[(0, 0)].RawValue.Should().Be(42.5);
    }

    [Fact]
    public async Task ParseAsync_BooleanCell_ParsesCorrectly()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = true;
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].Cells[(0, 0)].DataType.Should().Be(CellDataType.Boolean);
        result[0].Cells[(0, 0)].RawValue.Should().Be(true);
    }

    [Fact]
    public async Task ParseAsync_DateTimeCell_ParsesCorrectly()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15, 10, 30, 0);
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = date;
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].Cells[(0, 0)].DataType.Should().Be(CellDataType.DateTime);
        result[0].Cells[(0, 0)].RawValue.Should().Be(date);
    }

    [Fact]
    public async Task ParseAsync_FormulaCell_ParsesAsFormula()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = 10;
            ws.Cell(1, 2).Value = 20;
            ws.Cell(1, 3).FormulaA1 = "=A1+B1";
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].Cells[(0, 2)].DataType.Should().Be(CellDataType.Formula);
    }

    [Fact]
    public async Task ParseAsync_MultipleCells_ParsesDimensions()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "A";
            ws.Cell(2, 1).Value = "B";
            ws.Cell(3, 2).Value = "C";
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].RowCount.Should().Be(3);
        result[0].ColumnCount.Should().Be(2);
        result[0].Cells.Should().HaveCount(3);
    }

    [Fact]
    public async Task ParseAsync_MultipleWorksheets_ParsesAll()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws1 = wb.AddWorksheet("Sheet1");
            ws1.Cell(1, 1).Value = "A";

            var ws2 = wb.AddWorksheet("Sheet2");
            ws2.Cell(1, 1).Value = "B";

            var ws3 = wb.AddWorksheet("Sheet3");
            ws3.Cell(1, 1).Value = "C";
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Sheet1");
        result[1].Name.Should().Be("Sheet2");
        result[2].Name.Should().Be("Sheet3");
    }

    [Fact]
    public async Task ParseAsync_BoldFormatting_ParsesCorrectly()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Bold";
            ws.Cell(1, 1).Style.Font.Bold = true;
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].Cells[(0, 0)].IsBold.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_ItalicFormatting_ParsesCorrectly()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Italic";
            ws.Cell(1, 1).Style.Font.Italic = true;
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].Cells[(0, 0)].IsItalic.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_WrapText_ParsesCorrectly()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Wrapped";
            ws.Cell(1, 1).Style.Alignment.WrapText = true;
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].Cells[(0, 0)].WrapText.Should().BeTrue();
    }

    [Fact]
    public async Task ParseAsync_CenterAlignment_ParsesCorrectly()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Center";
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        var cell = result[0].Cells[(0, 0)];
        cell.HorizontalAlignment.Should().Be(HorizontalAlignment.Center);
        cell.VerticalAlignment.Should().Be(VerticalAlignment.Middle);
    }

    [Fact]
    public async Task ParseAsync_LineBreaks_NormalizesToNewline()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Line1\r\nLine2\nLine3";
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].Cells[(0, 0)].DisplayValue.Should().Be("Line1\nLine2\nLine3");
    }

    [Fact]
    public async Task ParseAsync_CustomRowHeight_Parsed()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Data";
            ws.Row(1).Height = 30;
        });

        // Act
        var result = await _parser.ParseAsync(stream);

        // Assert
        result[0].RowHeights.Should().ContainKey(0);
        result[0].RowHeights[0].Should().Be(30);
    }

    [Fact]
    public async Task ParseAsync_CancellationRequested_ThrowsOperationCanceled()
    {
        // Arrange
        using var stream = CreateExcelStream(wb =>
        {
            var ws = wb.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Data";
        });
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => _parser.ParseAsync(stream, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// Creates an in-memory Excel stream using ClosedXML.
    /// </summary>
    private static MemoryStream CreateExcelStream(Action<XLWorkbook> configure)
    {
        var workbook = new XLWorkbook();
        configure(workbook);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
