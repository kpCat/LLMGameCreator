# Goal 152B — Unity Worktree Churn Cleanup + External Build Workspace Hardening

## Identity

- Task ID: `goal-152b-unity-worktree-churn-cleanup-and-external-build-workspace-hardening`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `832d9de32d07a37c121fc6e4719ea8d82ec55316`
- Required base message: `BLOCKED Goal 152A standalone PlayerAdapter UX framebuffer refresh and Unity execution policy hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: Medium
```

Reason: Goal152A product code is already complete and verified. This task is a bounded filesystem-safety cleanup plus a small Application-service hardening refactor. No gameplay or Unity presentation architecture work is required.

## Pre-approval and cleanup authorization

The owner approved execution by launching this task and explicitly authorizes:

1. inventorying the exact dirty paths present at task start;
2. restoring tracked Unity/test-generated churn paths to `HEAD`;
3. deleting only exact untracked Unity/test-generated churn paths from that captured inventory;
4. preserving every unrecognized/user-authored path;
5. committing Goal152B task/docs/evidence and external-build-workspace hardening.

Do not ask for plan confirmation.
Do not ask the owner to clean files manually.
Do not run Unity Editor.

## Current state

Goal152A at required base already proved:

```text
clearing Camera present
opaque full-screen repaint present
responsive Russian standalone shell present
standalone self-check GREEN
hidden smoke GREEN with all five markers
one host build
second payload reused host with no Unity rebuild
real-copy 20 frames
3/8/2/12
3/6/9
2/12
source project byte-identical
focused tests GREEN
```

The commit is `BLOCKED` only because the authorized Unity build left approximately 188 uncommitted generated changes in the local worktree:

```text
historical proof/artifact churn
Unity .meta files
ProjectSettings changes
other Unity import/build side effects
```

The remote commit contains only intended Goal152A files. The 188 dirty paths were not committed.

## Root cause

`ProjectStandaloneBuildService.BuildHost()` currently launches Unity with:

```text
-projectPath <repository>\unity\LLMGameCreatorAlpha
```

Unity therefore treats the live repository Unity project as writable and may rewrite `.meta`, ProjectSettings and generated/historical artifacts.

## Primary objectives

1. Clean the local worktree safely and exactly.
2. Preserve every committed Goal152A product change.
3. Prevent future host builds from using the repository Unity project as writable `-projectPath`.
4. Prove existing host-cache reuse and hidden standalone smoke without invoking Unity Editor.
5. Publish Goal152A as implementation GREEN and ready for the short human gate.
6. Keep Goal152 and Goal152A human acceptance false.

## Unity invocation budget

```text
Unity Editor invocation budget: 0
Standalone cached-player smoke budget: 1 hidden run
```

Any `Unity.exe` process start is a P1 task violation.

## Phase A — exact dirty inventory

Before changing any repository file:

1. verify:
   ```text
   HEAD=832d9de32d07a37c121fc6e4719ea8d82ec55316
   origin/main=832d9de32d07a37c121fc6e4719ea8d82ec55316
   ```
2. capture `git status --porcelain=v2 -z`;
3. capture exact tracked/untracked path lists;
4. write raw local inventory under:
   ```text
   %LOCALAPPDATA%\LLMGameCreator\Goal152B\dirty-before.json
   ```
5. record per path:
   ```text
   path
   status
   tracked
   current SHA-256 for regular file
   HEAD blob SHA when tracked
   category
   cleanupDecision
   ```

The Goal152B task directory unpacked by the owner is not pre-existing churn and must be retained.

## Phase B — classification and exact cleanup

### Tracked paths authorized for restore

A dirty tracked path may be restored only when it was present in the initial inventory and is inside:

```text
unity/LLMGameCreatorAlpha/**
.llmgc/procedural/**
.llmgc/exports/**
```

Rules:

- `.llmgc/manual/**` is forbidden;
- `src/**`, `tests/**`, `docs/**`, `AGENTS.md` and `.devflow/**` are not cleanup targets unless they are Goal152B files created after inventory;
- restore each exact path from `HEAD`;
- do not use `git reset --hard`;
- do not use broad `git restore .`.

Use exact pathspecs, preferably `--pathspec-from-file` with NUL separation.

### Untracked paths authorized for deletion

An untracked path may be deleted only when present in the initial inventory and matching:

