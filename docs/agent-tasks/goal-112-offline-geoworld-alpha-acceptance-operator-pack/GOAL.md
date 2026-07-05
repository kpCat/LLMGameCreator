# Goal 112 — Offline Geoworld Alpha Manual Acceptance Operator Pack + RC Readiness Dashboard

Status target: GREEN/BLOCKED/FAILED commit to `origin/main`.

## Repository

- Repo: https://github.com/kpCat/LLMGameCreator
- Local working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Work on `main` only. Do not create branches unless the user explicitly instructs otherwise.

## Human purpose

Goal110 created the Unity/manual acceptance runner and Goal111 created the deterministic result-intake verifier. Current `main` is still honestly blocked at `BLOCKED_PENDING_MANUAL_RESULT` because no real human Goal110 manual result JSON exists yet.

This goal must not jump to the next feature track. It must turn the current acceptance gap into a usable operator flow:

`Goal110 checklist/result template + Goal111 decision -> operator pack -> WinForms RC readiness dashboard -> exact human run instructions -> deterministic evidence that no acceptance was faked`

The user should be able to open the Visual World Stream Preview Workspace and see exactly what remains to do, which result file path to use, whether the operator pack is ready, and why the Alpha Slice is still not accepted.

## Why this goal exists now

A new feature after Goal111 would be premature because the active human gate is still `offline_geoworld_alpha_manual_acceptance_verification`. But doing nothing leaves the user with a manual JSON/path workflow that is easy to botch.

Goal112 is therefore a large acceptance-operations goal: it hardens the handoff between the repo, WinForms, and the existing Unity runner without fabricating the human result and without starting live geodata/Runtime/provider/schema work.

## Important policy boundary

This goal is operator tooling and release-candidate readiness visibility only.

It must not:

- mark the alpha accepted;
- create or commit a real accepted human result;
- convert `BLOCKED_PENDING_MANUAL_RESULT` into acceptance unless a real pre-existing human result is already present and valid;
- start live geodata/provider/network work;
- modify Runtime/GamePackage schema/Lua/provider/generator-library;
- modify Unity files, Unity scenes, prefabs, project settings, package settings, or build settings.

If a valid real manual result already exists in one of the Goal111 deterministic candidate paths before the task starts, Goal112 may surface it as `manualResultAvailableForHumanReview=true`, but it still must keep `acceptedByCodex=false` and `humanAcceptanceStillRequired=true`.

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
9. Goal109 export/evidence:
   - `.llmgc/exports/goal-109-offline-geoworld-alpha-slice/`
   - `.llmgc/procedural/goal-109-offline-geoworld-alpha-slice-export-package/`
10. Goal110 package/evidence and Unity runner sources as read-only inputs:
   - `.llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/`
   - `.llmgc/procedural/goal-110-offline-geoworld-alpha-manual-acceptance-gate/`
   - `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/OfflineGeoworldGoal110/`
   - `unity/LLMGameCreatorAlpha/Assets/Editor/OfflineGeoworldAlphaAcceptanceRunnerWindow.cs`
   - `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResult.cs`
   - `unity/LLMGameCreatorAlpha/Assets/Scripts/OfflineGeoworldAlphaAcceptanceResultStore.cs`
11. Goal111 result-intake implementation/evidence:
   - `.llmgc/exports/goal-111-offline-geoworld-alpha-manual-result-intake/`
   - `.llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake/`
   - `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/`
