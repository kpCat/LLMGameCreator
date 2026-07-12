# Goal 152 — Accepted Mechanics Milestone + Project-Scoped Windows Standalone Build & Launch

## Identity

- Task ID: `goal-152-accepted-mechanics-milestone-and-project-scoped-windows-standalone-build-launch`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `2516931f9c8242bbd59fe5cf73f9e66b405ef16c`
- Required base message: `GREEN Goal 151 real saved project build recovery and diagnostic truth hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: this is a large but well-bounded product vertical slice. It integrates existing Runtime,
PlayerAdapter and Unity-build primitives into the normal `Игры` workflow. It does not introduce a
new gameplay primitive family or public GamePackage schema, so Sol is not the default choice.

## Pre-approval

The owner approved execution by launching this task.

- Produce a concise internal plan.
- Do not ask for plan confirmation.
- Start immediately after base/worktree checks.
- Do not ask the owner to perform intermediate tests.
- Create and push at most one final GREEN/BLOCKED/FAILED commit.
- No validation-candidate commits.

## Product direction

Goal151 and the bundled Goals149/150/150A/150B mechanics gate are now explicitly accepted by the owner.

The next product gap is:

```text
Игры
→ configure mechanics and parameters
→ Собрать и проверить игру
→ currently stops at a qualified package
→ user still does not receive a project-scoped Windows standalone build
```

Goal152 must add the first normal project-scoped standalone path:

```text
Игры
→ Сборка и проверка
→ Собрать Windows-игру (Alpha)
→ qualified current project
→ Runtime-owned PlayerAdapter payload
→ generic cached Unity host
→ project-scoped Windows build folder
→ launch smoke
→ Запустить игру
→ Открыть папку сборки
```

This is not a Goal-number diagnostic panel.
This is not the historical three-style Alpha build.
This is not Goal142 selected-candidate fallback.
This is not a copy of a sample package.

It must build the currently opened game project and its currently selected mechanics/parameters.

## Required first deliverable — record human acceptance

Record the exact owner decision:

```text
Я принимаю Goal151 и объединённую ручную проверку Goals149/150/150A/150B: свежий бинарник commit 2516931f успешно собрал и проверил проект goal148-manual с параметрами 3/8/2/12; equipment/stat/total=3/6/9, level/XP=2/12, интерфейс и диагностика корректны.
```

Required accepted milestone state:

```text
goal149Accepted=true
goal149AcceptedByHuman=true
goal150Accepted=true
goal150AcceptedByHuman=true
goal150aAccepted=true
goal150aAcceptedByHuman=true
goal150bAccepted=true
goal150bAcceptedByHuman=true
goal151Accepted=true
goal151AcceptedByHuman=true
acceptedByCodex=false
manualReviewPerformed=true
acceptedCommit=2516931f9c8242bbd59fe5cf73f9e66b405ef16c
customValues=3/8/2/12
equipmentStatTotal=3/6/9
levelExperience=2/12
interfaceAndDiagnosticsAccepted=true
```

Update:

```text
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md
docs/manual-acceptance/real-saved-project-build-recovery-and-diagnostic-truth-hotfix.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
```

Write a compact normalized acceptance record under Goal152 procedural/export roots.
Do not commit screenshots or unrelated raw chat content.

Goals150C/150D/150E/150F remain historical validation-infrastructure records/debt.
They must not remain the active product gate.

Goal152 itself remains:

```text
accepted=false
acceptedByHuman=false
acceptedByCodex=false
manualReviewPerformed=false
```

until its own final user review.

## User-visible outcome

In the existing `Игры → Сборка и проверка` tab add a clear Windows standalone section.

Required controls:

```text
Unity Editor:
  resolved path
  Найти автоматически
  Выбрать...

Собрать Windows-игру (Alpha)
Отменить
Запустить игру
Открыть папку сборки
```

Required status surface:

```text
Текущий этап
Общий статус
Последняя успешная standalone-сборка
Путь к exe
Путь к папке
Unity version
Host cache: rebuilt|reused
Package SHA-256
Final Runtime state hash
Selected mechanics count
Configured parameters count
PlayerAdapter frame count
Launch smoke status
First causal diagnostic
```

The UI must remain responsive while Unity runs.
Build, launch and folder buttons must be enabled only when valid for the current state.
No PowerShell/test/compiler child process from the UI. The WinForms UI calls an in-process Application service.
The Application service may launch the explicitly resolved Unity Editor and the produced standalone executable.

## Truthful product label

The button and output must state that this is:

```text
Windows standalone Alpha
Runtime-backed PlayerAdapter
Gameplay truth: Runtime
```

Do not claim that Unity owns gameplay.
Do not call it a final release.
Do not hide that it is the first project-scoped standalone PlayerAdapter host.

## Architecture

### A. Generic cached standalone host

Build a generic Unity Windows host that is independent of any specific game project.

The host must:

- contain no hardcoded project ID, package ID, project title, module IDs or parameter values;
- load project payload from its own external StreamingAssets directory;
- display and navigate Runtime-derived PlayerAdapter frames;
- show project identity and current build metadata;
- expose first/previous/next/last/auto-step/auto-play/reset/quit controls;
- never calculate or mutate gameplay independently;
- preserve `runtimeAuthority=true`, `unityGameplayTruth=false`, `projectionOnly=false`;
- support command-line smoke mode and deterministic exit code.

Use a new compact runtime bootstrap rather than adding more project-specific branches to the historical
`AlphaRuntimeBootstrap`.

Recommended files:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/ProjectStandalonePlayerAdapterBootstrap.cs
unity/LLMGameCreatorAlpha/Assets/Editor/ProjectStandaloneBuildEntrypoint.cs
```

