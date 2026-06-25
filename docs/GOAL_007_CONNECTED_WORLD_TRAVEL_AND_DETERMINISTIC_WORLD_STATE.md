# Goal 007 - Connected World Travel And Deterministic World State

## Purpose

Goal 007 starts after the user accepted:

```text
semantic_selected_runtime_composition_artifact_verification passed
```

Goal 006 proved that semantic-selected quest/dialogue/interaction declarations can be materialized into a validator-clean package and executed headlessly with runtime-owned state evidence. Goal 007 must extend the generated playable/simulatable loop from a single generated map/package scenario into a bounded connected world proof:

```text
generated regions
-> exact region/map bindings
-> deterministic travel between connected regions
-> variable map dimensions/layouts
-> bounded chunk/world structure
-> runtime-owned discovered/mutated world state
-> deterministic save/load roundtrip
```

This goal is intentionally not full infinite-world streaming and not Unity. It is a bounded, headless product vertical slice that proves the next world/travel primitive without changing public GamePackage schema.

## Final Gate

Stop at exactly one final gate:

```text
connected_world_travel_state_artifact_verification
```

Do not create Goal 008 or any post-Goal-007 work in this run.

## Product Slices Inside This Goal

Use these slice ids in docs/report evidence. The exact class names may vary, but the behavior must be covered.

- S064: record Goal 006 gate as passed and create a deterministic connected-world acceptance spine.
- S065: region graph, variable map and exact binding materialization.
- S066: deterministic travel execution across connected regions.
- S067: runtime-owned world/session state, discovered-region and travel-log evidence.
- S068: bounded chunk/world structure and runtime-delta ownership proof.
- S069: invalid graph/reference/chunk/delta rejection.
- S070: product smoke, state docs and final verification artifacts.

## Required Outcome

A valid Goal 007 run must produce deterministic artifacts under:

```text
.llmgc/procedural/connected-world-travel/
```

Required files:

```text
connected-world-travel-report.json
connected-world-travel-report.md
connected-world-travel-verification.md
```

The JSON report must be structured enough that tests can deserialize and assert fields without string contains checks.

## Non-Goals

- No Runtime Preview UI.
- No WinForms Designer work.
- No Unity, Unity archive, Unity project or Windows build work.
- No LLM, RAG, provider, ComfyUI, Suno or media execution.
- No Lua execution or generator execution.
- No GamePackage public schema redesign.
- No broad item/equipment/crafting/economy/combat/faction work.
- No infinite-world mature streaming.
- No provider-specific scene objects.
- No genre/project/term-specific C# branches.

## Explicit Narrow Runtime Permission

Goal 007 is allowed to add or extend runtime-owned world/travel state and a bounded travel primitive only when the implementation proves it is needed for honest runtime evidence.

Allowed runtime changes must be narrow:

- a travel command or service for moving between exact connected region/map ids;
- serializable world/travel runtime state such as current region, visited regions, discovered connections, travel log and runtime chunk deltas;
- deterministic save/load support for that state;
- tests proving invalid travel is rejected.

Do not redesign the public GamePackage contract. If a public GamePackage schema change appears necessary, stop and report a schema blocker instead of implementing it.

## Data Ownership Rules

Immutable generated/source content may include:

- world profile sidecar;
- region graph sidecar;
- region-to-map binding sidecar;
- bounded chunk rule/config sidecar;
- deterministic seed/config/rules version;
- authored map ids and connection ids.

Runtime/save-only state may include:

- current region and current map;
- visited/discovered regions;
- travel history;
- discovered chunks;
- opened/harvested/mutated chunk deltas;
- per-region runtime evidence hashes.

Runtime deltas must never be counted as immutable package/source content.

## Required Valid Scenarios

Build at least four deterministic valid scenarios:

1. `connected_world_core_route`
   - Start in a hub region.
   - Travel to a neighboring region and back.
   - Prove current region/map changes and save/load preserves the final state.

2. `connected_world_branching_route`
   - At least four regions.
   - At least one branching choice from the hub.
   - Prove all required regions are reachable from the start.

