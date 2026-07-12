# Goal 152A — Standalone PlayerAdapter UX, Framebuffer Refresh & Unity Execution Policy Hotfix

## Identity

- Task ID: `goal-152a-standalone-playeradapter-ux-framebuffer-refresh-and-unity-execution-policy-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `a2da741befd028e48046bbdd401361db0015a74f`
- Required base message: `GREEN Goal 152 accepted mechanics milestone and project-scoped Windows standalone build launch`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: this is a bounded Unity presentation/usability hotfix plus a repository-wide execution policy. Runtime gameplay, GamePackage schema and project composition architecture must not change.

## Pre-approval

The owner approved execution by launching this task.

- Produce a concise internal plan.
- The AGENTS.md plan requirement is already satisfied by this approved GOAL; do not ask for confirmation.
- Start immediately after base/worktree checks.
- Do not ask the owner to inspect intermediate hashes, logs or diagnostics.
- Create and push at most one final GREEN/BLOCKED/FAILED commit.
- No validation-candidate commits.

## Human review result

Goal152 automated implementation is GREEN but **not accepted**.

The owner launched the real standalone and reported:

```text
- the window is mostly empty;
- all useful information is tiny and packed into the upper-left corner;
- too many technical values must be inspected manually;
- clicking Next causes the changing Frame text to visually accumulate/ghost;
- similar ghosting is expected for other changing labels;
- the manual gate is too complex and inconvenient.
```

Observed project facts remain:

```text
project=game/goal148-manual
selected optional modules=6
configured parameters=10
frames=20
equipment/stat/total=3/6/9
attributes=stat/strength=8
progression=progression/character_level=12:level/2
```

Do not record Goal152 as accepted.

Required state:

```text
goal152Accepted=false
goal152AcceptedByHuman=false
goal152ManualReviewPerformed=true
goal152ManualGateReady=false until Goal152A GREEN
goal152aAccepted=false
goal152aAcceptedByHuman=false
```

## Confirmed root causes

### A. No framebuffer-clearing camera

`ProjectStandaloneBuildEntrypoint.BuildWindowsHost()` creates an empty scene and only adds
`ProjectStandalonePlayerAdapterBootstrap`. No Camera exists.

The standalone uses immediate-mode `OnGUI`. Without a camera or another guaranteed full-frame clear,
previous pixels may remain visible when dynamic text changes. This matches the observed Frame label ghosting.

### B. Fixed debug layout

The bootstrap uses a fixed upper-left area with a maximum width of 980 pixels and mostly default small GUI styles.
On a large desktop window this produces small technical text, weak hierarchy and a large unused area.

### C. Manual gate duplicates automation

The current review instructions ask the owner to verify values already available to automated payload/build/smoke checks.

### D. Unity smoke opens visible windows

`ProjectStandaloneBuildService.RunSmoke()` starts the standalone without headless/batch arguments.

## Primary product objective

Produce a readable, stable standalone Alpha shell whose normal human review is:

```text
1. launch the standalone;
2. observe one obvious green self-check summary;
3. click Next, Previous, Last and Reset;
4. confirm the screen updates cleanly without ghosting;
5. close the standalone.
```

Everything else must be automated.

## Required repository-level Unity execution policy

Create `docs/UNITY_EXECUTION_POLICY.md` and link it from `AGENTS.md` and `docs/CONTEXT_INDEX.md`.

Required rules:

```text
1. Default Unity execution budget for an ordinary Goal is 0.
2. Unity may run only when GOAL.md explicitly authorizes Unity-host changes or a real Unity proof.
3. Project standalone assembly uses ProjectStandaloneBuildService and StandaloneHostCache.
4. Direct ad-hoc Unity.exe experiments are forbidden.
5. Validate cache key and cache manifest before any Unity invocation.
6. Valid cache means payload/project changes reuse it with no Unity Editor launch.
7. A Unity Windows player is one atomic set: EXE, matching _Data, UnityPlayer.dll,
   MonoBleedingEdge and all build-manifest files.
