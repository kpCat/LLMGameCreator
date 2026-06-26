# Goal 018: Unity Multi-Variant Playable Scenario

Status: ready for Codex after user confirms the starting gate

Starting gate required before any edit:

```text
unity_generated_quest_completion_loop_verification passed
```

Final gate for this goal:

```text
unity_generated_multi_variant_playable_scenario_verification
```

The final gate must remain `required`, not `passed`.

## Purpose

Goal 017 proved one generated micro-quest completion loop in Unity Alpha.

Goal 018 must prove the same pipeline works for multiple generated variants, not only the handiest frontier-survival candidate.

This goal must produce at least three distinct Unity Alpha playable scenario variants from generated content evidence:

```text
frontier_survival
gothic_mystery
trade_caravan
```

Each variant must have its own generated package/style/thread, scene projection, quest/dialogue/item/event ids, objective checklist, quest completion proof and reward proof.

The user-visible result should be primitive but clear:

```text
Run the Unity Alpha smoke for three generated variants -> each variant has distinct labels/ids/nodes/objective flow -> each completes its generated micro-quest.
```

This is not visual polish. Do not spend the slice on nicer UI unless it is required to expose variant identity. Goal 019 is reserved for readability/presentation polish.

## Scope

Complete only S146-S153.

- S146: record the accepted Goal 017 gate and add a multi-variant Application-layer acceptance service.
- S147: add or extend narrow candidate-selection seams so Goal 017 quest loop can run for three generated style candidates.
- S148: produce per-variant Unity Alpha quest-completion plan/state/report evidence.
- S149: prove per-variant player logs with distinct scene nodes, quest ids, objective ids, command ids and reward ids.
- S150: validate cross-variant distinctness and reject fake variation.
- S151: add causal invalid/fake/leak matrix for multi-variant false positives.
- S152: add focused xUnit and product smoke route `unity-multi-variant-playable-scenario`.
- S153: write compact root artifacts and update state/context/goal queue handoff.

Do not create S154.

Do not create or start Goal 019.

## Required Read-First Order

Read these before planning or editing:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_016_UNITY_GENERATED_RUNTIME_STATE_LOOP.md`
8. `docs/GOAL_017_UNITY_GENERATED_QUEST_COMPLETION_LOOP.md`
9. `.gitignore`
10. `src/LLMGameCreator.Application/Design/UnityQuestLoop/UnityQuestCompletionLoopAcceptanceService.cs`
11. `src/LLMGameCreator.Application/Design/UnityRuntimeState/UnityRuntimeStateLoopAcceptanceService.cs`
12. `src/LLMGameCreator.Application/Design/UnityGeneratedScene/UnityGeneratedSceneProjectionAcceptanceService.cs`
13. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
14. `src/LLMGameCreator.Application/Design/ContentGeneration/ContentGenerationScaleAcceptanceService.cs`
15. `src/LLMGameCreator.Application/Design/Assets/MinimumAssetPipelineAcceptanceService.cs`
16. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
17. `tests/LLMGameCreator.Tests/Application/UnityQuestLoop/UnityQuestCompletionLoopAcceptanceTests.cs`
18. `tests/LLMGameCreator.Tests/ProductSmoke/UnityQuestCompletionLoopSmokeTests.cs`
19. `.devflow/scripts/run-product-smoke.ps1`
20. `.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-plan.json`
21. `.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-state.json`
22. `.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-report.json`
23. `.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-verification.md`

If accepted Goal 017 root evidence is missing, regenerate it through the existing product smoke route or stop with a blocker. Do not hand-create evidence.

## Allowed Files

You may create:

- `src/LLMGameCreator.Application/Design/UnityMultiVariant/`
- `tests/LLMGameCreator.Tests/Application/UnityMultiVariant/`
- `tests/LLMGameCreator.Tests/ProductSmoke/UnityMultiVariantPlayableScenarioSmokeTests.cs`

You may make bounded edits to:

- `src/LLMGameCreator.Application/Design/UnityQuestLoop/UnityQuestCompletionLoopAcceptanceService.cs`
- `src/LLMGameCreator.Application/Design/UnityRuntimeState/UnityRuntimeStateLoopAcceptanceService.cs`
- `src/LLMGameCreator.Application/Design/UnityGeneratedScene/UnityGeneratedSceneProjectionAcceptanceService.cs`
- `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- `.devflow/scripts/run-product-smoke.ps1`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Only use bounded edits to existing services if candidate selection cannot be done otherwise. Do not refactor them broadly.

