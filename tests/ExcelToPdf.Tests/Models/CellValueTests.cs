#nullable enable

using ExcelToPdf.Core.Models;
using FluentAssertions;

namespace ExcelToPdf.Tests.Models;

/// <summary>
/// Unit tests for <see cref="CellValue"/> model.
/// </summary>
public class CellValueTests
{
    [Fact]
    public void Constructor_Default_SetsExpectedDefaults()
    {
        // Arrange & Act
        var cell = new CellValue();

        // Assert
        cell.DataType.Should().Be(CellDataType.String);
        cell.RawValue.Should().BeNull();
        cell.DisplayValue.Should().BeEmpty();
        cell.FontFamily.Should().Be("Arial");
        cell.FontSize.Should().Be(11);
        cell.IsBold.Should().BeFalse();
        cell.IsItalic.Should().BeFalse();
        cell.IsUnderline.Should().BeFalse();
        cell.IsStrikethrough.Should().BeFalse();
        cell.FontColor.Should().Be("#000000");
        cell.BackgroundColor.Should().BeNull();
        cell.WrapText.Should().BeFalse();
        cell.HorizontalAlignment.Should().Be(HorizontalAlignment.Left);
        cell.VerticalAlignment.Should().Be(VerticalAlignment.Bottom);
        cell.Row.Should().Be(0);
        cell.Column.Should().Be(0);
    }

    [Fact]
    public void Properties_SetValues_ReturnsExpectedValues()
    {
        // Arrange
        var cell = new CellValue
        {
            DataType = CellDataType.Number,
            RawValue = 42.0,
            DisplayValue = "42",
            FontFamily = "Calibri",
            FontSize = 14,
            IsBold = true,
            IsItalic = true,
            IsUnderline = true,
            IsStrikethrough = true,
            FontColor = "#FF0000",
            BackgroundColor = "#00FF00",
            WrapText = true,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Middle,
            Row = 5,
            Column = 3
        };

        // Assert
        cell.DataType.Should().Be(CellDataType.Number);
        cell.RawValue.Should().Be(42.0);
        cell.DisplayValue.Should().Be("42");
        cell.FontFamily.Should().Be("Calibri");
        cell.FontSize.Should().Be(14);
        cell.IsBold.Should().BeTrue();
        cell.IsItalic.Should().BeTrue();
        cell.IsUnderline.Should().BeTrue();
        cell.IsStrikethrough.Should().BeTrue();
        cell.FontColor.Should().Be("#FF0000");
        cell.BackgroundColor.Should().Be("#00FF00");
        cell.WrapText.Should().BeTrue();
        cell.HorizontalAlignment.Should().Be(HorizontalAlignment.Center);
        cell.VerticalAlignment.Should().Be(VerticalAlignment.Middle);
        cell.Row.Should().Be(5);
        cell.Column.Should().Be(3);
    }

    [Theory]
    [InlineData(CellDataType.String)]
    [InlineData(CellDataType.Number)]
    [InlineData(CellDataType.DateTime)]
    [InlineData(CellDataType.Boolean)]
    [InlineData(CellDataType.Formula)]
    [InlineData(CellDataType.Error)]
    [InlineData(CellDataType.Blank)]
    public void DataType_AllSevenTypes_CanBeAssigned(CellDataType dataType)
    {
        // Arrange & Act
        var cell = new CellValue { DataType = dataType };

        // Assert
        cell.DataType.Should().Be(dataType);
    }
}