8. Never rename/copy the EXE independently from its matching _Data/runtime files.
9. Automated proof roots use short LocalAppData paths.
10. Automated standalone smoke runs hidden/headless.
11. Never run more than one Unity build process simultaneously.
12. Never repeat an unchanged failed Unity command.
13. Rerun only after a concrete diagnosis and code/configuration change.
14. Per authorized Goal: one planned host build, at most one corrective retry.
15. Historical proof tests may not launch Unity to reconstruct old evidence.
16. Raw Unity logs remain ignored under .devflow/runs or LocalAppData.
17. Future GOAL.md files state Unity invocation budget explicitly.
```

Update AGENTS.md so every future agent reads this policy before Unity work.

## A. Framebuffer refresh fix

### A1. Background camera

Update `ProjectStandaloneBuildEntrypoint` to create one camera in the generated scene:

```text
name=ProjectStandaloneBackgroundCamera
enabled=true
clearFlags=SolidColor
backgroundColor=opaque dark neutral
cullingMask=0
depth behind UI
no AudioListener
```

The camera exists only to guarantee a clean framebuffer. Do not add committed scenes or prefabs.

### A2. Full-screen opaque UI background

At the beginning of every `EventType.Repaint` in the bootstrap:

- draw an opaque full-screen background;
- restore `GUI.color` afterwards;
- then draw the UI.

Do not use a transparent primary surface over uncleared pixels.

### A3. Rendering contract

Tests/source checks must prove:

```text
generated scene has exactly one enabled clearing camera
camera clearFlags=SolidColor
camera cullingMask=0
bootstrap draws full-screen opaque background before dynamic labels
GUI color/matrix restored
dynamic frame text uses one stable clipped/wrapped region
```

## B. Human-facing standalone UX

Replace the debug list with a responsive presentation.

### B1. Responsive canvas

Use a reference canvas such as `1280x720` and calculate scale/centering from `Screen.width/height`.

Required:

```text
usable at 1024x576, 1280x720, 1600x900 and 1920x1080
no fixed 980-pixel upper-left layout
no clipped controls
GUI.matrix restored after OnGUI
```

### B2. Visual hierarchy

Required default view:

```text
Header:
  project title
  Windows-игра Alpha
  Игровая логика: Runtime
  Режим Unity: PlayerAdapter

Large status banner:
  АВТОПРОВЕРКА ПРОЙДЕНА
  or clear red failure

Project summary:
  package ID/version
  selected mechanics count
  configured parameters count
  frame count

Current frame:
  frame N / total
  title
  category
  shortened state hash
  no overlap/ghosting

Gameplay summary:
  equipment
  attributes
  progression
  concise damage summary when available

Controls:
  В начало
  Назад
  Далее
  В конец
  Автошаг
  Автовоспроизведение
  Сбросить
  Закрыть

Collapsed technical details:
  full hashes
  runtime plan
  raw IDs
