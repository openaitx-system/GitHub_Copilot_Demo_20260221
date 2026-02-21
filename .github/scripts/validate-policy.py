#!/usr/bin/env python3
"""
Policy Validation Script for PR Code Review.

Analyzes a PR diff against project policies defined in docs/wiki/.
Outputs a Markdown review comment with violations and suggestions.

Reference: Issue #3 - Phase 3: PR/Issue Auto AI Validation
Policies: docs/wiki/AI-Validation-Rules.md
"""

import argparse
import re
import sys
from pathlib import Path
from dataclasses import dataclass, field


@dataclass
class Violation:
    """Represents a policy violation found in the diff."""
    rule_id: str
    level: str  # "error" or "warning"
    file: str
    line: int
    message: str
    rule_source: str
    suggested_fix: str = ""


@dataclass
class ValidationResult:
    """Aggregated validation results."""
    violations: list = field(default_factory=list)
    passed_checks: list = field(default_factory=list)


def parse_diff(diff_text: str) -> list:
    """Parse unified diff into file changes with line numbers."""
    files = []
    current_file = None
    current_line = 0

    for line in diff_text.split("\n"):
        # New file in diff
        if line.startswith("+++ b/"):
            current_file = line[6:]
            continue
        # Hunk header
        if line.startswith("@@"):
            match = re.search(r"\+(\d+)", line)
            if match:
                current_line = int(match.group(1))
            continue
        # Added line
        if line.startswith("+") and not line.startswith("+++"):
            if current_file:
                files.append({
                    "file": current_file,
                    "line": current_line,
                    "content": line[1:],  # Remove leading +
                })
            current_line += 1
        elif not line.startswith("-"):
            current_line += 1

    return files


def check_naming_conventions(changes: list) -> list:
    """Check E001/E002: Naming conventions."""
    violations = []

    for change in changes:
        content = change["content"]
        file_path = change["file"]

        # Skip non-C# files
        if not file_path.endswith(".cs"):
            continue

        # Skip test files for some rules
        is_test = "/tests/" in file_path.lower() or file_path.lower().endswith("tests.cs")

        # E001: Class names must be PascalCase
        class_match = re.search(r"(?:public|internal|private|protected)\s+(?:static\s+)?(?:abstract\s+)?(?:sealed\s+)?class\s+([a-z]\w*)", content)
        if class_match:
            violations.append(Violation(
                rule_id="E001",
                level="error",
                file=file_path,
                line=change["line"],
                message=f"Class name '{class_match.group(1)}' should be PascalCase",
                rule_source="Development-Standards#2",
                suggested_fix=f"Rename to '{class_match.group(1)[0].upper() + class_match.group(1)[1:]}'"
            ))

        # E002: Private fields must use _camelCase
        field_match = re.search(r"private\s+(?:readonly\s+)?(?:static\s+)?\w+\s+([A-Z]\w*)\s*[;=]", content)
        if field_match and not field_match.group(1).startswith("_"):
            violations.append(Violation(
                rule_id="E002",
                level="error",
                file=file_path,
                line=change["line"],
                message=f"Private field '{field_match.group(1)}' should use _camelCase prefix",
                rule_source="Development-Standards#2",
                suggested_fix=f"Rename to '_{field_match.group(1)[0].lower() + field_match.group(1)[1:]}'"
            ))

    return violations


def check_file_scoped_namespaces(changes: list) -> list:
    """Check E004: File-scoped namespaces."""
    violations = []

    for change in changes:
        if not change["file"].endswith(".cs"):
            continue

        # Check for block-scoped namespace (namespace followed by {)
        if re.search(r"^namespace\s+[\w.]+\s*$", change["content"].rstrip()):
            # This could be file-scoped (ends with ;) or block-scoped (next line is {)
            pass  # Need multi-line context for this check

        if re.search(r"^namespace\s+[\w.]+\s*\{", change["content"]):
            violations.append(Violation(
                rule_id="E004",
                level="error",
                file=change["file"],
                line=change["line"],
                message="Use file-scoped namespace (ending with ';') instead of block-scoped namespace",
                rule_source="Development-Standards#3",
                suggested_fix="Change 'namespace X {' to 'namespace X;'"
            ))

    return violations


def check_console_writeline(changes: list) -> list:
    """Check E005: No Console.WriteLine in library code."""
    violations = []

    for change in changes:
        file_path = change["file"]

        # Only check src/ files (not tests)
        if not file_path.startswith("src/") or not file_path.endswith(".cs"):
            continue

        if re.search(r"Console\.(Write|WriteLine|Error)", change["content"]):
            violations.append(Violation(
                rule_id="E005",
                level="error",
                file=file_path,
                line=change["line"],
                message="Console.WriteLine found in library code. Use ILogger<T> instead",
                rule_source="Development-Standards#7",
                suggested_fix="Replace with _logger.LogInformation() or appropriate log level"
            ))

    return violations


def check_static_mutable_state(changes: list) -> list:
    """Check E006: No static mutable state."""
    violations = []

    for change in changes:
        if not change["file"].endswith(".cs"):
            continue

        # Static non-readonly, non-const fields
        if re.search(r"(?:private|public|internal|protected)\s+static\s+(?!readonly|const)\w+\s+\w+\s*[;=]", change["content"]):
            violations.append(Violation(
                rule_id="E006",
                level="error",
                file=change["file"],
                line=change["line"],
                message="Static mutable state detected. Use instance fields with dependency injection",
                rule_source="Development-Standards#6",
                suggested_fix="Make field 'static readonly' or convert to instance field"
            ))

    return violations


