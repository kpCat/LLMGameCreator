# Goal 019: Unity Alpha Human-Readable Presentation

Status: ready for Codex after user confirms the starting gate

Starting gate required before any edit:

```text
unity_generated_multi_variant_playable_scenario_verification passed
```

Final gate for this goal:

```text
unity_alpha_readable_presentation_verification
```

The final gate must remain `required`, not `passed`.

## Purpose

Goal 018 proved three generated Unity Alpha variants can complete generated micro-quests through the same pipeline.

Goal 019 must make the Unity Alpha readable enough for manual play review.

This is not a full UI framework and not a visual polish pass. It is a bounded readable IMGUI presentation over the existing generated Alpha loop:

- clear scenario header;
- generated style/package/thread identity;
- readable quest panel;
- objective checklist with completed/pending state;
- selected target details;
- inventory/reward panel;
- event/status log;
- command/help controls;
- per-variant labels that are not just raw ids everywhere.

The user-visible result should be:

```text
When the Windows player opens, the generated scenario is understandable without reading JSON.
```

Goal 019 should preserve all existing generated gameplay proof. It must not weaken Goal 018.

## Scope

Complete only S154-S161.

- S154: record the accepted Goal 018 gate and add a readable-presentation Application-layer acceptance service.
- S155: derive deterministic human-readable presentation models from generated package/quest/objective/item/event/variant evidence.
- S156: update Unity Alpha IMGUI to show readable panels and controls without adding Unity packages.
- S157: add automated player presentation proof lines for visible panels, labels, objective states, target details, inventory/reward and event log.
- S158: validate readable presentation evidence for all three variants or at least the selected primary variant plus distinct model evidence for all variants.
- S159: reject fake/readability false positives through causal invalid matrix.
- S160: add focused xUnit and product smoke route `unity-alpha-readable-presentation`.
- S161: write compact root artifacts and update state/context/goal queue handoff.

Do not create S162.

Do not create or start Goal 020.

## Required Read-First Order