```

The default screen must not emphasize long hashes or raw IDs.
Primary labels/buttons are Russian. Use readable font sizes, wrapping and scroll where necessary.
Use status text in addition to color.

### B3. No placeholder values or ambiguous counts

Replace `Configured parameters: payload-backed` with the actual configured parameter count.

The current UI shows `Selected mechanics: 6` while WinForms shows `Selected module count: 16`.
This is not a data corruption: the standalone payload currently carries six user-selected optional
modules, while WinForms counts ten required core modules plus six optional modules. The wording is
misleading and must be corrected generically.

Carry and display separate fields:

```text
requiredMechanicCount
selectedOptionalMechanicCount
activeMechanicCount
configuredParameterCount
```

Human-facing example:

```text
Обязательных механик: 10
Дополнительно выбрано: 6
Всего активно: 16
Настроено параметров: 10
```

Do not hardcode 10/6/16; these are current regression values only.

Do not show raw summaries such as `stat/strength=8` or
`progression/character_level=12:level/2` as the primary human view. Application code should provide
data-driven human review facts such as:

```text
Сила: 8
Бонус оружия: 3
Бонус от характеристик: 6
Общий дополнительный урон: 9
Уровень: 2
Опыт: 12
```

Raw IDs remain available only in collapsed technical details.
No hardcoded expected project.


## C. Automated standalone self-check

Extend the payload generically with explicit review fields:

```text
selectedModuleCount
configuredParameterCount
plannedActionCount
checkpointActionCount
finalReplayActionCount
equipmentDamageBonus
statDamageBonus
totalAdditionalDamage
humanReviewFacts[] or equivalent data-driven facts
```

No fixed module list or fixed content count.

The standalone self-check validates at least:

```text
payload files present
supported schema versions
physical game-package SHA matches project manifest
project ID/title/version present
Runtime authority markers valid
frame count > 0
frame indexes contiguous
every frame title/category/state hash present
first/next/last/reset cursor transitions deterministic
selectedModuleCount matches selectedModuleIds length
configuredParameterCount matches effective parameter count
final state hash present
required human-review facts non-empty
```

Display:

```text
Автопроверка пройдена: N/N
```

A collapsed list may show individual checks. The owner must not inspect every check manually.

### Strong smoke

Strengthen `Smoke()` and service reconciliation. Required markers:

```text
LLMGC_PROJECT_STANDALONE_LOAD_PASS
LLMGC_PROJECT_STANDALONE_INTEGRITY_PASS
LLMGC_PROJECT_STANDALONE_NAVIGATION_PASS
LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS
LLMGC_PROJECT_STANDALONE_SMOKE_PASS
```

The service requires every marker, not only the final marker.

## D. Hidden automation

Update automated smoke arguments:

```text
-batchmode
-nographics
-llmgcStandaloneSmokeExit
-llmgcStandaloneSmokeLogPath <path>
```

Required:

```text
UseShellExecute=false
CreateNoWindow=true
short LocalAppData working/output root
no visible standalone window during automated smoke
normal user Launch button starts without batchmode/nographics
```

Do not hide the actual user-launched game.

## E. WinForms review simplification

After GREEN show a concise card:

```text
Автоматическая проверка: ПРОЙДЕНА
Payload integrity: GREEN
Runtime authority: GREEN
Navigation self-check: GREEN
Frames: 20
Host cache: reused|rebuilt

Для ручной проверки:
1. Запустите игру.
2. Нажмите Далее, Назад, В конец и Сбросить.
3. Убедитесь, что текст обновляется без наложения.
```

Keep full paths/hashes in a separate technical section.
Do not require manual SHA comparison.

## F. Host-cache and Unity invocation discipline

Bootstrap/entrypoint hashes change, so the cache key must change.

Required sequence:

```text
1. all .NET/source/contract tests pass;
2. PowerShell parse checks pass;
3. Unity source frozen;
4. old/new cache keys calculated;
5. exactly one planned real host build through ProjectStandaloneBuildService;
6. hidden smoke;
7. second payload proves host reuse with zero additional Unity build;
8. no further Unity invocation.
```

If the planned build fails:

- read exact log;
- make one concrete fix;
- one corrective retry allowed;
- no further Unity execution;
- publish BLOCKED if still failing.

No direct Unity.exe experiments.

## G. Regression tests

### Render contract

```text
clearing camera generated
opaque repaint background present
responsive scaling/centering contract
stable dynamic frame region
primary controls present
Russian labels present
technical details not primary default content
```

### Self-check

```text
valid 20-frame payload -> GREEN
package hash mismatch -> failure
noncontiguous frames -> failure
missing state hash -> failure
selected count mismatch -> failure
configured parameter count mismatch -> failure
Runtime authority false -> failure
cursor first/next/last/reset deterministic
```

### Hidden smoke

Prove automated arguments include `-batchmode -nographics`.
Prove normal `LaunchLastBuild()` does not include them.

### WinForms review

Prove concise GREEN summary and three short manual instructions.
No manual SHA requirement.

### Real project copy

Use a read-only source and short LocalAppData copy of `goal148-manual`.

Required:

```text
normal build GREEN
3/8/2/12
3/6/9
2/12
20 frames
new host key differs from Goal152 key
exactly one host build
hidden smoke GREEN
second assembly reuses host
original source byte-identical
```

## Manual gate after GREEN

Only:

```text
1. Launch standalone.
2. Confirm large green “Автопроверка пройдена”.
3. Click Далее, Назад, В конец and Сбросить.
4. Confirm no ghosting/overlap and readable controls.
5. Close.
```

No manual hash/count comparison.

## Command and investigation budget

```text
read-first: maximum 8 primary files
.NET build/focused tests: maximum 12 minutes
Unity budget: one planned build, one corrective retry maximum
hidden smoke: maximum 3 minutes
second payload/cache reuse: maximum 5 minutes
total target: 35 minutes
maximum two testhost processes
maximum one owned Unity process
```

Forbidden:

```text
full suite
85-case closure
all-ProductSmoke
historical snapshot repair
interactive Unity Editor experimentation
multiple unchanged Unity launches
visible smoke windows
```

## Required validation

```powershell
dotnet build
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal152A"
dotnet test ... --filter "FullyQualifiedName~Goal152ProjectStandaloneBuildTests"
dotnet test ... --filter "FullyQualifiedName~Goal151"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then perform the one real host build and cache-reuse proof.

