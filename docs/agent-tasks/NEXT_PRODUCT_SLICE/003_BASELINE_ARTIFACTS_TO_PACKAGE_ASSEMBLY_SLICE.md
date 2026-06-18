# Task 003: Baseline Artifacts to Package Assembly Slice

## Goal

Turn accepted baseline strict artifacts into a narrow GamePackage draft state.

This is the first product-value slice after M4.1 gate pass.

## Input artifacts

Baseline strict artifacts:
- game_profile_v1
- scene_pack_v1
- quest_pack_v1
- mechanics_pack_v1

## Intended flow

```text
LLM Artifacts
-> Artifact Review
-> accepted artifacts
-> package assembly mapper
-> validate GamePackage
-> save/export sample package
```

## Allowed files

To be refined after source refresh. Expected areas:
- `src/LLMGameCreator.Application/**`
- `src/LLMGameCreator.GamePackage/**`
- `src/LLMGameCreator.WinForms/Pages/ArtifactReview/**`
- `src/LLMGameCreator.WinForms/Pages/PackageExport/**`
- `tests/LLMGameCreator.Tests/**`
- sample package files if needed
- docs/current state

## Forbidden

- Lua executor
- runtime preview repair loop
- broad contract expansion
- Unity runtime
- provider/runtime mutation

## Acceptance

- User can approve baseline strict artifacts.
- Accepted artifacts can be mapped into a package draft.
- Package validates.
- Package can be saved/exported.
- No direct LLM call during apply/export.
- check-all passes.

## Stop condition

This task must be source-refreshed before execution. Do not run directly from this draft spec.