Read these before planning or editing:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_017_UNITY_GENERATED_QUEST_COMPLETION_LOOP.md`
8. `docs/GOAL_018_UNITY_MULTI_VARIANT_PLAYABLE_SCENARIO.md`
9. `.gitignore`
10. `src/LLMGameCreator.Application/Design/UnityMultiVariant/UnityMultiVariantPlayableScenarioAcceptanceService.cs`
11. `src/LLMGameCreator.Application/Design/UnityQuestLoop/UnityQuestCompletionLoopAcceptanceService.cs`
12. `src/LLMGameCreator.Application/Design/UnityRuntimeState/UnityRuntimeStateLoopAcceptanceService.cs`
13. `src/LLMGameCreator.Application/Design/UnityGeneratedScene/UnityGeneratedSceneProjectionAcceptanceService.cs`
14. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
15. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
16. `tests/LLMGameCreator.Tests/Application/UnityMultiVariant/UnityMultiVariantPlayableScenarioAcceptanceTests.cs`
17. `tests/LLMGameCreator.Tests/ProductSmoke/UnityMultiVariantPlayableScenarioSmokeTests.cs`
18. `.devflow/scripts/run-product-smoke.ps1`
19. `.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-variants.json`
20. `.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.json`
21. `.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-verification.md`

If accepted Goal 018 root evidence is missing, regenerate it through the existing product smoke route or stop with a blocker. Do not hand-create evidence.

## Allowed Files

You may create:

- `src/LLMGameCreator.Application/Design/UnityReadablePresentation/`
- `tests/LLMGameCreator.Tests/Application/UnityReadablePresentation/`
- `tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaReadablePresentationSmokeTests.cs`

You may make bounded edits to:

- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- `src/LLMGameCreator.Application/Design/UnityMultiVariant/UnityMultiVariantPlayableScenarioAcceptanceService.cs`
- `src/LLMGameCreator.Application/Design/UnityQuestLoop/UnityQuestCompletionLoopAcceptanceService.cs`
- `.devflow/scripts/run-product-smoke.ps1`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Only edit existing services if needed to expose already-generated labels or presentation metadata. Do not refactor them broadly.

You may write compact review artifacts under:

```text
.llmgc/procedural/unity-alpha-readable-presentation/
```

## Forbidden Files And Areas

Do not edit:

- `.sln`
- `.csproj`
- public GamePackage schema contracts
- public runtime schema contracts
- WinForms/UI Runtime Preview
- generator-library
- provider execution
- LLM/RAG execution
- Lua execution
- media generation
- Unity package/project settings
- `unity/LLMGameCreatorAlpha/Packages/manifest.json`
- `unity/LLMGameCreatorAlpha/ProjectSettings/ProjectVersion.txt`

Do not use git commands.

Do not add Unity UI Toolkit, TextMeshPro, Addressables, new packages, external fonts, external images or generated media.

Keep this to IMGUI / existing Unity Alpha code.

## Required Output Artifacts

Write compact root artifacts:

```text
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-model.json
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-report.json
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-report.md
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-verification.md
```

The presentation model JSON must include:

```text
schemaVersion
selectedVariantCount
selectedStyleIds
primaryStyleId
primaryPackageId
primaryThreadId
scenarioCards
primaryQuestPanel
objectiveChecklist
selectedTargetPanel
inventoryPanel
rewardPanel
eventLogPanel
controlsPanel
readabilityMetrics
modelHash
```

Do not include local absolute paths, usernames, machine names, temp paths, timestamps or GUIDs in deterministic compact artifacts.

Heavy output under these locations must remain ignored and must not be part of the compact review artifact set:

```text
.llmgc/procedural/**/build/
.llmgc/procedural/**/logs/
.llmgc/procedural/**/unity-work/
unity/**/Library/
unity/**/Temp/
unity/**/Obj/
unity/**/Build/
unity/**/Builds/
unity/**/Logs/
unity/**/UserSettings/
```

## Application Service Contract

Create:

```text
src/LLMGameCreator.Application/Design/UnityReadablePresentation/UnityAlphaReadablePresentationAcceptanceService.cs
```

Required constants:

```csharp
public const string RelativeOutputDirectory = ".llmgc/procedural/unity-alpha-readable-presentation";
public const string ModelJsonFileName = "unity-alpha-readable-presentation-model.json";
public const string ReportJsonFileName = "unity-alpha-readable-presentation-report.json";
public const string ReportMarkdownFileName = "unity-alpha-readable-presentation-report.md";
public const string VerificationMarkdownFileName = "unity-alpha-readable-presentation-verification.md";
public const string FinalGate = "unity_alpha_readable_presentation_verification";
```

The service must:

1. accept `ContentGenerationScaleAcceptanceResult` and `MinimumAssetPipelineAcceptanceResult`;
2. reuse the Goal 018 multi-variant acceptance path;
3. build deterministic presentation models for the selected variants;
4. validate Unity player presentation proof lines;
5. validate that readable labels are present and not only raw ids;
6. validate that the UI exposes quest/objectives/target/inventory/reward/event log/controls;
7. reject invalid/fake presentation evidence causally;
8. write compact artifacts to `.llmgc/procedural/unity-alpha-readable-presentation/`;
9. leave `Accepted=false`;
10. leave `FinalStatus=unity_alpha_readable_presentation_verification`;
11. leave `ManualGate=unity_alpha_readable_presentation_verification`.

## Readable Presentation Model

The model must derive labels from existing generated package/config/evidence.

If generated content lacks a clean title, deterministic fallback labels are allowed, but they must be readable:

```text
Quest 000
Frontier Survival
Open generated dialogue
Obtain generated item
Reward: item 004
```

Do not expose only full raw ids as the main visible label.

Raw ids may appear as secondary debug/detail text.

Required model pieces:

### Scenario Cards

One per selected variant:

```text
styleId
displayName
packageId
questTitle
questId
rewardLabel
objectiveSummary
sceneSummary
```

### Primary Quest Panel

```text
title
styleName
questId
phaseLabel
completionLabel
rewardLabel
```

### Objective Checklist

At least six objectives:

```text
objectiveId
label
state
sourceGeneratedId
requiredCommandId
```

States must include completed/pending values.

### Selected Target Panel

```text
targetKind
targetLabel
sourceGeneratedId
positionLabel
interactionHint
```

### Inventory / Reward / Event Log

```text
inventoryLabel
rewardLabel
eventLogEntries
lastCommandLabel
statusLabel
```

### Controls Panel

Must include visible control hints:

```text
Move: WASD/arrows
Focus: Tab
Interact: Space/Enter
Reset: R
Quit: Esc
```

## Unity Runtime Requirements

Edit only:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

The IMGUI should remain simple but readable.

Required visible sections:

- scenario header;
- variant/style identity;
- quest panel;
- objective checklist;
- selected target details;
- inventory/reward;
- event/status log;
- controls help.

Required runtime behavior:

- existing movement works;
- existing focus/select works;
- existing quest completion loop works;
- existing automated smoke flags work;
- reset works;
- quit works.

Do not add persistent local settings. Do not add networking. Do not depend on Runtime Preview.

## Required Player Log Lines

The automated player play-loop log must include all previous quest-loop evidence plus presentation proof lines.

Required lines:

```text
alpha_runtime.presentation_started=true
alpha_runtime.presentation_model_loaded=true
alpha_runtime.presentation.panel.scenario_header=true
alpha_runtime.presentation.panel.variant_identity=true
alpha_runtime.presentation.panel.quest=true
alpha_runtime.presentation.panel.objectives=true
alpha_runtime.presentation.panel.selected_target=true
alpha_runtime.presentation.panel.inventory=true
alpha_runtime.presentation.panel.reward=true
alpha_runtime.presentation.panel.event_log=true
alpha_runtime.presentation.panel.controls=true
alpha_runtime.presentation.primary_style_label=<readable label>
alpha_runtime.presentation.primary_quest_label=<readable label>
alpha_runtime.presentation.primary_phase_label=<readable label>
alpha_runtime.presentation.reward_label=<readable label>
alpha_runtime.presentation.objective_count=<count>
alpha_runtime.presentation.completed_objective_count=<count>
alpha_runtime.presentation.control_hint.move=true
alpha_runtime.presentation.control_hint.focus=true
alpha_runtime.presentation.control_hint.interact=true
alpha_runtime.presentation.control_hint.reset=true
alpha_runtime.presentation.control_hint.quit=true
alpha_runtime.presentation_readable=true
```

The Application service must reject logs that claim `presentation_readable=true` without the required panels and labels.

## Readability Metrics

The acceptance report must compute:

```text
visiblePanelCount
requiredPanelCount
readableLabelCount
rawIdOnlyLabelCount
objectiveLabelCount
completedObjectiveCount
controlHintCount
variantCardCount
```

Minimums:

```text
visiblePanelCount >= 8
requiredPanelCount >= 8
readableLabelCount >= 12
rawIdOnlyLabelCount == 0 for primary labels
objectiveLabelCount >= 6
completedObjectiveCount >= 6 after automated loop
controlHintCount >= 5
variantCardCount >= 3
```

## Report Fields

The report JSON must include:

```text
accepted
finalStatus
manualGate
previousAcceptedGate
completedSlices
productSmokeRoute
selectedStyleIds
primaryStyleId
primaryPackageId
primaryThreadId
presentationModel
visiblePanelCount
requiredPanelCount
readableLabelCount
rawIdOnlyLabelCount
objectiveLabelCount
completedObjectiveCount
controlHintCount
variantCardCount
readablePresentationVerified
presentationModelVerified
presentationPlayerEvidenceVerified
questCompletionStillVerified
multiVariantEvidenceVerified
firewallSafeBuildVerified
invalidMatrix
publicGamePackageSchemaChanged
projectFilesChanged
generatorLibraryChanged
noExternalProviderLlmRagLuaMedia
runtimePreviewDependency
modelHash
deterministicHash
diagnostics
```

Required values:

```text
accepted=false
finalStatus=unity_alpha_readable_presentation_verification
manualGate=unity_alpha_readable_presentation_verification
previousAcceptedGate=unity_generated_multi_variant_playable_scenario_verification passed
completedSlices=S154..S161
productSmokeRoute=unity-alpha-readable-presentation
variantCardCount>=3
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
noExternalProviderLlmRagLuaMedia=true
runtimePreviewDependency=false
```

## Invalid / Fake / Leak Matrix

Reject at least 22 invalid scenarios.

The invalid matrix must mutate real validation inputs. Do not create synthetic diagnostics that bypass the validator under test.

Required invalid scenario families:

1. missing accepted Goal 018 evidence;
2. missing multi-variant variants artifact;
3. missing multi-variant report artifact;
4. copied readable presentation report without player log;
5. `presentation_readable=true` without required panels;
6. missing quest panel;
7. missing objective checklist panel;
8. missing selected target panel;
9. missing inventory panel;
10. missing reward panel;
11. missing event log panel;
12. missing controls panel;
13. empty primary quest label;
14. raw-id-only primary quest label;
15. raw-id-only reward label;
16. too few objective labels;
17. objective labels not tied to Goal 017 objective ids;
18. completed objective count mismatch;
19. variant card copied across styles;
20. cross-style readable label leakage;
21. controls claim without required key hints;
22. readable model hash mismatch;
23. Runtime Preview dependency claim;
24. development/profiler/debug build option reintroduced.

Each invalid scenario must record:

```text
scenarioId
expectedValid=false
actualValid=false
mutatedEvidenceKind
diagnostics
```

The matrix passes only if all invalid scenarios reject through real presentation model, player log, previous-evidence, quest-loop, multi-variant or firewall validation paths.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/UnityReadablePresentation/
```

