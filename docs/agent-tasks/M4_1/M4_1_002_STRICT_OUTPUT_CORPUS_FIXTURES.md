# M4_1_002 — Strict output corpus fixtures for parser/repair evidence

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_002`

Milestone: `M4.1 real-model evaluation gate`

Status: `proposed_requires_user_start`

Depends on:

```text
- check-all is green;
- strict LLM artifact generation service and tests exist.
```

Unlocks:

```text
- evidence-based parser/repair hardening;
- safer local-agent repair tasks.
```

Risk level: low

Expected changed files count: 4-8

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because it adds new test fixtures/corpus structure.

Approval text required in NEXT_TASK.md:

```text
User approval: approved for M4_1_002 corpus fixture task
```

## Source of truth

Source-of-truth docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/VALIDATION_STRATEGY.md
.devflow/MODELING_STRATEGY.md
docs/CONTEXT_INDEX.md
```

Context budget:

```text
Read the existing strict artifact generation service/tests and only target parser/extractor/repair code mentioned by those tests.
```

Read only these docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
.devflow/MODELING_STRATEGY.md
```

Do not read:

```text
- all docs/;
- all tests/;
- all generator-library/;
- M5/M6 phase plans.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactGenerationService.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmArtifactGenerationServiceTests.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmEvaluationServiceTests.cs
```

## File boundaries

Allowed files:

```text
tests/LLMGameCreator.Tests/Design/*StrictLlmArtifactGeneration*.cs
tests/fixtures/llm_raw_outputs/**
src/LLMGameCreator.Application/Design/GeneratorPlans/*StrictLlm*.cs  (only if a fixture proves a bounded issue)
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.WinForms/**
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: none.

New classes: optional test helper only.

New methods: optional private/internal parser helper only if existing implementation needs extraction for tests.

Modified classes:

```text
Prefer tests/fixtures only. Modify production parser/repair code only after a failing fixture proves a bounded bug.
```

Public contracts changed: no.

Schema changed: no.

New dependencies: no.

## Exact behavior

Input contract:

```text
Saved raw LLM output text fixtures for existing strict artifact contract(s).
```

Output contract:

```text
Tests prove current strict output extraction/parser/repair behavior for representative raw outputs.
```

Success behavior:

```text
- fenced JSON is extracted or rejected according to existing contract;
- text before/after JSON is handled deterministically;
- broken JSON gives stable diagnostic;
- wrong root gives stable diagnostic;
- wrong contract/id drift gives stable diagnostic;
- placeholder text is rejected if current validator owns that rule.
```

Failure behavior:

```text
All parser/validator failures must produce deterministic diagnostics; no silent pass and no unhandled exception.
```

Diagnostic codes:

Use existing codes if present. If new codes are needed, use lowercase dot-separated names:

```text
strict_output.extract.none
strict_output.extract.multiple_json
strict_output.parse.invalid_json
strict_output.contract.wrong_root
strict_output.contract.id_drift
strict_output.content.placeholder
```

Validation rules:

```text
- no real LLM/provider call;
- fixtures must be small and readable;
- expected result must be stated in test name or fixture sidecar.
```

Security/sandbox rules: no provider/network calls.

Persistence rules: test fixtures only.

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- valid minimal JSON fixture passes;
- markdown fenced JSON fixture has deterministic outcome;
- text-before JSON fixture has deterministic outcome;
- text-after JSON fixture has deterministic outcome;
- broken trailing comma fixture fails with parse diagnostic;
- wrong root fixture fails with contract diagnostic;
- id drift fixture fails with contract/id diagnostic;
- repair success/failure behavior remains bounded if existing repair path is touched.
```

Required pass tests:

```text
valid_minimal.json.txt -> parse/validate pass or existing service success result.
```

Required fail/reject tests:

```text
broken_trailing_comma.txt -> stable parse diagnostic.
wrong_root.txt -> stable contract diagnostic.
id_drift.txt -> stable id/contract diagnostic.
```

Regression tests: add one if a real evaluation report exposed a recurring failure.

Golden/snapshot fixtures: optional; keep fixtures small.

Fake/corpus requirements:

```text
Use saved files under tests/fixtures/llm_raw_outputs/<contract_id>/.
```

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlmArtifactGeneration"
```

Docs consistency commands: not required unless docs changed.

Manifest integrity commands: not applicable.

Artifact schema commands: covered by strict generation tests.

Package validator commands: not applicable.

Runtime scenario commands: not applicable.

Snapshot/golden commands: optional if explicit expected files are added.

## Stop conditions

Stop if:

```text
- parser/extractor ownership is unclear after reading existing tests;
- implementation needs more than 8 files;
- task requires production refactor;
- task requires public schema/contract changes;
- test requires real LLM/provider call;
- there is no stable diagnostic path.
```

## Non-goals

```text
- Do not expand artifact contracts.
- Do not improve prompt text without a failing fixture.
- Do not call LLM.
- Do not mutate GamePackage.
- Do not touch UI.
```

## Expected final report

Final report must include:

```text
- fixture list;
- expected outcome per fixture;
- production code changed or not;
- diagnostics used/added;
- test command results;
- next recommended hardening task.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_003
Task spec file: docs/agent-tasks/M4_1/M4_1_003_REPAIR_POLICY_HARDENING.md
Reason: Use corpus evidence or real evaluation hot spots to harden one bounded parser/repair/validator behavior.
```

On block, write BLOCKERS:

```text
M4_1_002 blocked: parser ownership unclear or proof tests would require a broad refactor.
```