The build entrypoint generates its scene at build time.
Do not add committed scenes or prefabs.

### B. Host cache

A Unity host rebuild must not happen for every project or parameter edit.

Implement a cache outside the repository and outside individual game projects, under LocalAppData:

```text
%LOCALAPPDATA%\LLMGameCreator\StandaloneHostCache\<host-cache-key>\
```

The cache key must include at least:

```text
Unity Editor version
ProjectStandalonePlayerAdapterBootstrap source hash
ProjectStandaloneBuildEntrypoint source hash
relevant Unity project/package manifest hashes
target=StandaloneWindows64
development/debug/profiler flags
```

Required behavior:

```text
first build -> host rebuilt once
second project assembly with only payload/config changes -> same host reused
host source or Unity version change -> cache invalidated and rebuilt
partial/corrupt cache -> rejected and rebuilt transactionally
```

Do not use timestamps as cache identity.

### C. Project-scoped standalone output

Output under the opened game project:

```text
<project>\Builds\Windows\<safe-project-slug>\
```

Required output:

```text
<safe-project-slug>.exe
<safe-project-slug>_Data\
UnityPlayer.dll and other Unity runtime files as required
build-manifest.json
README.txt
```

Project slug is derived generically from package ID/folder identity.
No hardcoded `goal148-manual`.

Build through a temporary sibling/staging directory, then atomically replace the successful output.
A failed build must preserve the previous successful standalone folder byte-for-byte.

### D. Project payload

After the current project has passed normal `BuildAndQualify`, create a payload from the actual activated project.

Required payload in the built player's StreamingAssets:

```text
LLMGameCreatorProject/project-manifest.json
LLMGameCreatorProject/game-package.json
LLMGameCreatorProject/player-adapter-model.json
LLMGameCreatorProject/player-adapter-frames.json
LLMGameCreatorProject/standalone-launch.json
```

Payload requirements:

```text
project package ID/title/version
project-scoped composition ID
activated package SHA-256
composition package SHA-256
final Runtime state hash
selected module IDs as an arbitrary data-driven list
effective parameter values as an arbitrary data-driven list
Runtime playthrough plan ID
capability IDs as an arbitrary data-driven list
ordered PlayerAdapter frames
equipment/attributes/progression summaries when present
checkpoint/full-replay/action-binding proofs
runtimeAuthority=true
unityGameplayTruth=false
projectionOnly=false
sourceCommit
```

No fixed module count and no fixed content count.
The `3/8/2/12` case is a regression fixture, not a product limit.

### E. Current-project Runtime payload service

Add an Application service that derives the payload from the currently opened, successfully qualified project.

It must reuse:

```text
FeatureModuleParameterBindingService
CapabilityDrivenRuntimePlaythroughPlanner
ProductLineRuntimeQualifier
canonical Runtime session/snapshots/action journal
Goal151 project identity and build-attempt truth
```

Do not duplicate gameplay calculations.
Do not use Goal142/Goal143 historical artifacts as source data.
Do not fall back to `samples/minimal-map-game/package.json`.

PlayerAdapter frames must come from the current project's real Runtime qualification.
A project/package/hash mismatch must fail before Unity build assembly.

### F. Unity discovery and persisted setting

Resolve Unity Editor in this order:

```text
1. project-local saved standalone build setting
2. explicit value chosen in UI
3. UNITY_EDITOR_PATH environment variable
4. installed Unity Hub editor discovery
```

If multiple Hub editors exist, select deterministically and display the selected version/path.
Do not silently select an incompatible or missing executable.

Persist only the chosen editor path and safe build preferences under:

```text
<project>\.llmgc\standalone-build-settings.json
```

Do not place machine-specific paths in GamePackage or public schema.

### G. Build lifecycle service

Add a project-scoped service with explicit stages:

