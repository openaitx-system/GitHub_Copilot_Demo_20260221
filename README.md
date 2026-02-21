# GitHub Copilot Demo 20260221

A demonstration project for **GitHub AI-Driven Development** workflows.

## Overview

This project showcases how to integrate GitHub Copilot and AI-powered automation into a real-world .NET development workflow, including:

- **GitHub Wiki as RAG Knowledge Base** — Centralized specifications (Excel & PDF standards) for AI-assisted development
- **Development Standards** — Coding conventions, commit policies, and review guidelines
- **Automated PR & Issue AI Validation** — GitHub Copilot-powered code review against company policies
- **Excel-to-PDF .NET Library** — A library for converting Excel files to PDF with basic text and line-break support
- **Benchmark & Review Pipeline** — Automated quality gates with performance benchmarks
- **Policy Violation Examples** — A reference project demonstrating common policy errors for training purposes

## Project Structure

```
├── docs/                    # Documentation and specifications
├── src/                     # Source code
│   └── ExcelToPdf/          # Excel-to-PDF .NET library
├── tests/                   # Unit and benchmark tests
├── .github/                 # GitHub Actions, issue templates, PR templates
│   ├── workflows/           # CI/CD and AI validation workflows
│   ├── ISSUE_TEMPLATE/      # Issue templates
│   └── PULL_REQUEST_TEMPLATE.md
└── wiki/                    # Wiki source content (synced to GitHub Wiki)
```

## Getting Started

> Prerequisites: .NET 8.0 SDK, GitHub CLI (`gh`)

```bash
# Clone the repository
git clone https://github.com/openaitx-system/GitHub_Copilot_Demo_20260221.git
cd GitHub_Copilot_Demo_20260221

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Issue-Driven Development

All features are tracked via GitHub Issues. Reference issues in commits and PRs using `#issue_id` for full traceability.

## License

MIT
