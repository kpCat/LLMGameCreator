# Navigation Pathfinding Candidate Contract Note

Status: candidate design only
Candidate id: `candidate_navigation_pathfinding_v1`
Base accepted gate: `modular_generator_kernel_parallel_readiness_verification passed`
Final candidate status target: `candidate_ready_for_serial_adoption`

## Purpose

This candidate proposes an LLMGameCreator-owned navigation/pathfinding contract for future serial adoption. It does not implement production navigation, does not integrate Recast/Detour, does not modify runtime commands/state, does not change public `GamePackage` schema, and does not claim an accepted manual gate.

The contract is meant to support later runtime/export work by separating:

- package/generated navigation intent;
- deterministic internal path queries;
- optional future native/Unity/navmesh adapters;
- validation and absence diagnostics.

## Repo-Local Evidence Reused

- `docs/WORLD_TOPOLOGY_AND_CHUNKING_CONTRACTS.md` already names `path_network_v1`, `reachability_report_v1`, grid/region/chunk topology and blocked-path diagnostics.
- `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md` already names pathfinding profiles: `grid_4way`, `grid_8way`, `navmesh_like_2d`, `waypoint_graph`, `region_graph`, `chunk_aware_pathfinding`, `tactical_grid_pathfinding`, `first_person_grid_movement`, `conveyor_logistics_routing` and `city_agent_pathing`.
- `docs/RUNTIME_MODEL.md` and `src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs` keep runtime movement command/state separate from generation and external providers.
- `src/LLMGameCreator.Runtime/DefaultGameRuntime.cs` has the current deterministic local tile movement behavior: bounds check, walkable tile check and collidable entity blocking.
- `src/LLMGameCreator.Application/Design/World/ConnectedWorldTravelAcceptanceService.cs` and its tests provide the closest world/navigation analog: region graph reachability, route binding evidence, deterministic report hashes, invalid scenario rejection and runtime-owned state evidence.
- `docs/PROCESS_TASK_MODULAR_CONTRACT_GOAL_POLICY_ADOPTION.md` explicitly notes that pathfinding proof should not overfit one caravan route and should include an independent `npc_city_walk`-style consumer.

No existing repo-local full pathfinding or navmesh helper was found. Existing behavior is movement, travel graph/reachability and Unity movement proof, not reusable path planning.

## Proposed Internal Contract

Contract id: `navigation_query_contract_v1`

Candidate-owned future module id:

```text
candidate_navigation_pathfinding
```

Candidate-owned future artifact root:

```text
.llmgc/procedural/candidate-navigation-pathfinding-v1
```

The query contract should be data-only:

- `queryId`
- `profileId`
- `topologyId`
- `mapId` or `regionGraphId`
- `start`
- `goal`
- `agentProfileId`
- `movementMode`
- `blockedCellRefs`
- `dynamicObstacleRefs`
- `costProfileId`
- `maxExpandedNodes`
- `determinismPolicyId`

The result contract should be data-only:

- `queryId`
- `status`
- `pathSteps`
- `totalCost`
- `expandedNodeCount`
- `usedFallback`
- `adapterKind`
- `diagnostics`
- `deterministicHash`

Allowed statuses:

- `path_found`
- `path_absent`
- `start_invalid`
- `goal_invalid`
- `topology_missing`
- `adapter_unavailable`
- `validation_failed`
- `future_required`

## Grid A* Fallback

The default fallback should be an LLMGameCreator-owned deterministic grid A* reference algorithm when the topology is grid-like and no navmesh adapter is available.

Required deterministic rules:

- use integer coordinates and integer movement costs;
- use 4-way movement for `pathfinding/grid_4way`;
- use 8-way movement only when diagonal movement is explicitly selected;
- forbid diagonal corner cutting unless a future contract explicitly allows it;
- sort neighbors by a contract-defined order;
- sort open-set candidates by `fCost`, then `hCost`, then `y`, then `x`, then direction ordinal, then insertion ordinal;
- use Manhattan distance for 4-way grids;
- use scaled octile distance for 8-way grids;
- return the same path for repeated identical inputs;
- include `deterministicHash` over normalized query, normalized topology signature, normalized path and diagnostics.