Required focused tests:

1. builds deterministic readable-presentation artifacts;
2. model contains three variant cards;
3. primary labels are readable and not raw-id-only;
4. all required panels are validated from player log lines;
5. completion loop remains verified after presentation changes;
6. invalid matrix scenarios are causal and rejected;
7. missing Goal 018 evidence rejects.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaReadablePresentationSmokeTests.cs
```

Smoke must:

1. build content and assets through existing test factories;
2. run `UnityAlphaReadablePresentationAcceptanceService`;
3. execute Unity build/player evidence when environment is available;
4. write compact root artifacts under `.llmgc/procedural/unity-alpha-readable-presentation/`;
5. assert report exists;
6. assert `Accepted=false`;
7. assert final gate is `unity_alpha_readable_presentation_verification`;
8. assert readable presentation booleans are true;
9. assert quest completion still verifies;
10. assert multi-variant evidence still verifies;
11. assert no Runtime Preview dependency;
12. assert no external provider/LLM/RAG/Lua/media execution.

## Product Smoke Route

Update:

```text
.devflow/scripts/run-product-smoke.ps1
```

Add scenario:

```text
unity-alpha-readable-presentation
```

The smoke summary must point to the repo-local compact root report:

```text
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-report.json
```

Do not write only to `.devflow/runs/.../package-output`.

## State And Context Updates

Update:

```text
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

