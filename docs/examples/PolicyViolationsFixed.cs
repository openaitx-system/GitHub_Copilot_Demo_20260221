#nullable enable

// ============================================================================
// CORRECTED VERSION - Follows all project coding standards
// This file demonstrates the proper way to write code following
// the Development Standards defined in the Wiki.
// Each fix corresponds to a violation in docs/examples/PolicyViolations.cs.
// ============================================================================

using Microsoft.Extensions.Logging;

// FIX E004: File-scoped namespace (ends with semicolon)
namespace ExcelToPdf.Examples.Corrected;

// FIX E001: Class name is PascalCase
/// <summary>
/// Example service demonstrating correct coding standards.
/// </summary>
public class GoodExampleService
{
    // FIX E002: Private field uses _camelCase prefix
    private readonly string _connectionString;

    // FIX E006: Static field is readonly (no mutable static state)
    private static readonly int _maxRetryCount = 3;

    // FIX E005: Use ILogger<T> instead of Console.WriteLine
    private readonly ILogger<GoodExampleService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="GoodExampleService"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionString">The connection string.</param>
    public GoodExampleService(ILogger<GoodExampleService> logger, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _connectionString = connectionString;
    }

    /// <summary>
    /// Processes the input data with proper logging.
    /// FIX E005: Uses ILogger instead of Console.WriteLine.
    /// FIX W004: Uses structured logging instead of string interpolation.
    /// </summary>
    /// <param name="input">The input data to process.</param>
    public void ProcessData(string input)
    {
        // FIX W004: Structured logging parameters (no string interpolation)
        _logger.LogInformation("Processing: {Input}", input);
        _logger.LogDebug("Max retry count: {MaxRetryCount}", _maxRetryCount);
    }

    /// <summary>
    /// Loads data from a file asynchronously.
    /// FIX E007: Method has 'Async' suffix.
    /// FIX E005: Uses ILogger instead of Console.WriteLine.
    /// </summary>
    /// <param name="path">The file path to load from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content as a string.</returns>
    public async Task<string> LoadDataAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading data from: {Path}", path);
        var content = await File.ReadAllTextAsync(path, cancellationToken);
        return content;
    }

    /// <summary>
    /// Calculates the sum of two integers.
    /// FIX: Has XML documentation comment on public method.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>The sum of a and b.</returns>
    public int Calculate(int a, int b)
    {
        return a + b;
    }

    /// <summary>
    /// Gets a value by key from the internal dictionary.
    /// FIX: Return type is nullable to indicate possible null return.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value if found; otherwise, null.</returns>
    public string? GetValue(string key)
    {
        var dict = new Dictionary<string, string>();
        dict.TryGetValue(key, out var value);
        return value;
    }

    /// <summary>
    /// Reads a file asynchronously.
    /// FIX: All I/O operations are async.
    /// FIX E007: Method has 'Async' suffix.
    /// FIX E005: Uses ILogger instead of Console.WriteLine.
    /// </summary>
    /// <param name="path">The file path to read.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file contents as a byte array.</returns>
    public async Task<byte[]> ReadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reading file asynchronously: {Path}", path);
        return await File.ReadAllBytesAsync(path, cancellationToken);
    }
}

/// <summary>
/// Example interface with proper 'I' prefix.
/// FIX E001: Interface name starts with 'I' and uses PascalCase.
/// </summary>
public interface IDataProcessor
{
    /// <summary>
    /// Processes data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ProcessAsync(CancellationToken cancellationToken = default);
}
