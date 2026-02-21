#nullable enable

// ============================================================================
// INTENTIONAL POLICY VIOLATIONS - DO NOT USE AS REFERENCE CODE
// This file demonstrates common coding standard violations that the
// AI validation pipeline (validate-policy.py) should detect.
// See: docs/examples/PolicyViolationsFixed.cs for the corrected version.
// ============================================================================

// VIOLATION E004: Block-scoped namespace instead of file-scoped namespace
namespace ExcelToPdf.Examples.Violations {

// VIOLATION E001: Class name is not PascalCase (starts with lowercase)
public class badExampleService
{
    // VIOLATION E002: Private field does not use _camelCase prefix
    private readonly string ConnectionString;

    // VIOLATION E006: Static mutable state (not readonly or const)
    private static int instanceCount = 0;

    // VIOLATION E005: Console.WriteLine in library code (should use ILogger<T>)
    public void ProcessData(string input)
    {
        Console.WriteLine($"Processing: {input}");

        instanceCount++;
        Console.WriteLine($"Instance count: {instanceCount}");
    }

    // VIOLATION E007: Async method without Async suffix
    public async Task<string> LoadData(string path)
    {
        Console.WriteLine($"Loading data from: {path}");
        var content = await File.ReadAllTextAsync(path);
        return content;
    }

    // VIOLATION: Missing XML documentation on public method
    public int Calculate(int a, int b)
    {
        return a + b;
    }

    // VIOLATION W004: String interpolation in logger calls
    // (Shown as comment since we don't have _logger here)
    // _logger.LogInformation($"Processing item {item.Id} at {DateTime.Now}");

    // VIOLATION: No nullable reference types awareness
    public string GetValue(string key)
    {
        // Could return null but method signature doesn't indicate it
        var dict = new Dictionary<string, string>();
        dict.TryGetValue(key, out var value);
        return value;
    }

    // VIOLATION: Synchronous I/O operation (should be async)
    public byte[] ReadFile(string path)
    {
        Console.WriteLine("Reading file synchronously");
        return File.ReadAllBytes(path);
    }
}

// VIOLATION E001: Interface not starting with 'I' prefix (convention)
public interface dataProcessor
{
    void Process();
}

} // end block-scoped namespace
