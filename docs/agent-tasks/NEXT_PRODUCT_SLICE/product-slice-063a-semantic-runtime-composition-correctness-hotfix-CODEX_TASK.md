# Product Slice 063A - Semantic Runtime Composition Correctness Hotfix

## Purpose

Repair false-positive binding and runtime acceptance found during external review of Goal 006.

This is a bounded correctness hotfix for the existing S059-S063 implementation. It is not S064 or Goal 007 and must keep `semantic_selected_runtime_composition_artifact_verification` as the only final gate.

## Starting State And Confirmed Defects

Goal 006 reports green tests, smoke and full verification, but the gate is not accepted.

External review found two connected correctness defects:

1. `ResolvePackageTargetId("npc/generated_contact")` returns `dialogue/semantic_selected`, while each materialized package contains only a scenario-specific id such as `dialogue/semantic_selected_core_genre_project_overlay`. The package validator does not detect this dangling binding.
2. A selected quest carries `RequiredInteractionPatternIds`, but the package materializes only the separately selected semantic interaction. For example, the project-overlay scenario selects `quest_pattern/two_step_sequence`, which requires `interaction/talk_contact` and `interaction/take_cache_item`, while the package contains only `interaction/use_reward_on_contact`. The test adapter then calls `AdvanceQuestObjective` directly, so acceptance can become green without materializing or executing the interactions required by the selected quest declaration.

The current report therefore does not yet prove the required chain:

```text
selected semantic declarations
-> fully resolved package bindings
-> corresponding successful runtime commands
-> runtime-owned quest/reward/completion evidence
```

## Context Budget

Read first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. this task
5. `docs/GOAL_006_SEMANTIC_SELECTED_RUNTIME_COMPOSITION.md`

Then read only:

- `src/LLMGameCreator.Application/Design/Semantics/SemanticSelectedRuntimeCompositionAcceptanceService.cs`
- `src/LLMGameCreator.Application/Design/Semantics/SemanticGuidedCompositionAcceptanceService.cs` only for source scenario semantics
- `src/LLMGameCreator.Application/RuntimePreview/QuestDialogInteractionFamilyAcceptanceService.cs` only for Goal 004 declarations/validation
- existing package validator and runtime services needed to verify exact target semantics
- Goal 006 focused tests and product smoke

Do not read historical task packs or broad strategy documents unless a concrete unresolved contract forces it.

## Allowed Files

Primary allowed files:

