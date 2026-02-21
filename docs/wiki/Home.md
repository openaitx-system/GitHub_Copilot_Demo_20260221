# GitHub Copilot Demo - Knowledge Base

Welcome to the **GitHub Copilot Demo** Wiki — a centralized knowledge base for AI-assisted development.

This Wiki serves as a **RAG-like (Retrieval-Augmented Generation) knowledge source** for GitHub Copilot, providing specifications and standards that guide AI code generation.

---

## 📋 Table of Contents

### Specifications
- [[Excel Specification]] — Excel file format rules, data types, cell conventions, and styling standards
- [[PDF Specification]] — PDF layout standards, font rules, page structure, and rendering guidelines

### Standards
- [[Development Standards]] — Coding conventions, naming rules, and architecture guidelines
- [[Git Workflow Standards]] — Branch naming, commit message format, PR/Issue conventions
- [[Code Review Policy]] — What reviewers should check, AI-assisted review criteria

### AI Integration
- [[Copilot Context Guide]] — How to use this Wiki as context for GitHub Copilot
- [[AI Validation Rules]] — Rules that automated AI checks enforce on PRs

---

## 🔍 How to Use This Wiki as RAG Context

1. **Reference Wiki pages in Issues/PRs** — Use `[[Page Name]]` links to point Copilot to relevant specs
2. **Include spec keywords in code comments** — e.g., `// See Wiki: Excel Specification - Cell Formatting`
3. **Use `.github/copilot-instructions.md`** — Configure Copilot to reference this Wiki automatically

---

## 🏷️ Conventions

| Convention | Format |
|------------|--------|
| Page names | PascalCase with spaces (e.g., `Excel Specification`) |
| Code blocks | Always specify language (e.g., ` ```csharp `) |
| Tables | Used for structured data, always with headers |
| Cross-references | Use `[[Page Name]]` wiki links |

---

> **Tip**: This Wiki is version-controlled. Clone it with:
> ```bash
> git clone https://github.com/openaitx-system/GitHub_Copilot_Demo_20260221.wiki.git
> ```
