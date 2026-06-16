# M4_1_003 — Bounded repair/parser/validator hardening from evidence

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_003`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_after_real_hot_spot_or_corpus_failure`

Depends on:

```text
- M4_1_001 real report hot spot, or
- M4_1_002 failing corpus fixture, or
- user-provided concrete strict output failure.
```

Unlocks:

```text
- re-run of strict LLM evaluation with improved diagnostics/repair behavior.
```

Risk level: medium

Expected changed files count: 4-8

## Gate status

Allowed before current gate review: yes

Requires user approval: no if the target hot spot is already documented in CURRENT_RUN/BLOCKERS/report. Stop if hot spot is vague.

Approval text required in NEXT_TASK.md: required only if the task would modify prompt text or public contract behavior.

## Source of truth

Source-of-truth docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/PROMPT_AND_ARTIFACT_CONTRACT_HARDENING.md
.devflow/MODELING_STRATEGY.md
docs/VALIDATION_STRATEGY.md
```

Context budget:

```text
Read only the failing fixture/report excerpt, owning parser/repair/validator files, and 2-3 adjacent tests.
```

Read only these docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/PROMPT_AND_ARTIFACT_CONTRACT_HARDENING.md
.devflow/MODELING_STRATEGY.md
```

Do not read:

```text
- all docs/;
- all src/;
- all tests/;
- M5/M6 specs.
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactGenerationService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmEvaluationService.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmArtifactGenerationServiceTests.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmEvaluationServiceTests.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/*StrictLlm*.cs
src/LLMGameCreator.Generation/**  (only prompt/context templates if the hot spot is prompt-owned)
tests/LLMGameCreator.Tests/Design/*StrictLlm*.cs
tests/fixtures/llm_raw_outputs/**
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.WinForms/** unless presenter mapping is the proven issue
LLMGameCreator.sln
*.csproj
```

Deleted files: none

## API / implementation contract

New interfaces: none.

New classes: only if splitting a testable parser/repair helper avoids broad service changes.

New methods:

```text
A small helper method may be added for one bounded behavior: extraction, parse diagnostics, repair prompt guidance, or validator rule.
```

Modified classes:

```text
One owner class plus focused tests.
```

Public contracts changed: avoid; stop if required.

Schema changed: no.

New dependencies: no.

## Exact behavior

Input contract:

```text
One concrete failing raw output/report hot spot.
```

Output contract:

```text
One bounded behavioral improvement with deterministic diagnostics and proof tests.
```

Success behavior:

```text
- The known failure is now rejected/repaired/diagnosed according to the task evidence.
- Repair remains bounded by max attempts.
- Valid existing cases still pass.
```

Failure behavior:

```text
- Invalid input produces stable diagnostic.
- Repair failure remains a failed result, not a staged artifact.
```

Diagnostic codes:

Prefer existing codes. New codes must be lowercase dot-separated and domain-specific.

Validation rules:

```text
- no silent pass;
- no acceptance of placeholder/test/example content as valid final artifact;
- no contract id drift;
- no staging when validation fails.
```

Security/sandbox rules: no provider call in tests.

Persistence rules: valid artifacts may be staged only through existing strict generation pipeline behavior; tests should use fakes/temp fixtures.

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- failing fixture reproduces the current problem;
- fixed behavior proves the desired result;
- existing valid fixture still passes;
- repair max attempts remains enforced if repair is touched;
- no real ILlmChatClient/provider call in tests.
```

Required pass tests:

```text
Existing valid strict artifact generation case still passes.
```

Required fail/reject tests:

```text
The hot spot input fails with stable diagnostic or repairs exactly once then validates.
```

Regression tests:

```text
Add regression test named after the hot spot/failure category.
```

Golden/snapshot fixtures: optional.

Fake/corpus requirements: mandatory for LLM-facing behavior.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Unit/integration test commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~StrictLlm"
```

Docs consistency commands: run `check-all.ps1`; update docs only if behavior changes the documented contract.

Manifest integrity commands: not applicable.

Artifact schema commands: strict artifact tests.

Package validator commands: not applicable.

Runtime scenario commands: not applicable.

Snapshot/golden commands: optional.

## Stop conditions

Stop if:

```text
- no concrete report/corpus failure exists;
- fix requires broad refactor;
- fix requires changing GamePackage schema;
- fix requires expanding artifact contracts broadly;
- fix requires M5/M6 work;
- fix requires more than 8 changed files;
- after 2 repair attempts check-all is not green.
```

## Non-goals

```text
- Do not add new artifact families.
- Do not unlock M5/M6.
- Do not run real LLM in tests.
- Do not mutate package.
- Do not rewrite the strict generation service wholesale.
```

## Expected final report

Final report must include:

```text
- exact hot spot fixed;
- fixture/test proving it;
- diagnostic code used/added;
- changed files;
- repair attempts;
- check-all result;
- recommendation whether to rerun real evaluation.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: phase_plan
Phase plan file: .devflow/phase-plans/10_M4_1_EVALUATION_STABILIZATION.md
Reason: Rerun/review real evaluation after bounded hardening; do not proceed to M5/M6 until current state is updated.
```

On block, write BLOCKERS:

```text
M4_1_003 blocked: no concrete hot spot or fix would require broad architecture/schema change.
```