```text
unity/LLMGameCreatorAlpha/Library/**
unity/LLMGameCreatorAlpha/Temp/**
unity/LLMGameCreatorAlpha/Logs/**
unity/LLMGameCreatorAlpha/obj/**
unity/LLMGameCreatorAlpha/UserSettings/**
unity/LLMGameCreatorAlpha/Assets/__LLMGC_ProjectStandaloneBuild__.unity
unity/LLMGameCreatorAlpha/Assets/__LLMGC_ProjectStandaloneBuild__.unity.meta
unity/LLMGameCreatorAlpha/**/*.meta
.llmgc/procedural/**
.llmgc/exports/**
.devflow/runs/**
```

Never delete:

```text
docs/agent-tasks/goal-152b-unity-worktree-churn-cleanup-and-external-build-workspace-hardening/**
.llmgc/manual/**
any user game under %LOCALAPPDATA%\LLMGameCreator\Games
any path outside repository root
```

Delete exact inventory paths only. Do not use broad `git clean -fdx`.

### Unknown paths

If any pre-existing dirty path cannot be classified:

- do not touch it;
- publish `BLOCKED`;
- list exact unknown paths.

## Phase C — clean baseline proof

After cleanup and before production edits, require:

```text
all authorized churn restored/deleted
unknown path count=0
git diff --cached empty
git diff empty
git status contains only Goal152B task files
```

Commit evidence counts:

```text
trackedRestored
untrackedDeleted
unityMetaDeleted
projectSettingsRestored
historicalArtifactsRestored
unknownPreserved
```


## Phase D — external Unity build workspace hardening

Modify `ProjectStandaloneBuildService` so future host rebuilds never use the live repository Unity project as writable `-projectPath`.

### External short workspace

Use:

```text
%LOCALAPPDATA%\LLMGameCreator\UnityHostBuildWorkspaces\<workspace-key>\
```

Required:

```text
outside repository root
outside user game project
short deterministic root
one workspace per host-source/toolchain key
transactional preparation
```

### Source snapshot

Prepare a writable Unity project snapshot from repository:

```text
unity/LLMGameCreatorAlpha/Assets
unity/LLMGameCreatorAlpha/Packages
unity/LLMGameCreatorAlpha/ProjectSettings
```

Exclude:

```text
Library
Temp
Logs
obj
UserSettings
Assets/StreamingAssets
Assets/__LLMGC_ProjectStandaloneBuild__.unity
Assets/__LLMGC_ProjectStandaloneBuild__.unity.meta
repository build outputs
```

The repository Unity tree is read-only input. The external workspace may generate `.meta`, Library and ProjectSettings churn.

### Transactional preparation

Required flow:

```text
compute workspace source key
prepare <workspace>.staging-<attempt>
copy exact source snapshot
validate required Assets/Packages/ProjectSettings
atomically publish prepared workspace
invoke Unity only against external workspace
clean staging on failure
```

A persistent imported Library is optional. Correct isolation is more important than import speed.

### Invocation contract

The constructed Unity command must satisfy:

```text
-projectPath points to LocalAppData external workspace
-projectPath never points inside repository
output remains transactional host-cache temp
-batchmode
-nographics
-quit
one owned process maximum
```

### Repository immutability guard

Before and after workspace preparation, hash repository Unity:

```text
Assets excluding ignored generated paths
Packages
ProjectSettings
```

Require byte-identical equality.

Application code may use filesystem hashes but must not invoke Git.

## Phase E — tests without Unity

Add focused tests proving:

```text
external workspace outside repository
external workspace under LocalAppData
Unity -projectPath uses external workspace
repository Unity path never used as writable projectPath
source snapshot excludes Library/Temp/Logs/obj/UserSettings/StreamingAssets/generated scene
required Assets/Packages/ProjectSettings copied
repository source manifest unchanged
failed workspace preparation leaves repository unchanged
host-cache hit bypasses workspace preparation and process launch
hidden standalone smoke still uses -batchmode -nographics
normal user launch remains visible
```

Use a fake/injected process runner or command planner. Do not launch Unity.

## Phase F — cached standalone proof

After code/tests are GREEN:

1. use a short disposable copy of `goal148-manual`;
2. use the existing valid Goal152A host cache;
3. require:
   ```text
   HostReused=true
   HostRebuilt=false
   Unity process start count=0
   hidden standalone smoke GREEN
   all five smoke markers present
   source project byte-identical
   ```
