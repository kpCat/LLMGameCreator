# M4_1_007 — M4.1 gate decision report

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_007`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_after_real_evaluation_summary_exists`

Depends on:

```text
- BASELINE/check-all is green.
- A real M4.1 evaluation result exists or M4_1_001 imported/analyzed it.
- User explicitly approves generating a gate decision report.
```

Unlocks:

```text
- Human review input for updating CURRENT_GENERATOR_STATE.
```

Risk level: medium

Expected changed files count: 2-7

## Gate status

Allowed before current gate review: yes, but only as report generation. This task must not unlock M5/M6 automatically.

Requires user approval: yes.

## Source of truth

Source-of-truth docs:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/ROADMAP_TO_FULL_GENERATOR.md
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationMarkdownRenderer.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmEvaluationServiceTests.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmEvaluationMarkdownRendererTests.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/*StrictLlmEvaluation*.cs
tests/LLMGameCreator.Tests/Design/*StrictLlmEvaluation*.cs
docs/agent-tasks/M4_1/*
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files unless this task stops and requests explicit human approval:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
```

Always forbidden files:

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

New classes:

```text
Optional small report DTO, for example M4GateDecisionSummary, only if it avoids overloading existing evaluation models.
```

New methods:

```text
Prefer a pure method that maps GeneratorPlanStrictLlmEvaluationResult -> gate decision summary/report.
```

Public contracts changed: avoid.

Schema changed: no.

New dependencies: no.

## Exact behavior

The report must classify gate status without performing the gate decision itself:

```text
- pass_candidate: high pass rate, low repair reliance, no blocking diagnostics, no serious quality warnings.
- repair_required: meaningful failures/hotspots but bounded and actionable.
- fail_or_repeat_required: missing report, very low pass rate, invalid schema, repeated critical diagnostics, or unreliable repair.
- insufficient_evidence: too few runs/contracts or missing real-model evidence.
```

Input contract:

```text
GeneratorPlanStrictLlmEvaluationResult from real evaluation or imported report summary.
```

Output contract:

```text
deterministic summary/report with status, reasons, blocking diagnostics, recommended next task, and explicit statement that human approval is required before M5/M6 unlock.
```

Failure behavior:

```text
- Missing evidence -> insufficient_evidence, not pass.
- Contradictory metrics -> repair_required or fail_or_repeat_required with diagnostic reason.
```

Diagnostic/failure behavior:

```text
Use existing evaluation diagnostics where possible. Add no new diagnostic code unless necessary for deterministic reporting.
```

Validation rules:

```text
- Report must not modify CURRENT_GENERATOR_STATE.
- Report must not say M5/M6 are unlocked.
- Report must include a human-review-required sentence.
- Same input result -> same decision category and reasons order.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- high pass rate + no serious diagnostics -> pass_candidate and human-review-required;
- missing/zero total runs -> insufficient_evidence;
- high JsonInvalidCount or MarkdownFenceErrorCount -> repair_required;
- low pass rate -> fail_or_repeat_required;
- report output never contains automatic M5/M6 unlock wording;
- same input produces deterministic reasons order.
```

Required pass tests:

```text
Constructed stable evaluation result -> deterministic gate report.
```

Required fail/reject tests:

```text
Missing real evidence -> insufficient_evidence, not pass_candidate.
```

Fake/corpus requirements: constructed evaluation result only; no real LLM call.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Focused test command:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlmEvaluation"
```

Docs consistency commands: manual review; do not edit current-state docs in this task.

Runtime scenario commands: not applicable.

## Stop conditions

Stop if:

```text
- task requires updating CURRENT_GENERATOR_STATE directly;
- task requires deciding to unlock M5/M6;
- task requires real LLM/provider call;
- task requires DB schema change;
- task requires more than 7 files;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not unlock M5/M6.
- Do not mutate package.
- Do not run real evaluation batch.
- Do not change artifact contracts.
```

## Expected final report

Final report must include:

```text
- gate decision category;
- evidence used;
- blocking diagnostics/hotspots;
- recommended next task;
- explicit statement that human review is still required;
- tests added/run;
- check-all run directory.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_008
Task spec file: docs/agent-tasks/M4_1/M4_1_008_AGENT_TASK_DOCS_CONSISTENCY_GUARD.md
Reason: Add consistency checks around agent task specs before expanding more packs.
User approval: required
```
