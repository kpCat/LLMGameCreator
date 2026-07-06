# Narrow Alpha Expansion Policy

Status: proposed strategy policy.

## Why Narrowing Is Not a Dead End

A narrow alpha is dangerous if it hardcodes one game. It is useful if it proves
the reusable kernel that future games will use.

The alpha should not attempt the whole dream scope. It should prove that selected
feature modules can produce a real, validated, playable package through the
canonical runtime path.

Narrow alpha must be an expansion-safe kernel, not a hardcoded demo.
Projection-only goals are not enough for product readiness.
Canonical runtime playthrough is required for the next product milestone.

This policy uses the seams defined in `docs/PRODUCT_LINE_CORE_STRATEGY.md`:
`FeatureModule`, `RuntimePrimitive`, `SemanticPack`, `VisualPartPack`,
`WorldSourceAdapter` and `PlayerAdapter`.

## Alpha Scope

The preferred alpha scope is:

```text
primary world model: deterministic grid/chunk world
primary presentation: top-down/isometric/2.5D grid adapter
primary runtime: canonical GameRuntimeService path
primary gameplay: quest/dialogue/inventory/equipment/crafting/combat/reputation
primary visual: deterministic fallback/part-pack visuals
primary verification: one-click automated matrix
```

This does not prohibit future first-person, third-person, geoworld, space,
advanced building or advanced NPC simulation. It means those lanes must later
attach to the same module seams.

## Required Alpha Kernel

The alpha kernel should include:

1. selected candidate package review;
2. package validation;
3. canonical runtime state creation;
4. deterministic scripted playthrough;
5. save/load/replay proof;
6. feature coverage ledger;
7. player adapter presentation;
8. WinForms one-click workflow;
9. automated failure diagnostics;
10. rare manual milestone review.

## Deferred Until After Alpha

Unless explicitly selected by the owner, defer:

- free first-person movement;
- free third-person movement;
- real geoworld gameplay source;
- live provider art generation;
- arbitrary Lua gameplay behavior;
- runtime LLM conversations;
- advanced NPC intelligence/planning;
- large-scale city/planet/space-station construction;
- production release packaging;
- multiplayer.

## Expansion-Safety Rules

Every alpha implementation must preserve:

- stable IDs for package entities and generated artifacts;
- clear feature capability flags;
- no game-specific C# in Unity Player;
- no UI-only gameplay truth;
- no runtime LLM/provider dependency;
- deterministic package generation and replay;
- validator coverage for all generated references;
- save/load compatibility for all runtime-owned state;
- a migration note when data shape changes;
- an explicit future-expansion note for deferred lanes.

## Kill Criteria

Stop and reassess before starting more projection/candidate/UI work if any of
these remain true for three consecutive product goals:

```text
projectionOnly=true
canonicalRuntimeCoverage=false
saveLoadReplayCoverage=false
selectedCandidateNotExecutedByRuntime=true
UnityConsumesProjectionLocalTruth=true
```

The project can keep broad ambition only if the narrow alpha continuously
removes these blockers.
