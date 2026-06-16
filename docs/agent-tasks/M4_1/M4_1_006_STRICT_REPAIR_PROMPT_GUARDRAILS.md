# M4_1_006 — Strict repair prompt guardrails

This task spec is executable guidance. If implementation conflicts with this file, stop and report.

## Header

Task ID: `M4_1_006`

Milestone: `M4.1 real-model evaluation gate`

Status: `ready_after_diagnostics_or_parser_corpus`

Depends on:

```text
- BASELINE/check-all is green.
- Either M4_1_004 is complete or real evaluation diagnostics show repair-related failures.
```

Unlocks:

```text
- Evidence-based M4_1_003 repair policy hardening if real diagnostics justify it.
```

Risk level: low/medium

Expected changed files count: 2-6

## Gate status

Allowed before current gate review: yes

Requires user approval: yes, because prompt text may be changed.

## Source of truth

Source-of-truth docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
.devflow/MODELING_STRATEGY.md
```

Existing patterns to inspect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactRepairPromptBuilder.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactPromptBuilder.cs
tests/LLMGameCreator.Tests/Design/*StrictLlmArtifact*Tests.cs
```

## File boundaries

Allowed files:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactRepairPromptBuilder.cs
tests/LLMGameCreator.Tests/Design/*StrictLlmArtifact*Repair*Tests.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanStrictLlmArtifactGenerationServiceTests.cs
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

New classes: none unless a focused test class is needed.

New methods:

```text
No new public methods unless existing builder tests require easier construction.
```

Public contracts changed: no.

Schema changed: no.

New dependencies: no.

## Exact behavior

Repair prompt must remain targeted and constrained:

```text
- Returns exactly one corrected JSON object.
- No Markdown fences.
- No prose/explanations/comments/scripts/provider instructions.
- Does not redesign selected variants, feature bundles, contract id, artifact_kind, or source context.
- Includes validation diagnostics in machine-readable form.
- Includes invalid response excerpt bounded to existing max length.
- Includes contract repair guidance.
```

Input contract:

```text
contract definition + original prompt + invalid response + diagnostics + repair attempt index
```

Output contract:

```text
GeneratorPlanStrictLlmArtifactPrompt
```

Failure behavior:

```text
- Null contract/original prompt/diagnostics should retain existing ArgumentNullException behavior.
- Huge invalid response must be truncated, not copied unbounded.
```

Diagnostic codes: no new codes required unless production behavior changes.

Validation rules:

```text
- Prompt must mention exact output schema.
- Prompt must include diagnostics JSON.
- Prompt must forbid Markdown and prose.
- Prompt must not ask model to change contract identity.
```

GamePackage mutation rule: forbidden.

## Proof tests

Tests to add before/with implementation:

```text
- repair prompt contains contract id and exact output schema;
- repair prompt contains serialized diagnostics with code/target/message;
- repair prompt contains invalid response excerpt but truncates overlong response;
- repair prompt forbids Markdown fences/prose/provider instructions/package mutation;
- repair prompt preserves contract id and does not ask to redesign selected variants/artifact_kind/source_context;
- no real ILlmChatClient/provider call is needed.
```

Required pass tests:

```text
Constructed diagnostics -> repair prompt includes stable targeted guidance.
```

Required fail/reject tests:

```text
Overlong invalid response -> prompt length is bounded and includes only excerpt.
```

Fake/corpus requirements: constructed diagnostics only; no real LLM call.

## System gates

Build commands:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Focused test command:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~RepairPrompt"
```

Runtime scenario commands: not applicable.

## Stop conditions

Stop if:

```text
- task requires changing actual repair loop control flow;
- task requires real LLM call;
- task requires public contract/schema change;
- task requires more than 6 changed files;
- prompt change conflicts with current contract catalog behavior;
- check-all fails after 2 repair attempts.
```

## Non-goals

```text
- Do not change parser semantics.
- Do not change validator severity.
- Do not change max repair attempts.
- Do not stage artifacts differently.
- Do not unlock M5/M6.
```

## Expected final report

Final report must include:

```text
- prompt guardrails covered;
- tests added/updated;
- whether production prompt text changed;
- focused test result;
- check-all run directory;
- next task pointer.
```

## Next task pointer

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M4_1_007
Task spec file: docs/agent-tasks/M4_1/M4_1_007_M4_GATE_DECISION_REPORT.md
Reason: Prepare a deterministic gate decision summary once evaluation data exists.
User approval: required
```
