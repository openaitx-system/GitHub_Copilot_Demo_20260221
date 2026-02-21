#nullable enable

using ExcelToPdf.Core.Models;
using FluentAssertions;

namespace ExcelToPdf.Tests.Models;

/// <summary>
/// Unit tests for <see cref="ConversionOptions"/> model.
/// </summary>
public class ConversionOptionsTests
{
    [Fact]
    public void Constructor_Default_SetsExpectedDefaults()
    {
        // Arrange & Act
        var options = new ConversionOptions();

        // Assert
        options.PageSize.Should().Be(PageSize.A4);
        options.Orientation.Should().Be(PageOrientation.Portrait);
        options.MarginTop.Should().Be(72f);
        options.MarginBottom.Should().Be(72f);
        options.MarginLeft.Should().Be(72f);
        options.MarginRight.Should().Be(72f);
        options.DefaultFontFamily.Should().Be("Arial");
        options.DefaultFontSize.Should().Be(11f);
        options.SheetPerPage.Should().BeTrue();
    }

    [Fact]
    public void Properties_CustomValues_ReturnsExpectedValues()
    {
        // Arrange & Act
        var options = new ConversionOptions
        {
            PageSize = PageSize.Letter,
            Orientation = PageOrientation.Landscape,
            MarginTop = 36f,
            MarginBottom = 36f,
            MarginLeft = 48f,
            MarginRight = 48f,
            DefaultFontFamily = "Calibri",
            DefaultFontSize = 10f,
            SheetPerPage = false
        };

        // Assert
        options.PageSize.Should().Be(PageSize.Letter);
        options.Orientation.Should().Be(PageOrientation.Landscape);
        options.MarginTop.Should().Be(36f);
        options.MarginBottom.Should().Be(36f);
        options.MarginLeft.Should().Be(48f);
        options.MarginRight.Should().Be(48f);
        options.DefaultFontFamily.Should().Be("Calibri");
        options.DefaultFontSize.Should().Be(10f);
        options.SheetPerPage.Should().BeFalse();
    }

    [Theory]
    [InlineData(PageSize.A4)]
    [InlineData(PageSize.Letter)]
    [InlineData(PageSize.A3)]
    [InlineData(PageSize.Legal)]
    public void PageSize_AllSupportedSizes_CanBeAssigned(PageSize size)
    {
        // Arrange & Act
        var options = new ConversionOptions { PageSize = size };

        // Assert
        options.PageSize.Should().Be(size);
    }
}
