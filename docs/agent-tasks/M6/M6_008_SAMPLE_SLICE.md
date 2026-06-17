# M6_008 — First rich sample package vertical slice

This is a locked draft task spec. It is not executable while M4.1/M5 gates are unresolved.

Task-pack files alone cannot unlock M6. Before execution, refresh this draft from current source layout and rewrite it into an executable spec with exact allowed files, local patterns, proof tests, and approval text.

## Header

Task ID: `M6_008`

Milestone: `M6 Rich GamePackage Assembly`

Status: `locked_draft_by_m4_1_gate_and_artifact_envelope_stability`

Depends on:

```text
- M4.1 gate passed in docs/CURRENT_GENERATOR_STATE.md and .json;
- M5 safe artifact envelope exists, or current-state docs explicitly choose a non-Lua assembly path;
- M6_001 artifact-to-package mapping contracts are refreshed and complete;
- check-all is green before conversion to executable.
```

Unlocks:

```text
M8 runtime preview validation loop only after package assembly vertical slice is stable and current-state docs unlock runtime preview work.
```

Risk level: high

Expected changed files count after unlock: 5-8.

## Gate status

Allowed before current gate review: no.

Requires user approval: yes, after gates pass.

Approval text required in NEXT_TASK.md after unlock:

```text
User approval: approved to start M6_008 after M4.1/M5 gates and M6 prerequisites are satisfied
```

## Source of truth

Source-of-truth docs after unlock:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/GAME_PACKAGE_FORMAT.md
docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md
docs/VALIDATION_STRATEGY.md
docs/CONTEXT_INDEX.md
docs/agent-tasks/M6/M6_001_ARTIFACT_TO_PACKAGE_MAPPING_CONTRACTS.md
docs/ASSET_CONTRACT_SPEC.md
docs/VALIDATION_STRATEGY.md
```

Context budget:

```text
Read this spec, M6_001, selected artifact contract docs, package format/validator docs, one existing package fixture, and 2-3 local mapping/validation analogs. Do not read all phase docs or all samples.
```

Existing patterns to inspect after unlock:

```text
samples/minimal-map-game/package.json
src/LLMGameCreator.Application/Validation/GamePackageValidator.cs
tests/LLMGameCreator.Tests/SmokeTests.cs
tests/LLMGameCreator.Tests/Design/**
```

## File boundaries after unlock

Allowed files after refresh:

```text
src/LLMGameCreator.Application/Design/**
src/LLMGameCreator.Application/Validation/**
tests/LLMGameCreator.Tests/Design/**
tests/LLMGameCreator.Tests/Validation/**
tests/fixtures/artifact_mapping/rich_sample/**
samples/** only if user approves sample fixture update
.devflow/CURRENT_RUN.md
.devflow/NEXT_TASK.md
```

Forbidden files:

```text
src/LLMGameCreator.Runtime/** unless a separate runtime smoke task explicitly allows it
src/LLMGameCreator.WinForms/** unless a separate UI task explicitly allows it
src/LLMGameCreator.GamePackage/** unless schema change is explicitly approved
src/LLMGameCreator.Scripting/** unless only a stable artifact envelope type is referenced by tests
LLMGameCreator.sln
*.csproj
```

Deleted files: none.

## API / implementation contract after unlock

Implementation intent:

```text
Assemble a small but meaningful sample package from reviewed artifacts across selected families. This is a vertical proof slice, not broad game generation.
```

Public contracts changed: no GamePackage schema change unless separately approved.

Schema changed: no by default.

New dependencies: no.

## Exact behavior after unlock

Success behavior:

```text
- transform only reviewed/approved artifact envelopes or dry-run assembly inputs;
- emit deterministic package patch/dry-run output;
- validate before any apply;
- preserve review/apply boundary;
- do not call LLM/provider/Lua/runtime/UI.
```

Failure behavior:

```text
- unsupported contract/family -> deterministic diagnostic;
- missing or unknown refs -> deterministic diagnostic;
- duplicate ids -> deterministic diagnostic;
- schema-required change -> diagnostic and stop, not silent schema mutation.
```

Diagnostic codes to refresh after unlock:

```text
artifact_mapping.rich_sample.validation_failed
artifact_mapping.rich_sample.incomplete
artifact_mapping.rich_sample.ref.invalid
artifact_mapping.rich_sample.apply.failed
```

Validation rules:

```text
- no direct artifact -> package mutation;
- no package schema change without explicit approved task;
- output order must be deterministic;
- package validator or mapping validator must reject invalid references before apply.
```

Security/sandbox rules: no LLM/provider/Lua/runtime execution.

Persistence rules: use existing review/apply or patch boundaries; do not introduce a second persistence path.

GamePackage mutation rule:

```text
Only the existing validated apply boundary may mutate package data, and only after dry-run/validation succeeds.
```

## Future proof tests

When this draft is converted to executable, proof tests must include:

```text
- fixture set assembles one package with scenes/items/dialogue/quest references resolved;
- package validator passes for the assembled sample;
- deterministic output snapshot pins ids/order;
- incomplete fixture -> artifact_mapping.rich_sample.incomplete;
- invalid ref fixture -> artifact_mapping.rich_sample.ref.invalid;
- no runtime/provider/LLM/Lua calls occur.
```

No proof test may rely only on broad success/failure. Pin exact diagnostics, output fragments, ids, order, and validation outcome.

## System gates after unlock

Build/check command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Focused test command must be refreshed from current source. Prefer filters around `Package`, `Patch`, `Artifact`, `Validation`, and the selected artifact family.

## Stop conditions

Stop if:

```text
- M4.1 gate has not passed;
- artifact envelope is not stable enough to map;
- M6_001 is incomplete or stale;
- implementation requires package schema/project/dependency change not approved;
- proof tests cannot pin exact mapping behavior;
- task exceeds 8 changed files.
```

## Non-goals

```text
- Do not implement broad package assembly pipeline.
- Do not bypass review/apply boundary.
- Do not call LLM/provider/Lua/runtime.
- Do not change GamePackage schema silently.
- Do not add UI wiring.
```

## Expected final report after unlock

Final report must include:

```text
- artifact family/contract mapped;
- diagnostics added/reused;
- proof tests and exact assertions;
- package validator/dry-run result;
- confirmation Runtime/UI/provider/Lua untouched;
- check-all result;
- next task pointer.
```

## Next task pointer after unlock

On success, suggest NEXT_TASK:

```text
Task source: agent_task_spec
Task id: M8_001
Reason: Continue M6 assembly sequence after M6_008 passes.
```

On block, write BLOCKERS with the unmet gate or stale assumption.