4. do not invalidate or rebuild the host cache.

If cache is unavailable/invalid, publish `BLOCKED`. Do not invoke Unity.

## Phase G — state publication

On GREEN update:

```text
Goal152 implementationStatus=GREEN
Goal152 accepted=false
Goal152 acceptedByHuman=false
Goal152 manualReviewPerformed=true

Goal152A implementationStatus=GREEN
Goal152A accepted=false
Goal152A acceptedByHuman=false
Goal152A manualGateReady=true

Goal152B implementationStatus=GREEN
Goal152B accepted=false
```

Clear the active cleanup blocker.

Next human gate:

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
read-first: maximum 8 primary files
dirty inventory/cleanup: maximum 5 minutes
external-workspace implementation/tests: maximum 10 minutes
focused build/tests: maximum 8 minutes
cached hidden smoke: maximum 3 minutes
total target: 25 minutes
maximum two testhost processes
Unity Editor process count: 0
```

Forbidden:

```text
Unity.exe
full suite
85-case closure
all-ProductSmoke
broad git reset
broad git clean
manual user cleanup
historical snapshot repair
```

## Required validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal152B"
dotnet test ... --filter "FullyQualifiedName~Goal152A"
dotnet test ... --filter "FullyQualifiedName~Goal152ProjectStandaloneBuildTests"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"

.\.devflow\scripts\check-current-goal.ps1
```

Then run one existing-cache hidden standalone proof with Unity process start count asserted zero.

## Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal152b-unity-worktree-cleanup.ps1
.devflow/scripts/run-goal152b-unity-worktree-cleanup.cmd

src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildService.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/ProjectStandaloneBuildModels.cs
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/UnityHostBuildWorkspaceService.cs

tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal152BUnityBuildWorkspaceIsolationTests.cs
tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal152ProjectStandaloneBuildTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal152BUnityWorktreeCleanupScriptTests.cs

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

docs/agent-tasks/goal-152b-unity-worktree-churn-cleanup-and-external-build-workspace-hardening/
.llmgc/procedural/goal-152b-unity-worktree-churn-cleanup-and-external-build-workspace-hardening/
.llmgc/exports/goal-152b-unity-worktree-churn-cleanup-and-external-build-workspace-hardening/
```

No Unity source, Runtime, GamePackage, samples, providers, Lua, generator-library, ProjectSettings or Packages changes may be committed.

Cleanup may restore/delete exact initial-inventory paths outside artifact scope because they return the worktree to published HEAD rather than creating commit diff.

## Compact evidence

Maximum 8 files per root:

```text
goal152b-dashboard.json
dirty-worktree-inventory-summary.json
exact-cleanup-proof.json
external-unity-workspace-proof.json
repository-immutability-proof.json
cached-hidden-smoke-proof.json
artifact-scope-proof.json
goal152b-report.md
```

Raw 188-path inventory remains under LocalAppData. Commit sanitized counts/categories only.

## Publication

Create exactly one final commit:

```text
GREEN Goal 152B Unity worktree cleanup and external build workspace hardening
```

or honest BLOCKED/FAILED.

Codex pushes it.

Required:

```text
HEAD == origin/main
worktree clean
no Unity process started
Goal152/Goal152A accepted=false
Goal152A manualGateReady=true only on GREEN
```

## GREEN criteria

```text
initial dirty inventory captured
all authorized generated churn restored/deleted exactly
unknown dirty count=0
worktree clean before task edits
Goal152A committed code preserved
future Unity projectPath external to repository
repository Unity source byte-identical in tests
Unity invocation count=0
existing host cache reused
hidden smoke GREEN with five markers
focused tests GREEN
artifact scope 0 violations
Goal152A GREEN/manualGateReady=true
one final commit pushed
final worktree clean
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- initial dirty path counts/categories;
- tracked restored/untracked deleted/unknown counts;
- confirmation no user/manual paths touched;
- external workspace path contract;
- repository before/after source manifest;
- Unity invocation count;
- host cache reused/rebuilt;
- hidden smoke markers;
- focused tests;
- artifact scope;
- Goal152/152A/152B flags;
- five-step manual gate;
- final commit/push/HEAD/worktree.