```text
validate_current_project
qualify_current_project
create_runtime_payload
resolve_unity_editor
resolve_or_build_host_cache
assemble_project_output
validate_output_manifest
launch_smoke
publish_success
```

Required result fields include:

```text
attemptId
status
stage
diagnostics
projectFolder
outputFolder
executablePath
packageSha256
finalStateHash
selectedModuleCount
configuredParameterCount
runtimePlanId
capabilityCount
frameCount
unityEditorPath
unityVersion
hostCacheKey
hostRebuilt
hostReused
launchSmokePassed
buildManifestPath
duration
```

On failure, always return a stable stage and non-empty diagnostics.
Persist compact successful/failed standalone attempt history under the project `.llmgc` area.

### H. Cancellation and concurrency

- one standalone build per controller/project at a time;
- second concurrent request rejected deterministically;
- Cancel requests process-tree termination only for the Unity process started by this service;
- cancellation preserves prior successful output and host cache;
- app shutdown must not leave a child Unity build process orphaned when cancellation was requested;
- never kill unrelated Unity processes.


## Standalone runtime UI

The produced executable must visibly show at least:

```text
project title
package ID/version
selected mechanics count
configured parameters count
current frame / total
frame title/category
canonical state hash
map summary
inventory summary
quest summary
combat summary
equipment summary
attributes summary
progression summary
Gameplay truth: Runtime
Unity mode: PlayerAdapter
```

Controls:

```text
First
Previous
Next
Last
Auto Step
Auto Play
Reset
Quit
```

Keyboard equivalents are allowed but buttons must exist.

The standalone must use the selected project title, not `LLMGameCreator Alpha` or a historical candidate name.

## Smoke contract

The standalone supports:

```text
-llmgcStandaloneSmokeExit
-llmgcStandaloneSmokeLogPath <path>
```

Smoke must exit `0` only when:

```text
payload files exist
payload schema versions supported
package SHA matches manifest
project identity matches
frame count > 0
first and last frame reachable
Runtime authority markers valid
final state hash present
selected module list loaded
custom equipment/attributes/progression summaries loaded when configured
```

Smoke log contains stable markers:

```text
LLMGC_PROJECT_STANDALONE_LOAD_PASS
LLMGC_PROJECT_STANDALONE_FRAME_PASS
LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS
LLMGC_PROJECT_STANDALONE_SMOKE_PASS
```

No product data is sourced from command-line text.

## Acceptance regression fixture

Use a disposable copy of:

```text
C:\Users\endim\AppData\Local\LLMGameCreator\Games\goal148-manual
```

The source is read-only. Confirm before/after manifest equality.

Through the normal controller path on the copy:

```text
select equipment/attributes/progression
weapon=3
strength=8
damage-per-strength=2
level2 XP=12
save
reopen
normal BuildAndQualify
standalone build
launch smoke
```

Required:

```text
normal build GREEN
equipment/stat/total=3/6/9
level/XP=2/12
capability/action/checkpoint/replay=14/20/16/20
standalone payload project identity matches
standalone frame count > 0
standalone launch smoke GREEN
output exe exists
Data directory exists
build manifest hashes physical files
original source project byte-identical
```

Then change only one project parameter on a second disposable copy/assembly.

Required:

```text
hostReused=true
Unity host build count remains 1
payload/build-manifest hash changes
host executable hash remains unchanged
standalone smoke remains GREEN
```

## Negative tests

Required:

```text
dirty/invalid project build failure blocks standalone build
package changed after qualification is rejected
payload package hash mismatch rejected
final-state hash mismatch rejected
missing Unity path gives actionable stage
invalid Unity executable rejected
corrupt host cache rebuilt transactionally
Unity build failure preserves previous output
launch smoke failure preserves previous successful output
path escape rejected
concurrent build rejected
cancellation kills only owned Unity process tree
Goal142/Goal143/sample fallback absent
```

## Acceptance recording and validation-policy cleanup

Update current source-of-truth so:

```text
Goals149/150/150A/150B/151 = accepted by human
Goal150F 64 failures = historical validation debt
repair_64_historical_closure_failures_before_rerun is not active nextProductGoal
Goal152 is the active product goal
```

Do not mark the 64 failures fixed.
Do not delete their evidence.
Do not run their closure in Goal152.

## Command and investigation budget

Mandatory:

```text
read-first: maximum 12 primary files
first production edit after one focused architecture pass
focused .NET build/tests: maximum 15 minutes
Unity host build: exactly one real build, maximum 25 minutes
standalone smoke + cache-reuse proof: maximum 10 minutes
total target wall clock: 55 minutes
maximum two testhost processes
maximum one owned Unity build process
```

Rules:

- no unchanged Unity rebuild;
- if the first Unity build fails, diagnose from its log and rerun only after a concrete code/config fix;
- no full suite;
- no 85-case historical closure;
- no all-ProductSmoke sweep;
- no repair of unrelated historical snapshots;
- raw Unity logs remain ignored under `.devflow/runs`;
- compact committed evidence only.

## Required validation

### T0 and focused product tests

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal152"
dotnet test ... --filter "FullyQualifiedName~Goal151"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
```

### Existing product regressions

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

### Real standalone proof

Run exactly one real Unity host build and the two project assembly/smoke scenarios described above.

Do not invoke the historical AlphaRunnableBuild three-style service as the project source.

## Artifact scope

Add an exact Goal152 scenario.

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal152-project-windows-standalone-build-launch.ps1
.devflow/scripts/run-goal152-project-windows-standalone-build-launch.cmd

src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs
src/LLMGameCreator.WinForms/CompositionRoot.cs

unity/LLMGameCreatorAlpha/Assets/Scripts/ProjectStandalonePlayerAdapterBootstrap.cs
unity/LLMGameCreatorAlpha/Assets/Editor/ProjectStandaloneBuildEntrypoint.cs

tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/**
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal152ProjectStandaloneWorkspaceTests.cs
tests/LLMGameCreator.Tests/WinForms/Goal152ProjectsPageStandaloneBuildTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal152ProjectStandaloneBuildScriptTests.cs

docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md
docs/manual-acceptance/real-saved-project-build-recovery-and-diagnostic-truth-hotfix.md
docs/manual-acceptance/project-scoped-windows-standalone-build-launch.md

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/agent-tasks/goal-152-accepted-mechanics-milestone-and-project-scoped-windows-standalone-build-launch/
.llmgc/procedural/goal-152-accepted-mechanics-milestone-and-project-scoped-windows-standalone-build-launch/
.llmgc/exports/goal-152-accepted-mechanics-milestone-and-project-scoped-windows-standalone-build-launch/
```

If exact discovery shows a required existing project settings/model path:

1. record why;
2. add only the exact path;
3. do not authorize broad Settings/Infrastructure prefixes.

Forbidden without a separately proven blocker:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
unity/LLMGameCreatorAlpha/Assets/Scenes/**
unity/LLMGameCreatorAlpha/Assets/Prefabs/**
unity/LLMGameCreatorAlpha/Assets/StreamingAssets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
```

No public schema change.

## Compact evidence

Maximum 12 files per root:

```text
accepted-mechanics-milestone-record.json
goal152-dashboard.json
standalone-host-cache-proof.json
project-payload-proof.json
real-project-copy-standalone-proof.json
host-reuse-parameter-change-proof.json
standalone-launch-smoke-proof.json
negative-proof.json
focused-regression-proof.json
artifact-scope-proof.json
goal152-file-index.json
goal152-report.md
```

Do not commit:

```text
Unity build output
host cache
copied real project
raw Unity logs
TRX files
screenshots
machine-specific Unity path
```

## Publication policy

Create exactly one final commit:

```text
GREEN Goal 152 accepted mechanics milestone and project-scoped Windows standalone build launch
```

or honest `BLOCKED` / `FAILED`.

Codex performs:

```powershell
git commit
git push origin main
git rev-parse HEAD
git rev-parse origin/main
git status --short
```

Required final state:

```text
HEAD == origin/main
worktree clean
```

The owner must not push manually.

## GREEN criteria

```text
Goals149/150/150A/150B/151 acceptedByHuman=true
exact owner acceptance recorded
Goal150F historical failures remain debt and inactive as product gate
normal Games UI exposes standalone build/launch/folder controls
current project is the sole package source
no Goal142/Goal143/sample fallback
Unity editor resolution is deterministic and actionable
generic host cache works
one real Unity host build succeeded
second payload assembly reused host
real copied goal148-manual standalone built
3/8/2/12 payload correct
standalone smoke passed
previous successful output protected on failure
UI responsive and stage-aware
focused validation GREEN
artifact scope 0 violations
Goal152 accepted=false
Goal152 manualGateReady=true
one final commit pushed
```

## Final report

Return exactly `GREEN`, `BLOCKED` or `FAILED`, then include:

- model/reasoning used;
- acceptance-record fields;
- final commit/push/HEAD/worktree;
- Unity path/version used for validation;
- host cache key and rebuilt/reused counts;
- real project-copy normal build result;
- standalone output/exe manifest result;
- smoke markers and exit code;
- project identity/package/final hash/frame count;
- 3/8/2/12 and 3/6/9, 2/12 results;
- second payload host-reuse result;
- negative tests;
- focused commands;
- artifact scope;
- Goal152 manualGateReady;
- confirmation Goal152 human acceptance not claimed.