Recommended 4-way neighbor order:

```text
up, right, down, left
```

Recommended 8-way neighbor order:

```text
up, up_right, right, down_right, down, down_left, left, up_left
```

The fallback is not a substitute for full navmesh navigation. It is the reference path for deterministic proof, small grid maps, first-person grid movement, tactical grids and simple generated city/walk fixtures.

## Navmesh And Export Boundary

Recast/Detour and Unity AI Navigation should remain behind adapters until a serial adoption task approves them.

Future boundary:

```text
navigation_query_contract_v1
  -> INavigationPathQueryAdapter
  -> DeterministicGridAStarAdapter
  -> optional DetourNavmeshAdapter
  -> optional UnityNavigationExportAdapter
```

Direct dependency rules:

- Recast/Detour direct native integration requires a kernel or serial adoption task.
- Unity AI Navigation integration requires a Unity runtime/export task.
- Any `.csproj`, `.sln`, native packaging or Unity build entrypoint change is out of candidate scope.
- Navmesh output can be proposed as a sidecar/export artifact, not as a public `GamePackage` schema mutation.

## Absence Behavior

Absence must be explicit and diagnostic-rich:

- missing optional navmesh adapter: `adapter_unavailable`, then grid fallback only if the topology supports it;
- missing required topology: `topology_missing`;
- missing path network: `future_required` for non-grid profiles, or fallback grid reachability when grid cells are present;
- invalid start or goal: reject before searching;
- blocked target: `path_absent`, not fake success;
- unsupported profile: `future_required`;
- missing optional candidate module during kernel compatibility: `absent_optional`.

No absence path may silently report success, mutate runtime state, execute Unity, call LLM/RAG/provider/media, or create generated output without deterministic validation.

## Validation Strategy

Focused future validation should cover:

- known pathfinding profile ids from `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md`;
- grid bounds, blocked cells, diagonal corner rules and start/goal validity;
- path step adjacency and allowed movement mode;
- total-cost consistency;
- deterministic replay for repeated query inputs;
- stable tie-breaking on symmetric maps;
- no path for disconnected or fully blocked goals;
- independent consumer fixtures, including `npc_city_walk` and a non-city grid/region consumer;
- no external execution flags;
- adapter absence and fallback diagnostics;
- artifact root containment under `.llmgc/procedural/candidate-navigation-pathfinding-v1`.

## Non-Goals

- No production navigation implementation in this candidate.
- No direct Recast/Detour native dependency.
- No accepted .NET pathfinding package dependency.
- No public `GamePackage` schema change.
- No `.sln` or `.csproj` change.
- No WinForms UI.
- No Unity runtime/build entrypoint change.
- No provider, media, RAG, LLM, Lua execution or network dependency.
- No modification of `docs/CURRENT_GENERATOR_STATE.*`, `docs/CONTEXT_INDEX.md` or `docs/FULL_GENERATOR_GOAL_QUEUE.md`.
- No claim that `candidate_navigation_pathfinding_v1` is an accepted product gate.

## Serial Adoption Recommendation

If adopted serially, implement the smallest deterministic proof first:

1. Add candidate-owned Application-layer models and a deterministic grid A* reference adapter under `Design/CandidateModules/NavigationPathfinding`.
2. Add 2-3 focused tests for deterministic tie-breaking, unreachable target diagnostics and adapter absence behavior.
3. Add a candidate product-smoke scenario manifest only if the manifest can point at real focused tests and a deterministic report artifact.
4. Keep Recast/Detour as reference only until a separate native packaging or Unity/export task selects it.

Stop with `requires_kernel_or_serial_adoption_task` if adoption needs shared kernel files, public schema, runtime state/command changes, `.csproj`, `.sln`, Unity runtime/build entrypoints or native dependency packaging.