You may write compact review artifacts under:

```text
.llmgc/procedural/unity-multi-variant-playable-scenario/
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

Do not add networking, sockets, HTTP listeners, Unity packages, Addressables, external assets or generated media.

## Required Output Artifacts

Write compact root artifacts:

```text
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-variants.json
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.json
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.md
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-verification.md
```

The variants JSON must include one object per variant with deterministic summaries of:

```text
styleId
packageId
threadId
questId
dialogueId
dialogueChoiceId
itemId
eventId
rewardId
sceneNodeIds
objectiveIds
commandIds
phaseTrace
questLoopHash
planHash
stateHash
buildManifestHash
playerLogRelativePath
accepted
diagnostics
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
src/LLMGameCreator.Application/Design/UnityMultiVariant/UnityMultiVariantPlayableScenarioAcceptanceService.cs
```

Required constants:

```csharp
public const string RelativeOutputDirectory = ".llmgc/procedural/unity-multi-variant-playable-scenario";
public const string VariantsJsonFileName = "unity-multi-variant-playable-scenario-variants.json";
public const string ReportJsonFileName = "unity-multi-variant-playable-scenario-report.json";
public const string ReportMarkdownFileName = "unity-multi-variant-playable-scenario-report.md";
public const string VerificationMarkdownFileName = "unity-multi-variant-playable-scenario-verification.md";
public const string FinalGate = "unity_generated_multi_variant_playable_scenario_verification";
```

The service must:

1. accept `ContentGenerationScaleAcceptanceResult` and `MinimumAssetPipelineAcceptanceResult`;
2. reuse the Goal 017 quest-completion loop validation path for each variant;
3. select at least three generated style candidates:
   - `frontier_survival`;
   - `gothic_mystery`;
   - `trade_caravan`;
4. run the same Unity Alpha build/player evidence path for every selected variant, or use a shared build only if the staged payload and player log are separately proven per variant;
5. validate each variant's quest loop with the same strict phase/objective/reward checks from Goal 017;
6. validate distinctness across variants;
7. reject invalid/fake/leak variant evidence causally;
8. write compact artifacts to `.llmgc/procedural/unity-multi-variant-playable-scenario/`;
9. leave `Accepted=false`;
10. leave `FinalStatus=unity_generated_multi_variant_playable_scenario_verification`;
11. leave `ManualGate=unity_generated_multi_variant_playable_scenario_verification`.

## Candidate Selection

If current services only select the primary candidate, add a narrow options seam, not a broad architecture rewrite.

Acceptable seam examples:

```csharp
public sealed class UnityQuestCompletionLoopOptions
{
    public string? SelectedStyleId { get; init; }
    public int? CandidateOrdinal { get; init; }
}
```

or:

```csharp
public sealed class AlphaRunnableBuildOptions
{
    public string? SelectedStyleId { get; init; }
}
```

The seam must:

- keep existing default behavior unchanged;
- be used only to select existing generated candidates;
- reject unknown style ids;
- reject candidates whose package/assets/export evidence do not match the selected style;
- not change public package/runtime schema.

## Variant Acceptance Requirements

Every valid variant must prove:

```text
questCompletionLoopVerified=true
questPlanVerified=true
questPhaseTraceVerified=true
objectiveChecklistVerified=true
objectiveCommandCorrelationVerified=true
questCompletedVerified=true
rewardGrantedVerified=true
movementVerified=true
focusVerified=true
interactionVerified=true
playLoopVerified=true
runtimeStateLoopEvidenceVerified=true
firewallSafeBuildVerified=true
```

Every valid variant must have:

- unique `styleId`;
- unique `packageId`;
- unique `threadId`;
- at least one unique scene node id;
- unique `questId`;
- unique objective ids or source ids;
- unique command ids;
- a non-empty phase trace;
- a non-empty reward id;
- a player play-loop log tied to that variant's package/style/thread.

The cross-variant report must prove:

```text
variantCount >= 3
acceptedVariantCount >= 3
distinctStyleCount >= 3
distinctPackageCount >= 3
distinctQuestCount >= 3
distinctSceneSignatureCount >= 3
distinctObjectiveSignatureCount >= 3
allVariantsQuestComplete=true
allVariantsRewardGranted=true
allVariantsUseSamePipeline=true
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
variantCount
acceptedVariantCount
selectedStyleIds
selectedPackageIds
selectedThreadIds
distinctStyleCount
distinctPackageCount
distinctQuestCount
distinctSceneSignatureCount
distinctObjectiveSignatureCount
allVariantsQuestComplete
allVariantsRewardGranted
allVariantsUseSamePipeline
multiVariantPlayableScenarioVerified
variantSummaries
invalidMatrix
publicGamePackageSchemaChanged
projectFilesChanged
generatorLibraryChanged
noExternalProviderLlmRagLuaMedia
runtimePreviewDependency
variantsHash
deterministicHash
diagnostics
```

Required values:

```text
accepted=false
finalStatus=unity_generated_multi_variant_playable_scenario_verification
manualGate=unity_generated_multi_variant_playable_scenario_verification
previousAcceptedGate=unity_generated_quest_completion_loop_verification passed
completedSlices=S146..S153
productSmokeRoute=unity-multi-variant-playable-scenario
variantCount>=3
acceptedVariantCount>=3
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
noExternalProviderLlmRagLuaMedia=true
runtimePreviewDependency=false
```

## Invalid / Fake / Leak Matrix

Reject at least 24 invalid scenarios.

The invalid matrix must mutate real validation inputs. Do not create synthetic diagnostics that bypass the validator under test.

Required invalid scenario families:

1. missing accepted Goal 017 evidence;
2. missing Goal 017 plan artifact;
3. missing Goal 017 state artifact;
4. missing Goal 017 report artifact;
5. copied multi-variant report without per-variant player logs;
6. only one variant repeated three times;
7. three variants with same package id;
8. three variants with same quest id;
9. three variants with same scene signature;
10. three variants with same objective signature;
11. style id changed without matching package evidence;
12. package id changed without matching staged payload;
13. thread id changed without matching command evidence;
14. quest completion claimed for one variant without phase trace;
15. quest completion claimed for one variant without objective checklist;
16. reward claimed for one variant without completion;
17. objective command id mismatch in one variant;
18. objective command type mismatch in one variant;
19. objective target mismatch in one variant;
20. cross-style asset leakage;
21. cross-style command leakage;
22. cross-style reward leakage;
23. build manifest copied from another variant;
24. player log copied from another variant;
25. Runtime Preview dependency claim;
26. development/profiler/debug build option reintroduced.

Each invalid scenario must record:

```text
scenarioId
expectedValid=false
actualValid=false
mutatedEvidenceKind
diagnostics
```

The matrix passes only if all invalid scenarios reject through real multi-variant, quest-loop, runtime-state, previous-evidence, artifact or firewall validation paths.

## Unity Runtime Requirements

The Unity Alpha runtime may already be sufficient if it reads staged payload dynamically and logs package/style/thread/quest/objective identity per run.

Only change:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

if the per-variant evidence is incomplete.

If changed, keep it bounded:

- add per-variant display labels only if needed;
- preserve existing Goal 017 behavior;
- preserve existing smoke flags;
- do not add networking or external dependencies.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/UnityMultiVariant/
```