def check_async_suffix(changes: list) -> list:
    """Check E007: Async methods must have Async suffix."""
    violations = []

    for change in changes:
        if not change["file"].endswith(".cs"):
            continue

        # async method without Async suffix
        match = re.search(r"async\s+(?:Task|ValueTask)(?:<\w+>)?\s+(\w+)\s*\(", change["content"])
        if match:
            method_name = match.group(1)
            if not method_name.endswith("Async") and method_name != "Main":
                violations.append(Violation(
                    rule_id="E007",
                    level="error",
                    file=change["file"],
                    line=change["line"],
                    message=f"Async method '{method_name}' must end with 'Async' suffix",
                    rule_source="Development-Standards#4",
                    suggested_fix=f"Rename to '{method_name}Async'"
                ))

    return violations


def check_logging_patterns(changes: list) -> list:
    """Check W004: Use structured logging."""
    violations = []

    for change in changes:
        if not change["file"].endswith(".cs"):
            continue

        # String interpolation in logger calls
        if re.search(r'_logger\.Log\w+\(\$"', change["content"]):
            violations.append(Violation(
                rule_id="W004",
                level="warning",
                file=change["file"],
                line=change["line"],
                message="Use structured logging instead of string interpolation in log messages",
                rule_source="Development-Standards#7",
                suggested_fix='Use _logger.LogInformation("Message {Param}", value) format'
            ))

    return violations


def check_xml_documentation(changes: list) -> list:
    """Check E003: Public methods must have XML documentation."""
    violations = []

    for change in changes:
        if not change["file"].endswith(".cs"):
            continue

        # Public method/class/property without preceding /// summary
        # This is a simplified check - full check would need multi-line context
        if re.search(r"^\s*public\s+(?!class|interface|enum|struct)", change["content"]):
            # We can't fully check this without context, so we flag it as advisory
            pass

    return violations


def run_validation(diff_text: str, policies_dir: str) -> ValidationResult:
    """Run all validation checks against the diff."""
    result = ValidationResult()

    changes = parse_diff(diff_text)

    if not changes:
        result.passed_checks.append("No code changes to validate")
        return result

    # Run all checks
    all_checks = [
        ("Naming Conventions (E001/E002)", check_naming_conventions),
        ("File-Scoped Namespaces (E004)", check_file_scoped_namespaces),
        ("No Console.WriteLine (E005)", check_console_writeline),
        ("No Static Mutable State (E006)", check_static_mutable_state),
        ("Async Suffix (E007)", check_async_suffix),
        ("Structured Logging (W004)", check_logging_patterns),
    ]

    for check_name, check_fn in all_checks:
        violations = check_fn(changes)
        if violations:
            result.violations.extend(violations)
        else:
            result.passed_checks.append(check_name)

    return result


def format_review_comment(result: ValidationResult) -> str:
    """Format validation results as a Markdown PR comment."""
    errors = [v for v in result.violations if v.level == "error"]
    warnings = [v for v in result.violations if v.level == "warning"]

    status = "FAIL" if errors else "PASS"
    status_icon = "❌" if errors else "✅"

    lines = [
        f"## {status_icon} AI Policy Review Results",
        "",
        f"**Status**: {status} | **Errors**: {len(errors)} | **Warnings**: {len(warnings)} | **Passed**: {len(result.passed_checks)}",
        "",
    ]

    if errors:
        lines.append("### ❌ Violations Found")
        lines.append("")
        for i, v in enumerate(errors, 1):
            lines.append(f"{i}. **[ERROR] {v.rule_id}** — `{v.rule_source}`")
            lines.append(f"   - File: `{v.file}` (line {v.line})")
            lines.append(f"   - Issue: {v.message}")
            if v.suggested_fix:
                lines.append(f"   - Fix: {v.suggested_fix}")
            lines.append("")

    if warnings:
        lines.append("### ⚠️ Warnings")
        lines.append("")
        for i, v in enumerate(warnings, 1):
            lines.append(f"{i}. **[WARN] {v.rule_id}** — `{v.rule_source}`")
            lines.append(f"   - File: `{v.file}` (line {v.line})")
            lines.append(f"   - Issue: {v.message}")
            if v.suggested_fix:
                lines.append(f"   - Fix: {v.suggested_fix}")
            lines.append("")

    if result.passed_checks:
        lines.append("### ✅ Passed Checks")
        lines.append("")
        for check in result.passed_checks:
            lines.append(f"- {check}")
        lines.append("")

    lines.append("---")
    lines.append("*Automated review by AI Policy Validator. See [AI Validation Rules](https://github.com/openaitx-system/GitHub_Copilot_Demo_20260221/wiki/AI-Validation-Rules) for details.*")

    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description="Validate PR diff against project policies")
    parser.add_argument("--diff", required=True, help="Path to the diff file")
    parser.add_argument("--policies", required=True, help="Path to policies directory")
    parser.add_argument("--output", required=True, help="Path to output review comment file")
    args = parser.parse_args()

    # Read diff
    diff_path = Path(args.diff)
    if not diff_path.exists():
        print(f"Error: Diff file not found: {args.diff}", file=sys.stderr)
        sys.exit(1)

    diff_text = diff_path.read_text(encoding="utf-8")

    # Run validation
    result = run_validation(diff_text, args.policies)

    # Format and write output
    comment = format_review_comment(result)
    Path(args.output).write_text(comment, encoding="utf-8")

    # Print summary
    errors = [v for v in result.violations if v.level == "error"]
    warnings = [v for v in result.violations if v.level == "warning"]
    print(f"Validation complete: {len(errors)} errors, {len(warnings)} warnings, {len(result.passed_checks)} passed")

    # Exit with error if violations found
    if errors:
        sys.exit(1)


if __name__ == "__main__":
    main()
