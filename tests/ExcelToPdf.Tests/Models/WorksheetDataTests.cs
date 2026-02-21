#nullable enable

using ExcelToPdf.Core.Models;
using FluentAssertions;

namespace ExcelToPdf.Tests.Models;

/// <summary>
/// Unit tests for <see cref="WorksheetData"/> model.
/// </summary>
public class WorksheetDataTests
{
    [Fact]
    public void Constructor_Default_SetsExpectedDefaults()
    {
        // Arrange & Act
        var data = new WorksheetData();

        // Assert
        data.Name.Should().BeEmpty();
        data.Cells.Should().BeEmpty();
        data.RowHeights.Should().BeEmpty();
        data.ColumnWidths.Should().BeEmpty();
        data.RowCount.Should().Be(0);
        data.ColumnCount.Should().Be(0);
        data.DefaultRowHeight.Should().Be(15.0);
        data.DefaultColumnWidth.Should().Be(8.43);
    }

    [Fact]
    public void GetRowHeight_CustomHeight_ReturnsCustomValue()
    {
        // Arrange
        var data = new WorksheetData();
        data.RowHeights[2] = 30.0;

        // Act
        var height = data.GetRowHeight(2);

        // Assert
        height.Should().Be(30.0);
    }

    [Fact]
    public void GetRowHeight_NoCustomHeight_ReturnsDefault()
    {
        // Arrange
        var data = new WorksheetData();

        // Act
        var height = data.GetRowHeight(5);

        // Assert
        height.Should().Be(15.0);
    }

    [Fact]
    public void GetColumnWidth_CustomWidth_ReturnsCustomValue()
    {
        // Arrange
        var data = new WorksheetData();
        data.ColumnWidths[3] = 20.0;

        // Act
        var width = data.GetColumnWidth(3);

        // Assert
        width.Should().Be(20.0);
    }

    [Fact]
    public void GetColumnWidth_NoCustomWidth_ReturnsDefault()
    {
        // Arrange
        var data = new WorksheetData();

        // Act
        var width = data.GetColumnWidth(7);

        // Assert
        width.Should().Be(8.43);
    }

    [Fact]
    public void Cells_AddAndRetrieve_WorksCorrectly()
    {
        // Arrange
        var data = new WorksheetData();
        var cell = new CellValue
        {
            Row = 0,
            Column = 0,
            DisplayValue = "Hello",
            DataType = CellDataType.String
        };

        // Act
        data.Cells[(0, 0)] = cell;

        // Assert
        data.Cells.Should().ContainKey((0, 0));
        data.Cells[(0, 0)].DisplayValue.Should().Be("Hello");
    }

    [Fact]
    public void GetRowHeight_ModifiedDefault_UsesNewDefault()
    {
        // Arrange
        var data = new WorksheetData { DefaultRowHeight = 25.0 };

        // Act
        var height = data.GetRowHeight(0);

        // Assert
        height.Should().Be(25.0);
    }

    [Fact]
    public void GetColumnWidth_ModifiedDefault_UsesNewDefault()
    {
        // Arrange
        var data = new WorksheetData { DefaultColumnWidth = 12.0 };

        // Act
        var width = data.GetColumnWidth(0);

        // Assert
        width.Should().Be(12.0);
    }
}
