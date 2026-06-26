# Goal 017: Unity Generated Quest Completion Loop

Status: ready for Codex after user confirms the starting gate

Starting gate required before any edit:

```text
unity_generated_runtime_state_loop_verification passed
```

Final gate for this goal:

```text
unity_generated_quest_completion_loop_verification
```

The final gate must remain `required`, not `passed`.

## Purpose

Goal 016 proved that generated command hints can update visible Unity Alpha runtime state.

Goal 017 must turn that state loop into one coherent generated micro-quest completion loop in Unity Alpha.

This is not a broad RPG system and not a full quest engine rewrite. It is the smallest honest end-to-end generated quest loop that proves:

1. the selected generated quest can be started;
2. generated objective steps are visible and ordered;
3. generated dialogue/item/event interactions advance objective state;
4. the quest reaches completed state;
5. a generated or package-derived reward/completion evidence becomes visible;
6. the evidence is produced by the built Unity player and verified by Application-layer acceptance.

The user-visible result should be primitive but coherent:

```text
Start quest -> talk/select -> obtain/apply item/event -> objective checklist advances -> quest completes -> reward/status shown.
```

## Scope

Complete only S138-S145.

- S138: record the accepted Goal 016 gate and add the Goal 017 Application-layer acceptance service.
- S139: derive a deterministic quest completion plan from the selected generated package/thread/projection/runtime-state evidence.
- S140: extend Unity Alpha with visible quest title/objective checklist/completion/reward/status presentation.
- S141: add automated player proof that executes the generated quest loop end-to-end and logs ordered objective state transitions.
- S142: validate Unity/player evidence against generated quest/dialogue/item/event/command ids, objective ids and reward/completion evidence.
- S143: add causal invalid/fake/leak matrix for quest completion false positives.
- S144: add focused xUnit and product smoke coverage plus smoke route `unity-quest-completion-loop`.
- S145: write compact root artifacts and update state/context/goal queue handoff.

Do not create S146.

Do not create or start Goal 018.

## Required Read-First Order

Read these before planning or editing:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_015_UNITY_GENERATED_SCENE_CONTENT_PROJECTION.md`
8. `docs/GOAL_016_UNITY_GENERATED_RUNTIME_STATE_LOOP.md`
9. `.gitignore`
10. `src/LLMGameCreator.Application/Design/UnityRuntimeState/UnityRuntimeStateLoopAcceptanceService.cs`
11. `src/LLMGameCreator.Application/Design/UnityGeneratedScene/UnityGeneratedSceneProjectionAcceptanceService.cs`
12. `src/LLMGameCreator.Application/Design/UnityPlayableAlpha/UnityPlayableAlphaAcceptanceService.cs`
13. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
14. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
15. `tests/LLMGameCreator.Tests/Application/UnityRuntimeState/UnityRuntimeStateLoopAcceptanceTests.cs`
16. `tests/LLMGameCreator.Tests/ProductSmoke/UnityRuntimeStateLoopSmokeTests.cs`
17. `.devflow/scripts/run-product-smoke.ps1`
18. `.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-state.json`
19. `.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-report.json`
20. `.llmgc/procedural/unity-runtime-state-loop/unity-runtime-state-loop-verification.md`

If any required artifact is missing, do not fake it. Regenerate it through existing services/smoke routes or stop with a blocker.

## Allowed Files

You may edit or create only:

- `src/LLMGameCreator.Application/Design/UnityQuestLoop/`
- `tests/LLMGameCreator.Tests/Application/UnityQuestLoop/`
- `tests/LLMGameCreator.Tests/ProductSmoke/`
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- `.devflow/scripts/run-product-smoke.ps1`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

You may write compact review artifacts under:

```text
.llmgc/procedural/unity-quest-completion-loop/
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

Do not add broad architecture. Reuse the existing Application acceptance pattern and the repo-local Unity Alpha runtime.

Do not use git commands.

## Required Output Artifacts

Write compact root artifacts:

```text
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-plan.json
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-state.json
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-report.json
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-report.md
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-verification.md
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
src/LLMGameCreator.Application/Design/UnityQuestLoop/UnityQuestCompletionLoopAcceptanceService.cs
```

Use a narrow Application-layer service similar to:

```csharp
UnityQuestCompletionLoopAcceptanceService
```

Required constants:

