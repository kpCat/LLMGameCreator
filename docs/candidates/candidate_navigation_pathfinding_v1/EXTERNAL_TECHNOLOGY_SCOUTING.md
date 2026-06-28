# External Technology Scouting - Navigation Pathfinding

External technology scouting

Subsystem: navigation/pathfinding
Candidate id: candidate_navigation_pathfinding_v1
Date: 2026-06-29
Agent: Codex

## Search Scope

- Libraries: Recast/Detour, .NET A* and graph/path libraries.
- Datasets: none required for this candidate.
- Algorithms: deterministic grid A*, waypoint/region graph traversal, navmesh query boundary.
- File formats: future path network, reachability report and optional navmesh sidecar/export formats.
- Unity packages: Unity AI Navigation as future export/runtime reference only.
- Existing .NET packages: RoyT.AStar, QuikGraph, GenericAStarPathFinder.
- Existing repo-local helpers: runtime map movement, connected world travel evidence, world topology and pathfinding taxonomy.

## Sources Reviewed

- Recast/Detour upstream repository: https://github.com/recastnavigation/recastnavigation
- Recast Navigation documentation/changelog: https://recastnav.com/md_CHANGELOG.html
- Unity AI Navigation manual: https://docs.unity3d.com/Packages/com.unity.ai.navigation%402.0/
- Unity AI Navigation package mirror/readme: https://github.com/needle-mirror/com.unity.ai.navigation
- RoyT.AStar upstream repository: https://github.com/roy-t/AStar
- RoyT.AStar NuGet package: https://www.nuget.org/packages/RoyT.AStar/
- QuikGraph NuGet package: https://www.nuget.org/packages/QuikGraph
- QuikGraph documentation: https://kernelith.github.io/QuikGraph/
- GenericAStarPathFinder NuGet package: https://www.nuget.org/packages/GenericAStarPathFinder
- Repo-local docs/code:
  - `docs/WORLD_TOPOLOGY_AND_CHUNKING_CONTRACTS.md`
  - `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md`
  - `docs/RUNTIME_MODEL.md`
  - `docs/UNITY_PLAYER_CONTRACT.md`
  - `docs/PROCESS_TASK_MODULAR_CONTRACT_GOAL_POLICY_ADOPTION.md`
  - `src/LLMGameCreator.Runtime.Abstractions/RuntimeContracts.cs`
  - `src/LLMGameCreator.Runtime/DefaultGameRuntime.cs`
  - `src/LLMGameCreator.Application/Design/World/ConnectedWorldTravelAcceptanceService.cs`
  - `tests/LLMGameCreator.Tests/Application/World/ConnectedWorldTravelAcceptanceTests.cs`

## Candidates Reviewed

| Candidate | Type | License | Runtime dependency? | Offline usable? | Deterministic? | Decision | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Recast/Detour | Native C/C++ navmesh generation and query toolkit | Zlib per upstream repo | Yes if integrated directly | Yes | Likely stable with pinned input/build, but native/floating behavior needs proof | reference_only | Strong fit for future navmesh/export work. Direct dependency would require native build/package decisions and likely `.csproj` or Unity/runtime integration, so it is out of scope for this candidate. |
| Detour-only adapter | Native navmesh query over prebuilt navmesh data | Zlib through Recast/Detour | Yes if integrated directly | Yes | Depends on pinned serialized navmesh and platform/runtime proof | defer | Useful future adapter if a serial adoption task approves native packaging. Not accepted now. |
| RoyT.AStar | .NET A* grid/graph library | MIT per upstream repo | Yes if accepted as package | Yes | Determinism depends on caller graph ordering and library internals | defer | Compatible shape for 2D grids and .NET Standard with no external deps, but accepting a package would require project/dependency changes. Use as reference only for this candidate. |
| QuikGraph | .NET graph algorithms including A* | Not accepted in this candidate | Yes if accepted as package | Yes | Depends on graph ordering and algorithm use | defer | General graph library is broader than the immediate need and would require dependency changes. |
| GenericAStarPathFinder | .NET Standard A* package | MIT per NuGet listing | Yes if accepted as package | Yes | Depends on caller ordering and implementation details | defer | Small candidate, but still a dependency and not needed for a contract-first slice. |
| LLMGameCreator-owned deterministic grid A* | Internal reference algorithm/contract | Project-owned | No external dependency | Yes | Yes if integer costs and tie-break rules are specified | adapt_behind_adapter | Best fallback/reference path for future serial implementation. It can be validated without native code and fits current runtime map movement. |
| Unity AI Navigation | Unity package/runtime navigation reference | Unity package license, exact project terms must be checked during Unity task | Unity-only | Yes inside Unity project | Engine-dependent; export evidence must be captured | reference_only | Future export/runtime reference only. Current candidate must not modify Unity runtime/build entrypoints or add Unity dependency. |

## Accepted/adapted/reference/rejected decision

- Accepted: no external dependency is accepted by this candidate.
- Adapted behind adapter: LLMGameCreator-owned deterministic grid A* contract and future adapter boundary.
- Used as reference only: Recast/Detour and Unity AI Navigation.
- Rejected: direct native Recast/Detour integration in this candidate.
- Deferred: Detour-only native query adapter, RoyT.AStar, QuikGraph, GenericAStarPathFinder.

## Adapter Boundary

- LLMGameCreator contract: `navigation_query_contract_v1`, with deterministic inputs and outputs independent of any engine or native toolkit.
- Adapter name: proposed future `INavigationPathQueryAdapter`.
- External dependency boundary: optional future adapters may translate the internal query to Detour, Unity AI Navigation, or a .NET pathfinder, but the external dependency must not own the public contract.
- Replacement plan: keep query inputs, path step outputs, diagnostics and reachability reports stable so an adapter can be replaced by another implementation without changing generated package artifacts.

## Risk Notes

- License/attribution: Recast/Detour is Zlib upstream; RoyT.AStar and GenericAStarPathFinder list MIT; Unity package terms must be checked in the Unity adoption task. No dependency is imported now.
- Runtime footprint: direct Recast/Detour introduces native binaries or source compilation; Unity AI Navigation belongs only to Unity runtime/export tasks; .NET packages add dependency management.
- Build impact: accepting any package or native library would require `.csproj`, `.sln`, native packaging, or Unity project changes. Those are forbidden for this candidate.
- Testability: internal grid A* can be tested with fixed small maps, stable diagnostics and byte-stable path output; navmesh adapters need fixture navmesh data and cross-platform determinism proof.
- Determinism: use integer costs, stable neighbor order and deterministic tie-breaking for the fallback; do not rely on unspecified priority queue ordering.
- Maintenance: Recast/Detour is mature and game-industry oriented; .NET packages vary in scope/maintenance and should be rechecked during adoption.
- Security: no runtime network, live API, LLM, RAG, provider or media dependency is introduced.
- Paid/proprietary/API dependency: none introduced.

## Conclusion

The candidate should remain contract/reference design first. Recast/Detour is the strongest future navmesh reference, but direct native integration is rejected for this candidate because it would require native packaging and likely project/runtime adoption work. The recommended immediate path is an LLMGameCreator-owned navigation query contract with deterministic grid A* fallback semantics, plus an adapter boundary that can later host Detour or Unity AI Navigation without changing the internal contract.
