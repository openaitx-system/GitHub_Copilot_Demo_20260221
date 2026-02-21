#nullable enable

using ExcelToPdf.Core.Interfaces;
using ExcelToPdf.Parsing;
using ExcelToPdf.Rendering;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ExcelToPdf.Tests.Rendering;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddExcelToPdf_NullServices_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ServiceCollectionExtensions.AddExcelToPdf(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddExcelToPdf_RegistersExcelParser()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddExcelToPdf();
        var provider = services.BuildServiceProvider();

        // Assert
        var parser = provider.GetService<IExcelParser>();
        parser.Should().NotBeNull();
        parser.Should().BeOfType<ExcelParser>();
    }

    [Fact]
    public void AddExcelToPdf_RegistersPdfRenderer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddExcelToPdf();
        var provider = services.BuildServiceProvider();

        // Assert
        var renderer = provider.GetService<IPdfRenderer>();
        renderer.Should().NotBeNull();
        renderer.Should().BeOfType<PdfRenderer>();
    }

    [Fact]
    public void AddExcelToPdf_RegistersConverter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddExcelToPdf();
        var provider = services.BuildServiceProvider();

        // Assert
        var converter = provider.GetService<IExcelToPdfConverter>();
        converter.Should().NotBeNull();
        converter.Should().BeOfType<ExcelToPdfConverter>();
    }

    [Fact]
    public void AddExcelToPdf_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddExcelToPdf();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddExcelToPdf_RegistersAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddExcelToPdf();

        // Assert
        services.Should().Contain(sd =>
            sd.ServiceType == typeof(IExcelParser) &&
            sd.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(sd =>
            sd.ServiceType == typeof(IPdfRenderer) &&
            sd.Lifetime == ServiceLifetime.Scoped);
        services.Should().Contain(sd =>
            sd.ServiceType == typeof(IExcelToPdfConverter) &&
            sd.Lifetime == ServiceLifetime.Scoped);
    }
}