12. Visual World Stream Preview Workspace implementation:
   - `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
   - `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`
13. Existing Goal110/Goal111 tests:
   - `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaSliceManualAcceptanceGate/`
   - `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualResultIntake/`
   - `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/`
   - `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaManualResultIntakeProductSmokeTests.cs`
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
- `unity/**` all files, including scripts and StreamingAssets
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- Unity scenes, prefabs, project settings, package settings, build settings
- network/provider/runtime geodata fetching
- any LFZ/Infection Free Zone source-code import, archive usage, copy, or task input

Also do not rewrite historical Goal101-111 artifacts except where a new Goal112 artifact-scope policy explicitly permits new Goal112 roots. Goal109/Goal110/Goal111 evidence is input, not history to be overwritten.

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
- `docs/agent-tasks/goal-112-offline-geoworld-alpha-acceptance-operator-pack/**`
- `docs/manual-acceptance/offline-geoworld-alpha-manual-acceptance-operator-pack.md`
- `.llmgc/procedural/goal-112-offline-geoworld-alpha-acceptance-operator-pack/**`
- `.llmgc/exports/goal-112-offline-geoworld-alpha-acceptance-operator-pack/**`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/**`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/**` only if tiny compatibility/read-only model additions are truly needed
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/**`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaAcceptanceOperatorPack/**`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualResultIntake/**` only if compatibility tests are needed
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaAcceptanceOperatorPackProductSmokeTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs`

Do not add dependencies. Do not change project files.

## Exact behavior to implement

### 1. Application service: acceptance operator pack builder

Add a focused service under:

`src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/`

Recommended names:

- `OfflineGeoworldAlphaAcceptanceOperatorPackService`
- `OfflineGeoworldAlphaAcceptanceOperatorPackModels`
- split partial/helper files as needed to keep files under the source-health limits.

The service must:

1. Resolve repository root safely.
2. Read Goal110 acceptance metadata from the real repository files:
   - checklist;
   - result template;
   - release-gate dashboard;
   - checksums;
   - file index;
   - manifest.
3. Read Goal111 result-intake evidence:
   - `offline-geoworld-alpha-manual-result-intake-decision.json`;
   - Goal111 export dashboard/readme/index if present.
4. Derive a single operator-readiness model with fields equivalent to:
   - `goalId = goal_112_offline_geoworld_alpha_acceptance_operator_pack`
   - `sourceGoalIds = [goal_110_offline_geoworld_alpha_manual_acceptance_gate, goal_111_offline_geoworld_alpha_manual_result_intake]`
   - `manualGate = offline_geoworld_alpha_manual_acceptance_verification`
   - `operatorStatus` enum-like string:
     - `OPERATOR_READY_PENDING_HUMAN_RUN`
     - `BLOCKED_GOAL110_PACKAGE_MISSING`
     - `BLOCKED_GOAL111_DECISION_MISSING`
     - `BLOCKED_GOAL111_INVALID`
     - `GREEN_MANUAL_RESULT_AVAILABLE_FOR_HUMAN_REVIEW`
   - `decisionStatusFromGoal111`
   - `preferredManualResultPath`
   - `candidateManualResultPaths`
   - `unityRunnerPath`
   - `unityResultStorePath`
   - `checklistStepCount`
   - `checklistHash`
   - `resultTemplateHash`
   - `manualResultPresent`
   - `manualResultAvailableForHumanReview`
   - `acceptedByCodex = false`
   - `humanAcceptanceStillRequired = true`
   - `notFinalReleaseOrRuntimeBuild = true`
   - `noRuntimeProviderOrNetworkChanges = true`
   - `noUnityFileChangesRequired = true`
   - `errors`
   - `warnings`
   - `nextHumanActions`
   - `doNotDoYet`

If Goal111 currently says `BLOCKED_PENDING_MANUAL_RESULT`, the expected Goal112 status should normally be `OPERATOR_READY_PENDING_HUMAN_RUN`, not failure. The result is operationally ready but acceptance is still human-pending.

### 2. Operator runbook generation

Generate a deterministic human-readable runbook that tells the user exactly what to do next, without pretending Codex can do it.

The runbook must include:

- the active manual gate name;
- the existing Unity project path relative to repo;
- the existing Goal110 Unity runner/script names as read-only references;
- the preferred result JSON path:
  `.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json`
- the alternate deterministic candidate paths already used by Goal111;
- how to treat `accepted=true`: only valid if all required steps pass and hashes match;
- how to treat `accepted=false`, failed, pending, skipped, malformed, duplicate, missing or unknown steps;
- clear statement that the user/human must run the checklist and decide the gate;
- clear statement that this is not final release packaging and not live geodata/provider/runtime work;
- explicit “do not start yet” list: live geodata, providers, Runtime consumer, public schema, Lua, generator-library, final art, atlas, scene/prefab/project settings, release packaging.

The runbook must not contain raw absolute Linux paths, `/mnt`, `/home/oai`, `sandbox:/`, or fake local paths. Windows repo path may appear only as the known user path `C:\Users\endim\LLMGameCreator\`.

### 3. Pending result template copy, not acceptance result

Create a clearly-labelled pending template artifact under Goal112 evidence, for example:

`.llmgc/procedural/goal-112-offline-geoworld-alpha-acceptance-operator-pack/offline-geoworld-alpha-pending-result-template-copy.json`

Rules:

- It must be labelled as template/sample/pending only.
- It must not be placed in `.llmgc/manual/...`.
- It must not claim to be a real human result.
- It must not set a final accepted state.
- If it includes `accepted`, it must be `false`.
- It must include notes/warnings that the human must fill or generate the real result separately.

### 4. Goal112 evidence and export artifacts

Write deterministic evidence to:

`.llmgc/procedural/goal-112-offline-geoworld-alpha-acceptance-operator-pack/`

Minimum files:

- `offline-geoworld-alpha-acceptance-operator-dashboard.json`
- `offline-geoworld-alpha-acceptance-operator-runbook.md`
- `offline-geoworld-alpha-acceptance-result-path-map.json`
- `offline-geoworld-alpha-acceptance-operator-preflight-report.md`
- `offline-geoworld-alpha-acceptance-notary-boundary.json`
- `offline-geoworld-alpha-acceptance-quality-gate-scan.json`
- `offline-geoworld-alpha-acceptance-negative-proof-no-result-no-acceptance.json`
- `offline-geoworld-alpha-pending-result-template-copy.json`
- `offline-geoworld-alpha-acceptance-operator-file-index.json`

Write compact export metadata to:

`.llmgc/exports/goal-112-offline-geoworld-alpha-acceptance-operator-pack/`

Minimum files:

- `offline-geoworld-alpha-acceptance-operator-dashboard.json`
- `offline-geoworld-alpha-acceptance-operator-readme.md`
- `offline-geoworld-alpha-acceptance-operator-file-index.json`

Also write or update:

`docs/manual-acceptance/offline-geoworld-alpha-manual-acceptance-operator-pack.md`

This docs runbook should be short, stable and user-facing. It can point to `.llmgc/procedural/...` for full machine evidence.

### 5. WinForms workspace integration

Integrate the operator pack into the existing Visual World Stream Preview Workspace.

The WinForms page must show a visible Goal112/operator-readiness group/section with at least:

- operator status;
- Goal111 decision status;
- manual result present yes/no;
- preferred manual result path;
- candidate result paths;
- checklist step count;
- checklist hash presence/match state;
- accepted by Codex false;
- human acceptance still required true;
- next human actions;
- do-not-start-yet list;
- evidence/export/runbook paths;
- top errors/warnings.

Do not redesign the entire page. Add/extend existing summary/status rendering in the same style as Goal110/Goal111. Keep UI simple and deterministic.

### 6. Tests

Add focused tests under:

`tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaAcceptanceOperatorPack/`

Required test cases:

1. Real current repository state with missing manual result produces `OPERATOR_READY_PENDING_HUMAN_RUN` and `acceptedByCodex=false`.
2. Missing Goal110 package produces `BLOCKED_GOAL110_PACKAGE_MISSING` or equivalent.
3. Missing Goal111 decision produces `BLOCKED_GOAL111_DECISION_MISSING` or equivalent.
4. Goal111 `FAILED_INVALID_RESULT` produces blocked operator status, not acceptance.
5. Goal111 `GREEN_ACCEPTABLE_CANDIDATE` produces `GREEN_MANUAL_RESULT_AVAILABLE_FOR_HUMAN_REVIEW`, but still `acceptedByCodex=false` and `humanAcceptanceStillRequired=true`.
6. Generated pending result template copy is not written to `.llmgc/manual/...`, is clearly labelled pending/template, and does not set accepted true.
7. Generated runbook contains the preferred result path and the active manual gate.
8. Generated runbook contains no `/mnt`, `/home/oai`, `sandbox:/`, or LFZ source/archive references.
9. Negative proof confirms no result means no acceptance.

Extend VisualWorld workspace tests to verify the new Goal112 section is present in report/service output.

Add product smoke test:

`tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaAcceptanceOperatorPackProductSmokeTests.cs`

Product smoke must validate real repository-relative Goal110/Goal111 artifacts when present, not purely synthetic fixtures.

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

- Goal112 is acceptance operator tooling and RC readiness visibility only.
- Goal112 does not mean Alpha accepted.
- Goal112 does not start live geodata/provider/network/runtime/schema/Lua/generator-library/final-art/final-release work.
- Manual acceptance remains a human gate.
- If no real manual result exists, current state must honestly remain pending.
- The recommended next human action remains running the Goal110 Unity checklist and placing the real result JSON in the preferred path.

### 8. Artifact-scope policy

Add a Goal112 scenario to `.devflow/artifact-scope/artifact-scope-policy.json` with allowed files/prefixes matching this goal.

Run the artifact-scope guard. If validation rewrites historical artifacts, restore them unless they are explicitly allowed Goal112 outputs.

### 9. Source health

Do not create large monolithic files.

Rules:

- Prefer files below 500 lines.
- New or changed files above 700 lines require a clear final-report justification.
- No file may exceed 1000 lines.
- Do not touch `AlphaRuntimeBootstrap.cs` or any Unity file.

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
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --filter "OfflineGeoworldAlphaAcceptanceOperatorPack|OfflineGeoworldAlphaManualResultIntake|VisualWorldStreamPreviewWorkspace|ProductSmoke"
```

Run existing project gates when present:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-current-goal.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-spine-fast.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-112-offline-geoworld-alpha-acceptance-operator-pack
```

If these scripts require different goal/scenario parameters, inspect nearby docs and use the correct Goal112 scenario value.

Also run:

```powershell
git diff --check
git diff --cached --check
```

Search changed files for common mojibake/encoding damage and escaped Cyrillic. Report exact result.

### 11. Stop conditions

Stop with BLOCKED commit/report if:

- any forbidden zone must be changed;
- any Unity file must be changed;
- `.sln`/`.csproj`/dependencies would be required;
- Goal110/Goal111 source artifacts are missing or inconsistent enough that deterministic operator tooling cannot be implemented;
- the existing repo is already broken before your changes and cannot be isolated;
- implementing this requires real geodata/network/provider/runtime/schema/Lua work;
- implementing this requires committing a fake accepted human result.

If blocked, still commit a BLOCKED report/artifact under the allowed Goal112 task/evidence/docs paths, without partial risky implementation.

### 12. Commit and push policy

You may use git for:

- status/diff/log checks;
- committing the finished goal;
- pushing to `origin/main`.

Do not rewrite history. Do not force push.

Final commit message must start with exactly one of:

- `GREEN Goal 112 offline geoworld alpha acceptance operator pack`
- `BLOCKED Goal 112 offline geoworld alpha acceptance operator pack`
- `FAILED Goal 112 offline geoworld alpha acceptance operator pack`

Push to `origin/main` after commit unless push fails. If push fails, report exact reason and leave local commit intact.

### 13. Final report requirements

Your final report must include:

- final status: GREEN/BLOCKED/FAILED;
- commit SHA;
- push status;
- changed files list;
- read-first summary;
- what the user can see in WinForms;
- confirmation that Unity files were not changed, or exact BLOCKED reason if they had to be;
- forbidden zones verification;
- accepted/final-release boundary verification;
- Goal111 decision status consumed;
- Goal112 operator status produced;
- preferred manual result path;
- evidence/export/runbook paths;
- validation commands and exact results;
- test counts or test names;
- artifact-scope result;
- source-health line counts for changed/new files near or above 500 lines;
- mojibake/escaped-Cyrillic scan result;
- any remaining P2/P3 debt.
