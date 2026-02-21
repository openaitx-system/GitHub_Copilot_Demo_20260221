#nullable enable

using ExcelToPdf.Core.Interfaces;
using ExcelToPdf.Parsing;
using Microsoft.Extensions.DependencyInjection;

namespace ExcelToPdf.Rendering;

/// <summary>
/// Extension methods for registering Excel-to-PDF services in DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Excel-to-PDF conversion services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddExcelToPdf(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IExcelParser, ExcelParser>();
        services.AddScoped<IPdfRenderer, PdfRenderer>();
        services.AddScoped<IExcelToPdfConverter, ExcelToPdfConverter>();

        return services;
    }
}
