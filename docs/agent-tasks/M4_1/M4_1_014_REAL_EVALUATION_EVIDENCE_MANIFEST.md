# M4_1_014 — Real evaluation evidence manifest

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_014`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_when_real_evidence_exists`

Depends on:

```text
- User has run or provided real/manual M4.1 strict evaluation evidence.
- M4_1_013 runbook exists or equivalent user evidence instructions are clear.
- check-all is green.
```

Unlocks:

```text
- M4_1_015 real report import fixture guard.
- M4_1_016 gate closure decision if evidence is sufficient.
```

Risk level: medium, because real model evidence may contain large or sensitive raw output.

Expected changed files count: 2-5

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because this records evidence location/metadata.

Approval text required in NEXT_TASK.md:

```text
User approval: approved
```

## Source of truth

Source-of-truth docs:

```text
docs/M4_1_REAL_EVALUATION_RUNBOOK.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
```

Context budget:

```text
Read only this task spec, M4.1 runbook/current state, and user-provided evidence files explicitly named by the user.
```

Existing patterns to inspect:

```text
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/M4_1/M4_1_001_REAL_EVALUATION_REPORT_IMPORT.md
```

## File boundaries

Allowed files:

```text
docs/M4_1_REAL_EVALUATION_EVIDENCE_MANIFEST.md
docs/agent-tasks/M4_1/M4_1_014_REAL_EVALUATION_EVIDENCE_MANIFEST.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
.devflow/BLOCKERS.md
```

Allowed only if user provides a small redacted evidence excerpt:

```text
tests/fixtures/strict-llm-real-evaluation/**
```

Forbidden files:

```text
src/**
*.sln
*.csproj
.devflow/scripts/**
.llmgc/**  (do not commit raw local evaluation outputs)
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
```

Deleted files: none.

## API / implementation contract

New interfaces: none.

New classes: none.

New methods: none.

Public contracts changed: no.

Schema changed: no.

New dependencies: no.

## Exact behavior

Create a bounded evidence manifest that summarizes real/manual M4.1 strict evaluation evidence without committing bulky raw outputs.

Manifest must include:

```text
- evidence date/time or user-provided run id;
- model/provider used, if known;
- prompt/profile/scenario id, if known;
- report/artifact file locations, as local paths or user-provided names;
- pass/fail/repair indicators observed;
- top diagnostics/hot spots, if available;
- redaction status;
- whether evidence is sufficient for gate decision;
- next task recommendation.
```

Failure behavior:

```text
If evidence is missing, ambiguous, huge, or sensitive, do not invent details. Write BLOCKERS.md and ask user for the exact report path or a redacted excerpt.
```

Diagnostic codes: none.

Validation rules:

```text
- Do not mark M4.1 passed in this task.
- Do not unlock M5/M6.
- Do not commit raw .llmgc generated outputs unless explicitly redacted and minimized into fixtures.
- Evidence manifest must distinguish observed evidence from inference.
```

Security/sandbox rules:

```text
No real LLM/provider calls. Evidence is user-provided/manual.
```

Persistence rules:

```text
Docs manifest only. Optional fixtures only if small/redacted/user-approved.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
No code tests unless a small redacted fixture is added and an existing analyzer/importer test is explicitly in scope.
```

Required proof checks:

```text
- docs/M4_1_REAL_EVALUATION_EVIDENCE_MANIFEST.md exists.
- It has sections: Evidence sources, Summary, Diagnostics, Redaction, Sufficiency, Next step.
- It does not claim M4.1 pass/fail without user decision.
- check-all passes.
```

Required pass tests: not applicable.

Required fail/reject tests: not applicable.

Regression tests: not applicable.

Golden/snapshot fixtures:

```text
Optional only: small redacted fixture under tests/fixtures/strict-llm-real-evaluation/**.
```

Fake/corpus requirements: not applicable.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Stop conditions

Stop if:

```text
- no real/manual evidence path or content is available;
- evidence appears sensitive and is not redacted;
- implementation would require changing source/project/schema;
- user asks the agent to run real provider calls;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not run real evaluation.
- Do not make the gate decision.
- Do not update current state.
- Do not unlock M5/M6.
- Do not import/analyze large reports unless a follow-up task allows it.
```

## Expected final report

Final report must include:

```text
- evidence manifest path;
- evidence sources named by user;
- redaction status;
- whether evidence is sufficient for M4_1_015 or M4_1_016;
- check-all result;
- next task pointer.
```

## Next task pointer

On success with importable report/fixture evidence, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_015
Task spec file: docs/agent-tasks/M4_1/M4_1_015_REAL_REPORT_IMPORT_FIXTURE_GUARD.md
Reason: Add a small redacted fixture/import guard for real strict evaluation evidence before gate closure.
User approval: required
```

On success with enough evidence for manual decision, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_016
Task spec file: docs/agent-tasks/M4_1/M4_1_016_M4_GATE_CLOSURE_DECISION.md
Reason: Close M4.1 gate based on user-reviewed evidence manifest.
User approval: required
```

On block, write BLOCKERS.
