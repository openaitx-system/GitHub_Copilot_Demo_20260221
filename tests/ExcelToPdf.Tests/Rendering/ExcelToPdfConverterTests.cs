#nullable enable

using ExcelToPdf.Core.Exceptions;
using ExcelToPdf.Core.Interfaces;
using ExcelToPdf.Core.Models;
using ExcelToPdf.Rendering;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ExcelToPdf.Tests.Rendering;

/// <summary>
/// Unit tests for <see cref="ExcelToPdfConverter"/>.
/// </summary>
public class ExcelToPdfConverterTests
{
    private readonly IExcelParser _parser;
    private readonly IPdfRenderer _renderer;
    private readonly ILogger<ExcelToPdfConverter> _logger;
    private readonly ExcelToPdfConverter _converter;

    public ExcelToPdfConverterTests()
    {
        _parser = Substitute.For<IExcelParser>();
        _renderer = Substitute.For<IPdfRenderer>();
        _logger = Substitute.For<ILogger<ExcelToPdfConverter>>();
        _converter = new ExcelToPdfConverter(_parser, _renderer, _logger);
    }

    [Fact]
    public void Constructor_NullParser_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ExcelToPdfConverter(
            null!, _renderer, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("excelParser");
    }

    [Fact]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ExcelToPdfConverter(
            _parser, null!, _logger);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("pdfRenderer");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ExcelToPdfConverter(
            _parser, _renderer, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task ConvertAsync_NullExcelStream_ThrowsArgumentNullException()
    {
        // Arrange
        using var pdfStream = new MemoryStream();

        // Act
        var act = () => _converter.ConvertAsync(null!, pdfStream);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("excelStream");
    }

    [Fact]
    public async Task ConvertAsync_NullPdfStream_ThrowsArgumentNullException()
    {
        // Arrange
        using var excelStream = new MemoryStream();

        // Act
        var act = () => _converter.ConvertAsync(excelStream, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("pdfStream");
    }

    [Fact]
    public async Task ConvertAsync_ValidInput_CallsParserAndRenderer()
    {
        // Arrange
        using var excelStream = new MemoryStream();
        using var pdfStream = new MemoryStream();
        var worksheets = new List<WorksheetData> { new() { Name = "Sheet1" } };

        _parser.ParseAsync(excelStream, Arg.Any<CancellationToken>())
            .Returns(worksheets);

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        await _parser.Received(1).ParseAsync(excelStream, Arg.Any<CancellationToken>());
        await _renderer.Received(1).RenderAsync(
            worksheets, pdfStream, Arg.Any<ConversionOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConvertAsync_NullOptions_UsesDefaultOptions()
    {
        // Arrange
        using var excelStream = new MemoryStream();
        using var pdfStream = new MemoryStream();
        var worksheets = new List<WorksheetData>();

        _parser.ParseAsync(excelStream, Arg.Any<CancellationToken>())
            .Returns(worksheets);

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream, options: null);

        // Assert
        await _renderer.Received(1).RenderAsync(
            worksheets,
            pdfStream,
            Arg.Is<ConversionOptions>(o => o.PageSize == PageSize.A4),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConvertAsync_CustomOptions_PassesOptionsToRenderer()
    {
        // Arrange
        using var excelStream = new MemoryStream();
        using var pdfStream = new MemoryStream();
        var worksheets = new List<WorksheetData>();
        var options = new ConversionOptions { PageSize = PageSize.Letter };

        _parser.ParseAsync(excelStream, Arg.Any<CancellationToken>())
            .Returns(worksheets);

        // Act
        await _converter.ConvertAsync(excelStream, pdfStream, options);

        // Assert
        await _renderer.Received(1).RenderAsync(
            worksheets, pdfStream, options, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConvertAsync_ParserThrowsInvalidFileFormat_PropagatesException()
    {
        // Arrange
        using var excelStream = new MemoryStream();
        using var pdfStream = new MemoryStream();

        _parser.ParseAsync(excelStream, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidFileFormatException("Bad file"));

        // Act
        var act = () => _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        await act.Should().ThrowAsync<InvalidFileFormatException>()
            .WithMessage("Bad file");
    }

    [Fact]
    public async Task ConvertAsync_RendererThrowsRenderingException_PropagatesException()
    {
        // Arrange
        using var excelStream = new MemoryStream();
        using var pdfStream = new MemoryStream();
        var worksheets = new List<WorksheetData>();

        _parser.ParseAsync(excelStream, Arg.Any<CancellationToken>())
            .Returns(worksheets);

        _renderer.RenderAsync(worksheets, pdfStream, Arg.Any<ConversionOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RenderingException("Render failed"));

        // Act
        var act = () => _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        await act.Should().ThrowAsync<RenderingException>()
            .WithMessage("Render failed");
    }

    [Fact]
    public async Task ConvertAsync_UnexpectedException_WrapsInConversionException()
    {
        // Arrange
        using var excelStream = new MemoryStream();
        using var pdfStream = new MemoryStream();

        _parser.ParseAsync(excelStream, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected"));

        // Act
        var act = () => _converter.ConvertAsync(excelStream, pdfStream);

        // Assert
        var ex = await act.Should().ThrowAsync<ConversionException>();
        ex.Which.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task ConvertAsync_CancellationRequested_PropagatesOperationCanceled()
    {
        // Arrange
        using var excelStream = new MemoryStream();
        using var pdfStream = new MemoryStream();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _parser.ParseAsync(excelStream, Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var act = () => _converter.ConvertAsync(excelStream, pdfStream, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