The state must say:

- previous accepted gate: `unity_generated_multi_variant_playable_scenario_verification passed`;
- current active gate: `unity_alpha_readable_presentation_verification`;
- final gate remains `required`;
- S154-S161 completed;
- S162/Goal 020 not started;
- next work is review of `unity_alpha_readable_presentation_verification`;
- heavy Unity build/log/cache outputs are ignored;
- compact `.json` / `.md` artifacts remain reviewable.

Do not mark `unity_alpha_readable_presentation_verification` passed.

## Anti-False-Positive Review

Before final report, perform and report this self-review:

1. Confirm compact root artifacts exist under `.llmgc/procedural/unity-alpha-readable-presentation/`.
2. Confirm product-smoke summary points to the repo-local root report.
3. Confirm visible panel proof is not accepted from `presentation_readable=true` alone.
4. Confirm all required panel lines are required.
5. Confirm labels are non-empty and not raw-id-only for primary fields.
6. Confirm objective labels are tied to Goal 017 objective ids.
7. Confirm quest completion remains verified.
8. Confirm multi-variant evidence remains verified.
9. Confirm copied/cross-style presentation evidence is rejected.
10. Confirm previous Goal 018 evidence is present and matching.
11. Confirm invalid scenarios mutate real validation inputs.
12. Confirm report has `accepted=false`.
13. Confirm final gate remains `required`.
14. Confirm no S162/Goal 020 markers except explicit prohibition text.
15. Confirm no `.sln`, `.csproj`, public schema, WinForms, LLM/provider/Lua/media/generator-library changes.
16. Confirm generated build/log/unity-work outputs are not part of the compact review artifact set.

If any item fails, fix it before final report or stop with a blocker.

## Verification Commands

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityReadablePresentation|FullyQualifiedName~UnityMultiVariant|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-alpha-readable-presentation
```

Run:

```powershell
.\.devflow\scripts\check-all.ps1
```

Verify compact root artifacts exist:

```text
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-model.json
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-report.json
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-report.md
.llmgc/procedural/unity-alpha-readable-presentation/unity-alpha-readable-presentation-verification.md
```

Scan changed text files and compact artifacts for mojibake markers.

Scan compact artifacts for:

- absolute local paths;
- temp paths;
- user names;
- machine names;
- timestamps;
- GUIDs;
- `S162`;
- `Goal 020`;
- `goal_020`.

Fix nondeterministic output by changing generation, not by hand-editing artifacts.

## Final Report

Report exactly:

1. changed files,
2. generated compact artifact files,
3. selected style/package/thread ids,
4. primary scenario/quest/reward labels,
5. required panels proven,
6. readability metrics,
7. quest completion still verified summary,
8. multi-variant evidence still verified summary,
9. model hash,
10. report deterministic hash,
11. build manifest hash if Unity build ran,
12. invalid/fake/leak matrix count,
13. verification command results,
14. anti-false-positive self-review results,
15. confirmation that `unity_alpha_readable_presentation_verification` remains `required`, not `passed`,
16. confirmation that S162/Goal 020 was not started,
17. confirmation that no git commands were used.

## Stop Conditions

Stop and report a blocker if:

- the starting gate line is missing;
- Goal 018 compact evidence cannot be found or regenerated;
- Unity build cannot execute due to environment;
- player launch cannot produce readable presentation evidence;
- readable presentation requires Unity package/project settings changes;
- public GamePackage/runtime schema changes appear necessary.

Do not solve blockers through broad architecture changes.
