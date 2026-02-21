# Copilot Context Guide

> Reference: Issue #1 — Phase 1: GitHub Wiki as RAG Knowledge Base

This guide explains how to use the GitHub Wiki as a knowledge source for GitHub Copilot and AI-assisted development.

---

## 1. Overview

The Wiki serves as a **RAG (Retrieval-Augmented Generation) knowledge base** — a structured collection of specifications, standards, and policies that AI tools can reference during code generation and review.

### Why Wiki as RAG?

| Benefit | Description |
|---------|-------------|
| Version controlled | Wiki is a Git repo, all changes are tracked |
| Searchable | GitHub search indexes Wiki content |
| Linkable | Reference specific sections via `[[Page Name]]` |
| AI-friendly | Markdown format is optimal for LLM consumption |
| Long-term memory | Issues reference Wiki pages via `#issue_id`, building context over time |

---

## 2. How to Reference Wiki in Code

### 2.1 Code Comments

Add structured comments to link code to Wiki specifications:

```csharp
// Ref: Wiki/Excel-Specification#Cell-Data-Types
// Handles all 7 data types as defined in the specification
public object ReadCellValue(ICell cell)
{
    return cell.CellType switch
    {
        CellType.String => cell.StringCellValue,
        CellType.Numeric => cell.NumericCellValue,
        CellType.Boolean => cell.BooleanCellValue,
        // ... per specification
    };
}
```

### 2.2 Issue References

When creating Issues or PRs, reference Wiki pages:

```markdown
## Context
This PR implements cell formatting as defined in [[Excel Specification#Cell-Formatting-Rules]].

## Changes
- Added bold/italic support (Ref: Wiki/Excel-Specification#Text-Formatting)
- Added border rendering (Ref: Wiki/PDF-Specification#Border-Rendering)
```

---

## 3. Copilot Instructions File

The `.github/copilot-instructions.md` file configures GitHub Copilot's behavior for this repository:

```markdown
# Copilot Instructions

## Project Context
This is an Excel-to-PDF conversion library built with .NET 8.0.

## Key Specifications (GitHub Wiki)
- Excel handling: See Wiki page "Excel Specification"
- PDF output: See Wiki page "PDF Specification"
- Code standards: See Wiki page "Development Standards"

## Rules
1. All cell data types must be handled per Excel Specification Section 2
2. PDF output must comply with PDF Specification Section 9
3. Follow C# coding standards in Development Standards
4. All public methods must have XML documentation
5. All new code must have unit tests with >80% coverage
```

---

## 4. Wiki Page Structure for AI Parsing

Each Wiki page follows a consistent structure optimized for AI consumption:

```markdown
# Page Title

> Reference: Issue #N — Context

## 1. Section with Rules Table
| Rule | Description |
|------|-------------|
| ...  | ...         |

## 2. Section with Code Examples
` ` `csharp
// Example code
` ` `

## N. Error Handling / Edge Cases
| Scenario | Behavior |
|----------|----------|
| ...      | ...      |

> **AI Context Note**: Instructions for AI tools
```

### Key Principles:
- **Tables for structured data** — AI can parse tables reliably
- **Code blocks with language tags** — enables syntax-aware suggestions
- **Numbered sections** — allows precise references like `#Section-3`
- **AI Context Notes** — explicit instructions at the end of each page

---

## 5. Building Long-Term Memory with Issues

### The Pattern

```
Issue #1 → References Wiki/Excel-Specification
  ├── PR #5 → Implements cell parsing (links to #1)
  ├── PR #8 → Fixes edge case (links to #1, updates Wiki)
  └── Comment → Documents decision rationale

Issue #2 → References Wiki/PDF-Specification
  ├── PR #10 → Implements page layout (links to #2)
  └── Comment → Links to #1 for cross-reference
```

### Benefits
- Every decision is traceable via Issue → PR → Commit chain
- AI tools can follow `#issue_id` references to understand context
- Wiki updates linked to Issues create a living specification

---

## 6. Quick Reference

| Action | How |
|--------|-----|
| Reference a spec in code | `// Ref: Wiki/Page-Name#Section` |
| Reference a spec in Issue/PR | `See [[Page Name#Section]]` |
| Link Issue to Wiki context | Include Wiki links in Issue body |
| Update spec after implementation | Edit Wiki page, reference Issue in commit |
| Configure Copilot | Edit `.github/copilot-instructions.md` |

---

> **AI Context Note**: This page itself serves as a meta-guide. When assisting with this project, always check the relevant Wiki specification pages before generating code.
