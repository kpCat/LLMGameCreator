# Goal 111 — Offline Geoworld Alpha Manual Result Intake + Acceptance Decision Bridge

Status target: GREEN/BLOCKED/FAILED commit to `origin/main`.

## Repository

- Repo: https://github.com/kpCat/LLMGameCreator
- Local working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Work on `main` only. Do not create branches unless the user explicitly instructs otherwise.

## Human purpose

Goal110 created an Alpha-only manual acceptance runner and release-gate dashboard over the real Goal109 offline geoworld export package, but the repository still lacks the deterministic bridge from a human Unity acceptance result into the WinForms/source-of-truth decision view.

This goal must add the next end-to-end layer:

`Unity/manual result JSON -> Application verifier -> WinForms decision dashboard -> deterministic .llmgc evidence -> current-state/queue/risk routing`

The user should be able to place a manual acceptance result JSON in a known location, open the Visual World Stream Preview Workspace in WinForms, and see whether the Alpha Slice is still pending, blocked, invalid, or acceptable for a separate human acceptance decision.

## Important policy boundary

This goal is a manual-result intake and decision bridge only.

It must not convert the project into a final release, must not mark the alpha slice accepted by Codex alone, and must not begin real geodata/provider/runtime work.

A valid human result may produce an `acceptableCandidate=true` style decision artifact, but the project-level/manual gate must remain clearly controlled by an explicit human acceptance step. Codex must not silently flip the whole product to final accepted/released.

## Read-first list

Before changing code, read these files and summarize the relevant facts in the final report:

1. `docs/CURRENT_GENERATOR_STATE.md`
2. `docs/CURRENT_GENERATOR_STATE.json`
3. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
4. `docs/CONTEXT_INDEX.md`
5. `docs/MILESTONE_GATES.md`
6. `docs/RELEASE_RISK_REGISTER.md`
7. `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
8. `.devflow/artifact-scope/artifact-scope-policy.json`
9. Goal109 package/evidence:
   - `.llmgc/exports/goal-109-offline-geoworld-alpha-slice/`
   - `.llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package/`
10. Goal110 package/evidence:
   - `.llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/`
   - `.llmgc/procedural/goal-110-offline-geoworld-alpha-manual-acceptance-gate/`
   - `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/`
11. Goal110 implementation:
   - `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/`
   - `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
   - `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
   - `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaAcceptanceRunnerWindow.cs`
   - `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs`
   - `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs`
12. Existing tests around Goal109/Goal110 and VisualWorld workspace:
   - `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceManualAcceptanceGate/`
   - `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
   - `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaSliceManualAcceptanceGateProductSmokeTests.cs`
   - `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`

## Hard forbidden zones

Do not change any of these unless you stop with BLOCKED and explain why it would be necessary:

- `src/LLMGameCreator.Runtime/**`
- `src/LLMGameCreator.Runtime.Abstractions/**`
- public GamePackage schema files/contracts
- providers / LLM / RAG / media provider code
- Lua / Scripting code
- `generator-library/**`
- `.sln`, `.csproj`, package/dependency files
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- Unity scenes, prefabs, project settings, package settings, build settings
- network/provider/runtime geodata fetching
- any LFZ/Infection Free Zone source-code import or copy

Also do not rewrite historical Goal101-110 artifacts except where a new Goal111 artifact-scope policy explicitly permits new Goal111 roots. Existing Goal109/Goal110 evidence is input, not history to be overwritten.

## Allowed files and folders

