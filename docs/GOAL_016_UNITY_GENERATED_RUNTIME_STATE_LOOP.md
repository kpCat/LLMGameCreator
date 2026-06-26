# Goal 016: Unity Generated Runtime State Loop

## Starting Gate

This goal may start only after the user explicitly provides:

```text
unity_generated_scene_content_projection_verification passed
```

If this line is missing, stop before editing.

## Purpose

Goal 015 proved that the Unity Alpha can project visible scene nodes from generated package/config/asset evidence.

Goal 016 must turn that projection into a visible generated runtime state loop.

The player must be able to execute generated interactions and see Unity-side state change in a deterministic, inspectable way:

- quest state changes;
- dialogue state changes;
- item/inventory state changes;
- event state changes;
- selected/focused scene node status changes;
- command log records before/after state.

This is still an Alpha. Do not build a full engine, final UI, combat system, asset renderer or broad runtime contract redesign.

## Final Gate

Stop at exactly one final gate:

```text
unity_generated_runtime_state_loop_verification
```

Leave it:

```text
required
```

Do not mark it `passed`.

## Product Slices

Complete S130-S137 only:

- S130: record the accepted Goal 015 gate and read current state/handoff.
- S131: add an Application-layer Unity generated runtime state loop acceptance service.
- S132: extend Unity Alpha runtime state to track generated quest/dialogue/item/event/inventory/status changes.
- S133: make movement/focus/interaction update visible state from generated scene nodes and command hints.
- S134: prove before/after state transitions through real player play-loop logs.
- S135: reject invalid/fake/leak runtime state evidence causally.
- S136: add focused tests and product smoke route.
- S137: write compact root artifacts and update state/context/goal queue handoff.

Do not create S138.

Do not create or start Goal 017.

## Required Reading

