# Product Slice 041 - Generation Presets and Options

## Purpose

Make one-click generated microgame previews configurable without turning the UI into a large generator studio.

After S038-S040, the loop should be runtime-backed. S041 should expose a narrow set of deterministic generation options so the user can produce different microgame previews intentionally.

## Functional goal

Add a small, focused generation-options path for Runtime Preview:

- seed input or generated seed display/edit;
- mode selector using existing supported modes;
- one small preset/style choice, such as:
  - survival/exploration;
  - faction_truce;
  - recover_resource;
  - dangerous/safe/mysterious tone pack;
- generated package title/id reflects selected seed/options;
- one-click workflow uses selected options.

## Implementation direction

Prefer:

- small model/service for generation options;
- compact Runtime Preview controls;
- default preset matching current behavior;
- deterministic option serialization in reports.

Avoid:

- large UI redesign;
- new page unless unavoidable;
- arbitrary free-form LLM prompt input;
- provider/LLM/media/Lua execution.

## Required behavior

Minimum acceptable behavior:

- default Generate Preview still works with no user input;
- changing seed changes generated microgame deterministically;
- changing one preset/style changes generated content visibly;
- generated acceptance still passes for the default preset;
- diagnostics include selected preset/options.

## Tests

Add focused tests:

- default options match current behavior;
- same options produce byte-stable output;
- different seed changes package id/hash or representative ids;
- preset/style changes content labels or selected hints;
- no external execution.

Suggested filter:

```text
FullyQualifiedName~GenerationPresetOptions
```

## Product smoke

Add scenario:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generation-preset-options
```

Smoke must prove:

```text
default preset -> accepted microgame
alternate seed/preset -> different deterministic package/preview evidence
```

## Docs/state

After S041:

- update state;
- next recommended task is S042 `microgame_variation_acceptance`;
- no broad systems unlocked.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~GenerationPresetOptions"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generation-preset-options
.\.devflow\scripts\check-all.ps1
```

## Constraints

Do not add LLM/provider/Lua/Unity/media execution.

Do not perform broad UI redesign.

Do not add arbitrary prompt-based generation.

