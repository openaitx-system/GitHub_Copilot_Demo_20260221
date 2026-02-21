# Copilot Instructions for GitHub_Copilot_Demo_20260221

## Project Context

This is an **Excel-to-PDF conversion library** built with **.NET 8.0** (C#). The project follows strict coding standards and AI-driven development practices.

## Key Specifications (GitHub Wiki)

When generating or reviewing code, always reference these specifications:

- **Excel handling**: See Wiki page "Excel Specification" for cell data types, formatting rules, and line break handling
- **PDF output**: See Wiki page "PDF Specification" for layout, font standards, and rendering requirements
- **Code standards**: See Wiki page "Development Standards" for naming conventions and architecture rules

## Code Generation Rules

1. **Cell Data Types**: All 7 Excel cell data types (String, Number, DateTime, Boolean, Formula, Error, Blank) must be handled per Excel Specification Section 2
2. **PDF Compliance**: Output must target PDF 1.7 (ISO 32000-1:2008) per PDF Specification Section 1
3. **Line Breaks**: Handle explicit `\n`, `\r\n`, and word wrap per Excel Specification Section 5
4. **Font Fallback**: Always implement font fallback chain per PDF Specification Section 3.2
5. **Error Handling**: Use specific exception types per PDF Specification Section 10

## C# Coding Standards

- Use **file-scoped namespaces**
- Use **PascalCase** for public members, **_camelCase** for private fields
- All public methods must have **XML documentation comments**
- Use **nullable reference types** (`#nullable enable`)
- Prefer **pattern matching** and **switch expressions**
- Use **`ILogger<T>`** for logging (no `Console.WriteLine` in library code)
- Async methods must end with **`Async`** suffix

## Testing Standards

- All new code must have unit tests with **>80% coverage**
- Use **xUnit** as the test framework
- Use **FluentAssertions** for assertion syntax
- Test method naming: `MethodName_Scenario_ExpectedResult`
- Each test must be independent (no shared mutable state)

## Architecture

- Follow **Clean Architecture** principles
- Separate concerns: Parsing (Excel) → Model → Rendering (PDF)
- Use **dependency injection** for all services
- No static mutable state
- All I/O operations must be async

## Commit Message Format

```
type(scope): description

Types: feat, fix, docs, style, refactor, perf, test, chore
Scope: excel, pdf, core, tests, ci, docs
```

## PR Requirements

- Reference Issue number in PR title or body
- All CI checks must pass
- At least one approval required
- No direct commits to `main` branch