Required focused tests:

1. builds deterministic multi-variant artifacts;
2. selects at least three expected styles;
3. each variant passes quest completion loop validation;
4. cross-variant distinctness rejects repeated package/quest/scene/objective signatures;
5. invalid matrix scenarios are causal and rejected;
6. unknown style id selection rejects;
7. previous Goal 017 evidence must be present and matching.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/UnityMultiVariantPlayableScenarioSmokeTests.cs
```

Smoke must:

1. build content and assets through existing test factories;
2. run `UnityMultiVariantPlayableScenarioAcceptanceService`;
3. execute Unity build/player evidence when environment is available;
4. write compact root artifacts under `.llmgc/procedural/unity-multi-variant-playable-scenario/`;
5. assert report exists;
6. assert `Accepted=false`;
7. assert final gate is `unity_generated_multi_variant_playable_scenario_verification`;
8. assert at least three variants;
9. assert all valid variants have quest completion and reward proof;
10. assert distinct style/package/quest/scene/objective signatures;
11. assert no Runtime Preview dependency;
12. assert no external provider/LLM/RAG/Lua/media execution.

## Product Smoke Route

Update:

```text
.devflow/scripts/run-product-smoke.ps1
```

Add scenario:

```text
unity-multi-variant-playable-scenario
```

The smoke summary must point to the repo-local compact root report:

```text
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.json
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

