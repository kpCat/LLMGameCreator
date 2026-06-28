# External Technology Scouting Template

Status: required template for future subsystem candidates.

## Purpose

LLMGameCreator must not reinvent mature libraries, datasets, algorithms, formats, or engine packages when a suitable free/open-source candidate exists.

At the same time, external technology must not silently become the architecture. It must sit behind LLMGameCreator contracts and adapters.

## Required Scouting Section

Every non-trivial subsystem candidate must include this section in its candidate report.

```text
External technology scouting

Subsystem:
Candidate id:
Date:
Agent:

Search scope:
- Libraries:
- Datasets:
- Algorithms:
- File formats:
- Unity packages:
- Existing .NET packages:
- Existing repo-local helpers:

Candidates reviewed:
| Candidate | Type | License | Runtime dependency? | Offline usable? | Deterministic? | Decision | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- |

Accepted/adapted/reference/rejected decision:
- Accepted:
- Adapted behind adapter:
- Used as reference only:
- Rejected:
- Deferred:

Adapter boundary:
- LLMGameCreator contract:
- Adapter name:
- External dependency boundary:
- Replacement plan:

Risk notes:
- License/attribution:
- Runtime footprint:
- Build impact:
- Testability:
- Determinism:
- Maintenance:
- Security:
- Paid/proprietary/API dependency:

Conclusion:
```

## Decision Vocabulary

Use exactly one decision per reviewed candidate:

- `accept_dependency`
- `adapt_behind_adapter`
- `reference_only`
- `defer`
- `reject`

Default decision is `reference_only` until an adoption task explicitly accepts a dependency.

## Hard Rejection Criteria

Reject or stop for a higher-level decision if the candidate requires:

- paid/proprietary dependency for core behavior;
- live runtime API;
- live runtime LLM/provider;
- live runtime RAG service;
- non-deterministic behavior that cannot be seeded or constrained;
- license incompatible with the intended generated-game distribution model;
- broad `.csproj` or `.sln` changes outside the task scope;
- engine lock-in before the Unity/runtime boundary is ready;
- hidden filesystem/network/thread side effects.

## Seed Candidates To Evaluate

These are not pre-approved dependencies. They are starting points for scouting.

| Area | Candidate | Starting point | Initial note |
| --- | --- | --- | --- |
| Dialogue/narrative | Ink | https://github.com/inkle/ink | C# interactive narrative scripting language; MIT license according to upstream README. Evaluate as reference/compiler/editor-time candidate first. |
| Dialogue/narrative | Yarn Spinner | https://github.com/YarnSpinnerTool/YarnSpinner | Core dialogue tooling is MIT according to upstream repo/docs, but some integrations use different licenses. Evaluate exact component, not the brand as a whole. |
| World/noise | FastNoise Lite | https://github.com/Auburn/FastNoiseLite | Portable noise library with C# support; MIT license according to upstream repo. Evaluate deterministic seeded noise behind internal contracts. |
| Navigation/pathfinding | Recast/Detour | https://github.com/recastnavigation/recastnavigation | Navmesh/pathfinding toolkit; Zlib license according to upstream repo. Likely reference/adaptation candidate, not direct dependency yet. |
| Semantic catalog | ConceptNet | https://conceptnet.io | Commonsense graph/dataset; CC BY-SA data/license impact must be reviewed carefully. Prefer offline subset/reference over runtime API dependency. |
| Semantic catalog | Open English WordNet | https://github.com/globalwordnet/english-wordnet | Lexical network; CC-BY 4.0 according to upstream repo. Useful for synonym/hypernym/tagging inputs, not a magic semantic engine. |

## Required Local Search

Before proposing external technology, search the repository for existing local helpers and contracts.

Use fast text search where available:

```powershell
rg -n "semantic|dialogue|quest|biome|noise|path|nav|manifest|module" docs src tests .devflow
```

If `rg` is unavailable, use the next best local search tool.

## Required Output File

Each candidate must write a scouting report under a candidate-owned path:

```text
docs/candidates/<candidate-id>/EXTERNAL_TECHNOLOGY_SCOUTING.md
```

Do not update accepted state docs from a parallel candidate.