```csharp
public const string RelativeOutputDirectory = ".llmgc/procedural/unity-quest-completion-loop";
public const string PlanJsonFileName = "unity-quest-completion-loop-plan.json";
public const string StateJsonFileName = "unity-quest-completion-loop-state.json";
public const string ReportJsonFileName = "unity-quest-completion-loop-report.json";
public const string ReportMarkdownFileName = "unity-quest-completion-loop-report.md";
public const string VerificationMarkdownFileName = "unity-quest-completion-loop-verification.md";
public const string FinalGate = "unity_generated_quest_completion_loop_verification";
```

The service must:

1. accept `ContentGenerationScaleAcceptanceResult` and `MinimumAssetPipelineAcceptanceResult`;
2. reuse `UnityRuntimeStateLoopAcceptanceService` with `RelativeOutputDirectoryOverride` or equivalent options so the Unity player is built/launched for this goal output folder;
3. reuse accepted Goal 016 root evidence from `.llmgc/procedural/unity-runtime-state-loop/`;
4. derive a quest completion plan from generated package/thread/projection/runtime-state evidence;
5. validate the player play-loop log produced by the built Windows player;
6. write compact artifacts to `.llmgc/procedural/unity-quest-completion-loop/`;
7. leave `Accepted=false`;
8. leave `FinalStatus=unity_generated_quest_completion_loop_verification`;
9. leave `ManualGate=unity_generated_quest_completion_loop_verification`.

## Quest Completion Plan

The plan JSON must be deterministic and derived from generated evidence, not hard-coded placeholder ids.

Required plan fields:

```text
schemaVersion
selectedPackageId
selectedStyleId
selectedThreadId
selectedQuestId
selectedQuestTitle
selectedQuestSourceId
selectedDialogueId
selectedDialogueChoiceId
selectedItemId
selectedEventId
selectedRewardId
selectedRewardKind
startMapId
questPhaseOrder
objectiveSteps
commandSequence
completionCriteria
expectedFinalState
planHash
```

Required quest phases:

```text
not_started
started
dialogue_opened
choice_selected
item_obtained
event_applied
completed
reward_granted
```

Required objective step model:

```text
objectiveId
objectiveKind
sourceGeneratedId
requiredCommandId
requiredCommandType
requiredTargetId
requiredSecondaryTargetId
before
after
visibleLabel
```

There must be at least four objective steps:

1. start generated quest;
2. open generated dialogue;
3. choose generated dialogue choice;
4. obtain generated item or apply generated event;
5. complete quest and grant reward.

The fifth can be represented as completion/reward step even if it is derived from the existing generated command sequence.

## Unity Runtime Requirements

Extend only:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

The visible IMGUI Alpha must show:

- generated quest title or fallback generated quest id;
- current quest phase;
- objective checklist;
- selected generated dialogue id/choice id;
- selected generated item id;
- selected generated event id;
- reward/completion status;
- existing map/player/NPC/item/status presentation.

The interactive loop must remain keyboard/mouse simple:

- movement stays available;
- focus/select stays available;
- `Space`/`Enter` or existing interact button advances generated command interaction;
- reset works;
- quit works.

The automated mode must support the existing smoke flags and add quest-completion evidence without requiring manual graphics interaction:

```text
-alphaSmokeExit
-alphaPlayLoopSmokeExit
```

Do not add networking. Do not add sockets. Do not add external provider calls. Do not add Unity packages.

## Required Player Log Lines

The player play-loop log must include all Goal 016 state-loop lines plus quest-completion lines.

Required identity/evidence lines:

```text
alpha_runtime.quest_loop_started=true
alpha_runtime.quest_loop_plan_loaded=true
alpha_runtime.quest_loop.package_id=<generated package id>
alpha_runtime.quest_loop.style_id=<style id>
alpha_runtime.quest_loop.thread_id=<thread id>
alpha_runtime.quest_loop.quest_id=<quest id>
alpha_runtime.quest_loop.dialogue_id=<dialogue id>
alpha_runtime.quest_loop.choice_id=<choice id>
alpha_runtime.quest_loop.item_id=<item id>
alpha_runtime.quest_loop.event_id=<event id>
alpha_runtime.quest_loop.reward_id=<reward id>
```

Required phase lines:

```text
alpha_runtime.quest_phase.before=not_started
alpha_runtime.quest_phase.after.started=started
alpha_runtime.quest_phase.after.dialogue_opened=dialogue_opened
alpha_runtime.quest_phase.after.choice_selected=choice_selected
alpha_runtime.quest_phase.after.item_obtained=item_obtained
alpha_runtime.quest_phase.after.event_applied=event_applied
alpha_runtime.quest_phase.after.completed=completed
alpha_runtime.quest_phase.after.reward_granted=reward_granted
```

Required objective lines for every objective step:

```text
alpha_runtime.quest_objective.<index>.objective_id=<objective id>
alpha_runtime.quest_objective.<index>.objective_kind=<objective kind>
alpha_runtime.quest_objective.<index>.source_id=<generated source id>
alpha_runtime.quest_objective.<index>.required_command_id=<command id>
alpha_runtime.quest_objective.<index>.required_command_type=<command type>
alpha_runtime.quest_objective.<index>.required_target_id=<target id>
alpha_runtime.quest_objective.<index>.required_secondary_target_id=<secondary target id>
alpha_runtime.quest_objective.<index>.before=false
alpha_runtime.quest_objective.<index>.after=true
```

Required completion/reward lines:

```text
alpha_runtime.quest_completed.before=false
alpha_runtime.quest_completed.after=true
alpha_runtime.reward_granted.before=false
alpha_runtime.reward_granted.after=true
alpha_runtime.reward.kind=<reward kind>
alpha_runtime.reward.id=<reward id>
alpha_runtime.quest_loop_completed=true
```

The Application service must reject logs that claim completion without the ordered phase/objective/reward evidence.

## Acceptance Report Fields

The report JSON must include:

```text
accepted
finalStatus
manualGate
previousAcceptedGate
completedSlices
productSmokeRoute
selectedPackageId
selectedStyleId
selectedThreadId
selectedQuestId
selectedDialogueId
selectedDialogueChoiceId
selectedItemId
selectedEventId
selectedRewardId
questCompletionLoopVerified
questPlanVerified
questPhaseTraceVerified
objectiveChecklistVerified
objectiveCommandCorrelationVerified
questCompletedVerified
rewardGrantedVerified
movementVerified
focusVerified
interactionVerified
playLoopVerified
runtimeStateLoopEvidenceVerified
firewallSafeBuildVerified
invalidMatrix
publicGamePackageSchemaChanged
projectFilesChanged
generatorLibraryChanged
noExternalProviderLlmRagLuaMedia
runtimePreviewDependency
questLoopHash
planHash
stateHash
buildManifestHash
deterministicHash
diagnostics
```

Required values:

```text
accepted=false
finalStatus=unity_generated_quest_completion_loop_verification
manualGate=unity_generated_quest_completion_loop_verification
previousAcceptedGate=unity_generated_runtime_state_loop_verification passed
completedSlices=S138..S145
productSmokeRoute=unity-quest-completion-loop
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

1. missing accepted Goal 016 evidence;
2. missing runtime state loop report;
3. missing runtime state loop state JSON;
4. copied quest completion report without player log;
5. completion claimed without phase trace;
6. completion claimed without objective checklist;
7. reward claimed without completion;
8. objective step changed without before/after delta;
9. objective step command id mismatch;
10. objective step command type mismatch;
11. objective step target id mismatch;
12. objective step secondary target id mismatch;
13. quest start objective caused by non-quest command;
14. dialogue objective caused by non-dialogue command;
15. item objective caused by non-item/event/loot command;
16. event objective caused by command from another event;
17. quest phase order mismatch;
18. reward id from another package/style;
19. selected package id mismatch;
20. selected style id mismatch;
21. selected thread id mismatch;
22. state leak from previous run;
23. Runtime Preview dependency claim;
24. development/profiler/debug build option reintroduced.

Each invalid scenario must record:

```text
scenarioId
expectedValid=false
actualValid=false
diagnostics
mutatedEvidenceKind
```

The matrix passes only if all invalid scenarios reject through the real quest-loop validator, runtime-state validator, previous-evidence validator or firewall validator.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/UnityQuestLoop/
```

Required focused tests:

1. deterministic plan/state/report artifacts;
2. parser accepts complete quest loop lines;
3. parser rejects completion without phase/objective/reward proof;
4. parser rejects objective command id/type/target mismatch;
5. invalid matrix scenarios are causal and rejected;
6. previous Goal 016 evidence must be present and matching.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/UnityQuestCompletionLoopSmokeTests.cs
```

Smoke must:

1. build content and assets through existing test factories;
2. run `UnityQuestCompletionLoopAcceptanceService`;
3. execute Unity build and player launch when environment is available;
4. write compact root artifacts under `.llmgc/procedural/unity-quest-completion-loop/`;
5. assert report exists;
6. assert `Accepted=false`;
7. assert final gate is `unity_generated_quest_completion_loop_verification`;
8. if Windows player is produced, assert quest loop verification booleans are true;
9. assert no Runtime Preview dependency;
10. assert no external provider/LLM/RAG/Lua/media execution.

## Product Smoke Route

Update:

```text
.devflow/scripts/run-product-smoke.ps1
```

Add scenario:

```text
unity-quest-completion-loop
```

The smoke summary must point to the repo-local compact root report:

```text
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-report.json
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

- previous accepted gate: `unity_generated_runtime_state_loop_verification passed`;
- current active gate: `unity_generated_quest_completion_loop_verification`;
- final gate remains `required`;
- S138-S145 completed;
- S146/Goal 018 not started;
- next work is review of `unity_generated_quest_completion_loop_verification`;
- heavy Unity build/log/cache outputs are ignored;
- compact `.json` / `.md` artifacts remain reviewable.

Do not mark `unity_generated_quest_completion_loop_verification` passed.

## Anti-False-Positive Review

Before final report, perform and report this self-review:

1. Confirm compact root artifacts exist under `.llmgc/procedural/unity-quest-completion-loop/`.
2. Confirm product-smoke summary points to the repo-local root report.
3. Confirm quest completion is not accepted from `quest_completed=true` alone.
4. Confirm ordered phase trace is required.
5. Confirm objective checklist before/after deltas are required.
6. Confirm objective steps are correlated to generated command id/type/target/secondary target.
7. Confirm reward evidence is rejected without quest completion evidence.
8. Confirm previous Goal 016 evidence is present and matching.
9. Confirm invalid scenarios mutate real validation inputs.
10. Confirm report has `accepted=false`.
11. Confirm final gate remains `required`.
12. Confirm no S146/Goal 018 markers except explicit prohibition text.
13. Confirm no `.sln`, `.csproj`, public schema, WinForms, LLM/provider/Lua/media/generator-library changes.
14. Confirm generated build/log/unity-work outputs are not part of the compact review artifact set.

If any item fails, fix it before final report or stop with a blocker.

## Verification Commands

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityQuestLoop|FullyQualifiedName~UnityRuntimeState|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-quest-completion-loop
```

Run:

```powershell
.\.devflow\scripts\check-all.ps1
```

Verify compact root artifacts exist:

```text
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-plan.json
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-state.json
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-report.json
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-report.md
.llmgc/procedural/unity-quest-completion-loop/unity-quest-completion-loop-verification.md
```

Scan changed text files and compact artifacts for mojibake markers.

Scan compact artifacts for:

- absolute local paths;
- temp paths;
- user names;
- machine names;
- timestamps;
- GUIDs;
- `S146`;
- `Goal 018`;
- `goal_018`.

Fix nondeterministic output by changing generation, not by hand-editing artifacts.

## Final Report

Report exactly:

1. changed files,
2. generated compact artifact files,
3. selected package/style/thread ids,
4. selected quest/dialogue/choice/item/event/reward ids,
5. quest phase trace proven,
6. objective checklist count and objective ids,
7. objective command correlation proof summary,
8. quest completion and reward proof summary,
9. quest-loop hash,
10. plan hash,
11. state hash,
12. report deterministic hash,
13. build manifest hash if Unity build ran,
14. invalid/fake/leak matrix count,
15. verification command results,
16. anti-false-positive self-review results,
17. confirmation that `unity_generated_quest_completion_loop_verification` remains `required`, not `passed`,
18. confirmation that S146/Goal 018 was not started,
19. confirmation that no git commands were used.

## Stop Conditions

Stop and report a blocker if:

- the starting gate line is missing;
- Goal 016 compact evidence cannot be found or regenerated;
- Unity build cannot execute due to environment;
- player launch cannot produce real quest-loop evidence;
- quest completion cannot be tied to generated quest/dialogue/item/event command ids;
- public GamePackage/runtime schema changes appear necessary.

Do not solve blockers through broad architecture changes.
