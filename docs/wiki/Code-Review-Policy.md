# Code Review Policy

> Reference: Issue #2 — Phase 2: Development Standards and Conventions

This document defines the code review process, automated checks, and quality gates.

---

## 1. Review Process

### 1.1 Review Flow

```
Developer creates PR
        │
        ▼
  Automated Checks ──── CI Build + Tests
        │                    │
        ▼                    ▼
  AI Review (Copilot) ── Policy Compliance Check
        │
        ▼
  Human Review (1+ approvals)
        │
        ▼
  Merge to main
```

### 1.2 Automated Checks (Must Pass)

| Check | Tool | Blocking |
|-------|------|----------|
| Build | `dotnet build` | ✅ Yes |
| Unit Tests | `dotnet test` | ✅ Yes |
| Code Style | `.editorconfig` + analyzers | ✅ Yes |
| Test Coverage | > 80% line coverage | ✅ Yes |
| Commit Format | Conventional commits check | ⚠️ Warning |
| AI Policy Review | GitHub Copilot | ⚠️ Warning |

---

## 2. Review Checklist

### 2.1 Architecture

- [ ] Changes follow Clean Architecture (Parsing → Model → Rendering)
- [ ] No circular dependencies between layers
- [ ] New services registered in DI container
- [ ] Interfaces used for dependencies (not concrete types)

### 2.2 Code Quality

- [ ] No `Console.WriteLine` in library code
- [ ] No static mutable state
- [ ] No `Thread.Sleep` — use `Task.Delay` if needed
- [ ] No `async void` — only `async Task` or `async ValueTask`
- [ ] No string concatenation in loops — use `StringBuilder`
- [ ] No catching generic `Exception` without rethrowing

### 2.3 Security

- [ ] No hardcoded credentials or secrets
- [ ] No sensitive data in log messages
- [ ] File paths are validated/sanitized
- [ ] Input from external sources is validated

### 2.4 Documentation

- [ ] All public APIs have XML documentation
- [ ] Complex algorithms have inline comments
- [ ] Wiki updated if spec changes are needed
- [ ] CHANGELOG updated for user-facing changes

### 2.5 Testing

- [ ] New public methods have unit tests
- [ ] Edge cases tested (null, empty, boundary values)
- [ ] Tests are independent (no shared mutable state)
- [ ] Test names follow `Method_Scenario_Expected` pattern
- [ ] FluentAssertions used for assertions

---

## 3. AI-Assisted Review Rules

### 3.1 What AI Reviews

The GitHub Copilot automated review checks the PR diff against:

| Category | Source | Severity |
|----------|--------|----------|
| Naming conventions | [[Development Standards#Naming-Conventions]] | Error |
| File organization | [[Development Standards#File-Organization]] | Warning |
| Code style | [[Development Standards#Code-Style-Rules]] | Error |
| Documentation | [[Development Standards#XML-Documentation-Requirements]] | Error |
| DI usage | [[Development Standards#Dependency-Injection-Rules]] | Error |
| Logging | [[Development Standards#Logging-Standards]] | Warning |
| Error handling | [[Development Standards#Error-Handling]] | Warning |
| Git workflow | [[Git Workflow Standards#Commit-Message-Format]] | Warning |
| Excel spec compliance | [[Excel Specification]] | Error |
| PDF spec compliance | [[PDF Specification]] | Error |

### 3.2 AI Review Output Format

```markdown
## AI Policy Review Results

### ❌ Violations Found (N)

1. **[ERROR] Naming Convention** — `Development-Standards#2`
   - File: `src/ExcelToPdf/Parsing/cellParser.cs`
   - Line: 15
   - Issue: Class name `cellParser` should be PascalCase: `CellParser`

2. **[ERROR] Missing Documentation** — `Development-Standards#5`
   - File: `src/ExcelToPdf/Parsing/CellParser.cs`
   - Line: 22
   - Issue: Public method `ParseCell()` missing XML documentation

### ⚠️ Warnings (N)

1. **[WARN] Commit Format** — `Git-Workflow-Standards#2`
   - Commit: `abc1234`
   - Issue: Missing scope in commit message: `add parser` → `feat(excel): add parser`

### ✅ Passed Checks (N)

- File-scoped namespaces: All files compliant
- DI pattern: No service locator usage detected
- Logging: Structured logging used correctly
```

---

## 4. Merge Requirements

| Requirement | Condition |
|-------------|-----------|
| CI Build | Must pass |
| All Tests | Must pass |
| Coverage | ≥ 80% on changed files |
| AI Review | No ERROR-level violations |
| Human Review | ≥ 1 approval |
| Conflicts | None |
| Branch | Up to date with `main` |

---

## 5. Post-Merge Actions

| Action | Automated |
|--------|-----------|
| Delete feature branch | ✅ Yes |
| Update Issue status | ✅ Yes (via `Closes #N`) |
| Run full test suite | ✅ Yes (CI on main) |
| Update Wiki if needed | ❌ Manual |
| Notify team | ✅ Yes (GitHub notifications) |

---

> **AI Context Note**: When performing code reviews, iterate through all items in Section 2 and produce output in the format defined in Section 3.2. Severity levels must match the table in Section 3.1.