- `src/LLMGameCreator.Application/Design/Semantics/SemanticSelectedRuntimeCompositionAcceptanceService.cs`
- `tests/LLMGameCreator.Tests/Application/Semantics/SemanticRuntimeCompositionAcceptanceTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/SemanticRuntimeCompositionSmokeTests.cs`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md` only if its current-next-work wording needs correction

Conditionally allowed only when a focused failing test proves it necessary:

- `src/LLMGameCreator.Application/Validation/GamePackageValidator.cs` for a small generic dangling-reference validation that is valid for all packages
- an existing runtime test file for regression coverage of already-supported behavior

Do not change any other file without first reporting a concrete blocker. Do not edit solution or project files.

## Required Fixes

### 1. Materialize every interaction binding required by the selected composition

Build a deterministic interaction-binding set containing:

- the semantic-selected interaction declaration;
- every interaction id referenced by every selected quest objective's `RequiredInteractionPatternIds`;
- no duplicate ids;
- the exact validated Goal 004 declaration for every id.

If any referenced interaction declaration is absent, emit a deterministic error and reject composition before runtime.

Materialize all of these interactions into `Game.Interactions`. Do not silently discard objective-required interaction ids. A narrow internal plan-record extension is allowed; do not change the public GamePackage schema.

### 2. Resolve source refs to exact ids that actually exist in the package

Replace suffixless placeholder mappings with scenario-aware exact mappings.

At minimum, resolve and audit:

- `npc/generated_contact` to the actual package entity/NPC id required by the relevant runtime contract, with the exact scenario dialogue id in dialogue metadata where required;
- `quest/generated_goal` to `plan.PackageQuestId`;
- item refs to actual package item ids;
- encounter refs to the actual package encounter id;
- object refs to an actual package entity id;
- dialogue, quest, objective, interaction and required-item refs to exact materialized ids.

Do not treat an allow-listed source ref as resolved merely because it appears in `BuildContentRefs`.

### 3. Add an independent materialized-binding audit before runtime

After package construction and before runtime execution, inspect the actual package object and reject with stable diagnostics if any selected binding is missing or mismatched.

The audit must verify:

- selected quest and every selected objective exist;
- every objective target resolves according to its package/runtime kind;
- every objective-required interaction exists in the package;
- semantic-selected interaction exists;
- each materialized interaction's target, dialogue, encounter and required-item references resolve;
- selected dialogue and its quest/objective references resolve;
- package/provenance ids equal the plan ids.

An error from this audit must make `ActualValid = false`, prevent runtime execution and prevent a runnable package from being counted. Add a focused fixture proving that the previous `dialogue/semantic_selected` placeholder is rejected.

### 4. Make runtime evidence prove the selected behavior rather than trust adapter claims

The production acceptance service must validate structured adapter evidence instead of accepting only copied ids and a boolean.

For a valid scenario require evidence that:

- runtime start succeeded;
- the exact package hash and package id are observed;
- the selected quest was started and reached the expected completed/progress state;
- every objective-required interaction was attempted against its resolved target and succeeded, or an explicit existing runtime primitive was used and correlated to that objective;
- the semantic-selected interaction was attempted and succeeded;
- the selected dialogue was opened and its relevant option executed successfully;
- every command required for acceptance succeeded;
- all selected quest objectives have runtime-owned evidence, not merely `Any(objective.Completed)`;
- declared reward/completion state is present and is attributable to the executed package;
- save/load restored the same required state evidence.

`AdvanceQuestObjective` may be retained only as an existing explicit runtime primitive after the corresponding required interaction succeeds and the evidence records that correlation. It must not substitute for an absent binding, absent interaction or failed command.

Add a negative test adapter that copies all selected ids and sets `SemanticSelectedIdsExecutedInRuntime = true` but omits/falsifies command or state evidence. The overall report must reject it.

### 5. Record meaningful state deltas

Extend structured runtime evidence with deterministic before/after or equivalent explicit fields for the state actually required by the selected package:

- quest state and every objective state;
- inventory/reward amounts;
- completion flags;
- encounter state when used;
- active/closed dialogue evidence;
- current package and map identity.

Do not accept reward/completion merely because the reward item was already pre-seeded in the starting inventory. Assert an actual delta or another unambiguous runtime-owned completion output.

### 6. Prove cross-variant isolation, not only distinct hashes

Distinct hashes alone do not prove isolation.

Run the required variants sequentially and verify each runtime evidence set contains only its own:

- package id/hash;
- quest/dialogue ids;
- selected declaration ids;
- scenario-specific flags/state;
- objective/interaction correlation records.

Add a negative regression fixture that injects a previous scenario id/state into later evidence and makes isolation fail.

### 7. Keep invalid rejection causally correct

Preserve the real `semantic_guided.excludes_conflict` rejection. The invalid scenario is accepted by the matrix only when:

- actual composition diagnostics contain an error caused by the invalid binding/conflict;
- no package is counted as runnable;
- runtime was not attempted.

`ExpectedValid = false` remains expectation metadata and must never itself cause actual rejection.

## Required Tests

Add focused behavioral tests for at least:

1. all selected quest objective target refs resolve to exact materialized package ids;
2. the project-overlay two-step quest materializes both `interaction/talk_contact` and `interaction/take_cache_item` in addition to `interaction/use_reward_on_contact`;
3. the old suffixless `dialogue/semantic_selected` target is absent and a fixture using it is rejected;
4. missing objective-required interaction prevents runtime execution;
5. fake adapter success without command/state proof is rejected;
6. failed required command makes scenario/runtime acceptance false;
7. all objectives, not merely one, have runtime-owned completion/progress evidence;
8. declared reward/completion produces unambiguous state evidence or delta;
9. save/load preserves the exact required evidence;
10. injected previous-variant state makes isolation fail;
11. invalid conflict rejection still comes from `semantic_guided.excludes_conflict` and does not run runtime;
12. repeated plan/package/runtime report output is byte/hash stable;
13. Goal 004 and Goal 005 focused regressions remain green.

Tests must deserialize/assert structured values. Product smoke must not rely only on `Assert.Contains` for top-level `true` strings; assert the critical scenario and evidence invariants structurally.

## Artifacts And State

Regenerate the existing artifacts under:

```text
.llmgc/procedural/semantic-runtime-composition/
```

Do not invent a parallel report folder.

Update state docs to record S063A correctness repair while keeping the only active gate:

```text
semantic_selected_runtime_composition_artifact_verification
```

Do not mark that gate passed. Do not recommend or create S064/Goal 007 in this run.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~SemanticRuntimeComposition|FullyQualifiedName~SemanticGuidedComposition|FullyQualifiedName~QuestDialogInteractionFamily|FullyQualifiedName~GeneratedPackageMvp|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario semantic-runtime-composition
.\.devflow\scripts\check-all.ps1
```

Also search changed files for mojibake markers and report the result.

## Stop Conditions

Stop and report a blocker instead of weakening acceptance if:

- a selected Goal 004 declaration cannot be represented by existing package/runtime primitives;
- fixing the chain requires a public GamePackage or runtime command/state contract redesign;
- an existing runtime command cannot honestly execute a required interaction family;
- the full suite exposes an unrelated pre-existing failure.

Do not convert an unsupported binding into report-only success. Do not add a new runtime command family under this hotfix.

## Hard Limits

- No S064 or Goal 007.
- No Runtime Preview UI work.
- No public GamePackage/runtime schema redesign.
- No LLM, RAG, provider, arbitrary Lua, Unity or media execution.
- No genre/project/term-specific C# branches.
- No unrelated refactor.
- No git commands.
- Use repository-relative Windows/PowerShell paths only; do not use `/mnt`, `/home/oai`, `sandbox:/...` or fabricated `C:\mnt` paths.

## Final Report

Report:

- root cause and exact correction for each binding/runtime false positive;
- changed files;
- the materialized interaction set for every required valid scenario;
- exact objective target and required-interaction audit results;
- runtime command-to-objective correlations and state deltas;
- negative fixtures that now fail correctly;
- focused/smoke/full verification results;
- regenerated artifact folder;
- confirmation that public package/runtime contracts were not changed, or stop-condition details if they would be required;
- confirmation that the gate remains `semantic_selected_runtime_composition_artifact_verification` and S064/Goal 007 were not started.
