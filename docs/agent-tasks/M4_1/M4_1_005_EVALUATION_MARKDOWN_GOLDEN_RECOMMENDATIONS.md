# M4_1_005 — Evaluation markdown golden recommendations

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_005`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_after_M4_1_004_or_real_report`

Depends on:

```text
- BASELINE/check-all is green.
- M4_1_004 completed, or a real evaluation report exists with recommendation gaps.
```

Unlocks:

```text
- M4_1_007 gate decision report task.
```

Risk level: low

Expected changed files count: 2-6

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because this may update expected markdown/golden text.

## Source of truth

Source-of-truth docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/CURRENT_GENERATOR_STATE.md
.devflow/MODELING_STRATEGY.md
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationMarkdownRenderer.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmEvaluationMarkdownRendererTests.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationService.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationMarkdownRenderer.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmEvaluationMarkdownRendererTests.cs
tests/fixtures/strict-llm-evaluation-markdown/**
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.WinForms/**
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: none.

New classes: none unless a small test-only fixture loader is needed.

New methods:

```text
Prefer private helper methods in tests. Avoid public API changes.
```

Public contracts changed: no.

Schema changed: no.

New dependencies: no.

## Exact behavior

The evaluation markdown report must communicate M4.1 gate evidence clearly:

```text
- Summary section exists.
- Per-contract summary exists.
- Diagnostic hot spots section exists.
- Samples section exists.
- Recommendations section exists.
- High JSON invalid/wrapper/fence counts recommend prompt/parser/repair hardening.
- High pass rate and no failures recommend contract stability, but must not unlock M5/M6 automatically.
- Warnings remain visible and are not hidden by pass-rate success.
```

Input contract:

```text
GeneratorPlanStrictLlmEvaluationResult
```

Output contract:

```text
Markdown report string
```

Failure behavior:

```text
- Missing optional samples/diagnostics should render an empty section or explicit no-data message, not throw.
- Recommendations must be deterministic for the same result.
```

Diagnostic codes: not applicable unless renderer already emits diagnostics elsewhere.

Validation rules:

```text
- Recommendations must be deterministic.
- Markdown must not claim M5/M6 are unlocked.
- Markdown must not hide diagnostics when OverallPassRate is high.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- high JsonInvalidCount recommends strict JSON prompt/parser/repair hardening;
- markdown with pass rate 1.0 and zero failures says contract looks stable but does not mention M5/M6 unlock;
- warnings are rendered even when pass rate is high;
- deterministic result renders identical markdown across two calls;
- empty diagnostics/samples do not crash.
```

Required pass tests:

```text
Stable result -> report contains Summary, Per-contract summary, Diagnostic hot spots, Samples, Recommendations.
```

Required fail/reject tests:

```text
Report must not contain phrases that imply automatic M5/M6 unlock.
```

Golden/snapshot fixtures:

```text
Optional: store one small expected markdown fixture if project style supports golden text. Avoid huge brittle snapshots.
```

Fake/corpus requirements: constructed result objects only; no real LLM call.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Focused test command:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~GeneratorPlanStrictLlmEvaluationMarkdownRendererTests"
```

Runtime scenario commands: not applicable.

Snapshot/golden commands: focused renderer tests.

## Stop conditions

Stop if:

```text
- renderer changes require UI changes;
- report wording would unlock M5/M6 automatically;
- task requires more than 6 changed files;
- golden snapshot becomes huge or brittle;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not change evaluation execution.
- Do not call provider/LLM.
- Do not redesign the WinForms page.
- Do not update CURRENT_GENERATOR_STATE.
```

## Expected final report

Final report must include:

```text
- markdown behaviors covered;
- tests added/updated;
- whether any wording was changed;
- focused test result;
- check-all run directory;
- next task pointer.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_006
Task spec file: docs/agent-tasks/M4_1/M4_1_006_STRICT_REPAIR_PROMPT_GUARDRAILS.md
Reason: Add guardrails for repair prompts after parser diagnostics and evaluation recommendations are covered.
User approval: required
```
