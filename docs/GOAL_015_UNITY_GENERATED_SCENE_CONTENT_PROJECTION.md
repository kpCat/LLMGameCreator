# Goal 015: Unity Generated Scene Content Projection

## Starting Gate

This goal may start only after the user explicitly provides:

```text
unity_playable_presentation_firewall_safe_build_verification passed
```

Do not start this goal if the gate is still `required`.

## Purpose

Goal 014 proved that the repository-local Unity Alpha can build, launch, show a simple playable IMGUI presentation, execute generated command hints and avoid Development/Profiler/Debug build flags.

Goal 015 must make the Unity Alpha presentation materially more generated-content-driven.

The current Alpha view may still rely on fixed coordinates, fixed marker positions and a generic UI shell. This goal must prove that the visible scene content is projected from the selected generated package, runtime config and asset refs produced by the existing Goal 010/011/012/014 pipeline.

This is still an Alpha. Do not build a full engine, map editor, renderer, UI framework or asset provider pipeline.

## Final Gate

Stop at exactly one final gate:

```text
unity_generated_scene_content_projection_verification
```

Leave it `required`, not `passed`.

## Product Slices

Complete S122-S129 only:

- S122: record the accepted Goal 014 gate and read current state/handoff.
- S123: add an Application-layer generated scene projection acceptance service.
- S124: extend the Unity Alpha runtime so visible map, player, NPC, item, quest/event and command/status presentation are derived from staged package/config evidence.
- S125: prove scene layout determinism and generated-id binding without changing public GamePackage/runtime schema.
- S126: prove interaction/focus/movement against generated scene nodes, not only fixed hardcoded placeholders.
- S127: reject invalid/fake/leak scene projection evidence causally.
- S128: add focused tests and a product smoke route.
- S129: write compact root artifacts and update state/context handoff.

Do not create S130.

Do not create or start Goal 016.

## Required Reading

