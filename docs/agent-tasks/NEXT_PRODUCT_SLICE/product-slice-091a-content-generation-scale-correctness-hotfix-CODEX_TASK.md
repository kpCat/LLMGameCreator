# Codex Task - Product Slice 091A Content Generation Scale Correctness Hotfix

## Purpose

This is a bounded correctness hotfix for Goal 010 after external artifact review.

Keep the existing final gate:

```text
content_generation_at_scale_artifact_verification
```

Do not mark it passed. Do not create or start S092, Goal 011, asset pipeline work, Unity/export work or post-Goal-010 planning.

## Starting Review Findings To Fix

The current Goal 010 artifact must not be accepted yet. The pushed implementation has false-positive acceptance gaps:

1. All three reference packs declare only one quest objective shape, `choose_dialogue`, so the Goal 010 requirement for at least three reusable objective shapes across the valid matrix is not proven.
2. `MaterializePackage` ignores `GeneratedQuestInstance.ObjectiveKind` and always writes package quest objectives as `choose_dialogue`.
3. Event motifs declare different actions such as `set_flag`, `change_reputation` and `add_item`, but materialization/runtime evidence coerces selected events into a flag-setting path instead of auditing and executing the declared action kind and target.
4. Runtime evidence validation checks command ids and target ids but is not strict enough about command type, value and inventory/secondary binding against the selected generated declaration.

