# External Technology Scouting And Adoption Policy

## Purpose

LLMGameCreator should not reinvent everything blindly. Before implementing a sizeable subsystem, task shaping should include a lightweight scouting pass for useful free/open-source libraries, Unity packages, datasets or algorithms.

This document is a policy and candidate register. It is not permission to add dependencies immediately.

## Default rule

For any non-trivial subsystem, ask:

```text
Is there a mature, free/open-source library, dataset, algorithm or Unity package that can reduce implementation risk?
```

Then decide:

- adopt;
- adapt behind our contract;
- use as reference only;
- reject and implement ourselves.

## Adoption gates

A dependency may be adopted only after checking:

- license compatibility;
- maintenance/health;
- runtime footprint;
- deterministic behavior;
- offline/no-provider compatibility;
- Unity compatibility, if relevant;
- C#/.NET compatibility, if relevant;
- ability to wrap behind LLMGameCreator contracts;
- ability to remove/replace later;
- no live runtime LLM/RAG/provider dependency;
- no paid/proprietary requirement.

## Preferred integration style

Do not let external libraries become the architecture.

Wrap them behind internal contracts:

```text
LLMGameCreator contract
  -> adapter/wrapper
  -> external library
```

Generated packages and runtime should depend on LLMGameCreator contracts, not directly on the external library's data model.

## Candidate areas

### Narrative/dialogue authoring

Candidates:

- Ink / inkle for interactive narrative scripting reference or editor-time compilation ideas.
- Yarn Spinner for dialogue authoring workflow and possible compiler/runtime reference.

Primary use: editor-time/generation-time authoring, not live runtime dependency unless compiled and validated into internal contracts.

### Procedural world/noise

Candidate:

- FastNoise Lite for portable noise algorithms and terrain/biome generation references.

Primary use: deterministic world/biome/texture generation behind seed-controlled generator contracts.

### Navigation/pathfinding

Candidates:

- Recast/Detour for navmesh generation/pathfinding concepts.
- Unity navigation packages may be evaluated separately for Unity presentation/runtime layer.

Primary use: map/navigation module or Unity-side projection, behind pathfinding/nav contract.

### Semantic dictionaries / knowledge graphs

Candidates:

- ConceptNet-style commonsense graph for editor-time semantic enrichment.
- WordNet/Open English WordNet-style lexical relation data for semantic catalog compilation.
- Project-owned semantic catalog generated from accepted game profile/world bible/rule packs.

Primary use: offline/editor-time semantic compilation; runtime consumes compiled semantic catalog, not live web/API.

### ECS/job/performance architecture

Candidates:

- Unity DOTS/ECS/Burst/Jobs for future runtime performance if Unity-side simulation needs it.
- Pure C# data-oriented structures if Unity dependency is too heavy.

Primary use: only after contracts stabilize; not for early package-assembly proof.

### Spatial/geometry

Candidates:

- triangulation, polygon clipping, graph libraries, spatial indexing libraries.

Primary use: region/settlement/interior generation, wrapped behind generator contracts.

### Serialization and validation

Candidates:

- built-in System.Text.Json first, as current repo style.
- JSON schema libraries only if current hand-written validators become too costly.

Primary use: keep deterministic artifacts and stable diagnostics.

## Rejection reasons

Reject or postpone an external library if:

- license is incompatible or unclear;
- free tier is not enough;
- dependency forces runtime network/provider calls;
- output is nondeterministic without control;
- API is too large and would dominate architecture;
- Unity/editor runtime cannot package it cleanly;
- library solves a different problem than ours;
- adapter would cost more than implementing the needed subset.

## Required task-shaping addition

Future composite pack shaping should include an `External technology scouting` section:

```text
External technology scouting:
- searched/considered:
- candidate libraries/datasets:
- decision:
- reason:
- integration style:
- rejected alternatives:
- no live runtime provider dependency confirmed:
```

For implementation goals, Codex must not add the dependency unless the task explicitly says adoption is allowed.

## Initial candidate register

This register is intentionally non-final.

| Area | Candidate | Initial decision |
|---|---|---|
| Dialogue/narrative | Ink | evaluate for editor-time dialogue/quest authoring and compiled narrative contracts |
| Dialogue/narrative | Yarn Spinner | evaluate for dialogue workflow/format ideas and possible compiled dialogue adapter |
| Procedural noise | FastNoise Lite | strong candidate for deterministic world/biome/noise module |
| Navigation | Recast/Detour | evaluate for navmesh/pathfinding architecture; likely adapter/reference before direct adoption |
| Commonsense semantics | ConceptNet | evaluate as offline/editor-time semantic seed source; runtime must use compiled subset only |
| Lexical semantics | Open English WordNet | evaluate as lexical relation source for semantic catalog tooling |