Read these first, in order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md`
7. `docs/GOAL_014_UNITY_PLAYABLE_PRESENTATION_AND_FIREWALL_SAFE_BUILD.md`
8. `.gitignore`

Then read the relevant implementation files:

1. `src/LLMGameCreator.Application/Design/UnityPlayableAlpha/UnityPlayableAlphaAcceptanceService.cs`
2. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
3. `src/LLMGameCreator.Application/Design/UnityRuntimeExport/UnityRuntimeExportAcceptanceService.cs`
4. `src/LLMGameCreator.Application/Design/Assets/MinimumAssetPipelineAcceptanceService.cs`
5. `src/LLMGameCreator.Application/Design/ContentGeneration/ContentGenerationScaleAcceptanceService.cs`
6. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
7. `unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs`
8. `tests/LLMGameCreator.Tests/ProductSmoke/UnityPlayableAlphaSmokeTests.cs`
9. `tests/LLMGameCreator.Tests/Application/UnityPlayableAlpha/UnityPlayableAlphaAcceptanceTests.cs`
10. `.devflow/scripts/run-product-smoke.ps1`

Read compact artifacts if present:

1. `.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-report.json`
2. `.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-report.md`
3. `.llmgc/procedural/unity-playable-alpha/unity-playable-alpha-verification.md`

Do not rely on old `README_APPLY_*` files or old pre-S029 prompt files as planning authority.

## Allowed Areas

You may edit or add files only under:

1. `src/LLMGameCreator.Application/Design/UnityGeneratedScene/`
2. `tests/LLMGameCreator.Tests/Application/UnityGeneratedScene/`
3. `tests/LLMGameCreator.Tests/ProductSmoke/`
4. `unity/LLMGameCreatorAlpha/Assets/Scripts/`
5. `.devflow/scripts/run-product-smoke.ps1`
6. `docs/CURRENT_GENERATOR_STATE.json`
7. `docs/CURRENT_GENERATOR_STATE.md`
8. `docs/CONTEXT_INDEX.md`

You may regenerate compact review artifacts under:

```text
.llmgc/procedural/unity-generated-scene-projection/
```

You may create deterministic source evidence/staging/export fixture copies under that same artifact folder if they are required for review and are not under ignored `build/`, `logs/` or `unity-work` subfolders.

Heavy Unity outputs must remain ignored and outside the intended review artifact set:

```text
.llmgc/procedural/**/build/
.llmgc/procedural/**/logs/
.llmgc/procedural/**/unity-work/
```

## Forbidden Areas

Do not edit:

1. `.sln`
2. `.csproj`
3. public GamePackage schema contracts
4. public runtime command/state contracts
5. `src/LLMGameCreator.WinForms/`
6. `src/LLMGameCreator.Generation/` provider execution code
7. `src/LLMGameCreator.AssetPipeline/` provider execution code
8. `generator-library/`
9. `templates/`
10. Lua execution or sandbox code
11. LLM/RAG/provider/media generation code
12. Unity package manifest or ProjectSettings unless a real compile/build blocker proves it is necessary

Do not add NuGet packages.

Do not add Unity packages.

Do not call LLMs, RAG, providers, ComfyUI, media generators, Lua generators or external network services.

Do not use git commands.

## Required Behavior

### 1. Application Acceptance Service

Add a narrow Application-layer acceptance service:

```text
src/LLMGameCreator.Application/Design/UnityGeneratedScene/UnityGeneratedSceneProjectionAcceptanceService.cs
```

The service must reuse the existing accepted pipeline evidence:

- content generation packs from Goal 010,
- minimum asset pipeline evidence from Goal 011,
- Unity runtime export evidence from Goal 012,
- Alpha build/playable evidence from Goal 013/014.

It must not duplicate broad generator logic.

It must produce:

```text
.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-report.json
.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-report.md
.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-verification.md
```

The report must be deterministic:

- no timestamps,
- no absolute local paths,
- no temp paths,
- no user names,
- no machine names,
- no GUIDs,
- stable ordering,
- byte-stable for the same inputs.

### 2. Generated Scene Projection Contract

Define a compact internal projection model inside the new acceptance service or a closely scoped nested type area. Do not add public Domain/GamePackage schema.

The projection must derive from the selected package/runtime config evidence:

- selected package id,
- selected style id,
- selected thread id,
- selected map id,
- selected NPC id,
- selected quest id,
- selected dialogue id,
- selected item id,
- selected event id,
- selected command hints,
- selected asset refs,
- package hash,
- asset manifest hash,
- runtime config hash.

The projection must include a deterministic small scene graph:

- map node,
- player node,
- NPC node,
- item/loot node,
- quest/event node,
- at least one command/status node.

Every non-player node must have:

- stable node id,
- source generated id,
- node kind,
- deterministic grid position,
- display label derived from generated/package evidence,
- optional asset ref id/path/hash when available.

Positions must be derived deterministically from selected generated ids and package/config evidence. Do not keep the Goal 014 fixed marker positions as the source of truth.

You may still render a simple grid in Unity IMGUI. This goal is about data-driven projection, not visual polish.

### 3. Unity Alpha Runtime

Update only the existing Unity Alpha runtime script area to consume the generated scene projection from staged payload/config.

The visible presentation must show:

- package id,
- selected thread id,
- map id,
- player position,
- NPC generated id or label,
- item generated id or label,
- quest/event generated id or label,
- command count,
- asset ref count,
- focus target,
- command/status log.

The map markers must be derived from projection nodes:

- `P` for player,
- `N` for generated NPC,
- `I` for generated item/loot,
- `Q` or `E` for generated quest/event,
- `.` for empty cells.

Do not hardcode the final selected generated ids in C#.

The runtime may use lightweight JSON extraction/parsing consistent with the existing Alpha runtime style, but the evidence must prove the values came from the staged package/config/projection, not from fixed constants.

### 4. Movement And Interaction Evidence

The automated play-loop log must prove:

- projection loaded,
- map node resolved,
- player node resolved,
- NPC node resolved,
- item node resolved,
- quest/event node resolved,
- command/status node resolved,
- player moves from the projected initial player position,
- blocked movement is rejected at projected map bounds,
- focus selects a generated scene node,
- interaction executes generated command hints in deterministic order,
- executed command ids/types/targets match the selected generated command hints,
- state flags for quest/dialogue/item/event are recorded.

The report must expose:

```text
sceneProjectionVerified=true
sceneNodesResolved=true
generatedIdBindingVerified=true
assetBindingVerified=true
movementVerified=true
interactionVerified=true
playLoopVerified=true
firewallSafeBuildVerified=true
```

If Unity is available locally, the smoke must build and run the player just like Goal 014.

If Unity is not available, this goal should fail/stop honestly with a real environment diagnostic. Do not fake Unity execution.

### 5. Invalid/Fake/Leak Matrix

Reject at least these invalid scenarios causally:

1. missing accepted Goal 014 evidence,
2. missing generated scene projection file,
3. copied projection report without staged package/config files,
4. package hash mismatch,
5. asset manifest hash mismatch,
6. runtime config hash mismatch,
7. node source id not present in selected package/config,
8. NPC node bound to item id,
9. item node bound to NPC id,
10. duplicate scene node ids,
11. duplicate occupied grid position for non-stackable nodes,
12. out-of-bounds projected position,
13. command order mismatch,
14. command target mismatch,
15. cross-style package/projection leakage,
16. missing asset ref file/hash for an asset-bound node,
17. fake movement log without projection load,
18. fake interaction log without generated command ids,
19. Development/Profiler/Debug build option reintroduced,
20. Runtime Preview dependency claim.

The invalid matrix must not be marker-only. Each invalid scenario must mutate or remove the actual evidence path it claims to test, or call the real validator/parser path with invalid input.

### 6. Product Smoke

Add a product smoke route:

```text
unity-generated-scene-projection
```

Update:

```text
.devflow/scripts/run-product-smoke.ps1
```

The smoke must regenerate repo-local compact root artifacts under:

```text
.llmgc/procedural/unity-generated-scene-projection/
```

It must not rely only on temp output.

It must not attempt to commit or stage heavy build/log outputs.

### 7. Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/UnityGeneratedScene/
```

