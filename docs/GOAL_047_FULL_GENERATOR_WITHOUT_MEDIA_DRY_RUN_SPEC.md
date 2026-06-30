# Goal 047 Spec — Full Generator Without Media Dry Run

## Goal id

`goal-047-full-generator-without-media-dry-run-v1`

## Gate

`full_generator_without_media_verification required`

## Purpose

Turn the latest generated/simulatable multi-family proof into a real full-generator dry-run lane before media generation.

This goal aggressively absorbs the intent of the queue items:

- Goal 047: review and approval workflow hardening;
- Goal 048: repair diagnostics hardening;
- Goal 049: runtime preview validation across generated systems;
- Goal 050: Unity/export profile generalization at contract/payload level only;
- Goal 051: one-click full generator dry run;
- Goal 052: full generator without media verification gate.

It must not become another report-only goal. The output must prove a deterministic dry-run lifecycle from selected profiles/families through review/promotion, repair diagnostics, package/preview/export-compatible artifacts and multi-family runtime-facing validation.

## Non-goals

- No WinForms/UI implementation.
- No Unity source changes or Unity build execution.
- No Runtime/Runtime.Abstractions source changes.
- No public GamePackage schema changes.
- No provider/LLM/RAG calls.
- No media generation.
- No generator-library changes.
- No new external dependencies.

## Required proof

The goal should prove at least three families through the same dry-run lifecycle:

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

Minimum success requires more than abstract planning:

1. A review/promotion ledger with deterministic transitions.
2. Repair diagnostics that produce actionable repair plans.
3. A full-generator dry-run manifest per family.
4. Runtime-preview/export-compatible consumer payloads per family.
5. A package materialization or package-compatibility proof using existing schema and validators where safely possible.
6. Product smoke that loads the produced artifacts and validates deterministic replay/roundtrip/hash evidence.

If safe package materialization cannot be done without forbidden schema/runtime changes, the goal must clearly mark package materialization as blocked and commit/push `BLOCKED`, not fake GREEN.

## Evidence folder

`.llmgc/procedural/goal-047-full-generator-without-media-dry-run/`

Required compact artifacts:

- `dry-run-source-manifest.json`
- `review-promotion-ledger.json`
- `repair-diagnostics-matrix.json`
- `family-map-panel-rpg-dry-run.json`
- `family-survival-sandbox-dry-run.json`
- `family-first-person-grid-dungeon-dry-run.json`
- `runtime-preview-validation-matrix.json`
- `export-profile-selection-matrix.json`
- `package-compatibility-or-materialization-summary.json`
- `one-click-dry-run-summary.json`
- `invalid-fake-leak-matrix.json`
- `full-generator-without-media-report.md`

The report must contain:

- `full_generator_without_media_verification required`
- `accepted=false`
- `mediaGenerated=false`
- `providerCalled=false`
- `unityExecuted=false`
- `runtimeSourceChanged=false`

## Expected final user-visible/generator capability

A single product-smoke route can prove that three different game families pass through the same full-generator dry-run lifecycle without media and without runtime LLM/provider dependence.
