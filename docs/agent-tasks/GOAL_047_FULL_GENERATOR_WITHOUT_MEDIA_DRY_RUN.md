# Codex task — GOAL 047 Full Generator Without Media Dry Run

## Assignment metadata

Repository:

```text
https://github.com/kpCat/LLMGameCreator
```

Working copy:

```text
C:\Users\endim\LLMGameCreator\
```

Branch:

```text
main
```

Composite goal id/name:

```text
goal-047-full-generator-without-media-dry-run-v1
Goal 047: Full Generator Without Media Dry Run
```

Codex reasoning level:

```text
very high
```

Final gate:

```text
full_generator_without_media_verification required
```

## Mandatory process change

This task must commit and push final state to `origin/main` regardless of GREEN/BLOCKED/FAILED result.

Use honest commit messages:

- `GREEN Goal 047 full generator without media dry run`
- `BLOCKED Goal 047 full generator without media dry run`
- `FAILED Goal 047 full generator without media dry run`

Do not mark `full_generator_without_media_verification` passed inside this goal.

## Starting preflight

1. Confirm current branch is `main`.
2. Confirm working tree state. If untracked task-source files from the prompt pack exist, include the Goal 047 task/spec/scouting/launcher files in the final commit unless local repository policy clearly forbids task-source files.
3. Read current state and queue.
4. Record in docs that Goal 043 is accepted by this user handoff before Goal 047:

```text
multi_family_generated_template_vertical_slice_verification passed
```

5. Preserve Goal 031 and Goal 032 as produced-for-review/not passed unless current docs already say otherwise.
6. Do not start any future goal after Goal 047.

## Read-first list

