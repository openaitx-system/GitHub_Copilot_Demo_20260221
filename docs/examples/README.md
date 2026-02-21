# Policy Violation Examples Guide

This document explains intentional policy violations and their corrections, demonstrating how the AI validation pipeline enforces coding standards.

## File Reference

| File | Purpose |
|------|---------|
| `docs/examples/PolicyViolations.cs` | Intentional violations for testing |
| `docs/examples/PolicyViolationsFixed.cs` | Corrected version following standards |
| `.github/scripts/validate-policy.py` | Policy validation script |

## Violations and Fixes

### E001: Naming Convention — PascalCase Required

**Violation:**
```csharp
public class badExampleService  // lowercase start
public interface dataProcessor  // missing 'I' prefix, lowercase
```

**Fix:**
```csharp
public class GoodExampleService  // PascalCase
public interface IDataProcessor  // 'I' prefix + PascalCase
```

**Rule Source:** Development-Standards#2

---

### E002: Private Field Naming — _camelCase Required

**Violation:**
```csharp
private readonly string ConnectionString;  // PascalCase, no underscore
```

**Fix:**
```csharp
private readonly string _connectionString;  // _camelCase prefix
```

**Rule Source:** Development-Standards#2

---

### E004: File-Scoped Namespaces Required

**Violation:**
```csharp
namespace ExcelToPdf.Examples.Violations {
    // block-scoped with curly braces
}
```

**Fix:**
```csharp
namespace ExcelToPdf.Examples.Corrected;  // file-scoped with semicolon
```

**Rule Source:** Development-Standards#3

---

### E005: No Console.WriteLine in Library Code

**Violation:**
```csharp
Console.WriteLine($"Processing: {input}");
```

**Fix:**
```csharp
_logger.LogInformation("Processing: {Input}", input);
```

**Rule Source:** Development-Standards#7. Use `ILogger<T>` for all logging.

---

### E006: No Static Mutable State

**Violation:**
```csharp
private static int instanceCount = 0;  // mutable static field
```

**Fix:**
```csharp
private static readonly int _maxRetryCount = 3;  // immutable with readonly
```

**Rule Source:** Development-Standards#6. Use dependency injection instead.

---

### E007: Async Methods Must End with "Async" Suffix

**Violation:**
```csharp
public async Task<string> LoadData(string path)
```

**Fix:**
```csharp
public async Task<string> LoadDataAsync(string path, CancellationToken cancellationToken = default)
```

**Rule Source:** Development-Standards#4

---

### W004: Use Structured Logging (Warning)

**Violation:**
```csharp
_logger.LogInformation($"Processing item {item.Id} at {DateTime.Now}");
```

**Fix:**
```csharp
_logger.LogInformation("Processing item {ItemId} at {Timestamp}", item.Id, DateTime.Now);
```

**Rule Source:** Development-Standards#7. Structured logging enables log analysis.

---

## Additional Standards (Not Checked by Script)

| Standard | Description |
|----------|-------------|
| XML Documentation | All public members must have `///` comments |
| Nullable References | Use `#nullable enable` and proper null annotations |
| Async I/O | All I/O operations must be async |
| Clean Architecture | Separate concerns: Parsing → Model → Rendering |
| Unit Tests | >80% coverage with xUnit + FluentAssertions |

## How to Run Policy Validation

```bash
# Generate diff and validate
git diff main..HEAD | python .github/scripts/validate-policy.py --diff -
```

The validation script runs automatically on PRs via `.github/workflows/ai-policy-review.yml`.