Cover:

- deterministic projection generation,
- selected package/config ids are carried into scene nodes,
- node positions are deterministic and not the fixed Goal 014 placeholder positions,
- generated id binding validation,
- asset binding validation,
- command order/target validation,
- invalid/fake/leak matrix rejection,
- compact report root artifact writing.

Add/update product smoke tests under:

```text
tests/LLMGameCreator.Tests/ProductSmoke/
```

The smoke test must assert the final gate remains required and the report is not accepted.

### 8. State And Context Handoff

Update:

```text
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
```

Record:

- `unity_playable_presentation_firewall_safe_build_verification passed` as the accepted previous gate,
- Goal 015 completed S122-S129,
- final gate `unity_generated_scene_content_projection_verification` remains required,
- product smoke route `unity-generated-scene-projection`,
- compact artifact paths,
- no S130/Goal 016 started,
- heavy Unity build/log outputs remain ignored by `.gitignore`.

Do not mark the Goal 015 final gate as passed.

## Expected Report Fields

The generated JSON report must contain at least:

```text
accepted=false
finalStatus=unity_generated_scene_content_projection_verification
manualGate=unity_generated_scene_content_projection_verification
previousAcceptedGate=unity_playable_presentation_firewall_safe_build_verification passed
completedSlices=S122,S123,S124,S125,S126,S127,S128,S129
productSmokeRoute=unity-generated-scene-projection
selectedPackageId=game/content_generation/frontier-survival
selectedStyleId=frontier_survival
selectedThreadId=thread/frontier-survival/000
packageHash=3e8a42663e1a2fdabd98cdd8c30ab6188810bd4d0f4d36aa4e3089a71b952d53
assetManifestHash=3dd392bae4cbac24db34b1810a52c83cf64791521df8849c75ac61e8fdcfa595
sceneProjectionVerified=true
sceneNodesResolved=true
generatedIdBindingVerified=true
assetBindingVerified=true
movementVerified=true
interactionVerified=true
playLoopVerified=true
firewallSafeBuildVerified=true
invalidMatrix.passed=true
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
noExternalProviderLlmRagLuaMedia=true
```

The report must include deterministic hash fields:

- report deterministic hash,
- projection hash,
- build manifest hash if Unity build was executed.

## Verification Commands

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityGeneratedScene|FullyQualifiedName~UnityPlayableAlpha|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-generated-scene-projection
```

Run:

```powershell
.\.devflow\scripts\check-all.ps1
```

Then verify compact root artifacts exist:

```text
.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-report.json
.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-report.md
.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-verification.md
```

Scan changed text files and compact artifacts for mojibake markers.

Scan compact artifacts for:

- absolute local paths,
- temp paths,
- user names,
- machine names,
- timestamps,
- GUIDs,
- `S130`,
- `Goal 016`,
- `goal_016`.

If any nondeterministic marker appears, fix generation and rerun verification.

## Final Report

Report exactly:

1. changed files,
2. generated compact artifact files,
3. selected package/style/thread ids,
4. scene node count and node kinds,
5. projection hash,
6. report deterministic hash,
7. build manifest hash if Unity build ran,
8. invalid/fake/leak matrix count,
9. verification command results,
10. whether root compact artifacts exist,
11. confirmation that `unity_generated_scene_content_projection_verification` remains `required`, not `passed`,
12. confirmation that S130/Goal 016 was not started,
13. confirmation that no git commands were used.

## Stop Conditions

Stop immediately and report a blocker if:

- `unity_playable_presentation_firewall_safe_build_verification passed` was not provided,
- Unity build cannot execute due to missing local Unity environment,
- player launch cannot produce real play-loop evidence,
- generated scene projection cannot be tied to package/config evidence,
- public GamePackage/runtime schema changes appear necessary.

Do not solve those by broad architecture changes.
