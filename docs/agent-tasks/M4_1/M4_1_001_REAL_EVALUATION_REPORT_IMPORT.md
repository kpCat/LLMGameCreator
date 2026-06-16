# M4_1_001 — Real evaluation report import/analyzer hardening

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_001`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_when_real_report_exists`

Depends on:

```text
- A real strict LLM evaluation JSON or markdown report exists under .llmgc/generator-plans/ or is provided by the user.
- BASELINE/check-all is green.
```

Unlocks:

```text
- Evidence-based prompt/repair/parser/validator hardening task.
```

Risk level: low/medium

Expected changed files count: 3-8

## Gate status

Allowed before current gate review: yes

Requires user approval: no, if a real report exists. Stop if report is missing.

Approval text required in NEXT_TASK.md: not required unless the report location is ambiguous.

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md
docs/CONTEXT_INDEX.md
.devflow/MODELING_STRATEGY.md
```

Context budget:

```text
Read only this task spec, the source docs above, the existing evaluation service/presenter tests, and the real report file.
```

Read only these docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
```

Do not read:

```text
- all docs/;
- all .llmgc artifacts;
- all WinForms pages;
- M5/M6 phase plans.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationService.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmEvaluationServiceTests.cs
src/LLMGameCreator.WinForms/Pages/StrictLlmEvaluation/StrictLlmEvaluationPresenter.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/*StrictLlmEvaluation*.cs
src/LLMGameCreator.WinForms/Pages/StrictLlmEvaluation/*
tests/LLMGameCreator.Tests/Design/*StrictLlmEvaluation*.cs
tests/fixtures/strict-llm-evaluation/*
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/**
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: none unless existing service boundary proves one is needed.

New classes: only small DTO/helper if existing service becomes too large.

New methods:

```text
Prefer extending existing evaluation analyzer/import path with a small method that accepts a saved report path or saved report content and returns a typed summary/report model.
```

Modified classes:

```text
GeneratorPlanStrictLlmEvaluationService or nearest existing evaluation analyzer.
StrictLlmEvaluationPresenter only if UI mapping is missing.
```

Public contracts changed: avoid. Stop if required.

Schema changed: no.

New dependencies: no.

## Exact behavior

Input contract:

```text
- real evaluation JSON report and/or markdown report;
- existing strict generation audit references if the current code already supports them.
```

Output contract:

```text
- typed/imported summary with pass count, fail count, repair recovery, diagnostic hot spots, quality warnings, recommendations;
- no package mutation;
- no provider call.
```

Success behavior:

```text
- Report imports/analyzes deterministically.
- Missing sections produce warnings/diagnostics, not crashes.
- Recommendations mention whether gate is likely pass/repair/fail, but do not unlock M5/M6 automatically.
```

Failure behavior:

```text
- Missing file -> diagnostic/result state, not unhandled exception.
- Invalid JSON -> diagnostic/result state.
- Unknown report schema -> diagnostic/result state.
```

Diagnostic codes:

```text
strict_eval.report.missing
strict_eval.report.invalid_json
strict_eval.report.schema_unknown
strict_eval.report.no_samples
strict_eval.hotspots.detected
```

Validation rules:

```text
- negative counts are invalid;
- total samples must match grouped counts if both are present;
- missing pass/fail/repair metrics should be visible as warnings.
```

Security/sandbox rules: no network, no provider calls, no real LLM calls.

Persistence rules: only read report artifacts; do not mutate package.

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- imports valid saved evaluation JSON fixture;
- invalid JSON fixture returns strict_eval.report.invalid_json;
- missing report path returns strict_eval.report.missing;
- hot spot grouping produces deterministic order;
- import/analyzer does not call ILlmChatClient/provider.
```

Required pass tests:

```text
Valid report -> expected pass/fail/repair metrics and hot spots.
```

Required fail/reject tests:

```text
Invalid JSON -> diagnostic result, no crash.
Missing file -> diagnostic result, no crash.
```

Regression tests:

```text
If real report exposes a repeated problem, add a focused fixture from a redacted sample.
```

Golden/snapshot fixtures: optional small markdown expected-output fixture if markdown generation is touched.

Fake/corpus requirements: use fixtures, not a real provider.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlmEvaluation"
```

Docs consistency commands: `check-all.ps1` unless dedicated docs gate exists.

Manifest integrity commands: not applicable.

Artifact schema commands: fixture JSON parse/validation tests.

Package validator commands: not applicable.

Runtime scenario commands: not applicable.

Snapshot/golden commands: optional if markdown output is changed.

## Stop conditions

Stop if:

```text
- real evaluation report is missing;
- task requires DB schema change;
- task requires provider/LLM call;
- task requires GamePackage mutation;
- task exceeds 8 changed files;
- task requires M5/M6 unlock decision.
```

## Non-goals

```text
- Do not unlock M5/M6.
- Do not expand strict artifact contracts.
- Do not call LLM.
- Do not mutate package.
- Do not redesign the evaluation UI.
```

## Expected final report

Final report must include:

```text
- real report source path or reason it was missing;
- metrics extracted;
- diagnostic hot spots;
- quality warnings;
- changed files;
- tests added/run;
- whether a follow-up M4_1_003 hardening task is justified.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_003
Task spec file: docs/agent-tasks/M4_1/M4_1_003_REPAIR_POLICY_HARDENING.md
Reason: Harden the highest-impact diagnostic hot spot from the real evaluation report.
```

On block, write BLOCKERS:

```text
M4_1_001 blocked: real evaluation report missing or unreadable.
```
