# Candidate Navigation Route Graph v1

Candidate name: `candidate_navigation_route_graph_v1`

Lane: `lane-d`

Status: candidate-owned implementation, not serial adoption.

## Goal

Add a deterministic high-level route graph planner for roads, settlements,
regions, portals and future NPC schedule planning without forcing every
long-distance query through a low-level grid route search.

This candidate remains a lane-d proof. It does not integrate with public
`GamePackage` schema, runtime state, Unity, WinForms UI, Lua, providers,
media, LLM or RAG surfaces.

## Implemented

- Candidate-owned Application-layer model and planner under
  `src/LLMGameCreator.Application/Design/CandidateNavigationPathfinding/`.
- Route graph nodes with string ids, node kinds, optional integer `X/Y`
  coordinates, blocked state, overlay ids and tag ids.
- Route graph edges with string ids, `fromNodeId`, `toNodeId`, route kind,
  positive integer base cost, blocked state, directed-by-default behavior,
  optional deterministic bidirectional traversal, overlay ids and tag ids.
- Route graph movement profile with blocked node kinds, blocked route kinds,
  blocked overlay ids, route-kind overrides/additional costs and overlay
  overrides/additional costs.
- Deterministic integer Dijkstra planner.
- Explicit result statuses:
  - `Success`
  - `StartMissing`
  - `GoalMissing`
  - `StartBlocked`
  - `GoalBlocked`
  - `NoPath`
  - `SearchLimitReached`
  - `InvalidGraph`
- Ordered node steps, ordered edge steps, total cost, expanded/visited counts
  and diagnostics.
- Focused candidate tests under
  `tests/LLMGameCreator.Tests/Application/CandidateNavigationPathfinding/`.

## Deterministic Guarantees

- Uses Dijkstra for v1; no A* heuristic is used.
- Uses integer-only costs.
- Edges are directed by default.
- `isBidirectional=true` creates the reverse traversal deterministically.
- Graph validation, adjacency construction and traversal ordering use
  `StringComparer.Ordinal` / ordinal string comparison.
- Source node and edge order does not affect the selected route.
- Repeated identical requests return identical status, cost and route steps.
- Route failure returns a status and diagnostics instead of throwing. Null
  programmer arguments may still throw `ArgumentNullException`.

## Route Graph Model

Route graph node:

- `id`: stable string id.
- `kind`: string kind such as `settlement`, `road_junction`,
  `region_portal` or `point_of_interest`.
- `X/Y`: optional integer coordinate for future grid stitching.
- `isBlocked`: blocks using the node as a route endpoint or traversal target.
- `overlayIds`: deterministic overlays such as hazards, closures or ownership.
- `tagIds`: candidate metadata tags; not used by v1 planning.

Route graph edge:

- `id`: stable string id.
- `fromNodeId` / `toNodeId`: directed endpoint ids.
- `routeKind`: string kind such as `road`, `trail`, `river`,
  `mountain_pass` or `region_link`.
- `baseCost`: positive integer cost.
- `isBlocked`: skips the edge.
- `isBidirectional`: enables reverse traversal with the same edge id/cost
  rules.
- `overlayIds`: deterministic edge overlays used by movement profiles.
- `tagIds`: candidate metadata tags; not used by v1 planning.

Route graph movement profile:

- `id`: stable profile id.
- `blockedNodeKinds`: node kinds that cannot be used.
- `blockedRouteKinds`: route kinds that cannot be used.
- `blockedOverlayIds`: overlays on an edge or target node that block traversal.
- `routeKindCostOverrides`: positive route-kind replacement costs.
- `routeKindAdditionalCosts`: non-negative route-kind costs added after the
  override or base cost.
- `overlayEnterCostOverrides`: positive overlay replacement costs.
- `overlayAdditionalCosts`: signed overlay costs; final edge cost is clamped
  to at least `1`.

## Edge Cost Resolution Rules

For each candidate traversal edge:

1. Skip blocked edges.
2. Skip blocked target nodes.
3. Skip route kinds blocked by the movement profile.
4. Skip blocked overlays on the edge or target node.
5. Start from `baseCost`.
6. If a positive route-kind override exists, replace the base cost.
7. Add non-negative route-kind additional cost after the override or base.
8. If one or more edge overlays have positive enter-cost overrides, apply the
   minimum positive override.
9. Sum overlay additional costs in deterministic overlay-id order. These costs
   may be negative.
10. Clamp the final edge cost to at least `1`.

Unknown route kinds and overlays use base behavior.

## External Scouting Summary

- ALT / A* with landmarks: `reference_only` for future acceleration of
  repeated road-network queries.
- Contraction Hierarchies: `reference_only` for large stable road graphs with
  preprocessing.
- HPA* / HAA*: `reference_only` for future composition of a high-level graph,
  low-level grid routes and heterogeneous agent capabilities.

No ALT, Contraction Hierarchies, HPA*, HAA* or external dependency is added by
this candidate.

## Intentionally Not Implemented

- ALT / landmarks.
- Contraction Hierarchies.
- HPA* / HAA*.
- Road generation.
- Settlement, region or portal generation.
- NPC schedule execution.
- Local grid route stitching.
- Hierarchical planner.
- Unity/navmesh adapter.
- GamePackage integration.
- UI.
- Runtime command/state changes.
- Provider, LLM, RAG, media or Lua execution.

## Future Adoption Boundary

Future serial adoption can layer this candidate behind an internal navigation
query boundary after a product task explicitly selects it.

Expected future inputs and adapters:

- lane-b roads, regions and settlements as graph sources later;
- NPC schedule route planning later;
- local grid route stitching later;
- hierarchical planner later;
- Unity/navmesh adapter later.

## Scope Guard

This candidate does not claim an accepted product gate and does not mutate
state/routing docs. It does not change `.sln`, `.csproj`, public `GamePackage`
schema, WinForms/UI, Unity/runtime integration, Lua, generator-library or
provider/LLM/RAG/media surfaces.
