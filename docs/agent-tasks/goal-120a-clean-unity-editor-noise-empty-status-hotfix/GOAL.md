# Goal 120A — clean-unity-editor-noise empty-status hotfix

## Task ID

`goal-120a-clean-unity-editor-noise-empty-status-hotfix`

## Repo / working copy / branch

- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Why this hotfix exists

Goal120 added `.devflow/scripts/clean-unity-editor-noise.ps1` and `.cmd`.

Manual use proved the cleanup works: it removed Unity editor noise and final `git status --short --untracked-files=all` was clean.

But the script then failed when the post-apply status was empty:

```text
Get-CleanupTargets : Не удается привязать аргумент к параметру "StatusLines", так как он имеет значение NULL.
```

This is a script UX bug. A clean worktree must be treated as an empty status list, not an error.

## Goal type

Focused P1 process hotfix.

Do not turn this into another review/evidence-only goal. Fix the cleanup script and add tests so this does not recur.

## Read first

```text
.devflow/scripts/clean-unity-editor-noise.ps1
.devflow/scripts/clean-unity-editor-noise.cmd
tests/LLMGameCreator.Tests/DevFlow/CleanUnityEditorNoiseScriptTests.cs
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
```

## Allowed paths

You may create or modify only:

```text
docs/agent-tasks/goal-120a-clean-unity-editor-noise-empty-status-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/clean-unity-editor-noise.ps1
.devflow/scripts/clean-unity-editor-noise.cmd

.llmgc/procedural/goal-120a-clean-unity-editor-noise-empty-status-hotfix/**
.llmgc/exports/goal-120a-clean-unity-editor-noise-empty-status-hotfix/**

docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/CONTEXT_INDEX.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

tests/LLMGameCreator.Tests/DevFlow/CleanUnityEditorNoiseScriptTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/AcceptedAlphaUnityPlayableProjectionProductSmokeTests.cs
```

## Forbidden paths

Do not modify, stage, or commit:

```text
.llmgc/manual/**
unity/LLMGameCreatorAlpha/Assets/**
unity/LLMGameCreatorAlpha/ProjectSettings/**
unity/LLMGameCreatorAlpha/Packages/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
provider / LLM / RAG / media provider code
public GamePackage schema files
Lua / Scripting code
generator-library/**
*.sln
*.csproj
Directory.Build.*
dependency/package files
```

No Unity source changes for this hotfix. No Runtime/schema/providers/Lua/generator-library work.

## Exact behavior

Fix `.devflow/scripts/clean-unity-editor-noise.ps1` so that:

1. Empty `git status --porcelain=v1 --untracked-files=all` output is represented as an empty array, not `$null`.
2. `Get-CleanupTargets` accepts an empty status list without error.
3. `-Apply` exits 0 when cleanup succeeds and no status lines remain.
4. Final output still prints:

```text
Final status:
```

even if there are no lines after it.
5. Dry-run on a clean worktree also exits 0.
6. The safety constraints from Goal120 remain unchanged:
   - no broad `git clean`;
   - deletes only untracked `Assets/**/*.meta`, `Packages/packages-lock.json`, `ProjectSettings/*.asset`;
   - restores only `ProjectSettings/ProjectVersion.txt`;
   - refuses staged files by default;
   - never deletes `.cs`, `.json`, `.md`, `.unity`, `.prefab`.

Preferred implementation details:

- Make `Invoke-CleanupGitStatus` always return `[string[]]`.
- Change `Get-CleanupTargets` parameter to tolerate empty arrays.
- Avoid relying on PowerShell single-item/null coercion.

## Tests

Update/add tests to verify:

- script contains explicit empty-status handling;
- `Get-CleanupTargets` does not require non-null/non-empty status lines;
- script still refuses staged files by default;
- script still has no broad `git clean -fd`;
- script still never removes `.cs`, `.json`, `.md`, `.unity`, `.prefab`;
- script still removes only allowed Unity noise.

If safe and already practical, add a test that invokes the script with `-DryRun` on the current clean repo and expects exit 0. If process execution is too broad for the test suite, source-contract tests are acceptable.

## Artifacts

Create compact deterministic artifacts under:

```text
.llmgc/procedural/goal-120a-clean-unity-editor-noise-empty-status-hotfix/
.llmgc/exports/goal-120a-clean-unity-editor-noise-empty-status-hotfix/
```

Recommended files:

```text
clean-unity-editor-noise-empty-status-hotfix-dashboard.json
clean-unity-editor-noise-empty-status-hotfix-script-scan.json
clean-unity-editor-noise-empty-status-hotfix-report.md
clean-unity-editor-noise-empty-status-hotfix-negative-proof.json
clean-unity-editor-noise-empty-status-hotfix-file-index.json
```

Do not embed raw `.llmgc/manual/**`.

## Docs/current state

Update current-state and queue docs briefly:

- Goal120A fixes the cleanup script null/empty-status bug found during manual Goal120 verification.
- After Unity manual checks, the supported command remains:

```text
.devflow\scripts\clean-unity-editor-noise.cmd
```

or:

```text
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
```

- This is not final release and does not authorize forbidden lanes.

## Artifact-scope policy

Add scenario:

```text
goal-120a-clean-unity-editor-noise-empty-status-hotfix
```

It must allow only Goal120A expected files and exclude `.llmgc/manual/**`, Unity files, Runtime/schema/provider/Lua/generator-library.

## Validation

Run:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~CleanUnityEditorNoiseScript|FullyQualifiedName~AcceptedAlphaUnityPlayableProjection"
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun
.\.devflow\scripts\clean-unity-editor-noise.ps1 -Apply
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-120a-clean-unity-editor-noise-empty-status-hotfix
git diff --check
git diff --cached --check
git status --short --untracked-files=all
git ls-files .llmgc/manual
```

Final git status must be clean.

Unity batchmode smoke is not required because no Unity source files may change.

## Quality gate

GREEN requires:

- script no longer fails on clean/empty status;
- `-DryRun` and `-Apply` exit 0 on clean worktree;
- tests/checks pass;
- no forbidden path changes;
- no `.llmgc/manual/**` staged/tracked;
- final git status clean.

BLOCKED if the script cannot be made safe without broadening cleanup rules.

FAILED if build/tests break or forbidden changes are required.

## Commit / push policy

Before commit:

```powershell
git diff --cached --name-only
git diff --cached --check
git diff --cached --name-only | Select-String -SimpleMatch ".llmgc/manual"
```

The last command must produce no matches.

Commit and push with one of:

```text
GREEN Goal 120A clean unity editor noise empty status hotfix
BLOCKED Goal 120A clean unity editor noise empty status hotfix
FAILED Goal 120A clean unity editor noise empty status hotfix
```

Final report must include commit SHA, dry-run/apply results, tests/checks, final git status, and remaining debt.