Fix these defects without redesigning the architecture.

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/GOAL_010_CONTENT_GENERATION_AT_SCALE.md`
6. current `src/LLMGameCreator.Application/Design/ContentGeneration/ContentGenerationScaleAcceptanceService.cs`
7. current Goal 010 tests/smoke/artifacts directly touched by this hotfix
8. existing runtime/package definitions needed for honest objective/event execution

Do not read historical apply packs or old task prompts unless a concrete blocker requires it.

## Allowed Files

Allowed:

- `src/LLMGameCreator.Application/Design/ContentGeneration/ContentGenerationScaleAcceptanceService.cs`
- `tests/LLMGameCreator.Tests/Application/ContentGeneration/ContentGenerationScaleAcceptanceTests.cs`
- `tests/LLMGameCreator.Tests/Application/ContentGeneration/ContentGenerationScaleRealRuntimeAdapter.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/ContentGenerationScaleSmokeTests.cs`
- `samples/content-generation-packs/*.content-pack.json`
- `.llmgc/procedural/content-generation-scale/content-generation-scale-report.json`
- `.llmgc/procedural/content-generation-scale/content-generation-scale-report.md`
- `.llmgc/procedural/content-generation-scale/content-generation-scale-verification.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md` only if routing text must mention S091A

Conditionally allowed only if a focused failing test proves a narrow existing defect:

- the smallest existing runtime service file required to honestly execute an already-supported primitive;
- its focused regression test.

Forbidden:

- `.sln` or `.csproj` edits;
- public GamePackage/runtime schema redesign;
- WinForms/UI, Unity/export, asset/media, provider, RAG, LLM or Lua execution;
- `generator-library`;
- broad refactors outside Goal 010 content-generation acceptance;
- S092, Goal 011 or next-goal task files.

Do not use git commands.

## Required Fixes

### 1. Objective Shape Matrix

Update the compact reference packs and generation acceptance so the valid matrix proves at least three distinct quest objective shapes across the three valid packs.

Acceptable examples are existing supported primitives such as:

- `choose_dialogue`
- `collect_item`
- `set_flag`
- `complete_encounter`

Use only objective kinds that can be honestly materialized and executed through existing package/runtime primitives. If a kind cannot be represented, reject it rather than claiming it.

Required assertions:

- valid source packs contain at least three distinct objective kinds across the matrix;
- generated catalog records those objective kinds;
- package quest objectives preserve the generated objective kind instead of coercing all objectives to `choose_dialogue`;
- package target ids and required supporting generated ids resolve exactly for each kind;
- runtime-selected threads cover the objective kinds needed to prove the matrix, with at least six total threads remaining.

### 2. Event Action Materialization

Stop coercing all event actions to `set_flag`.

For each supported event action kind present in valid packs:

- preserve the declared `ConsequenceKind`;
- preserve and resolve the declared target, converted only through deterministic generated/package id mapping;
- materialize package interaction/effect/output data that matches the declared action;
- execute a runtime command path that produces the declared consequence through `GameRuntimeService`;
- record declaration-specific state deltas:
  - `set_flag` changes the expected generated flag id;
  - `add_item` changes the expected generated item/inventory evidence;
  - `change_reputation` changes the expected generated faction reputation evidence;
  - `advance_quest` changes the expected generated quest/objective evidence, if used.

If a trigger/action combination cannot be represented honestly by current primitives, validation must reject it with a stable diagnostic.

### 3. Exact Runtime Command Correlation

Strengthen runtime binding and evidence validation.

For every selected runtime command, acceptance must compare all relevant fields against the selected generated declaration:

- command id;
- command type;
- target id;
- secondary target id;
- value;
- inventory id;
- expected changed quest/item/flag/faction ids where applicable.

Copied command ids plus `Succeeded=true` must not be enough.

Add invalid scenarios that fail causally for:

- a package objective kind coerced to `choose_dialogue`;
- an event action kind coerced to `set_flag`;
- command type mismatch;
- command value mismatch;
- command inventory/secondary target mismatch.

Keep or expand the existing invalid matrix, but do not remove required Goal 010 invalid cases. The expectation-only invalid fixture must still fail when its mutation is removed.

### 4. Package Audit

Package audit must assert, structurally:

- every generated quest objective id exists in the package;
- package objective kind equals catalog `ObjectiveKind`;
- package objective target id points at the correct generated dialogue choice, item, flag, encounter or other supported target for that objective kind;
- every selected generated event id has package-backed interaction/effect/output data matching the catalog event action kind and target;
- runtime thread selected ids are a strict subset of generated/package-backed ids plus explicitly allowed runtime state ids;
- catalog/package hashes change when objective/event mappings change and are stable on replay.

### 5. Artifacts And State

Regenerate exactly the existing Goal 010 artifact files:

```text
.llmgc/procedural/content-generation-scale/content-generation-scale-report.json
.llmgc/procedural/content-generation-scale/content-generation-scale-report.md
.llmgc/procedural/content-generation-scale/content-generation-scale-verification.md
```

The report must include:

- completed slices including `S091A`;
- the unchanged manual gate `content_generation_at_scale_artifact_verification`;
- objective-kind distribution evidence with at least three kinds across valid packs;
- event-action distribution evidence with actual package/runtime correlation;
- six or more runtime threads accepted through the real adapter;
- causal invalid/fake/leak diagnostics including the new coercion/mismatch cases;
- all external execution flags false;
- no absolute paths, timestamps, GUIDs or machine-specific content in deterministic artifacts.

Update current-state docs to record S091A as a correctness hotfix under Goal 010, but leave:

```text
content_generation_at_scale_artifact_verification: required
```

Do not recommend or create Goal 011.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~RulePackGameplayFamily|FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario content-generation-scale
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for:

- mojibake markers;
- absolute local paths;
- nondeterministic timestamps or GUIDs in deterministic artifacts;
- `S092|Goal 011|goal_011` outside this prohibition text.

## Stop Conditions

Stop with a blocker report instead of weakening acceptance if:

- three objective kinds cannot be honestly materialized through existing package/runtime primitives;
- declared event action kinds cannot be executed without a public schema/runtime contract change;
- runtime evidence would require an Application-side simulator;
- `.sln` or `.csproj` edits are required;
- a public GamePackage/runtime schema redesign is required;
- full verification exposes an unrelated pre-existing failure.

## Final Report

Report:

- root cause fixed for objective-kind and event-action coercion;
- changed files;
- objective-kind and event-action distributions;
- exact package audit results;
- exact runtime command/evidence correlation rules;
- runtime thread count and state deltas;
- invalid/fake/leak diagnostics, including new coercion/mismatch cases;
- artifact folder and deterministic hash;
- focused/smoke/full verification totals;
- confirmation that the gate remains `content_generation_at_scale_artifact_verification` required;
- confirmation that S092/Goal 011, public schemas, UI, Unity/export, Lua/provider/media, generator-library and project files were untouched.
