# Git Workflow Standards

> Reference: Issue #2 — Phase 2: Development Standards and Conventions

This document defines the Git workflow, branching strategy, commit conventions, and PR process.

---

## 1. Branch Strategy

### 1.1 Branch Types

| Branch | Pattern | Purpose | Lifetime |
|--------|---------|---------|----------|
| `main` | `main` | Production-ready code | Permanent |
| Feature | `feature/<issue-id>-<description>` | New features | Until merged |
| Bugfix | `fix/<issue-id>-<description>` | Bug fixes | Until merged |
| Docs | `docs/<issue-id>-<description>` | Documentation changes | Until merged |
| Refactor | `refactor/<issue-id>-<description>` | Code refactoring | Until merged |

### 1.2 Branch Naming Examples

```
feature/4-excel-cell-parser
fix/12-font-fallback-crash
docs/2-update-development-standards
refactor/15-extract-rendering-service
```

### 1.3 Rules

| Rule | Description |
|------|-------------|
| No direct commits to `main` | All changes go through PRs |
| Branch from `main` | Always create branches from latest `main` |
| One issue per branch | Each branch addresses one issue |
| Delete after merge | Clean up merged branches |

---

## 2. Commit Message Format

### 2.1 Conventional Commits

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### 2.2 Types

| Type | Description | Example |
|------|-------------|---------|
| `feat` | New feature | `feat(excel): add cell formatting parser` |
| `fix` | Bug fix | `fix(pdf): correct font size calculation` |
| `docs` | Documentation | `docs(wiki): update Excel Specification` |
| `style` | Code style (no logic change) | `style(core): fix indentation` |
| `refactor` | Code refactoring | `refactor(pdf): extract border renderer` |
| `perf` | Performance improvement | `perf(excel): optimize large file parsing` |
| `test` | Adding/updating tests | `test(excel): add cell parser unit tests` |
| `chore` | Build/tooling/CI changes | `chore(ci): add GitHub Actions workflow` |

### 2.3 Scopes

| Scope | Description |
|-------|-------------|
| `excel` | Excel parsing code |
| `pdf` | PDF rendering code |
| `core` | Core models and shared code |
| `tests` | Test projects |
| `ci` | CI/CD configuration |
| `docs` | Documentation |
| `wiki` | Wiki content |

### 2.4 Rules

| Rule | Description |
|------|-------------|
| Lowercase description | `fix(pdf): correct margins` not `Fix(Pdf): Correct Margins` |
| No period at end | `feat(excel): add parser` not `feat(excel): add parser.` |
| Imperative mood | `add` not `added` or `adds` |
| Reference issues | Include `#issue-id` in body or footer |
| Max 72 chars subject | Keep subject line concise |

### 2.5 Examples

```
feat(excel): add cell data type detection (#4)

Implement detection for all 7 cell data types as defined in
Excel Specification Section 2.

- String, Number, DateTime, Boolean handled
- Formula cells evaluate to result type
- Error cells preserve error string
- Blank cells return null

Ref: Wiki/Excel-Specification#Cell-Data-Types
Closes #4
```

---

## 3. Pull Request Process

### 3.1 PR Requirements

| Requirement | Description |
|-------------|-------------|
| Issue reference | PR must reference an Issue (`Closes #N` or `Ref #N`) |
| Description | Clear description of changes and rationale |
| Tests | All new code must have tests |
| CI passing | All GitHub Actions checks must pass |
| Review approval | At least 1 approval required |
| No conflicts | Must be mergeable with `main` |
| Conventional title | PR title follows commit convention |

### 3.2 PR Title Format

```
<type>(<scope>): <description> (#issue-id)
```

Examples:
```
feat(excel): implement cell parser (#4)
fix(pdf): correct line break rendering (#12)
docs(wiki): add Git Workflow Standards (#2)
```

### 3.3 Merge Strategy

| Strategy | When |
|----------|------|
| Squash and merge | Default — keeps `main` history clean |
| Merge commit | Large features with meaningful commit history |
| Rebase and merge | Small, atomic changes |

---

## 4. Issue Management

### 4.1 Issue Labels

| Label | Color | Description |
|-------|-------|-------------|
| `bug` | Red | Something isn't working |
| `enhancement` | Green | New feature or improvement |
| `documentation` | Blue | Documentation changes |
| `question` | Purple | Further information requested |
| `phase-1` through `phase-6` | Various | Phase tracking |
| `priority:high` | Red | Urgent |
| `priority:medium` | Yellow | Normal priority |
| `priority:low` | Green | Nice to have |

### 4.2 Issue Lifecycle

```
Open → In Progress → In Review → Closed
  │                      │
  └── Won't Fix ←────────┘
```

---

## 5. Code Review Standards

### 5.1 Reviewer Checklist

- [ ] Code follows Development Standards naming conventions
- [ ] All public members have XML documentation
- [ ] File-scoped namespaces used
- [ ] Pattern matching preferred over switch statements
- [ ] No `Console.WriteLine` — uses `ILogger<T>`
- [ ] Async methods have `Async` suffix and `CancellationToken`
- [ ] No static mutable state
- [ ] Dependency injection used (no service locator)
- [ ] Unit tests included with >80% coverage
- [ ] xUnit + FluentAssertions used
- [ ] Test naming follows `Method_Scenario_Expected` pattern
- [ ] Commit messages follow conventional format
- [ ] Issue referenced in PR

### 5.2 AI Review Criteria

The automated AI review (Phase 3) will check:

1. **Naming violations** — PascalCase/camelCase/prefixes
2. **Missing documentation** — Public members without XML docs
3. **Style violations** — Block namespaces, traditional switch
4. **Anti-patterns** — Console.WriteLine, string interpolation in logs, service locator
5. **Test coverage** — Missing tests for new public methods
6. **Commit format** — Non-conventional commit messages

---

> **AI Context Note**: When reviewing PRs, check all items in the reviewer checklist (Section 5.1). Reference specific standards violations using `Ref: Wiki/Development-Standards#Section-N` or `Ref: Wiki/Git-Workflow-Standards#Section-N`.
