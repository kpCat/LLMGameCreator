# M4_1_015 — Real report import fixture guard

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_015`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_when_evidence_manifest_exists`

Depends on:

```text
- docs/M4_1_REAL_EVALUATION_EVIDENCE_MANIFEST.md exists.
- Manifest identifies a small redacted report/excerpt suitable for fixture use.
- User approval is present.
```

Unlocks:

```text
- M4_1_016 M4 gate closure decision.
```

Risk level: medium

Expected changed files count: 3-8

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because it may add redacted real-output fixtures and analyzer tests.

Approval text required in NEXT_TASK.md:

```text
User approval: approved
```

## Source of truth

Source-of-truth docs:

```text
docs/M4_1_REAL_EVALUATION_EVIDENCE_MANIFEST.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
```

Context budget:

```text
Read this task spec, evidence manifest, the existing strict evaluation analyzer/import tests, and only one small redacted evidence fixture.
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
tests/LLMGameCreator.Tests/Design/*StrictLlmEvaluation*Tests.cs
tests/fixtures/strict-llm-real-evaluation/**
docs/M4_1_REAL_EVALUATION_EVIDENCE_MANIFEST.md
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
.devflow/BLOCKERS.md
```

Allowed production files only if a real evidence fixture exposes a confirmed analyzer/renderer bug:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationMarkdownRenderer.cs
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.WinForms/**
*.sln
*.csproj
.devflow/scripts/**
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
```

Deleted files: none.

## API / implementation contract

New interfaces: none.

New classes: none unless a tiny test fixture loader is required in test namespace.

New methods: prefer test helper methods only.

Public contracts changed: no unless a confirmed bug requires one diagnostic/recommendation extension and user approves.

Schema changed: no.

New dependencies: no.

## Exact behavior

Add fixture-driven guard coverage for one redacted real evaluation report/excerpt.

Expected behavior:

```text
- fixture is small and redacted;
- analyzer/importer/renderer processes it deterministically;
- test pins exact recommendation/diagnostic/hotspot behavior;
- markdown output is stable enough for gate review;
- no real LLM/provider call is made.
```

Failure behavior:

```text
If fixture cannot be parsed/analyzed with existing code, either add a failing regression test and stop with blocker, or make a minimal fix only in allowed production files if the bug is clear.
```

Diagnostic codes:

```text
Use existing diagnostic/recommendation codes where available. Do not add vague prose-only signals.
```

Validation rules:

```text
- Tests must assert exact codes/sections/recommendations, not just NotEmpty.
- Fixture must not include secrets, API keys, personal data, or huge raw dumps.
- No current-state update in this task.
```

Security/sandbox rules:

```text
No real LLM/provider/network calls.
```

Persistence rules:

```text
Fixtures only under tests/fixtures/strict-llm-real-evaluation/**.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- redacted real evaluation fixture -> deterministic summary/hotspot/recommendation output;
- markdown renderer includes expected gate-relevant sections for that fixture;
- no real provider call is needed.
```

Required pass tests:

```text
At least one fixture-based test that pins exact expected behavior.
```

Required fail/reject tests:

```text
If malformed report fixture is added, assert exact failure diagnostic. Otherwise document why not applicable.
```

Regression tests:

```text
If fixture exposes a bug, add regression test before/with fix.
```

Golden/snapshot fixtures:

```text
Small redacted fixture only. Avoid giant snapshots.
```

Fake/corpus requirements:

```text
No real provider. Corpus/fixture only.
```

## System gates

Focused test command:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlmEvaluation"
```

Full gate:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Stop conditions

Stop if:

```text
- evidence manifest is missing;
- fixture is too large or not redacted;
- exact expected behavior cannot be asserted;
- task would require broad analyzer redesign;
- task would require M5/M6/runtime/package work;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not run real LLM evaluation.
- Do not decide M4.1 pass/fail.
- Do not update CURRENT_GENERATOR_STATE.
- Do not unlock M5/M6.
- Do not redesign strict evaluation architecture.
```

## Expected final report

Final report must include:

```text
- fixture path and redaction status;
- tests added/changed;
- exact recommendations/diagnostics asserted;
- focused test result;
- check-all result;
- next task pointer.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_016
Task spec file: docs/agent-tasks/M4_1/M4_1_016_M4_GATE_CLOSURE_DECISION.md
Reason: Close M4.1 gate based on user-reviewed evidence and fixture/analyzer guard coverage.
User approval: required
```

On block, write BLOCKERS.
