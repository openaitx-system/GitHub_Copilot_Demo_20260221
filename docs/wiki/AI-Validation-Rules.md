# AI Validation Rules

> Reference: Issue #2 — Phase 2: Development Standards and Conventions

This document defines the rules used by automated AI validation in GitHub Actions for PR review.

---

## 1. Overview

AI validation runs as a GitHub Actions workflow on every PR. It uses GitHub Copilot to analyze the code diff against project policies defined in this Wiki.

### 1.1 Validation Pipeline

```
PR Opened/Updated
       │
       ▼
  Fetch PR Diff
       │
       ▼
  Load Policy Rules (from docs/wiki/)
       │
       ▼
  AI Analysis (Copilot)
       │
       ▼
  Post Review Comment
       │
       ▼
  Set Check Status (pass/fail)
```

---

## 2. Rule Categories

### 2.1 Error-Level Rules (Block Merge)

| ID | Rule | Source |
|----|------|--------|
| E001 | Class/interface names must be PascalCase | Development-Standards#2 |
| E002 | Private fields must use _camelCase prefix | Development-Standards#2 |
| E003 | Public methods must have XML documentation | Development-Standards#5 |
| E004 | Must use file-scoped namespaces | Development-Standards#3 |
| E005 | No `Console.WriteLine` in library code | Development-Standards#7 |
| E006 | No static mutable state | Development-Standards#6 |
| E007 | Async methods must have `Async` suffix | Development-Standards#4 |
| E008 | Must handle all 7 Excel cell data types | Excel-Specification#2 |
| E009 | PDF output must use correct page dimensions | PDF-Specification#2 |
| E010 | Must use `ILogger<T>` for logging | Development-Standards#7 |

### 2.2 Warning-Level Rules (Advisory)

| ID | Rule | Source |
|----|------|--------|
| W001 | Commit message should follow conventional format | Git-Workflow-Standards#2 |
| W002 | PR should reference an Issue number | Git-Workflow-Standards#3 |
| W003 | Prefer pattern matching over switch statements | Development-Standards#4 |
| W004 | Use structured logging (no string interpolation) | Development-Standards#7 |
| W005 | Test coverage should be > 80% | Development-Standards (Testing) |
| W006 | Complex methods (>20 lines) should have comments | Code-Review-Policy#2 |
| W007 | Avoid LINQ in hot paths | Development-Standards#9 |

---

## 3. Rule Definitions

### E001: PascalCase Names

```
Pattern: Class, struct, interface, enum, method, property declarations
Check: Name starts with uppercase (interfaces with 'I' prefix)
Fix: Rename to PascalCase
```

### E002: Private Field Prefix

```
Pattern: Private field declarations
Check: Name starts with '_' followed by lowercase
Fix: Add '_' prefix and use camelCase
```

### E003: XML Documentation

```
Pattern: Public class, interface, method, property declarations
Check: Preceded by /// <summary> block
Fix: Add XML documentation comment
```

### E004: File-Scoped Namespace

```
Pattern: namespace declarations
Check: Uses ';' not '{' after namespace
Fix: Convert to file-scoped namespace
```

### E005: No Console.WriteLine

```
Pattern: Console.Write, Console.WriteLine, Console.Error
Check: Not present in src/ files
Fix: Replace with ILogger<T> call
```

### E006: No Static Mutable State

```
Pattern: static fields that are not readonly/const
Check: No 'static' non-readonly fields
Fix: Use instance fields with DI
```

---

## 4. Validation Output Schema

```json
{
  "summary": {
    "errors": 2,
    "warnings": 1,
    "passed": 8,
    "status": "fail"
  },
  "violations": [
    {
      "id": "E001",
      "level": "error",
      "file": "src/ExcelToPdf/Parsing/cellParser.cs",
      "line": 15,
      "message": "Class name 'cellParser' should be PascalCase: 'CellParser'",
      "rule_source": "Development-Standards#2",
      "suggested_fix": "Rename class to 'CellParser'"
    }
  ]
}
```

---

## 5. Bypass Conditions

| Condition | Behavior |
|-----------|----------|
| PR labeled `skip-ai-review` | Skip AI validation |
| PR only changes `.md` files | Skip code rules, only check docs formatting |
| PR only changes `tests/` | Relax documentation requirements |
| Draft PR | Run validation but don't block |

---

> **AI Context Note**: Use the rules in Section 2 as a checklist when reviewing PR diffs. Output results in the schema defined in Section 4. Apply bypass conditions from Section 5.
