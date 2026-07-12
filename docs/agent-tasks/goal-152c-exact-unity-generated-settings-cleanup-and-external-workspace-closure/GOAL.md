# Goal 152C — Exact Unity-Generated Settings Cleanup + External Workspace Closure

## Identity

- Task ID: `goal-152c-exact-unity-generated-settings-cleanup-and-external-workspace-closure`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `c0ba139fda7a1f8901ec59530828c170d064d2cb`
- Required base message: `BLOCKED Goal 152B Unity worktree cleanup and external build workspace hardening`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: Medium
```

Reason: the remaining work is exact cleanup of 21 already identified generated files plus bounded
Application-service hardening. Goal152A product UX is already implemented and validated.

## Pre-approval

The owner approved execution by launching this task.

- Do not ask for plan confirmation.
- Do not ask the owner to clean files manually.
- Do not run Unity Editor.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No validation-candidate commits.

## Explicit exact-path deletion authorization

The following 21 paths are known Unity-generated, untracked, absent from published `HEAD`, and safe
to delete. The owner explicitly authorizes deleting these exact paths:

```text
unity/LLMGameCreatorAlpha/Packages/packages-lock.json
unity/LLMGameCreatorAlpha/ProjectSettings/AudioManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/ClusterInputManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/DynamicsManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/EditorBuildSettings.asset
unity/LLMGameCreatorAlpha/ProjectSettings/EditorSettings.asset
unity/LLMGameCreatorAlpha/ProjectSettings/GraphicsSettings.asset
unity/LLMGameCreatorAlpha/ProjectSettings/InputManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/MemorySettings.asset
unity/LLMGameCreatorAlpha/ProjectSettings/MultiplayerManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/NavMeshAreas.asset
unity/LLMGameCreatorAlpha/ProjectSettings/Physics2DSettings.asset
unity/LLMGameCreatorAlpha/ProjectSettings/PresetManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/ProjectSettings.asset
unity/LLMGameCreatorAlpha/ProjectSettings/QualitySettings.asset
unity/LLMGameCreatorAlpha/ProjectSettings/SceneTemplateSettings.json
unity/LLMGameCreatorAlpha/ProjectSettings/TagManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/TimeManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/UnityConnectSettings.asset
unity/LLMGameCreatorAlpha/ProjectSettings/VFXManager.asset
unity/LLMGameCreatorAlpha/ProjectSettings/VersionControlSettings.asset
```

No other pre-existing dirty path is authorized for deletion or restore in Goal152C.

Before deletion, verify for every path:

```text
path exists
git ls-files --error-unmatch <path> fails
path is inside repository unity/LLMGameCreatorAlpha
path exactly matches the authorized list
```

If any authorized path is tracked, outside the repository, missing from the expected dirty inventory,
or resolves through a symlink/reparse escape, stop and publish BLOCKED.

Delete with exact literal paths only.

Forbidden:

```text
git clean
git reset --hard
broad git restore
wildcard Remove-Item over ProjectSettings or Packages
```

## Current state

Goal152B already completed:

```text
initial dirty inventory=189
tracked restored=13
untracked deleted=154
Unity .meta deleted=154
historical artifacts restored=12
remaining unknown=21
manual/user-game paths touched=0
Unity process count=0
```

Its remote commit added only the task files and `BLOCKED.md`; no generated Unity files entered Git.

Goal152A published implementation already contains:

```text
clearing Camera
opaque full-frame repaint
responsive Russian standalone UI
12/12 self-check
hidden smoke with five markers
one host build
second project host reuse
real-copy 3/8/2/12
damage 3/6/9
progression 2/12
```

Goal152 and Goal152A remain human-unaccepted.

## Objective

1. Delete the exact 21 authorized generated files.
2. Prove clean baseline before product edits.
3. Complete external Unity build workspace hardening from Goal152B.
4. Reuse the existing Goal152A host cache with zero Unity Editor invocations.
5. Publish Goal152A implementation GREEN and ready for the five-step human gate.
6. End with clean worktree and synchronized `origin/main`.

## Unity execution budget

```text
Unity Editor invocation budget: 0
Standalone cached-player hidden smoke budget: 1
```

Starting `Unity.exe` is a P1 violation.

## Phase A — exact cleanup closure

### A1. Capture current status

Capture:

```text
git rev-parse HEAD
git rev-parse origin/main
git status --porcelain=v2 -z
```

Require both refs equal `c0ba139fda7a1f8901ec59530828c170d064d2cb`.

The only pre-existing dirty paths allowed before Goal152C files are the exact 21 paths above.

Goal152C task files under:

```text
docs/agent-tasks/goal-152c-exact-unity-generated-settings-cleanup-and-external-workspace-closure/
```

are expected and must be preserved.

### A2. Delete exact files

For each authorized path:

1. resolve full path and prove it stays under repository root;
2. prove it is untracked;
3. record SHA-256 and size in local raw inventory;
4. delete with `Remove-Item -LiteralPath`;
5. verify path no longer exists.

Raw inventory:

```text
%LOCALAPPDATA%\LLMGameCreator\Goal152C\exact-cleanup-before.json
```

Commit only sanitized counts and path basenames/categories, not machine-local absolute paths.

### A3. Baseline proof

After deleting all 21 files require:

```text
authorizedDeleted=21
authorizedRemaining=0
unknownDirtyPaths=0
trackedDiff=0
stagedDiff=0
worktree contains only Goal152C unpacked files
```

Do not proceed to Phase B until this passes.

## Phase B — external writable Unity workspace

Future host builds must never use the repository Unity project as writable `-projectPath`.

### B1. New workspace service

Add:

```text
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/UnityHostBuildWorkspaceService.cs
```

Responsibilities:

```text
compute deterministic workspace source key
resolve short LocalAppData workspace root
prepare transactional writable Unity project snapshot
produce Unity command projectPath
hash source repository snapshot before/after preparation
clean failed staging
```

Workspace root:

```text
%LOCALAPPDATA%\LLMGameCreator\UnityHostBuildWorkspaces\<workspace-key>\
```

### B2. Source snapshot

Read from repository:

```text
unity/LLMGameCreatorAlpha/Assets
unity/LLMGameCreatorAlpha/Packages
unity/LLMGameCreatorAlpha/ProjectSettings
```

Copy only into external staging.

Exclude:

```text
Library/**
Temp/**
Logs/**
obj/**
UserSettings/**
Assets/StreamingAssets/**
Assets/__LLMGC_ProjectStandaloneBuild__.unity
Assets/__LLMGC_ProjectStandaloneBuild__.unity.meta
all repository-local standalone output
```

It is valid for the external workspace to generate:

```text
Packages/packages-lock.json
ProjectSettings/*.asset
Library/**
*.meta
```

None may appear in the repository worktree.

### B3. Transactional publication

Flow:

```text
workspaceRoot.staging-<attempt>
copy source snapshot
validate Assets/Packages/ProjectSettings exist
write workspace manifest
atomically replace prepared workspace
```

If preparation fails, repository and prior prepared workspace remain unchanged.

### B4. BuildHost integration

Modify `ProjectStandaloneBuildService.BuildHost()`:

```text
repository Unity tree = read-only source
external prepared workspace = Unity -projectPath
host output = existing transactional host cache temp
```

Unity command must contain:

```text
-batchmode
-nographics
-quit
-projectPath <LocalAppData external workspace>
```

It must never contain repository Unity path as the `-projectPath` value.

### B5. Cache hit behavior

When `HostIsComplete(hostRoot)` is true:

```text
do not prepare external workspace
do not start Unity
assemble payload/output only
run hidden standalone smoke
```

This is the path used for Goal152C proof.

## Phase C — test seams without Unity

Introduce a small process/command seam only as needed. Do not refactor unrelated code.

Tests must prove:

```text
external workspace is under LocalAppData
external workspace is outside repository
external workspace is outside user game folders
source snapshot excludes generated paths
required Assets/Packages/ProjectSettings are copied
repository snapshot manifest unchanged after preparation
Unity command projectPath is external
repository Unity path is absent as writable projectPath
failed preparation is transactional
cache hit starts zero Unity processes
cache hit skips workspace preparation
hidden smoke includes -batchmode -nographics
normal LaunchLastBuild remains visible/no headless flags
```

Do not invoke Unity in tests.

## Phase D — existing-cache hidden proof

Use the existing valid Goal152A cache.

Use a short disposable LocalAppData copy of `goal148-manual`.

Required:

```text
normal project qualification GREEN
3/8/2/12
3/6/9
2/12
20 frames
HostReused=true
HostRebuilt=false
Unity process start count=0
hidden standalone smoke exit=0
all five markers present
source project byte-identical
```

Required markers:

```text
LLMGC_PROJECT_STANDALONE_LOAD_PASS
LLMGC_PROJECT_STANDALONE_INTEGRITY_PASS
LLMGC_PROJECT_STANDALONE_NAVIGATION_PASS
LLMGC_PROJECT_STANDALONE_RUNTIME_AUTHORITY_PASS
LLMGC_PROJECT_STANDALONE_SMOKE_PASS
```

If the existing cache is absent or invalid, publish BLOCKED. Do not invoke Unity.

## Phase E — source-of-truth

On GREEN:

```text
Goal152 implementationStatus=GREEN
Goal152 accepted=false
Goal152 acceptedByHuman=false
Goal152 manualReviewPerformed=true
Goal152 manualGateReady=false

Goal152A implementationStatus=GREEN
Goal152A accepted=false
Goal152A acceptedByHuman=false
Goal152A manualReviewPerformed=false
Goal152A manualGateReady=true

Goal152B implementationStatus=BLOCKED historical cleanup attempt
Goal152C implementationStatus=GREEN
Goal152C accepted=false
```

Active next action:

```text
perform_goal152a_five_step_human_gate
```

The human gate is exactly:

```text
1. Launch standalone.
2. Confirm large green “Автопроверка пройдена”.
3. Click Далее, Назад, В конец and Сбросить.
4. Confirm no ghosting/overlap and readable controls.
5. Close.
```

No hash/count inspection.

## Command and investigation budget

```text
read-first: maximum 7 files
exact cleanup: maximum 3 minutes
external-workspace implementation/tests: maximum 10 minutes
focused build/tests: maximum 8 minutes
cached hidden proof: maximum 3 minutes
total target: 22 minutes
maximum two testhost processes
Unity Editor process count: 0
```

Forbidden:

```text
Unity.exe
full suite
85-case closure
all-ProductSmoke
broad cleanup
manual user cleanup
historical snapshot repair
```

## Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal152C"
dotnet test ... --filter "FullyQualifiedName~Goal152B"
dotnet test ... --filter "FullyQualifiedName~Goal152A"
dotnet test ... --filter "FullyQualifiedName~Goal152ProjectStandaloneBuildTests"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"

.\.devflow\scripts\check-current-goal.ps1
```

Then one existing-cache hidden standalone proof.

## Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal152c-exact-unity-cleanup.ps1
.devflow/scripts/run-goal152c-exact-unity-cleanup.cmd

src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/UnityHostBuildWorkspaceService.cs

tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal152CUnityWorkspaceClosureTests.cs
tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal152ProjectStandaloneBuildTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal152CExactUnityCleanupScriptTests.cs

AGENTS.md
docs/UNITY_EXECUTION_POLICY.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/project-scoped-windows-standalone-build-launch.md
docs/manual-acceptance/standalone-playeradapter-ux-framebuffer-refresh-hotfix.md

docs/agent-tasks/goal-152c-exact-unity-generated-settings-cleanup-and-external-workspace-closure/
.llmgc/procedural/goal-152c-exact-unity-generated-settings-cleanup-and-external-workspace-closure/
.llmgc/exports/goal-152c-exact-unity-generated-settings-cleanup-and-external-workspace-closure/
```

No Unity source, Runtime, GamePackage, samples, providers, Lua, generator-library, ProjectSettings or
Packages files may be committed.

The exact deletion of the 21 untracked paths creates no commit diff.

## Compact evidence

Maximum 8 files per root:

```text
goal152c-dashboard.json
exact-21-cleanup-proof.json
external-workspace-proof.json
repository-immutability-proof.json
cache-hit-zero-unity-proof.json
hidden-smoke-proof.json
artifact-scope-proof.json
goal152c-report.md
```

## Publication

Create exactly one final commit:

```text
GREEN Goal 152C exact Unity generated settings cleanup and external workspace closure
```

or honest BLOCKED/FAILED.

Codex pushes it.

Required final state:

```text
HEAD == origin/main
worktree clean
Unity process start count=0
authorized 21 paths absent
Goal152/Goal152A accepted=false
Goal152A manualGateReady=true only on GREEN
```

## GREEN criteria

```text
21 exact generated files deleted
unknown dirty paths=0
clean baseline reached
external writable Unity workspace implemented
future Unity projectPath outside repository
repository source immutable in tests
existing host cache reused
Unity invocation count=0
hidden smoke five markers GREEN
focused tests GREEN
artifact scope 0 violations
Goal152A GREEN/manualGateReady=true
one final commit pushed
final worktree clean
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- exact deletion count;
- missing/tracked/escape anomalies;
- unknown dirty paths count;
- external workspace path contract;
- repository source manifest before/after;
- Unity invocation count;
- cache reuse/rebuild;
- hidden smoke markers;
- real-copy values;
- focused tests;
- artifact scope;
- Goal152/152A/152B/152C flags;
- five-step human gate;
- final commit/push/HEAD/worktree.