3. `connected_world_variable_maps`
   - At least three regions have different map dimensions and/or deterministic layout signatures.
   - Prove each region maps to an exact existing `GameDefinition.Maps` entry.

4. `connected_world_chunk_delta_persistence`
   - Use a bounded chunk grid around at least two regions.
   - Prove deterministic chunk ids from seed/rules/config.
   - Record discovered/mutated runtime chunk deltas.
   - Save/load must preserve exact delta evidence.

The scenarios may reuse the existing Goal 006 semantic-selected package/runtime composition as a source of quest/dialogue/interaction proof, but Goal 007 must add world/travel evidence. It must not merely re-label Goal 006 as world travel.

## Required Invalid Scenarios

Add deterministic invalid scenarios that fail for real diagnostics:

1. `invalid_disconnected_region_graph`
   - A required region is unreachable from the start.

2. `invalid_missing_region_or_map_ref`
   - A connection or binding references a missing region/map.

3. `invalid_chunk_boundary_or_rules`
   - Adjacent chunks have incompatible boundary exits, or chunk config is missing seed/rules version.

4. `invalid_runtime_delta_as_source`
   - Runtime chunk deltas are placed in immutable source/package-side content.

Invalid scenarios must not execute runtime travel and must not count as accepted.

## Required Evidence In The Report

For every valid scenario include:

- scenario id, seed and deterministic hash;
- world profile id and rules version;
- region ids and region graph edges;
- start region id and final region id;
- exact region-to-map binding list;
- map dimensions/signature per region;
- route steps with from-region, to-region, connection id and command result;
- graph connectivity/reachability result;
- chunk config and bounded chunk ids;
- runtime delta evidence, clearly marked runtime/save-only;
- runtime state hash and restored runtime state hash;
- save/load roundtrip boolean;
- artifact/report deterministic hash;
- diagnostics.

For the overall report include:

- `accepted`;
- `manualGate = connected_world_travel_state_artifact_verification`;
- `goal006GateRecorded = true`;
- `externalExecution` flags all false;
- valid/invalid scenario counts;
- deterministic replay result;
- travel runtime execution result;
- save/load result;
- invalid rejection result;
- remaining primitive limits.

## Acceptance Criteria

Goal 007 is accepted only when:

- Goal 006 gate is recorded as passed from the user prompt.
- At least four valid connected-world scenarios pass.
- At least four invalid connected-world scenarios fail with error diagnostics.
- Every valid region graph is connected from its start region.
- Every route step references an existing connection and exact destination region/map binding.
- Runtime travel changes current region/map through the bounded travel primitive or service, not by report-only field edits.
- Runtime travel rejects missing/disconnected destinations.
- Variable maps are real package maps or exact runtime map bindings, not just names in a report.
- Chunk ids are deterministic from seed/rules/config and are bounded.
- Runtime chunk deltas are persisted only in runtime/save evidence.
- Save/load restores the exact world/travel/chunk evidence.
- Repeated identical builds are byte/hash stable.
- Product smoke deserializes JSON and asserts critical structured fields.
- `check-all.ps1` passes.
- No S071 or Goal 008 work is created.

## Expected Final State Update

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md` if current-next-work wording must move from Goal 007 gate to the next stop marker.

The state docs must record:

```text
goal_007_connected_world_travel_and_deterministic_state
```

The active gate must become:

```text
connected_world_travel_state_artifact_verification
```

Do not mark that gate passed. Do not recommend Goal 008 until the user/assistant accepts the Goal 007 gate after reviewing the pushed code.

## Stop Conditions

Stop and report a blocker instead of weakening acceptance if:

- connected travel cannot be represented honestly without a public GamePackage schema redesign;
- a runtime state/serialization change would require broad runtime architecture replacement;
- a travel command/service cannot be made deterministic;
- save/load cannot preserve the world/travel/chunk evidence;
- invalid graph/reference/chunk cases can only be rejected by expectation metadata instead of real diagnostics;
- full verification exposes an unrelated pre-existing failure.

Do not convert unsupported travel into report-only success.