## Artifact scope

Initially allowed:

```text
AGENTS.md
docs/UNITY_EXECUTION_POLICY.md
docs/CONTEXT_INDEX.md

.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal152a-standalone-playeradapter-ux-hotfix.ps1
.devflow/scripts/run-goal152a-standalone-playeradapter-ux-hotfix.cmd

unity/LLMGameCreatorAlpha/Assets/Scripts/ProjectStandalonePlayerAdapterBootstrap.cs
unity/LLMGameCreatorAlpha/Assets/Editor/ProjectStandaloneBuildEntrypoint.cs

src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal152AStandaloneUxAndSelfCheckTests.cs
tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal152ProjectStandaloneBuildTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal152AProjectsPageReviewSurfaceTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal152AStandaloneUxScriptTests.cs

docs/manual-acceptance/project-scoped-windows-standalone-build-launch.md
docs/manual-acceptance/standalone-playeradapter-ux-framebuffer-refresh-hotfix.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-152a-standalone-playeradapter-ux-framebuffer-refresh-and-unity-execution-policy-hotfix/
.llmgc/procedural/goal-152a-standalone-playeradapter-ux-framebuffer-refresh-and-unity-execution-policy-hotfix/
.llmgc/exports/goal-152a-standalone-playeradapter-ux-framebuffer-refresh-and-unity-execution-policy-hotfix/
```

No Runtime, Runtime.Abstractions, GamePackage schema, samples, providers, Lua, generator-library, committed scenes/prefabs, ProjectSettings or Packages changes.

## Compact evidence

Maximum 10 files per root:

```text
goal152a-dashboard.json
human-feedback-record.json
framebuffer-refresh-proof.json
responsive-ux-proof.json
standalone-self-check-proof.json
hidden-smoke-proof.json
unity-cache-discipline-proof.json
real-project-copy-proof.json
artifact-scope-proof.json
goal152a-report.md
```

Do not commit screenshots, Unity build output, cache contents or raw logs.

## Publication

Create exactly one final commit:

```text
GREEN Goal 152A standalone PlayerAdapter UX framebuffer refresh and Unity execution policy hotfix
```

or honest BLOCKED/FAILED.

Codex pushes it.

Required:

```text
HEAD == origin/main
worktree clean
Goal152 accepted=false
Goal152A accepted=false
Goal152A manualGateReady=true only on GREEN
```

## GREEN criteria

```text
user feedback recorded
Goal152 remains unaccepted
clearing camera present
opaque full-screen repaint present
no-frame-ghosting contract satisfied
responsive readable UI
actual configured parameter count displayed
prominent automated self-check GREEN
manual gate reduced to five actions
hidden smoke uses batchmode/nographics
normal user launch visible
all smoke markers required
new host built at most once successfully
second assembly reused host
original project byte-identical
Unity execution policy active through AGENTS.md
focused validation GREEN
artifact scope 0 violations
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- exact ghosting root cause/fix;
- responsive UI result;
- self-check result;
- hidden smoke arguments/markers;
- Unity invocation count;
- old/new cache keys;
- first build and second reuse;
- real project-copy results;
- focused tests;
- artifact scope;
- Goal152/Goal152A acceptance flags;
- five-step manual gate;
- final commit/push/HEAD/worktree;
- confirmation no human acceptance claimed.
