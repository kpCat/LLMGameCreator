# Goal 145A — WinForms Candidate Selector Reentrancy + Selection Stability Hotfix

## Identity

- Task: `goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: commit `b88669c98ac7ffe1e15184be79c2049710cca95e` or a direct descendant

## Status model

Goal145 remains produced for review:

```text
goal145Accepted=false
acceptedByHuman=false
acceptedByCodex=false
```

Do not record Goal145 human acceptance in this hotfix.

## Defect

The Goal145 candidate combo subscribes to `SelectedValueChanged`. Its handler calls `SelectCandidate(id)` and then `BindGoal145VariantSessions()`. That bind method programmatically assigns both `DataSource` and `SelectedValue` on the same combo. Programmatic binding can fire `SelectedValueChanged`, re-enter the handler and invoke the bind again.

Possible result:

```text
Load Candidate Matrix
→ DataSource selects first row
→ SelectedValueChanged fires
→ controller changes from default exploration to alchemy
→ BindGoal145VariantSessions re-enters
→ recursive rebind / unstable selection / stack overflow
```

Existing Goal145 WinForms tests only inspect source strings and do not test selector event semantics.

## Required result

Fix candidate selector lifecycle so that:

1. Programmatic candidate-list binding never invokes user selection logic.
2. Programmatic restoration of `SelectedValue` never invokes user selection logic.
3. Only a real operator candidate commit changes the controller candidate.
4. A user selection is applied exactly once.
5. `Load Candidate Matrix` leaves `minimal-map-game-exploration-resource-focus` selected.
6. Selecting `minimal-map-game-combat-focus` remains selected after session start, state refresh, action refresh, checkpoint/replay refresh and all-candidate matrix refresh.
7. Candidate change resets the previous in-memory session/checkpoint/action/replay state.
8. Starting after combat selection uses combat identity and combat package SHA, never exploration or baseline.
9. WinForms starts no compiler, test or PowerShell child process.
10. The Goal145 four-candidate Runtime matrix, save/replay and Unity smoke remain GREEN.

## Preferred correction

Use `SelectionChangeCommitted` for actual operator changes instead of `SelectedValueChanged`.

Also use a bounded programmatic-binding guard where appropriate, for example `_goal145BindingCandidateList`, around all programmatic `DataSource`, `SelectedIndex` and `SelectedValue` changes.

The handler must:

```text
if binding guard → return
if no candidate ID → return
if ID equals controller.SelectedCandidateId → no reset and no recursive bind
otherwise select exactly once and refresh dependent controls
```

Do not use delayed timers, `Application.DoEvents`, swallowed recursion/stack exceptions, or removal of candidate refresh.

## UI behavior

Preserve the existing Goal145 tab and controls.

After load, displayed selection:

```text
minimal-map-game-exploration-resource-focus
```

After operator selects combat, displayed and controller selection:

```text
minimal-map-game-combat-focus
```

Action combo and session state must reflect the chosen candidate.

## Behavioral regression tests

Add tests that are behavioral, not only source scans.

### Controller persistence

```text
load matrix → default exploration
select combat → controller selected combat
start selected → session candidate combat
session package SHA == combat candidate SHA
refresh/read candidate rows → still combat
```

### Candidate-change reset

```text
start exploration
save checkpoint
select combat
session == null
checkpoint == null
last action == null
last replay == null
start combat → fresh combat session
```

### Programmatic binding does not re-enter

Exercise the production candidate binding/event seam, preferably on an STA thread with a real `ComboBox`, or extract a small internal binder/event coordinator that can be tested directly.

Required assertions:

```text
programmatic DataSource bind invokes selection callback 0 times
programmatic SelectedValue restore invokes selection callback 0 times
operator committed selection invokes callback exactly 1 time
rebind after selection preserves selected candidate
maximum recursive callback depth == 1
```

A source-string assertion alone is insufficient for this defect.

Retain source guards:

```text
SelectionChangeCommitted present
candidate combo not subscribed to SelectedValueChanged
ProcessStartInfo absent
dotnet test absent
powershell absent
```

## Required artifacts

Write under both:

```text
.llmgc/procedural/goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix/
.llmgc/exports/goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix/
```

At minimum:

```text
winforms-candidate-selector-hotfix-dashboard.json
winforms-candidate-selector-regression-proof.json
winforms-candidate-selector-hotfix-report.md
winforms-candidate-selector-hotfix-file-index.json
```

Dashboard fields:

```text
status=GREEN
programmaticBindInvokesSelectionCount=0
programmaticRestoreInvokesSelectionCount=0
operatorCommitInvokesSelectionCount=1
maximumSelectionCallbackDepth=1
defaultSelectionPreserved=true
combatSelectionPreserved=true
candidateChangeResetsSession=true
candidateChangeResetsCheckpoint=true
combatSessionUsesCombatPackage=true
goal145MatrixStillGreen=true
candidateCount=4
passedCandidateCount=4
distinctFinalStateHashCount=4
unitySmokeStillGreen=true
goal145Accepted=false
accepted=false
```

File index includes SHA-256.

## Current-state updates

Update truthfully:

```text
current_phase_title=Goal 145A WinForms candidate selector reentrancy and selection stability hotfix
goal145Accepted=false
goal145CandidateSelectorReentrancyFixed=true
goal145ProgrammaticBindingSelectionCount=0
goal145OperatorCommitSelectionCount=1
goal145DefaultSelectionPreserved=true
goal145CombatSelectionPreserved=true
goal145CandidateChangeResetsSession=true
goal145CandidateChangeResetsCheckpoint=true
nextProductGoal=retry_goal_145_winforms_operator_then_review
```

Do not mark Goal141 accepted.

## Read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_LINE_CORE_STRATEGY.md
docs/NARROW_ALPHA_EXPANSION_POLICY.md
docs/AUTOMATED_VALIDATION_TIERS.md

docs/agent-tasks/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/GOAL.md

src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal145.cs
src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionSelectionController.cs
src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixService.cs

tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal145Tests.cs
tests/LLMGameCreator.Tests/Application/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixTests.cs
```

