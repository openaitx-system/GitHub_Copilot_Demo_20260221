# AGENTS.md

Shared operating guide for all AI/code agents working in this repository.

## Core Rules

1. **Use English only** for all generated content (code comments, docs, issue/PR text, commit messages).
2. **Work phase by phase**. Do not start the next phase until the current phase is completed and verified.
3. **Every feature must be GitHub issue-driven** for long-term traceability and memory.

## Why Issue-Driven Development

Issue-driven workflow provides durable project memory:
- Keeps full implementation context in one place.
- Allows quick backtracking using `#issue_ID`.
- Improves collaboration, review quality, and historical understanding.
- Connects requirements, code changes, and validation results.

## Mandatory Workflow (Per Feature)

For each feature/task, follow this exact sequence:

1. **Create/confirm issue first**
   - Open a GitHub issue before writing code.
   - Include: objective, scope, acceptance criteria, constraints, and phase label.

2. **Implement only within the current phase**
   - Keep scope minimal and aligned to the active phase.
   - Avoid cross-phase changes unless explicitly required.

3. **Reference issue everywhere**
   - Branch name (if used): `feature/<issue-id>-<short-name>`.
   - Commits should include issue reference (e.g., `Refs #123`).
   - Completion commit/PR should close issue (e.g., `Closes #123`).

4. **Validate before completion**
   - Run build/tests/lint/policy checks relevant to the change.
   - Record validation outcomes in issue comments or PR description.

5. **Close issue after acceptance**
   - Close only when acceptance criteria are met.
   - Post a concise summary with what changed, what was verified, and any follow-up.

## Phase Execution Policy

- Execute phases strictly in order.
- Each phase must have a tracking issue (or parent issue + child issues).
- Before starting the next phase, ensure:
  - Required deliverables are merged.
  - CI checks pass.
  - Related issue(s) are updated/closed.

## Issue Template (Minimum Content)

Each issue should include:
- **Title**: `[Phase X] <Feature Name>`
- **Goal**
- **Scope / Out of Scope**
- **Acceptance Criteria**
- **Implementation Notes**
- **Validation Plan**
- **Traceability**: links to related issues/PRs/wiki pages

## Agent Behavior Expectations

- Prefer small, reviewable changes.
- Do not invent requirements outside the issue scope.
- Keep architecture and coding standards consistent with repository rules.
- If ambiguity exists, ask for clarification or document assumptions in the issue.

## Definition of Done (Per Feature)

A feature is done only when all are true:
- Implemented according to issue scope.
- Tests/validation completed successfully.
- Documentation updated if needed.
- Changes linked to the issue.
- Issue closed with final summary.
