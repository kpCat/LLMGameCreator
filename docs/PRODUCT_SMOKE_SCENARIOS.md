# Product Smoke Scenarios

This document tracks headless product smoke scenarios that verify product flows without UI automation, LLM providers, LM Studio, Lua execution or runtime preview.

## baseline-strict-package-assembly

Validates the sampled M4.1 baseline approved-artifact flow:

```text
fixture approved artifacts
-> GamePackage assembly service
-> package.json export
-> package validation assertions
-> product smoke report
```

Command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
```

The scenario uses deterministic in-test approved artifacts for:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

Expected run output:

```text
.devflow/runs/<timestamp>-product-smoke/product-smoke-summary.md
.devflow/runs/<timestamp>-product-smoke/product-smoke-summary.json
.devflow/runs/<timestamp>-product-smoke/test-results/
.devflow/runs/<timestamp>-product-smoke/package-output/package.json
```

Expected assertions:

- `package.json` exists.
- manifest title and description are populated from `game_profile_v1`.
- `generatedContent.profile.title` is not empty.
- `generatedContent.scenes`, `generatedContent.quests` and `generatedContent.mechanics` contain baseline content.
- `generatedContent.appliedArtifacts` includes provenance and content hashes for all four baseline contracts.
- assembly diagnostics and package validation have no package-blocking errors.

No-LLM guarantee:

- The smoke test constructs fixture approved artifacts directly.
- It calls `GeneratorPlanGamePackageAssemblyService` with `JsonGamePackageRepository`.
- It does not call provider APIs, LM Studio, repair prompts, Lua, runtime preview or WinForms UI.

Relationship to manual UI tests:

This smoke proves the baseline approved-artifact package assembly/export path headlessly. Manual UI checks remain useful for Artifact Review button flow, status text, layout and other UI-specific behavior, but they are not required for this headless package assembly smoke.
