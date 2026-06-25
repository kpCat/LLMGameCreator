# Goal 007 Codex Task - Connected World Travel And Deterministic World State

## Routing

Use this task only after the user has accepted Goal 006 with:

```text
semantic_selected_runtime_composition_artifact_verification passed
```

This is a multi-slice Codex goal. Execute the goal and stop at one final gate:

```text
connected_world_travel_state_artifact_verification
```

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CONTEXT_INDEX.md`
5. `docs/GOAL_007_CONNECTED_WORLD_TRAVEL_AND_DETERMINISTIC_WORLD_STATE.md`
6. `docs/WORLD_TOPOLOGY_AND_CHUNKING_CONTRACTS.md`

Then read only the implementation files needed for the exact behavior:

- existing Goal 006 semantic runtime composition service/tests;
- existing runtime abstractions and runtime state/serializer files;
- existing package validator and runtime validator helpers;
- `.devflow/scripts/run-product-smoke.ps1`;
- current state docs tests.

Do not scan historical apply/readme packs.

## Goal

Implement a bounded connected-world/travel acceptance slice that proves:

```text
region graph + exact map bindings
-> deterministic travel across connected regions
-> bounded chunk/world structure
-> runtime-owned discovered/mutated state
-> deterministic save/load
```

This goal must remain headless. Do not add UI.

## Allowed Files

Primary allowed files:

- `docs/GOAL_007_CONNECTED_WORLD_TRAVEL_AND_DETERMINISTIC_WORLD_STATE.md`
- `src/LLMGameCreator.Application/Design/World/**`
- `src/LLMGameCreator.Application/Design/Semantics/SemanticSelectedRuntimeCompositionAcceptanceService.cs` only if Goal 006 evidence needs read-only reuse or a tiny public helper
- `src/LLMGameCreator.Application/RuntimePreview/**` only when reusing existing runtime preview acceptance helpers
- `tests/LLMGameCreator.Tests/Application/World/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/ConnectedWorldTravelSmokeTests.cs`
- `.devflow/scripts/run-product-smoke.ps1`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only if required for honest runtime-owned travel/state evidence:

- `src/LLMGameCreator.Runtime.Abstractions/**`
- `src/LLMGameCreator.Runtime/**`
- existing runtime serializer/snapshot files
- focused existing runtime tests

Hard-forbidden unless you stop and report a blocker first:

- `src/LLMGameCreator.GamePackage/**` public schema/model changes
- `src/LLMGameCreator.Domain/**` public package definition changes
- `src/LLMGameCreator.WinForms/**`
- `generator-library/**`
- `src/LLMGameCreator.Scripting/**`
- `src/LLMGameCreator.AssetPipeline/**`
- `.sln`
- `*.csproj`
- Unity/export/media/provider files
- unrelated docs or historical task packs

## Exact Behavior

### S064 - Goal 006 Gate Recording And Goal 007 Spine

Record in Goal 007 report/state docs that the user accepted:

```text
semantic_selected_runtime_composition_artifact_verification passed
```

Create a deterministic headless acceptance service for Goal 007. Suggested name:

```text
ConnectedWorldTravelAcceptanceService
```

Suggested output folder:

```text
.llmgc/procedural/connected-world-travel/
```

Suggested report files:

```text
connected-world-travel-report.json
connected-world-travel-report.md
connected-world-travel-verification.md
```

### S065 - Region Graph And Variable Map Bindings

Create internal deterministic sidecar records for:

- world profile;
- region graph;
- region node;
- region connection;
- region-to-map binding;
- map signature/dimensions;
- travel rule;
- validation diagnostic.

Do not add these to public GamePackage schema.

Generate at least four regions:

- a hub/start region;
- a wildland/frontier-style region;
- a mystery/dungeon-style region;
- a trade/caravan-style region.

These are scenario labels, not hardcoded gameplay behavior branches. Selection must remain data/config driven inside the deterministic acceptance service.

Each valid region must bind to an exact existing package map id. At least three valid region maps must differ by dimensions or layout signature.

Validate:

- duplicate region ids;
- missing start region;
- missing map binding;
- missing target region in a connection;
- disconnected required regions;
- duplicate connection ids;
- self-loop only graphs that cannot reach required regions.

### S066 - Deterministic Travel Execution

Add a bounded headless travel runtime/service/adapter that operates on runtime-owned state.

It must prove:

- current region/map before travel;
- connection selected;
- destination region/map after travel;
- travel command succeeded or failed;
- invalid travel is rejected by diagnostics/result status;
- repeated route execution is deterministic.

Travel must not be accepted by mutating report fields only. It must update a runtime-owned state object that is serialized and restored.

If existing runtime abstractions cannot honestly represent this, add the narrowest runtime-owned travel service/state needed. Do not redesign all runtime commands.

### S067 - Runtime-Owned World State And Save/Load

Runtime-owned state must include at minimum:

- world profile id;
- current region id;
- current map id;
- visited region ids;
- discovered connection ids;
- travel log entries;
- per-region evidence hash or state marker;
- deterministic state hash.

Save/load must restore exact required evidence. The tests must compare structured state, not only hashes.

### S068 - Bounded Chunk/World Structure

Add bounded deterministic chunk evidence, not infinite streaming.

Required:

- seed;
- rules version;
- chunk size;
- bounded chunk coordinates;
- deterministic chunk id/hash;
- boundary exits or edge compatibility markers;
- discovered chunk set;
- runtime chunk deltas.

Runtime chunk deltas may represent simple deterministic mutations, for example:

- discovered chunk;
- opened route marker;
- harvested marker;
- visited landmark marker.

They must be clearly runtime/save-only and not package/source content.

Validate:

- missing seed;
- missing rules version;
- invalid chunk size;
- incompatible adjacent boundary exits;
- runtime delta placed in source/package-side content.

### S069 - Invalid Scenario Rejection

Add invalid scenarios for:

- disconnected graph;
- missing region or map ref;
- chunk boundary/rules failure;
- runtime delta as immutable source content.

Invalid scenarios must:

- have `expectedValid = false`;
- have `actualValid = false`;
- include at least one error diagnostic with a stable code;
- not execute travel runtime;
- not contribute to accepted valid counts.

Expectation metadata must not itself cause rejection. Rejection must come from validators/audits.

### S070 - Smoke, State Docs And Final Gate

Add product smoke route:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario connected-world-travel
```

The smoke test must deserialize `connected-world-travel-report.json` and assert structured values:

- `accepted == true`;
- `manualGate == connected_world_travel_state_artifact_verification`;
- valid scenario count;
- invalid rejection count;
- at least one roundtrip route;
- variable maps;
- chunk delta persistence;
- save/load exact evidence;
- external execution flags all false.

Update state docs to stop at:

```text
connected_world_travel_state_artifact_verification
```

Do not mark the gate passed.

## Required Tests

Add focused tests covering at least:

1. valid connected world scenarios are accepted and deterministic;
2. every valid route step references a real connection and destination map binding;
3. graph reachability covers all required regions;
4. variable maps have distinct exact dimensions/signatures;
5. travel runtime changes current region/map through runtime-owned state;
6. invalid travel is rejected;
7. save/load restores exact travel/world evidence;
8. bounded chunk ids are deterministic from seed/rules/config;
9. runtime chunk deltas persist after save/load;
10. runtime deltas are not accepted as source/package content;
11. disconnected graph is rejected by validator diagnostics;
12. missing region/map ref is rejected by validator diagnostics;
13. chunk boundary/rules failure is rejected by validator diagnostics;
14. Goal 006 semantic runtime composition regressions still pass;
15. state docs tests pass.

Prefer a small number of test files with multiple focused tests.

## Validation Commands

Run these from the repository root using normal Windows/PowerShell paths:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~SemanticRuntimeComposition|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario connected-world-travel
.\.devflow\scripts\check-all.ps1
```

Also check changed files for mojibake markers such as `Рџ`, `Р”`, `Рє`, `â`, `�`.

Search for accidental next-goal work:

```powershell
Select-String -Path .\docs\**\*,.\src\**\*,.\tests\**\* -Pattern "S071","Goal 008","goal_008" -SimpleMatch -ErrorAction SilentlyContinue
```

Only expected references are guard/stop-condition text. Do not implement S071 or Goal 008.

## Path And Tool Rules

Use repository-relative paths or normal Windows/PowerShell paths only.

Do not use:

- Unix mount paths;
- Linux home paths;
- sandbox URI paths;
- fabricated Windows mount paths.

Do not use git commands. Do not create/switch/merge/rebase/cherry-pick/push branches.

## External Execution Rules

Do not call or require:

- LLM/provider;
- RAG index;
- ComfyUI/Fooocus/Suno/media providers;
- Lua execution/generators;
- Unity;
- WinForms UI.

All acceptance must be headless and deterministic.

## Stop Conditions

Stop with a blocker report if:

- public GamePackage schema redesign is required;
- Runtime/GamePackage serialization cannot preserve the required evidence without broad redesign;
- travel can only be faked as report-only metadata;
- chunk deltas cannot be clearly separated from immutable source content;
- invalid scenarios cannot fail through real diagnostics;
- full check fails due to unrelated pre-existing issues.

Do not weaken acceptance to make the report green.

## Final Report

Report all of the following:

- changed files;
- valid scenario ids and route summaries;
- invalid scenario ids and diagnostic codes;
- region graph size and reachability evidence;
- variable map ids/dimensions/signatures;
- travel runtime command/state evidence;
- save/load hashes and exact state comparison result;
- chunk config, bounded chunk ids and runtime delta evidence;
- regenerated artifact folder;
- focused test result;
- product smoke result;
- full check result;
- mojibake search result;
- confirmation that public GamePackage schema was not changed, or blocker details if it was required;
- confirmation that the final gate is `connected_world_travel_state_artifact_verification`;
- confirmation that S071/Goal 008 were not started.