- previous accepted gate: `unity_generated_quest_completion_loop_verification passed`;
- current active gate: `unity_generated_multi_variant_playable_scenario_verification`;
- final gate remains `required`;
- S146-S153 completed;
- S154/Goal 019 not started;
- next work is review of `unity_generated_multi_variant_playable_scenario_verification`;
- heavy Unity build/log/cache outputs are ignored;
- compact `.json` / `.md` artifacts remain reviewable.

Do not mark `unity_generated_multi_variant_playable_scenario_verification` passed.

## Anti-False-Positive Review

Before final report, perform and report this self-review:

1. Confirm compact root artifacts exist under `.llmgc/procedural/unity-multi-variant-playable-scenario/`.
2. Confirm product-smoke summary points to the repo-local root report.
3. Confirm at least three variants are actually present.
4. Confirm variants are not accepted from distinct hashes alone.
5. Confirm style/package/thread/quest/scene/objective/command identities differ where required.
6. Confirm each variant has its own player evidence or a rigorously proven staged payload/log association.
7. Confirm every variant passes quest phase/objective/reward checks.
8. Confirm repeated/copy-pasted variant evidence is rejected.
9. Confirm previous Goal 017 evidence is present and matching.
10. Confirm invalid scenarios mutate real validation inputs.
11. Confirm report has `accepted=false`.
12. Confirm final gate remains `required`.
13. Confirm no S154/Goal 019 markers except explicit prohibition text.
14. Confirm no `.sln`, `.csproj`, public schema, WinForms, LLM/provider/Lua/media/generator-library changes.
15. Confirm generated build/log/unity-work outputs are not part of the compact review artifact set.

If any item fails, fix it before final report or stop with a blocker.

## Verification Commands

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityMultiVariant|FullyQualifiedName~UnityQuestLoop|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-multi-variant-playable-scenario
```

Run:

```powershell
.\.devflow\scripts\check-all.ps1
```

Verify compact root artifacts exist:

```text
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-variants.json
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.json
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-report.md
.llmgc/procedural/unity-multi-variant-playable-scenario/unity-multi-variant-playable-scenario-verification.md
```

Scan changed text files and compact artifacts for mojibake markers.

Scan compact artifacts for:

- absolute local paths;
- temp paths;
- user names;
- machine names;
- timestamps;
- GUIDs;
- `S154`;
- `Goal 019`;
- `goal_019`.

Fix nondeterministic output by changing generation, not by hand-editing artifacts.

## Final Report

Report exactly:

1. changed files,
2. generated compact artifact files,
3. selected style/package/thread ids for every variant,
4. selected quest/dialogue/choice/item/event/reward ids for every variant,
5. per-variant quest completion proof summary,
6. per-variant objective checklist count,
7. cross-variant distinctness summary,
8. variants hash,
9. report deterministic hash,
10. build manifest hashes if Unity builds ran,
11. invalid/fake/leak matrix count,
12. verification command results,
13. anti-false-positive self-review results,
14. confirmation that `unity_generated_multi_variant_playable_scenario_verification` remains `required`, not `passed`,
15. confirmation that S154/Goal 019 was not started,
16. confirmation that no git commands were used.

## Stop Conditions

Stop and report a blocker if:

- the starting gate line is missing;
- Goal 017 compact evidence cannot be found or regenerated;
- candidate selection cannot be added without public schema changes;
- Unity build cannot execute due to environment;
- fewer than three generated style candidates can produce quest completion proof;
- variants cannot be proven distinct by generated ids and player evidence;
- public GamePackage/runtime schema changes appear necessary.

Do not solve blockers through broad architecture changes.