## Allowed paths

Only:

```text
docs/agent-tasks/goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix/**
.devflow/artifact-scope/artifact-scope-policy.json

.llmgc/procedural/goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix/**
.llmgc/exports/goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix/**

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix.md

src/LLMGameCreator.Application/Design/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionSelectionController.cs
src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspacePageControl.Goal145.cs

tests/LLMGameCreator.Tests/Application/ProductLineInteractiveSessionMatrix/ProductLineInteractiveSessionMatrixTests.cs
tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/VisualWorldStreamPreviewWorkspaceGoal145Tests.cs
tests/LLMGameCreator.Tests/WinForms/Goal145CandidateSelectorBindingTests.cs
```

If the test project has a more appropriate existing WinForms test directory, use it without modifying the project file.

## Forbidden paths

Do not modify, stage or commit:

```text
.llmgc/manual/**
samples/minimal-map-game/**

.llmgc/procedural/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/exports/goal-142-runtime-significant-product-line-variant-matrix-and-selection-handoff/**
.llmgc/procedural/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**
.llmgc/exports/goal-143-selected-runtime-variant-end-to-end-playeradapter-handoff/**
.llmgc/procedural/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.llmgc/exports/goal-144-selected-runtime-variant-interactive-action-session-and-save-replay/**
.llmgc/procedural/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/**
.llmgc/exports/goal-144a-live-session-action-target-binding-and-replay-evidence-hotfix/**
.llmgc/procedural/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/**
.llmgc/exports/goal-145-operator-selectable-product-line-runtime-sessions-and-cross-variant-save-replay-matrix/**

src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.AssetPipeline/**
src/LLMGameCreator.Scripting/**
generator-library/**
provider/**
LLM/**
RAG/**
unity/**

*.sln
*.csproj
Directory.Build.*
dependency/package files
```

This is a WinForms selection-lifecycle hotfix. Do not change Runtime, candidate discovery, matrix semantics, candidate packages, Unity, public schema or dependencies.

## Validation

Run sequentially:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln -c Debug --no-restore
```

Required: 0 warnings, 0 errors.

Focused tests:

```powershell
dotnet test .\LLMGameCreator.sln -c Debug --no-build --filter "FullyQualifiedName~Goal145CandidateSelector|FullyQualifiedName~Goal145|FullyQualifiedName~ProductLineInteractiveSessionMatrix|FullyQualifiedName~VisualWorldStreamPreviewWorkspace"
```

Rerun existing Goal145 command:

```powershell
.\.devflow\scripts\run-product-line-interactive-session-matrix.ps1 -DryRun
.\.devflow\scripts\run-product-line-interactive-session-matrix.ps1 -ApplyCleanup
```

Goal145 artifacts must be byte-identical or restored to `HEAD` by exact paths before staging. Goal145A must not commit modifications under the Goal145 root.

Required guards:

```powershell
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-145a-winforms-candidate-selector-reentrancy-and-selection-stability-hotfix
.\.devflow\scripts\clean-unity-editor-noise.ps1 -DryRun

git diff --check
git diff --cached --check
git status --short --untracked-files=all
git diff --name-only
git diff --cached --name-only
git ls-files .llmgc/manual
```

Check changed text files for mojibake and escaped Cyrillic: zero matches.

Forbidden-zone diff must be empty.

Validation-generated churn outside Goal145A allowlist must be restored only by exact paths computed from the Goal145A scenario policy. Do not use broad restore, `git reset --hard`, `git clean`, branch switching, merge, rebase or cherry-pick.

## Publication

Before staging:

```text
selector behavioral tests GREEN
Goal145 matrix regression GREEN
Goal145 Unity smoke remains GREEN
Goal145 artifacts unchanged
Goal145 accepted=false
artifact scope clean
forbidden diff empty
```

Stage only explicit Goal145A allowlisted paths.

Commit:

```text
GREEN Goal 145A WinForms candidate selector reentrancy and selection stability hotfix
```

Push `origin main`.

Final report must include commit SHA, exact event model, callback counts, maximum callback depth, default selection, combat-selection persistence, session/checkpoint reset proof, Goal145 matrix/Unity regressions, test counts, scope, forbidden diff and clean `HEAD == origin/main`.

Do not report GREEN if candidate binding can still re-enter or if selection changes during programmatic refresh.