Read these first, in order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_047_FULL_GENERATOR_WITHOUT_MEDIA_DRY_RUN_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_047_FULL_GENERATOR_WITHOUT_MEDIA_DRY_RUN.md`
8. `docs/GOAL_043_MULTI_FAMILY_GENERATED_TEMPLATE_VERTICAL_SLICE_SPEC.md`
9. Goal 034-043 evidence folders under `.llmgc/procedural/`:
   - `goal-034-strict-llm-draft-artifact-loop`
   - `goal-035-lua-module-manifest-registry`
   - `goal-036-lua-sandbox-execution-gate`
   - `goal-037-hybrid-llm-draft-lua-deterministic-expansion`
   - `goal-038-world-scale-region-map-foundation`
   - `goal-039-runtime-chunk-delta-traversal-smoke`
   - `goal-040-chunked-runtime-preview-export-multifamily-smoke`
   - `goal-043-multi-family-generated-template-vertical-slice`
10. Existing Application/test patterns around:
   - `Design/MultiFamilyGeneratedTemplateVerticalSlice`
   - `Design/ChunkedRuntimePreviewExportSmoke`
   - `Design/RuntimeChunkDeltaTraversal`
   - `Design/UnityRuntimeExport` if present
   - `Design/GameProfiles`
   - package assembly acceptance services from Goals 025-028 if present.
11. Existing `GamePackage` validators/assembler consumers only as needed. Do not change public schema.

## Allowed files / areas

You may create or edit:

```text
docs/GOAL_047_FULL_GENERATOR_WITHOUT_MEDIA_DRY_RUN_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_047_FULL_GENERATOR_WITHOUT_MEDIA_DRY_RUN.md
docs/agent-tasks/GOAL_047_FULL_GENERATOR_WITHOUT_MEDIA_DRY_RUN.md
docs/agent-tasks/GOAL_047_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/FullGeneratorWithoutMediaDryRun/**
tests/LLMGameCreator.Tests/Application/FullGeneratorWithoutMediaDryRun/**
tests/LLMGameCreator.Tests/ProductSmoke/FullGeneratorWithoutMediaDryRunProductSmokeTests.cs
.llmgc/procedural/goal-047-full-generator-without-media-dry-run/**
```

You may read existing Application package assembly/runtime-preview/export services. If a tiny compatibility call or adapter is required inside the new `Design/FullGeneratorWithoutMediaDryRun/**` seam, keep it there.

## Conditional allowed files

Only if unavoidable for product-smoke routing or artifact-scope guard, and only with a tiny documented diff:

```text
tests/LLMGameCreator.Tests/Devflow/**
tests/LLMGameCreator.Tests/**CurrentState*Tests*.cs
```

Do not touch these if not needed.

## Forbidden files / areas

Do not modify:

```text
src/LLMGameCreator.GamePackage/** public schema/model definitions
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Also forbidden:

- new external dependencies;
- media generation;
- live provider/LLM/RAG calls;
- arbitrary Lua input/execution beyond already accepted bounded Goal 037 fixtures;
- Unity Editor/build execution;
- final prose generation as accepted content;
- weakening acceptance/evidence tests.

## Exact behavior

### 1. Source loading and dry-run manifest

Create a deterministic source loader that consumes Goal 034-043 artifacts by relative paths and hashes. It must not copy prior JSON blobs blindly into new evidence; it should summarize refs, hashes, counts and selected ids.

Produce a `FullGeneratorDryRunManifest` with:

- goal/source artifact ids;
- accepted/preflight gates;
- selected family ids;
- profile/capability refs;
- selected world/chunk/runtime refs;
- selected template loop refs;
- selected draft/Lua expansion refs where relevant;
- media policy: `without_media`;
- deterministic ordering key.

### 2. Review and promotion workflow hardening

Implement a small in-house state machine / transition table for generated artifact review:

- `candidate_loaded`
- `validated`
- `repair_required`
- `approved_for_dry_run`
- `promoted_to_preview_payload`
- `promoted_to_export_candidate`
- `blocked`
- `rejected`

The workflow must be deterministic, serializable and evidence-friendly.

It must record:

- transition id;
- source artifact id;
- before/after state;
- required evidence hash;
- reviewer/provenance kind (`programmatic`, `user_handoff`, `inherited`, `llm_quarantined`, `lua_bounded`, `manual_candidate`);
- diagnostics;
- promotion decision.

### 3. Repair diagnostics hardening

Implement a diagnostic normalizer and repair planner for dry-run blocking issues.

At minimum cover:

- missing source artifact;
- hash mismatch;
- missing family loop;
- missing runtime preview payload;
- missing export profile;
- unresolved profile/capability ref;
- rejected candidate provenance;
- final prose leakage;
- provider/LLM/RAG leakage;
- media leakage;
- Unity/runtime source mutation claim;
- GamePackage schema mutation claim;
- nondeterministic ordering;
- cross-family leakage.

Each diagnostic must map to a bounded repair action or explicit `manual_required` action.

### 4. Multi-family dry-run lifecycle

For each family:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

build a dry-run record with:

- family profile refs;
- scenario/family lens refs from Goal 043;
- region/chunk/runtime traversal refs from Goals 038-040;
- review/promotion ledger refs;
- generated system coverage: world/entity/quest/dialogue/item/economy/combat/progression/settlement/event where available;
- runtime-preview-compatible payload summary;
- export-candidate payload summary;
- package compatibility or materialization summary;
- replay/hash proof.

Minimum GREEN condition: three families must produce distinct dry-run records through the same code path, with at least one state-changing loop proof each.

### 5. Package compatibility or materialization proof

Use existing package assembly/validation seams if safely available.

GREEN requires one of these, in priority order:

1. Actual validator-clean package materialization for each family through existing public schema and existing Application services; or
2. A strict package-compatibility proof that maps each selected family dry-run output to existing package fields/assemblers and proves why direct materialization is not yet safely available.

If neither is possible without forbidden changes, status must be BLOCKED. Do not fake package support.

### 6. Runtime preview / export validation

Build validation over existing preview/export-compatible payloads without modifying WinForms/UI/Runtime/Unity.

For each family, prove:

- payload has stable relative refs;
- source hashes match prior artifacts;
- command/state transition summaries are internally consistent;
- chunk/window refs are within declared bounds;
- export profile selection is deterministic;
- no media/provider/runtime source mutation claims.

### 7. One-click full generator dry-run service

Implement an Application-layer orchestration service, for example:

```text
FullGeneratorWithoutMediaDryRunService.RunDefaultScenarios()
```

It should produce all evidence in one deterministic call from selected default profiles/families. No UI/CLI is required.

### 8. Evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-047-full-generator-without-media-dry-run/
```

Required files:

```text
dry-run-source-manifest.json
review-promotion-ledger.json
repair-diagnostics-matrix.json
family-map-panel-rpg-dry-run.json
family-survival-sandbox-dry-run.json
family-first-person-grid-dungeon-dry-run.json
runtime-preview-validation-matrix.json
export-profile-selection-matrix.json
package-compatibility-or-materialization-summary.json
one-click-dry-run-summary.json
invalid-fake-leak-matrix.json
full-generator-without-media-report.md
```

Evidence must be deterministic, compact, JSON parseable, no absolute paths, no timestamps unless an existing deterministic convention is present.

### 9. Invalid/fake/leak matrix

At minimum include causal mutations for:

- missing Goal 043 source;
- wrong accepted gate;
- fake family id;
- duplicate promotion transition id;
- invalid transition order;
- missing repair action;
- hash mismatch;
- cross-family source leakage;
- missing state-changing loop;
- final prose promoted as content;
- provider/LLM/RAG call claim;
- media generated claim;
- Runtime source changed claim;
- Unity executed claim;
- GamePackage schema mutation claim;
- unsafe absolute path;
- nondeterministic ordering.

### 10. State/context/queue docs

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Record:

- Goal 043 accepted by this user handoff;
- Goal 047 produced for review;
- `full_generator_without_media_verification required`;
- Goal 031 and Goal 032 remain produced-for-review/not passed if that is current policy;
- next work should be media pipeline / Unity profile hardening / editor workflow only after Goal 047 review, depending on evidence.

## Tests

Add focused tests for:

- source manifest loading;
- review/promotion transitions;
- repair diagnostic mapping;
- per-family dry-run records;
- runtime-preview validation matrix;
- export profile selection matrix;
- package compatibility/materialization summary;
- invalid/fake/leak matrix;
- deterministic evidence writing;
- product smoke one-click dry run.

Suggested names:

```text
FullGeneratorDryRunSourceManifestTests
FullGeneratorReviewPromotionWorkflowTests
FullGeneratorRepairDiagnosticsTests
FullGeneratorFamilyDryRunTests
FullGeneratorRuntimePreviewValidationTests
FullGeneratorPackageCompatibilityTests
FullGeneratorWithoutMediaEvidenceTests
FullGeneratorWithoutMediaInvalidMatrixTests
FullGeneratorWithoutMediaDryRunProductSmokeTests
```

## Validation commands

Run from repository root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~FullGeneratorWithoutMedia|FullyQualifiedName~Goal047"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~FullGeneratorWithoutMediaDryRunProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal047|FullyQualifiedName~FullGenerator"

.\.devflow\scripts\check-all.ps1
```

Then run existing artifact scope guard for Goal 047 if the repository has the standard command/policy pattern. Do not invent unrelated scripts.

Also inspect artifacts directly:

```powershell
Get-ChildItem .\.llmgc\procedural\goal-047-full-generator-without-media-dry-run -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-047-full-generator-without-media-dry-run\full-generator-without-media-report.md -TotalCount 120
```

Run JSON parse check for all `.json` files in the Goal 047 artifact folder.

## Bounded repairs pre-authorized

You may perform these bounded repairs without stopping:

1. Update stale current-state/handoff guard tests if they hardcode the previous latest gate and fail after the legitimate Goal 047 state update.
2. Update `.devflow/artifact-scope/artifact-scope-policy.json` for the new Goal 047 artifact folder and docs/code/test paths.
3. Restore accidental tracked historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates unrelated old evidence.
4. Repair deterministic ordering/JSON formatting/mojibake inside changed Goal 047 files.

Every bounded repair must be listed in the final report.

## Stop conditions

Commit/push final state even when stopping.

Use BLOCKED if:

- package materialization/compatibility proof cannot be done without forbidden changes;
- runtime-preview/export validation cannot consume existing evidence honestly;
- `check-all.ps1` fails after bounded repairs;
- the implementation would need public GamePackage schema changes;
- the implementation would need Runtime/UI/Unity source changes;
- the result is only a report and lacks three family dry-run proofs.

Use FAILED if:

- restore/build cannot complete due to changes made in this task;
- the code is left in a known broken compile state.

## Git policy

Allowed inspection:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit paths>
git diff --stat --cached
```

Final commit/push is mandatory:

```powershell
git add <explicit changed Goal 047 paths>
git commit -m "GREEN Goal 047 full generator without media dry run"
git push origin main
```

or:

```powershell
git commit -m "BLOCKED Goal 047 full generator without media dry run"
```

or:

```powershell
git commit -m "FAILED Goal 047 full generator without media dry run"
```

Forbidden:

```text
git checkout
git reset
git clean
git stash
git merge
git rebase
git cherry-pick
git push --force
```

## Final report format

Report in Russian:

```text
Goal 047 выполнен / заблокирован / провален
Status: GREEN / BLOCKED / FAILED
Gate: full_generator_without_media_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<review workflow / repair diagnostics / dry-run / runtime preview / export profile / package compatibility>

Evidence artifacts:
<список>

Family dry-run proof:
map_panel_rpg: <summary>
survival_sandbox: <summary>
first_person_grid_dungeon: <summary>

Package proof:
<materialized or compatibility proof, or blocker>

Проверки:
<commands/results>

Invalid/fake/leak matrix:
<summary>

Bounded repairs:
<none/list>

Git:
<commit hash/push result>

Ограничения:
<forbidden areas not touched>

Следующий разумный шаг:
<media/editor/Unity/profile hardening or blocker repair>
```
