# Goal 039 Spec — Runtime Chunk Delta And Traversal Smoke

## Goal name

`goal_039_runtime_chunk_delta_traversal_smoke`

## Manual gate

```text
runtime_chunk_delta_traversal_smoke_verification required
```

## Purpose

Goal 038 produced world-scale region graph, reachability, finite map packs and chunk-config prelude artifacts. Goal 039 must prove that these artifacts can drive a real runtime-facing chunk traversal and chunk-delta state loop.

This goal is intentionally bigger than a narrow queue item. It combines the practical intent of the old:

- Runtime Chunk Delta Validation;
- Infinite/Chunked World Smoke.

The result must be a generated simulatable loop, not another pure registry.

## What must become real

A selected scenario must be able to move through generated region/chunk evidence, create runtime-owned chunk deltas, persist those deltas, reload them, and continue/replay traversal deterministically.

Minimum loop:

```text
Goal038 scenario map/chunk evidence
 -> chunk traversal itinerary
 -> runtime chunk visit/discovery/mutation commands
 -> runtime-owned chunk delta state
 -> save/load or serializer roundtrip
 -> replay/reload proof
 -> compact review artifacts
```

## Non-goals

- No WinForms/UI work.
- No Unity work.
- No provider/LLM/RAG calls.
- No generated Lua source.
- No new Lua execution work beyond consuming Goal 037 evidence ids as upstream facts if useful.
- No external dependencies.
- No GamePackage public schema change.
- No broad Runtime refactor.

## Expected implementation shape

Create a narrow seam such as:

```text
src/LLMGameCreator.Application/Design/RuntimeChunkDeltaTraversal/
```

Suggested components:

- `RuntimeChunkDeltaTraversalCatalog`
- `RuntimeChunkTraversalPlanner`
- `RuntimeChunkDeltaProjector`
- `RuntimeChunkDeltaValidator`
- `RuntimeChunkDeltaEvidenceService`
- small runtime-facing models/traces as needed

If the existing Runtime state model can store extension/dynamic state without schema change, use it. If not, a narrow serializable runtime chunk state/delta record may be added under Runtime/Runtime.Abstractions only if it is strictly needed and backward-compatible.

## Required scenarios

Use at least these four scenarios if available from Goal 038:

- `frontier_survival`
- `gothic_intrigue`
- `caravan_trade`
- `metamodule_kingdoms`

At least two scenarios must have actual chunk mutation deltas, not only visits.

Required chunk delta kinds:

- visited/discovered chunk;
- region-entered marker;
- landmark discovered;
- route checkpoint;
- local mutation such as resource depleted / gate opened / hazard cleared / camp established;
- deterministic replay marker.

## Runtime proof requirements

The proof must not be paper-only.

At least one focused test and one product smoke must prove a real runtime-owned state transition path. Prefer the existing runtime serializer/snapshot store if available. If the repository has a canonical runtime save/load service, use it. Do not replace it with a parallel fake persistence layer.

Required proofs:

- before/after runtime state differs after traversal;
- chunk deltas are keyed by scenario/region/chunk ids;
- save/load or serializer roundtrip preserves chunk deltas;
- replay with same seed/itinerary produces deterministic result;
- invalid chunk ids / fake scenario ids / out-of-bounds chunk coordinates are rejected causally;
- runtime evidence does not mutate package definitions.

## Evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-039-runtime-chunk-delta-traversal-smoke/
```

Required artifacts:

```text
chunk-traversal-plan-frontier.json
chunk-traversal-plan-gothic.json
chunk-traversal-plan-caravan.json
chunk-traversal-plan-metamodule.json
runtime-chunk-delta-state-frontier.json
runtime-chunk-delta-state-metamodule.json
runtime-save-load-roundtrip-proof.json
chunk-replay-determinism-proof.json
invalid-chunk-diagnostics-matrix.json
runtime-chunk-delta-traversal-smoke-report.md
```

Artifacts must be deterministic:

- stable ordering;
- no timestamps unless the repo already uses deterministic timestamp convention;
- no absolute paths;
- no heavy logs/build outputs.

## Invalid/fake/leak matrix

Cover at least:

- fake Goal038 scenario id;
- fake region id;
- fake chunk id;
- route edge not in reachability plan;
- chunk coordinate outside finite/chunk config bounds;
- duplicate delta id;
- conflicting delta mutation;
- replay seed mismatch;
- mutation tries to edit GamePackage/package definitions;
- Runtime/UI/Unity/provider/LLM/RAG/Lua source/generator-library leakage;
- filesystem/network/process/reflection/thread/time/random/native interop leakage;
- missing save/load proof;
- nondeterministic ordering.

## Final docs state

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Expected state:

- Goal 038 accepted by user handoff before Goal 039.
- Goal 039 produced for review.
- Gate: `runtime_chunk_delta_traversal_smoke_verification required`.
- Goal 040 or next multi-family goal is only recommended/not started depending on the queue after this goal.
