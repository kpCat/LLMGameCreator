# Product Slice 005: Generated Package Runtime Preview

## Goal

Make the assembled generated package useful in the existing Runtime Preview page.

Current state:
- Package assembly can export a package.
- Headless product smoke verifies all four baseline artifacts.
- Runtime Preview can start a map package, move and interact.
- But generated profile/scenes/quests/mechanics are not surfaced as a readable preview.

Slice 005 should bridge that gap.

## Desired user flow

```text
1. Run package assembly from Artifact Review or product smoke.
2. Open Runtime Preview.
3. Start preview.
4. See:
   - package title
   - current generated scene title/description/purpose
   - generated game profile summary
   - generated quests summary
   - generated mechanics summary
   - applied artifact provenance summary
5. Move around the map as before.
```

## Current Runtime Preview facts

Existing `RuntimePreviewPageControl` starts `IGameRuntime` from current package and appends runtime events to a log. It uses `RuntimeMapCanvas` for map display.

Existing `DefaultGameRuntime` supports:
- start current map;
- move player;
- interact with nearby interactable entities;
- message events.

Do not replace this runtime. Add generated package preview around it.

## Required behavior

### 1. Generated content preview projection

Add a small service/model that can build a read-only preview summary from `GamePackageDefinition` + current runtime state.

Possible name:

```text
GeneratedPackageRuntimePreviewService
GeneratedPackageRuntimePreviewModel
```

It should expose:

```text
package title/description
current map id/name
current generated scene source id/title/description/purpose
generated profile title/description/genre/tone/core loop/pillars
generated quest summaries
generated mechanic summaries
applied artifact provenance counts/contracts
warnings if generatedContent is empty
```

Do not build a full simulation engine.

### 2. Runtime Preview UI

Enhance `RuntimePreviewPageControl` to show generated content summary.

Acceptable UI:
- add a generated-content details TextBox/TabControl to the right panel;
- or split the existing log panel into tabs:
  - Log
  - Generated content
  - Quests
  - Mechanics
  - Provenance

Do not overbuild.

Must avoid another early `SplitterDistance` crash:
- RuntimePreview Designer currently has a hard splitter distance.
- Use a safe SizeChanged pattern like CapabilityPicker/ArtifactReview if necessary.

### 3. Runtime events

When starting runtime on an assembled generated package, append helpful messages:

```text
Игра запущена: <title>
Сцена: <scene title>
<scene description>
Доступно квестов: N
Доступно механик: N
```

This can be done in UI using preview projection, not necessarily inside `DefaultGameRuntime`.

### 4. Headless runtime-preview smoke

Add a headless test proving:

```text
product-smoke fixture approved artifacts
-> package assembly
-> runtime starts package
-> generated preview projection is non-empty
-> current scene description is resolved
-> quests/mechanics are visible
-> movement still works
```

Do not call UI automation.
Do not call LLM.

### 5. Optional script integration

Extend `run-product-smoke.ps1` with a second scenario if simple:

```powershell
-Scenario generated-package-runtime-preview
```

Or keep it as a test filter if script change is too risky.

Preferred:
Support both:
- `baseline-strict-package-assembly`
- `generated-package-runtime-preview`

## Non-goals

Do not implement:
- Unity runtime;
- new map transitions;
- full quest objective tracking;
- real combat mechanics;
- dialogue UI;
- Lua execution;
- LLM generation.

## Done

This slice is done when a developer can:
- assemble a baseline package;
- start Runtime Preview;
- see generated scenes/profile/quests/mechanics;
- run a headless runtime preview smoke test.
