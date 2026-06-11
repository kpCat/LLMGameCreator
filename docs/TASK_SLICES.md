# TASK_SLICES.md

Purpose: keep Codex/LLM work in reusable, measurable, limit-efficient slices.

A slice should be larger than a one-line fix but smaller than a subsystem rewrite.

Good slice size:
- one architectural area;
- 3-5 related acceptance points;
- 3-8 changed files;
- 2-4 tests;
- build/test must pass;
- no hidden integration beyond the stated scope.

Bad slice size:
- “fix one typo” as a separate Codex goal, unless build is blocked;
- “implement the whole game generator”;
- “add Lua, Unity, assets, quests and combat” in one task.

## Current phase

The project is still in contract/validation/editor-shell phase.

Preferred order:
1. strengthen package contracts and validators;
2. make project/package management usable;
3. make authoring flows explicit;
4. add Lua execution only after script contracts are validated;
5. add asset provider integration only after asset contracts are validated;
6. add Unity Player prototype only after GamePackage/runtime contracts are stable.

## Backlog slices

### v0.2.1 — Script and Asset reference validation

Goal:
Strengthen validation for script paths, generators, assets, asset contracts, fallback assets, and asset references.

Expected files:
- `GamePackageValidator.cs`
- `AssetDefinitions.cs` only if a missing model is truly required
- `GameDefinitions.cs` only if a missing model is truly required
- `SmokeTests.cs`

Acceptance:
- detect `../` path traversal;
- detect script path escaping project folder;
- detect duplicate generator ids;
- detect missing/invalid asset path, fallback asset, contract id;
- detect tile/entity asset refs pointing to missing assets;
- build/test pass.

Forbidden:
- no Lua engine;
- no ComfyUI;
- no Unity;
- no package format split.

### v0.2.2 — Human-readable validation report

Goal:
Improve validation issue display and grouping without changing validator semantics.

Expected files:
- `ValidationIssue.cs`
- `ValidationReport.cs`
- `ValidationPageControl.cs`
- maybe tests

Acceptance:
- issues have stable code/severity/target/message;
- ValidationPage can show grouped text;
- no Designer breakage;
- build/test pass.

Forbidden:
- no new UI framework;
- no full validation rewrite.

### v0.2.3 — Map and entity reference validator

Goal:
Validate map bounds and basic entity placement/reference consistency.

Acceptance:
- map start position inside bounds;
- tile overrides inside bounds;
- entity instances inside bounds;
- duplicate entity instance ids per package or per map, decided explicitly;
- collidable/blocking rules are not implemented unless already modeled.

### v0.2.4 — Entity component validation baseline

Goal:
Validate generic `ComponentDefinition` conventions.

Acceptance:
- component type/name must not be empty;
- component values should be JSON-safe primitives/arrays/objects if applicable;
- known baseline components documented;
- unknown components are allowed initially unless strict mode is added.

### v0.2.5 — Asset contract validator

Goal:
Validate asset contracts themselves, not only references.

Acceptance:
- contract id/type required;
- dimensions positive when set;
- spritesheet directions/frames rules checked;
- portrait expression set has required fallback/neutral variant if modeled;
- sound/music contract rules documented and minimally validated.

### v0.2.6 — Safe package load diagnostics

Goal:
Avoid hard crashes for predictable package read problems.

Acceptance:
- malformed package/invalid enum can be reported as diagnostics by a safe load path;
- existing repository API stays usable;
- no UI rewrite;
- tests cover invalid JSON or invalid enum if practical.

### v0.2.7 — Project manager: create new game from template

Goal:
Make Projects page useful for multiple games.

Acceptance:
- GamesRootPath from settings is used;
- create new game from minimal template;
- list games under root;
- open selected game;
- no direct JSON writing from UI if an application service is added.

### v0.2.8 — Package save use-case

Goal:
Introduce controlled package save path.

Acceptance:
- application service handles save;
- storage remains in Infrastructure;
- UI does not serialize JSON directly;
- sample package remains valid.

### v0.2.9 — GamePackage format version guard

Goal:
Prevent silent loading of unsupported package formats.

Acceptance:
- validate `FormatVersion`;
- warn/error for missing or unsupported format;
- document migration policy;
- no migration engine yet.

## v0.3 Lua phase

Start only after v0.2 validators are stable.

### v0.3.1 — Script manifest export/read model alignment

Goal:
Decide whether `ScriptCatalog` stays embedded in `package.json` or later exports to `script-manifest.json`.

Acceptance:
- decision documented;
- code remains backward-compatible;
- no real Lua execution.

### v0.3.2 — Lua sandbox design spike

Goal:
Choose Lua engine and sandbox model.

Acceptance:
- options compared;
- no package dependency added unless task explicitly approves;
- security limitations documented.

### v0.3.3 — Null-to-real script engine adapter

Goal:
Add real Lua execution behind `IScriptEngine` for one safe formula/generator dry-run.

Acceptance:
- one engine integration;
- strict allow-list;
- no runtime state mutation;
- tests cover one formula or generator dry-run.

## v0.4 Asset phase

### v0.4.1 — Manual asset import model

Goal:
Allow assets to be registered/imported manually before provider integration.

Acceptance:
- asset catalog updates through application service;
- paths remain relative;
- validation checks references;
- no ComfyUI.

### v0.4.2 — Asset workflow profile model hardening

Goal:
Validate workflow profiles before any provider calls.

Acceptance:
- profile id/type/provider required;
- node mapping/spec fields validated;
- no external HTTP calls.

### v0.4.3 — ComfyUI provider skeleton

Goal:
Add provider interface implementation skeleton with disabled-by-default behavior.

Acceptance:
- no call unless explicitly invoked;
- settings based endpoint;
- errors are diagnostics, not crashes.

## v0.5 Authoring/generation phase

### v0.5.1 — GenerationSession persistence model

Goal:
Persist authoring sessions and jobs safely.

Acceptance:
- session/job models;
- JSON persistence or storage decision;
- no LLM provider call yet.

### v0.5.2 — ContextPack builder baseline

Goal:
Build small ContextPack for one task type.

Acceptance:
- explicit included files/entities;
- token budget estimate placeholder;
- no full-project dumping.

### v0.5.3 — Draft/apply pipeline baseline

Goal:
LLM output becomes draft, not direct mutation.

Acceptance:
- draft model;
- validation-before-apply rule;
- apply path for one simple entity/prototype change.

## v0.6 Unity phase

Start only after runtime/package contracts are stable.

### v0.6.1 — Unity Player prototype plan lock

Goal:
Freeze the minimal Unity Player contract before code.

Acceptance:
- BootstrapScene responsibilities;
- GamePackageLoader responsibilities;
- RuntimeBridge responsibilities;
- asset fallback policy;
- no game-specific MonoBehaviour logic.

### v0.6.2 — Unity Player minimal project

Goal:
Load minimal-map-game and render a basic tilemap.

Acceptance:
- loads package;
- WASD command bridge;
- player/NPC render;
- no LLM;
- no ComfyUI;
- no hardcoded game rules.

## Slice reporting format

Each slice final report should include:
- files read;
- files changed;
- exact behavior added;
- tests added/changed;
- build/test result;
- what was intentionally not done;
- known follow-up risks.
