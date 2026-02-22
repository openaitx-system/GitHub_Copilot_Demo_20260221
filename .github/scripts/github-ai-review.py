#!/usr/bin/env python3
"""
GitHub AI Review Script for PR Code Review.

Reads PR diff and policy documents, calls GitHub Models inference API,
and writes a Markdown review comment.
"""

from __future__ import annotations

import argparse
import json
import os
import sys
from pathlib import Path
from typing import Iterable
from urllib import request, error


def read_policy_snippets(policies_dir: str, max_chars: int = 12000) -> str:
    """Load policy files and return a compact text snippet for prompting."""
    policy_path = Path(policies_dir)
    if not policy_path.exists():
        return "Policy directory not found."

    wiki_files = sorted(policy_path.rglob("*.md"))
    chunks: list[str] = []
    budget = max_chars

    for file in wiki_files:
        try:
            content = file.read_text(encoding="utf-8")
        except Exception:
            continue

        header = f"\n## {file.as_posix()}\n"
        if budget <= len(header):
            break

        allowance = min(len(content), budget - len(header))
        if allowance <= 0:
            break

        chunks.append(header + content[:allowance])
        budget -= len(header) + allowance
        if budget <= 0:
            break

    if not chunks:
        return "No policy markdown files were loaded."

    return "\n".join(chunks)


def truncate_diff(diff_text: str, max_chars: int = 50000) -> str:
    """Trim large diffs to stay within model request limits."""
    if len(diff_text) <= max_chars:
        return diff_text

    head = diff_text[: max_chars // 2]
    tail = diff_text[-(max_chars // 2) :]
    return (
        head
        + "\n\n... [DIFF TRUNCATED FOR LENGTH] ...\n\n"
        + tail
    )


def call_github_models(
    endpoint: str,
    token: str,
    model: str,
    messages: Iterable[dict[str, str]],
    temperature: float = 0.1,
) -> str:
    """Call GitHub Models endpoint and return generated review text."""
    payload = {
        "model": model,
        "messages": list(messages),
        "temperature": temperature,
    }

    data = json.dumps(payload).encode("utf-8")
    req = request.Request(
        endpoint,
        data=data,
        headers={
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json",
        },
        method="POST",
    )

    with request.urlopen(req, timeout=90) as response:
        raw = response.read().decode("utf-8")
        body = json.loads(raw)

    choices = body.get("choices", [])
    if not choices:
        raise RuntimeError("No choices returned by model endpoint")

    message = choices[0].get("message", {})
    content = message.get("content")
    if not content:
        raise RuntimeError("Empty content returned by model endpoint")

    return content.strip()


def build_prompt(diff_text: str, policy_text: str) -> tuple[str, str]:
    """Create system and user prompts for AI review."""
    system_prompt = (
        "You are a senior .NET code reviewer. Review pull request diffs against policy documents. "
        "Focus on concrete issues only. Be concise and practical. "
        "Output valid Markdown with sections: Summary, Critical Findings, Suggestions, Compliance Notes."
    )

    user_prompt = (
        "Review this pull request diff against the provided project policies.\n"
        "Rules:\n"
        "1) Report only findings supported by the diff.\n"
        "2) Use severity tags [HIGH]/[MED]/[LOW].\n"
        "3) Include file paths when possible.\n"
        "4) If no issues are found, explicitly state that and provide 2 focused quality suggestions.\n\n"
        "=== POLICIES ===\n"
        f"{policy_text}\n\n"
        "=== PR DIFF ===\n"
        f"{diff_text}\n"
    )

    return system_prompt, user_prompt


def format_comment(model: str, review_body: str) -> str:
    """Wrap model output as PR comment markdown."""
    lines = [
        "## 🤖 GitHub AI Review",
        "",
        f"Model: `{model}`",
        "",
        review_body.strip(),
        "",
        "---",
        "*Automated review by GitHub AI Models. Validate suggestions before applying.*",
    ]
    return "\n".join(lines)


def main() -> None:
    parser = argparse.ArgumentParser(description="Run GitHub AI review for PR diff")
    parser.add_argument("--diff", required=True, help="Path to the diff file")
    parser.add_argument("--policies", required=True, help="Path to policies directory")
    parser.add_argument("--output", required=True, help="Path to output markdown file")
    args = parser.parse_args()

    diff_path = Path(args.diff)
    if not diff_path.exists():
        print(f"Error: Diff file not found: {args.diff}", file=sys.stderr)
        sys.exit(1)

    token = os.getenv("GITHUB_TOKEN", "")
    endpoint = os.getenv("GITHUB_MODELS_ENDPOINT", "https://models.inference.ai.azure.com/chat/completions")
    model = os.getenv("GITHUB_MODELS_MODEL", "gpt-4.1-mini")

    if not token:
        print("Error: GITHUB_TOKEN is required", file=sys.stderr)
        sys.exit(1)

    diff_text = truncate_diff(diff_path.read_text(encoding="utf-8"))
    policy_text = read_policy_snippets(args.policies)
    system_prompt, user_prompt = build_prompt(diff_text, policy_text)

    try:
        review_body = call_github_models(
            endpoint=endpoint,
            token=token,
            model=model,
            messages=[
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_prompt},
            ],
        )
    except error.HTTPError as ex:
        detail = ex.read().decode("utf-8", errors="replace")
        review_body = (
            "AI review request failed.\n\n"
            f"- HTTP status: {ex.code}\n"
            f"- Endpoint: `{endpoint}`\n"
            f"- Detail: `{detail[:1000]}`\n"
        )
    except Exception as ex:
        review_body = (
            "AI review request failed.\n\n"
            f"- Endpoint: `{endpoint}`\n"
            f"- Error: `{str(ex)}`\n"
        )

    comment = format_comment(model=model, review_body=review_body)
    Path(args.output).write_text(comment, encoding="utf-8")

    print(f"AI review output written to {args.output}")


if __name__ == "__main__":
    main()