Read these first, in order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_015_UNITY_GENERATED_SCENE_CONTENT_PROJECTION.md`
8. `.gitignore`

Then read implementation files:

1. `src/LLMGameCreator.Application/Design/UnityGeneratedScene/UnityGeneratedSceneProjectionAcceptanceService.cs`
2. `src/LLMGameCreator.Application/Design/UnityPlayableAlpha/UnityPlayableAlphaAcceptanceService.cs`
3. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
4. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
5. `tests/LLMGameCreator.Tests/Application/UnityGeneratedScene/UnityGeneratedSceneProjectionAcceptanceTests.cs`
6. `tests/LLMGameCreator.Tests/ProductSmoke/UnityGeneratedSceneProjectionSmokeTests.cs`
7. `.devflow/scripts/run-product-smoke.ps1`

Read compact artifacts if present:

1. `.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection.json`
2. `.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-report.json`
3. `.llmgc/procedural/unity-generated-scene-projection/unity-generated-scene-projection-verification.md`

Do not use old `README_APPLY_*` files or old pre-S029 prompts as planning authority.

## Allowed Areas

You may edit or add only:

1. `src/LLMGameCreator.Application/Design/UnityRuntimeState/`
2. `tests/LLMGameCreator.Tests/Application/UnityRuntimeState/`
3. `tests/LLMGameCreator.Tests/ProductSmoke/`
4. `unity/LLMGameCreatorAlpha/Assets/Scripts/`
5. `.devflow/scripts/run-product-smoke.ps1`
6. `docs/CURRENT_GENERATOR_STATE.json`
7. `docs/CURRENT_GENERATOR_STATE.md`
8. `docs/CONTEXT_INDEX.md`
9. `docs/FULL_GENERATOR_GOAL_QUEUE.md`

You may regenerate compact review artifacts under:

```text
.llmgc/procedural/unity-runtime-state-loop/
```

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
6. provider/LLM/RAG/media execution code
7. Lua execution or sandbox code
8. `generator-library/`
9. Unity `Packages/manifest.json`
10. Unity `ProjectSettings/`

Do not add packages.

Do not call external services.

Do not use git commands.

## Required Behavior

### 1. Application Acceptance Service

Add:

```text
src/LLMGameCreator.Application/Design/UnityRuntimeState/UnityRuntimeStateLoopAcceptanceService.cs
```

The service must reuse accepted Goal 015 scene projection evidence and existing Alpha build/playable pipeline.

It must produce compact root artifacts:

```text
.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-report.json
.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-report.md
.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-verification.md
```

If it writes a compact internal runtime state model artifact, use:

```text
.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-state.json
```

The service must be deterministic:

- no timestamps;
- no absolute paths;
- no temp paths;
- no user or machine names;
- no GUIDs;
- stable ordering;
- byte-stable for the same inputs.

### 2. Runtime State Model

Define a compact internal state-loop model. Do not modify public runtime contracts or GamePackage schema.

The model must include:

- selected package id;
- selected style id;
- selected thread id;
- selected map id;
- selected scene node ids;
- selected command hints;
- player position before/after movement;
- focused node id/source id;
- quest state before/after;
- dialogue state before/after;
- inventory/item state before/after;
- event state before/after;
- command execution trace.

Minimum state fields:

```text
questStarted
questCompletedCandidate
dialogueOpened
dialogueChoiceSelected
itemObtained
inventoryItemCount
eventApplied
lastCommandId
lastCommandType
lastCommandTargetId
statusText
```

This is Unity Alpha state, not a public runtime state redesign.

### 3. Unity Alpha Runtime

Extend `AlphaRuntimeBootstrap.cs` so the visible presentation shows state changes, not only ids.

The UI must show at least:

- quest state;
- dialogue state;
- inventory item count;
- event state;
- focused generated node;
- last executed command;
- command log with state transition summaries.

The automated play-loop log must include before/after lines for each state transition:

```text
alpha_runtime.state.before.quest_started=false
alpha_runtime.state.after.quest_started=true
alpha_runtime.state.before.dialogue_opened=false
alpha_runtime.state.after.dialogue_opened=true
alpha_runtime.state.before.dialogue_choice_selected=false
alpha_runtime.state.after.dialogue_choice_selected=true
alpha_runtime.state.before.item_obtained=false
alpha_runtime.state.after.item_obtained=true
alpha_runtime.state.before.inventory_item_count=0
alpha_runtime.state.after.inventory_item_count=1
alpha_runtime.state.before.event_applied=false
alpha_runtime.state.after.event_applied=true
```

It must also log per-command transition evidence:

```text
alpha_runtime.command_state_transition.<index>.command_id=...
alpha_runtime.command_state_transition.<index>.command_type=...
alpha_runtime.command_state_transition.<index>.target_id=...
alpha_runtime.command_state_transition.<index>.state_key=...
alpha_runtime.command_state_transition.<index>.before=...
alpha_runtime.command_state_transition.<index>.after=...
```

Do not hardcode the selected generated ids in the runtime script.

### 4. Movement, Focus And Interaction

The play-loop must prove:

- generated scene projection loaded;
- player starts at projected player node;
- movement changes player position;
- blocked movement is rejected by projected map bounds;
- focus selects a generated scene node;
- interaction executes generated command hints in deterministic order;
- command ids/types/targets match projection evidence;
- quest/dialogue/item/event state changes are caused by the matching generated commands.

The report must expose:

```text
runtimeStateLoopVerified=true
stateTransitionTraceVerified=true
questStateVerified=true
dialogueStateVerified=true
inventoryStateVerified=true
eventStateVerified=true
movementVerified=true
focusVerified=true
interactionVerified=true
playLoopVerified=true
```

### 5. Invalid/Fake/Leak Matrix

Reject at least these invalid scenarios causally:

1. missing accepted Goal 015 evidence,
2. missing scene projection evidence,
3. copied state-loop report without player log,
4. fake state changed=true without before/after trace,
5. quest state changed by non-quest command,
6. dialogue state changed without dialogue command,
7. item state changed without loot/item/event command,
8. event state changed without event command,
9. inventory count changed without matching item target,
10. command id mismatch,
11. command type mismatch,
12. command target mismatch,
13. command order mismatch,
14. focus target not a generated scene node,
15. movement proof without projected player node,
16. blocked bounds proof without projected map bounds,
17. state leak from previous run,
18. cross-style state/projection leakage,
19. Runtime Preview dependency claim,
20. Development/Profiler/Debug build option reintroduced.

Each invalid scenario must mutate or remove the actual evidence path it claims to test, or call the real parser/validator with invalid input.

Marker-only invalid scenarios are not acceptable.

### 6. Product Smoke

Add route:

```text
unity-runtime-state-loop
```

Update:

```text
.devflow/scripts/run-product-smoke.ps1
```

The route must regenerate compact root artifacts under:

```text
.llmgc/procedural/unity-runtime-state-loop/
```

It must not rely only on temp output.

### 7. Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/UnityRuntimeState/
```

Cover:

- deterministic state-loop report generation;
- before/after trace parsing;
- command id/type/target/state correlation;
- quest/dialogue/item/event/inventory transitions;
- focus generated node validation;
- movement and blocked bounds validation;
- invalid/fake/leak matrix;
- root artifact writing.

Add or update product smoke tests under:

```text
tests/LLMGameCreator.Tests/ProductSmoke/
```

The smoke must assert:

- final gate remains required;
- report accepted is false;
- root artifacts exist;
- runtime state loop flags are true when Unity build/player succeeds;
- no public schema/project/generator-library changes are claimed.

### 8. State And Context Handoff

Update:

```text
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Record:

- `unity_generated_scene_content_projection_verification passed` as previous accepted gate;
- Goal 016 completed S130-S137;
- final gate `unity_generated_runtime_state_loop_verification` remains required;
- product smoke route `unity-runtime-state-loop`;
- compact artifact paths;
- no S138/Goal 017 started;
- heavy Unity build/log outputs remain ignored.

Do not mark Goal 016 final gate as passed.

## Expected Report Fields

The report JSON must include at least:

```text
accepted=false
finalStatus=unity_generated_runtime_state_loop_verification
manualGate=unity_generated_runtime_state_loop_verification
previousAcceptedGate=unity_generated_scene_content_projection_verification passed
completedSlices=S130,S131,S132,S133,S134,S135,S136,S137
productSmokeRoute=unity-runtime-state-loop
selectedPackageId=game/content_generation/frontier-survival
selectedStyleId=frontier_survival
selectedThreadId=thread/frontier-survival/000
runtimeStateLoopVerified=true
stateTransitionTraceVerified=true
questStateVerified=true
dialogueStateVerified=true
inventoryStateVerified=true
eventStateVerified=true
movementVerified=true
focusVerified=true
interactionVerified=true
playLoopVerified=true
invalidMatrix.passed=true
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
noExternalProviderLlmRagLuaMedia=true
```

The report must include:

- deterministic report hash;
- state-loop hash;
- build manifest hash if Unity build executed.

## Anti-False-Positive Review

Before final response, perform and report this self-review:

1. Confirm root artifacts exist under `.llmgc/procedural/unity-runtime-state-loop/`.
2. Confirm the product smoke summary points to the repo-local report path for `unity-runtime-state-loop`.
3. Confirm state changes have explicit before/after values, not just `changed=true`.
4. Confirm each state change is correlated to a command id/type/target.
5. Confirm invalid scenarios are causal mutations, not marker-only.
6. Confirm report `accepted=false`.
7. Confirm final gate remains `required`.
8. Confirm no S138/Goal 017 markers except explicit prohibition text.
9. Confirm no `.sln`, `.csproj`, public schema, WinForms, LLM/provider/Lua/media/generator-library changes.
10. Confirm generated build/log/unity-work outputs are not part of the compact review artifact set.

If any item fails, fix it before final report or stop with a blocker.

## Verification Commands

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityRuntimeState|FullyQualifiedName~UnityGeneratedScene|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-runtime-state-loop
```

Run:

```powershell
.\.devflow\scripts\check-all.ps1
```

Verify compact root artifacts exist:

```text
.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-report.json
.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-report.md
.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-verification.md
```

Scan changed text files and compact artifacts for mojibake markers.

Scan compact artifacts for:

- absolute local paths;
- temp paths;
- user names;
- machine names;
- timestamps;
- GUIDs;
- `S138`;
- `Goal 017`;
- `goal_017`.

Fix nondeterministic output by changing generation, not by hand-editing artifacts.

## Final Report

Report exactly:

1. changed files,
2. generated compact artifact files,
3. selected package/style/thread ids,
4. runtime state fields proven,
5. command/state transition count,
6. state-loop hash,
7. report deterministic hash,
8. build manifest hash if Unity build ran,
9. invalid/fake/leak matrix count,
10. verification command results,
11. anti-false-positive self-review results,
12. confirmation that `unity_generated_runtime_state_loop_verification` remains `required`, not `passed`,
13. confirmation that S138/Goal 017 was not started,
14. confirmation that no git commands were used.

## Stop Conditions

Stop and report a blocker if:

- the starting gate line is missing;
- Unity build cannot execute due to environment;
- player launch cannot produce real play-loop evidence;
- state changes cannot be tied to generated command ids/types/targets;
- public GamePackage/runtime schema changes appear necessary.

Do not solve blockers through broad architecture changes.