Allowed, subject to minimal and necessary edits:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/MILESTONE_GATES.md`
- `docs/RELEASE_RISK_REGISTER.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/agent-tasks/goal-111-offline-geoworld-alpha-manual-result-intake/**`
- `.llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake/**`
- `.llmgc/exports/goal-111-offline-geoworld-alpha-manual-result-intake/**`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/**`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaSliceManualAcceptanceGate/**` only if tiny compatibility additions are truly needed
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/**`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualResultIntake/**`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaManualResultIntakeProductSmokeTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`
- `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal111/**` metadata only, no scenes/prefabs/settings

Do not add dependencies. Do not change project files.

## Exact behavior to implement

### 1. Application service: manual result intake verifier

Add a focused service under:

`src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/`

Recommended names:

- `OfflineGeoworldAlphaManualResultIntakeService`
- `OfflineGeoworldAlphaManualResultIntakeModels`
- split partial/helper files as needed to keep files under the source-health limits.

The service must:

1. Resolve repository root safely.
2. Locate and read Goal110 acceptance metadata from:
   - `.llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/`
   - `.llmgc/procedural/goal-110-offline-geoworld-alpha-manual-acceptance-gate/`
   - `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/`
3. Read these Goal110 files when present:
   - `offline-geoworld-alpha-acceptance-checklist.json`
   - `offline-geoworld-alpha-acceptance-result-template.json`
   - `offline-geoworld-alpha-release-gate-dashboard.json`
   - `offline-geoworld-alpha-acceptance-checksums.json`
   - `offline-geoworld-alpha-acceptance-file-index.json`
   - `offline-geoworld-alpha-acceptance-manifest.json`
4. Locate optional actual manual result JSON from deterministic candidate paths, for example:
   - `.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json`
   - `.llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake/input/offline-geoworld-alpha-acceptance-result.json`
   - `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/offline-geoworld-alpha-acceptance-result.json`

If more than one actual result file exists and they differ, return an invalid/blocked decision rather than guessing.

### 2. Result validation rules

Validate the actual result JSON against the Goal110 checklist/template contract. Required checks:

- `goalId` must match Goal110 manual acceptance goal identity, or an explicitly documented accepted alias.
- `manualGate` must equal `offline_geoworld_alpha_manual_acceptance_verification`.
- `resultSchema` must equal the Goal110 result schema when present.
- `checklistHash` must match the SHA-256 of the actual Goal110 checklist, or the checksum listed in Goal110 checksums if that is the canonical source.
- all required checklist step ids must be present exactly once in the result.
- unknown extra steps must be warnings or invalid according to your model, but the choice must be deterministic and tested.
- each required step must have a status.
- supported statuses must be documented, at minimum: `passed`, `failed`, `pending`, `skipped`.
- `skipped` is not acceptable for a required step unless the checklist explicitly marks it optional; current Goal110 checklist steps are required.
- any `failed`, `pending`, missing, duplicate, or invalid required step blocks acceptance.
- `accepted=true` in the result is not enough by itself; all validation checks must pass.
- `accepted=false` with all steps passed must produce a deterministic non-accepted/pending decision.
- empty/missing result file must produce `BLOCKED_PENDING_MANUAL_RESULT`, not failure.
- malformed JSON must produce `FAILED_INVALID_RESULT` with diagnostics.

### 3. Decision model

Produce a compact decision model with fields equivalent to:

- `goalId = goal_111_offline_geoworld_alpha_manual_result_intake`
- `sourceGoalId = goal_110_offline_geoworld_alpha_manual_acceptance_gate`
- `manualGate = offline_geoworld_alpha_manual_acceptance_verification`
- `decisionStatus` enum-like string:
  - `BLOCKED_PENDING_MANUAL_RESULT`
  - `FAILED_INVALID_RESULT`
  - `BLOCKED_INCOMPLETE_RESULT`
  - `GREEN_ACCEPTABLE_CANDIDATE`
  - `BLOCKED_ACCEPTED_FALSE`
- `acceptedByCodex = false`
- `humanAcceptanceStillRequired = true`
- `acceptableCandidate = true/false`
- `resultFilePath`
- `checklistHashExpected`
- `checklistHashActual`
- `stepSummary`
- `errors`
- `warnings`
- `inputPackageLineage`
- `notFinalReleaseOrRuntimeBuild = true`
- `noRuntimeProviderOrNetworkChanges = true`

Important: even with `GREEN_ACCEPTABLE_CANDIDATE`, do not claim the alpha is fully/finally accepted. Phrase it as “valid manual result available for human gate decision”.

### 4. Goal111 evidence writer

Write deterministic evidence to:

`.llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake/`

Minimum files:

- `offline-geoworld-alpha-manual-result-intake-decision.json`
- `offline-geoworld-alpha-manual-result-intake-report.md`
- `offline-geoworld-alpha-manual-result-intake-file-index.json`
- `offline-geoworld-alpha-manual-result-intake-quality-gate-scan.json`
- `offline-geoworld-alpha-manual-result-intake-negative-proof-missing-result.json`
- `offline-geoworld-alpha-manual-result-intake-negative-proof-invalid-result.json`
- `offline-geoworld-alpha-manual-result-intake-valid-sample-result.json` only if clearly labelled as sample/test fixture and not as real human acceptance.

Also write export metadata to:

`.llmgc/exports/goal-111-offline-geoworld-alpha-manual-result-intake/`

Minimum files:

- `offline-geoworld-alpha-manual-result-intake-dashboard.json`
- `offline-geoworld-alpha-manual-result-intake-readme.md`
- `offline-geoworld-alpha-manual-result-intake-file-index.json`

Optional metadata-only Unity StreamingAssets handoff:

`unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal111/`

Do this only if useful for discoverability. Do not touch Unity scenes/prefabs/settings.

### 5. WinForms workspace integration

Integrate the decision model into the existing Visual World Stream Preview Workspace.

The WinForms page must show a visible manual result intake group/section with at least:

- Goal110 package presence
- result file presence
- decision status
- acceptable candidate yes/no
- accepted by Codex false
- human acceptance still required yes/no
- checklist hash match/mismatch
- required step counts: passed/failed/pending/skipped/missing/duplicate
- top errors/warnings
- report/export artifact paths

Do not redesign the entire page. Add/extend existing summary/status rendering in the same style as Goal110. Keep UI simple and deterministic.

### 6. Tests

Add focused tests under:

`tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualResultIntake/`

Required test cases:

1. Missing actual result -> `BLOCKED_PENDING_MANUAL_RESULT`.
2. Malformed JSON -> `FAILED_INVALID_RESULT`.
3. Wrong checklist hash -> invalid/failed.
4. Missing required step -> blocked/invalid.
5. Duplicate required step -> blocked/invalid.
6. One failed required step -> `BLOCKED_INCOMPLETE_RESULT` or equivalent.
7. All steps passed but `accepted=false` -> `BLOCKED_ACCEPTED_FALSE` or equivalent.
8. All steps passed, correct hash, `accepted=true` -> `GREEN_ACCEPTABLE_CANDIDATE`, but `acceptedByCodex=false` and `humanAcceptanceStillRequired=true`.
9. Multiple differing result files -> blocked/invalid, not random winner.

Extend VisualWorld workspace tests to verify the new section is present in report/service output.

Add product smoke test:

`tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaManualResultIntakeProductSmokeTests.cs`

Product smoke must validate real repository-relative Goal109/Goal110 artifacts when present, not purely synthetic fixtures.

### 7. Documentation/state updates

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/MILESTONE_GATES.md`
- `docs/RELEASE_RISK_REGISTER.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`

Required wording:

- Goal111 is manual result intake/decision bridge only.
- Goal111 does not mean final release.
- Goal111 does not start live geodata/provider/network/runtime/schema/final-art work.
- Manual acceptance remains a human gate.
- If no real manual result exists, current state must honestly remain pending/blocked.

### 8. Artifact-scope policy

Add a Goal111 scenario to `.devflow/artifact-scope/artifact-scope-policy.json` with allowed files/prefixes matching this goal.

Run the artifact-scope guard. If validation rewrites historical artifacts, restore them unless they are explicitly allowed Goal111 outputs.

### 9. Source health

Do not create large monolithic files.

Rules:

- Prefer files below 500 lines.
- New or changed files above 700 lines require a clear final-report justification.
- No file may exceed 1000 lines.
- Do not make `AlphaRuntimeBootstrap.cs` worse; do not touch it.

### 10. Validation commands

Run the strongest practical validation set available in the repository. At minimum:

```powershell
cd C:\Users\endim\LLMGameCreator\
git status --short
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug
```

Run focused tests. Use the actual test project path present in the repo. Example:

```powershell
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "OfflineGeoworldAlphaManualResultIntake|VisualWorldStreamPreviewWorkspace|ProductSmoke"
```

Run existing project gates when present:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-current-goal.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-spine-fast.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-artifact-scope.ps1
```

If these scripts require goal/scenario parameters, inspect nearby docs and use the correct Goal111/scenario value.

Also run:

```powershell
git diff --check
git diff --cached --check
```

Search changed files for common mojibake/encoding damage and escaped Cyrillic. Report exact result.

### 11. Stop conditions

Stop with BLOCKED commit/report if:

- any forbidden zone must be changed;
- `.sln`/`.csproj`/dependencies would be required;
- Goal109/Goal110 source artifacts are missing or inconsistent enough that deterministic validation cannot be implemented;
- the existing repo is already broken before your changes and cannot be isolated;
- Unity scenes/prefabs/settings would need changes;
- implementing this requires real geodata/network/provider/runtime work.

If blocked, still commit a BLOCKED report/artifact under the allowed Goal111 task/evidence/docs paths, without partial risky implementation.

### 12. Commit and push policy

You may use git for:

- status/diff/log checks;
- committing the finished goal;
- pushing to `origin/main`.

Do not rewrite history. Do not force push.

Final commit message must start with exactly one of:

- `GREEN Goal 111 offline geoworld alpha manual result intake`
- `BLOCKED Goal 111 offline geoworld alpha manual result intake`
- `FAILED Goal 111 offline geoworld alpha manual result intake`

Push to `origin/main` after commit unless push fails. If push fails, report exact reason and leave local commit intact.

### 13. Final report requirements

Your final report must include:

- final status: GREEN/BLOCKED/FAILED;
- commit SHA;
- push status;
- changed files list;
- read-first summary;
- what the user can see in WinForms;
- whether Unity files were touched and exactly which ones;
- forbidden zones verification;
- accepted/final-release boundary verification;
- manual result decision status;
- evidence/export paths;
- validation commands and exact results;
- test counts or test names;
- artifact-scope result;
- source-health line counts for changed/new files near or above 500 lines;
- mojibake/escaped-Cyrillic scan result;
- any remaining P2/P3 debt.
