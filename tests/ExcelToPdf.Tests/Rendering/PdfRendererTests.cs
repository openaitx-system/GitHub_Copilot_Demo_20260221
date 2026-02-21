#nullable enable

using ExcelToPdf.Core.Exceptions;
using ExcelToPdf.Core.Models;
using ExcelToPdf.Rendering;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ExcelToPdf.Tests.Rendering;

/// <summary>
/// Unit tests for <see cref="PdfRenderer"/>.
/// </summary>
public class PdfRendererTests
{
    private readonly ILogger<PdfRenderer> _logger;
    private readonly PdfRenderer _renderer;

    public PdfRendererTests()
    {
        _logger = Substitute.For<ILogger<PdfRenderer>>();
        _renderer = new PdfRenderer(_logger);
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new PdfRenderer(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task RenderAsync_NullWorksheets_ThrowsArgumentNullException()
    {
        // Arrange
        using var stream = new MemoryStream();
        var options = new ConversionOptions();

        // Act
        var act = () => _renderer.RenderAsync(null!, stream, options);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("worksheets");
    }

    [Fact]
    public async Task RenderAsync_NullOutputStream_ThrowsArgumentNullException()
    {
        // Arrange
        var worksheets = new List<WorksheetData>();
        var options = new ConversionOptions();

        // Act
        var act = () => _renderer.RenderAsync(worksheets, null!, options);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("outputStream");
    }

    [Fact]
    public async Task RenderAsync_NullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var worksheets = new List<WorksheetData>();
        using var stream = new MemoryStream();

        // Act
        var act = () => _renderer.RenderAsync(worksheets, stream, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public async Task RenderAsync_EmptyWorksheets_GeneratesPdf()
    {
        // Arrange
        var worksheets = new List<WorksheetData>();
        using var stream = new MemoryStream();
        var options = new ConversionOptions();

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
        stream.Position = 0;
        var header = new byte[4];
        await stream.ReadAsync(header);
        // PDF files start with %PDF
        header.Should().BeEquivalentTo(new byte[] { 0x25, 0x50, 0x44, 0x46 });
    }

    [Fact]
    public async Task RenderAsync_SingleWorksheet_GeneratesPdf()
    {
        // Arrange
        var worksheet = CreateWorksheetWithCells("TestSheet", new Dictionary<(int, int), string>
        {
            { (0, 0), "Hello" },
            { (0, 1), "World" },
            { (1, 0), "Foo" },
            { (1, 1), "Bar" }
        }, rowCount: 2, columnCount: 2);

        var worksheets = new List<WorksheetData> { worksheet };
        using var stream = new MemoryStream();
        var options = new ConversionOptions();

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_MultipleWorksheets_GeneratesPdf()
    {
        // Arrange
        var ws1 = CreateWorksheetWithCells("Sheet1", new Dictionary<(int, int), string>
        {
            { (0, 0), "A" }
        }, rowCount: 1, columnCount: 1);

        var ws2 = CreateWorksheetWithCells("Sheet2", new Dictionary<(int, int), string>
        {
            { (0, 0), "B" }
        }, rowCount: 1, columnCount: 1);

        var worksheets = new List<WorksheetData> { ws1, ws2 };
        using var stream = new MemoryStream();
        var options = new ConversionOptions();

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_LandscapeOrientation_GeneratesPdf()
    {
        // Arrange
        var worksheet = CreateWorksheetWithCells("Sheet1", new Dictionary<(int, int), string>
        {
            { (0, 0), "Test" }
        }, rowCount: 1, columnCount: 1);

        var worksheets = new List<WorksheetData> { worksheet };
        using var stream = new MemoryStream();
        var options = new ConversionOptions { Orientation = PageOrientation.Landscape };

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(PageSize.A4)]
    [InlineData(PageSize.Letter)]
    [InlineData(PageSize.A3)]
    [InlineData(PageSize.Legal)]
    public async Task RenderAsync_AllPageSizes_GeneratesPdf(PageSize pageSize)
    {
        // Arrange
        var worksheet = CreateWorksheetWithCells("Sheet1", new Dictionary<(int, int), string>
        {
            { (0, 0), "Test" }
        }, rowCount: 1, columnCount: 1);

        var worksheets = new List<WorksheetData> { worksheet };
        using var stream = new MemoryStream();
        var options = new ConversionOptions { PageSize = pageSize };

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_CellWithLineBreaks_GeneratesPdf()
    {
        // Arrange
        var worksheet = new WorksheetData
        {
            Name = "Sheet1",
            RowCount = 1,
            ColumnCount = 1,
            Cells = new Dictionary<(int Row, int Column), CellValue>
            {
                {
                    (0, 0), new CellValue
                    {
                        Row = 0,
                        Column = 0,
                        DisplayValue = "Line1\nLine2\nLine3",
                        DataType = CellDataType.String
                    }
                }
            }
        };

        var worksheets = new List<WorksheetData> { worksheet };
        using var stream = new MemoryStream();
        var options = new ConversionOptions();

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_FormattedCell_GeneratesPdf()
    {
        // Arrange
        var worksheet = new WorksheetData
        {
            Name = "Formatted",
            RowCount = 1,
            ColumnCount = 1,
            Cells = new Dictionary<(int Row, int Column), CellValue>
            {
                {
                    (0, 0), new CellValue
                    {
                        Row = 0,
                        Column = 0,
                        DisplayValue = "Styled",
                        DataType = CellDataType.String,
                        IsBold = true,
                        IsItalic = true,
                        IsUnderline = true,
                        IsStrikethrough = true,
                        FontColor = "#FF0000",
                        BackgroundColor = "#FFFF00",
                        FontSize = 14
                    }
                }
            }
        };

        var worksheets = new List<WorksheetData> { worksheet };
        using var stream = new MemoryStream();
        var options = new ConversionOptions();

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_ErrorCell_GeneratesPdf()
    {
        // Arrange
        var worksheet = new WorksheetData
        {
            Name = "Errors",
            RowCount = 1,
            ColumnCount = 1,
            Cells = new Dictionary<(int Row, int Column), CellValue>
            {
                {
                    (0, 0), new CellValue
                    {
                        Row = 0,
                        Column = 0,
                        DisplayValue = "#REF!",
                        DataType = CellDataType.Error
                    }
                }
            }
        };

        var worksheets = new List<WorksheetData> { worksheet };
        using var stream = new MemoryStream();
        var options = new ConversionOptions();

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RenderAsync_EmptyWorksheet_GeneratesPdf()
    {
        // Arrange
        var worksheet = new WorksheetData
        {
            Name = "Empty",
            RowCount = 0,
            ColumnCount = 0
        };

        var worksheets = new List<WorksheetData> { worksheet };
        using var stream = new MemoryStream();
        var options = new ConversionOptions();

        // Act
        await _renderer.RenderAsync(worksheets, stream, options);

        // Assert
        stream.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Helper to create a simple worksheet with string cells.
    /// </summary>
    private static WorksheetData CreateWorksheetWithCells(
        string name,
        Dictionary<(int Row, int Col), string> cells,
        int rowCount,
        int columnCount)
    {
        var data = new WorksheetData
        {
            Name = name,
            RowCount = rowCount,
            ColumnCount = columnCount
        };

        foreach (var ((row, col), value) in cells)
        {
            data.Cells[(row, col)] = new CellValue
            {
                Row = row,
                Column = col,
                DisplayValue = value,
                DataType = CellDataType.String
            };
        }

        return data;
    }
}
