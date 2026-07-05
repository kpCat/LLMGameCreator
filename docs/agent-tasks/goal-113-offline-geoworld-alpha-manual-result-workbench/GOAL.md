# Goal 113 — Offline Geoworld Alpha Manual Result Workbench

## Task id

goal-113-offline-geoworld-alpha-manual-result-workbench

## Repo / working copy / branch

- Repo: https://github.com/kpCat/LLMGameCreator
- Local working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`

## Required outcome

Build a deterministic **manual result workbench** for the open offline geoworld Alpha manual gate.

Goal110 produced the Unity manual acceptance runner/result template.
Goal111 produced the deterministic result intake verifier and currently reports `BLOCKED_PENDING_MANUAL_RESULT`.
Goal112 produced the operator pack and currently reports `OPERATOR_READY_PENDING_HUMAN_RUN`.

Goal113 must make the remaining human action practical inside the existing Application/WinForms review flow, without fabricating a manual result and without accepting the gate by Codex.

The result should be visible in the existing **Visual World Stream Preview Workspace** as a new Goal113/manual-result-workbench section that shows:

- the Goal110 checklist and required step list;
- the preferred real manual result path;
- the deterministic candidate paths from Goal111/Goal112;
- whether a real manual result currently exists;
- a generated **draft/template** path that is safe to copy from but is not itself acceptance;
- validation status for any real result if present;
- explicit `acceptedByCodex=false` and `humanAcceptanceStillRequired=true`;
- next human actions and do-not-start-yet list.

If no real manual result exists, the correct Goal113 product status is a GREEN implementation with a pending workbench state, for example:

```text
workbenchStatus=WORKBENCH_READY_PENDING_HUMAN_RESULT
manualResultPresent=false
acceptedByCodex=false
humanAcceptanceStillRequired=true
```

Do **not** mark the Alpha accepted. Do **not** create or commit the real manual result at `.llmgc/manual/...`.

## Human value / why this exists

Goal112 tells the operator what to do, but the last step is still easy to mess up: the human has to understand the checklist, produce a result JSON, put it in the correct place, and then let Goal111 intake decide whether it is even a valid candidate.

Goal113 should provide a clear, deterministic authoring/review workbench so that the user can:

1. open WinForms;
2. see the 12 required acceptance steps;
3. see exactly what JSON structure is expected;
4. use a generated draft/template as a starting point;
5. place a real manually-created result later;
6. see whether the real result is missing, malformed, incomplete, hash-mismatched, or a valid candidate;
7. still keep the final gate decision human-owned.

## Why now

Current `docs/CURRENT_GENERATOR_STATE.json` says the active gate remains `offline_geoworld_alpha_manual_acceptance_verification`, with Goal112 operator tooling produced but no real manual result JSON. Starting live geodata, Runtime consumers, schema work, providers, Lua, generator-library, final art or release packaging now would violate the current handoff.

Goal113 stays inside the acceptance/manual-result lane and makes the pending gate executable instead of adding another unrelated proof layer.

## What this task explicitly does NOT do

- Does not accept the offline geoworld Alpha gate.
- Does not set any accepted flag to true.
- Does not create, modify, or commit `.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json`.
- Does not create fake human evidence.
- Does not edit Unity files.
- Does not change Unity scenes, prefabs, project settings, packages, build settings, binary assets, or StreamingAssets payloads.
- Does not change Runtime or Runtime.Abstractions.
- Does not change public GamePackage schema.
- Does not change providers, LLM, RAG, media-provider code, network code or geodata fetching.
- Does not change Lua/Scripting.
- Does not change generator-library.
- Does not change `.sln`, `.csproj`, dependencies, NuGet packages or project files.
- Does not read, use, copy, import, or reference LFZ source/archive contents.
- Does not add live geodata ingestion, scraping, tile downloads, public map bulk download, OCR, provider calls or runtime online behavior.

## Read first

Read these files before changing code:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/MILESTONE_GATES.md`
- `docs/RELEASE_RISK_REGISTER.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `.llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-checklist.json`
- `.llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result-template.json`
- `.llmgc/exports/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-release-gate-dashboard.json`
- `.llmgc/procedural/goal-111-offline-geoworld-alpha-manual-result-intake/offline-geoworld-alpha-manual-result-intake-decision.json`
- `.llmgc/procedural/goal-112-offline-geoworld-alpha-acceptance-operator-pack/offline-geoworld-alpha-acceptance-operator-dashboard.json`
- `.llmgc/procedural/goal-112-offline-geoworld-alpha-acceptance-operator-pack/offline-geoworld-alpha-acceptance-result-path-map.json`
- `.llmgc/procedural/goal-112-offline-geoworld-alpha-acceptance-operator-pack/offline-geoworld-alpha-acceptance-operator-runbook.md`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultIntake/`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaAcceptanceOperatorPack/`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/`

Use the real Goal110/Goal111/Goal112 artifacts. Do not synthesize their presence when files are missing.

## Allowed files / folders

You may add or modify only these paths unless a compile error forces a tiny directly-related fix inside the same bounded area:

- `.devflow/artifact-scope/artifact-scope-policy.json`
- `.llmgc/procedural/goal-113-offline-geoworld-alpha-manual-result-workbench/**`
- `.llmgc/exports/goal-113-offline-geoworld-alpha-manual-result-workbench/**`
- `docs/agent-tasks/goal-113-offline-geoworld-alpha-manual-result-workbench/**`
- `docs/manual-acceptance/offline-geoworld-alpha-manual-result-workbench.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/MILESTONE_GATES.md`
- `docs/RELEASE_RISK_REGISTER.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/**`
- `src/LLMGameCreator.Application/Design/VisualWorldStreamPreviewWorkspace/**`
- `src/LLMGameCreator.WinForms/Pages/VisualWorldStreamPreviewWorkspace/**`
- `tests/LLMGameCreator.Tests/Application/OfflineGeoworldAlphaManualResultWorkbench/**`
- `tests/LLMGameCreator.Tests/Application/VisualWorldStreamPreviewWorkspace/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/OfflineGeoworldAlphaManualResultWorkbenchProductSmokeTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualWorldStreamPreviewWorkspaceProductSmokeTests.cs` only if absolutely necessary for Goal113 binding coverage.

Prefer adding new files over growing already-large files. If an existing file is already near/over 700 lines, avoid modifying it unless there is no practical alternative, and keep the delta tiny.

## Forbidden files / folders

Do not change:

- `.llmgc/manual/**`
- `src/LLMGameCreator.Runtime/**`
- `src/LLMGameCreator.Runtime.Abstractions/**`
- public GamePackage schema/model files
- provider / LLM / RAG / media-provider code
- Lua / Scripting code
- `generator-library/**`
- `.sln`, `.csproj`, package/dependency files
- `unity/**` files, including scenes, prefabs, project settings, packages, build settings, StreamingAssets and scripts
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- LFZ archive/source files or any copied LFZ code
- binary media/assets

If a validation command rewrites historical `.llmgc` artifacts outside the allowed Goal113 paths, restore those exact paths and record the cleanup in the final report. Do not commit validation churn.

## Exact behavior

### 1. Application workbench service

Create a bounded BCL-only Application seam, for example:

```text
src/LLMGameCreator.Application/Design/OfflineGeoworldAlphaManualResultWorkbench/
```

Expected responsibilities:

- Load real Goal110 checklist/result-template/dashboard/checksum metadata.
- Load real Goal111 decision artifact.
- Load real Goal112 operator dashboard/path-map/runbook artifact.
- Resolve the preferred real result path:
  `.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json`
- Resolve candidate paths from Goal111/Goal112.
- Detect whether a real result exists in candidate paths.
- Validate a real result if present using the same safety meaning as Goal111:
  - checklist hash must match;
  - every required step must be present exactly once;
  - invalid/missing/duplicate/unknown statuses must be rejected;
  - failed/pending steps must not be accepted;
  - no `acceptedByCodex=true` may be inferred;
  - a valid result may only become a human-review candidate, not accepted by Codex.
- Generate a deterministic **draft/template copy** under the Goal113 procedural folder only. This draft is for human editing/copying later and must not be placed in `.llmgc/manual/**` by the task.
- Generate an authoring checklist/runbook that explains which fields need real human evidence.

Suggested status values:

```text
WORKBENCH_READY_PENDING_HUMAN_RESULT
WORKBENCH_RESULT_INVALID
WORKBENCH_RESULT_READY_FOR_HUMAN_REVIEW
WORKBENCH_BLOCKED_MISSING_GOAL110
WORKBENCH_BLOCKED_MISSING_GOAL111
WORKBENCH_BLOCKED_MISSING_GOAL112
```

If there is no real manual result, the task should still be GREEN if the workbench and evidence are implemented correctly.

### 2. Evidence artifacts

Write deterministic artifacts under:

```text
.llmgc/procedural/goal-113-offline-geoworld-alpha-manual-result-workbench/
.llmgc/exports/goal-113-offline-geoworld-alpha-manual-result-workbench/
```

Required procedural artifacts:

- `offline-geoworld-alpha-manual-result-workbench-dashboard.json`
- `offline-geoworld-alpha-manual-result-workbench-file-index.json`
- `offline-geoworld-alpha-manual-result-workbench-report.md`
- `offline-geoworld-alpha-manual-result-workbench-runbook.md`
- `offline-geoworld-alpha-manual-result-workbench-draft-template.json`
- `offline-geoworld-alpha-manual-result-workbench-field-map.json`
- `offline-geoworld-alpha-manual-result-workbench-quality-gate-scan.json`
- `offline-geoworld-alpha-manual-result-workbench-negative-proof-no-result-no-acceptance.json`
- `offline-geoworld-alpha-manual-result-workbench-negative-proof-invalid-result.json`

Required export artifacts:

- `offline-geoworld-alpha-manual-result-workbench-dashboard.json`
- `offline-geoworld-alpha-manual-result-workbench-file-index.json`
- `offline-geoworld-alpha-manual-result-workbench-readme.md`

Evidence must explicitly include:

- `acceptedByCodex=false`
- `humanAcceptanceStillRequired=true`
- `manualGate=offline_geoworld_alpha_manual_acceptance_verification`
- `preferredManualResultPath=.llmgc/manual/goal-110-offline-geoworld-alpha-acceptance/offline-geoworld-alpha-acceptance-result.json`
- `doesNotWritePreferredManualResultPath=true`
- `draftTemplateOnly=true`
- `notFinalReleaseOrRuntimeBuild=true`
- `noRuntimeProviderOrNetworkChanges=true`
- `noUnityFileChangesRequired=true`

### 3. WinForms / Visual World Stream Preview Workspace integration

Extend the existing Visual World Stream Preview Workspace to surface a Goal113 group, for example:

```text
offline_geoworld_alpha_manual_result_workbench
```

Show at least:

- workbench status;
- Goal111 decision status;
- Goal112 operator status;
- manual result presence;
- preferred manual result path;
- draft template path;
- checklist hash and step count;
- required step ids/titles summary;
- validation errors/warnings;
- next human actions;
- do-not-start-yet list;
- procedural/export/runbook paths.

Do not add a broad new UI framework. Keep the pattern consistent with existing partial page files.

### 4. Manual acceptance docs

Add:

```text
docs/manual-acceptance/offline-geoworld-alpha-manual-result-workbench.md
```

It should be short and practical:

- how to open the Unity runner;
- where the preferred result JSON must go;
- how to use the Goal113 draft template safely;
- how to re-run Goal111/Goal112/Goal113 validation after placing a real result;
- what must not be done before human acceptance.

### 5. Current state / queue / debt updates

Update docs to state:

- Goal113 produced a workbench for manual result authoring/review.
- The active manual gate remains open unless a real valid result already exists.
- If no result exists, the expected state is `WORKBENCH_READY_PENDING_HUMAN_RESULT`.
- Do not start live geodata/provider/Runtime/schema/Lua/generator-library/final-art/release work from this handoff.

Add/adjust debt as P2/P3 only. Do not mark a P0/P1 blocker unless there is a real compile/test/source-health defect.

## Tests

Add focused tests for the new Application service:

- missing real result -> workbench ready/pending, no acceptance;
- draft template is written only under Goal113 procedural/export paths, never `.llmgc/manual/**`;
- malformed result -> invalid, not accepted;
- checklist hash mismatch -> invalid, not accepted;
- duplicate/missing/unknown step -> invalid, not accepted;
- all required steps present with passing statuses in a temp candidate path -> `WORKBENCH_RESULT_READY_FOR_HUMAN_REVIEW`, still `acceptedByCodex=false` and `humanAcceptanceStillRequired=true`;
- source lineage detects missing Goal110/111/112 artifacts as blocked statuses.

Add product smoke:

- builds the workbench from repository root;
- writes Goal113 evidence/export artifacts;
- proves no real manual result was created;
- proves no Unity files are required/changed;
- proves WinForms workspace contains the Goal113 group/binding.

Add VisualWorld workspace test coverage via a new small Goal113 test file if possible. Avoid growing existing >700-line tests.

## Validation commands

Run these from repo root:

```powershell
dotnet restore
dotnet build .\LLMGameCreator.sln -c Debug

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~OfflineGeoworldAlphaManualResultWorkbench|FullyQualifiedName~VisualWorldStreamPreviewWorkspaceGoal113|FullyQualifiedName~OfflineGeoworldAlphaManualResultWorkbenchProductSmoke"

.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\artifact-scope\check-artifact-scope.ps1 -Scenario goal-113-offline-geoworld-alpha-manual-result-workbench

git diff --check
git diff --cached --check
```

`check-spine-fast.ps1` may print known historical product-smoke noise. The wrapper exit code is authoritative unless a new Goal113 failure is visible.

Also check changed files for mojibake / escaped Cyrillic regressions using the existing project pattern or a simple local scan. Report the result.

## Quality gate

Before commit, verify:

- no `.llmgc/manual/**` file is staged;
- no Unity file is staged;
- no Runtime/Runtime.Abstractions file is staged;
- no GamePackage schema file is staged;
- no provider/LLM/RAG/media/Lua/generator-library/project/dependency file is staged;
- no Goal101-112 historical evidence/export artifact was modified, except allowed docs/current-state/artifact-scope updates;
- all new/changed C# files are below 700 logical lines; prefer below 500 for new Goal113 files;
- no file exceeds 1000 logical lines because of this task;
- accepted flags remain false unless the file is a synthetic/temp test fixture and clearly named as such;
- `acceptedByCodex=false` appears in Goal113 evidence;
- `humanAcceptanceStillRequired=true` appears in Goal113 evidence;
- missing real result does not cause acceptance.

## Stop conditions

Stop and commit `BLOCKED` rather than improvising if:

- implementing this requires Runtime/schema/provider/Lua/generator-library/project/dependency changes;
- implementing this requires Unity file changes;
- existing Goal110/111/112 artifacts are missing or inconsistent and cannot be honestly consumed;
- the task would need to create or commit a real `.llmgc/manual/**` result;
- validation reveals a P0/P1 source-health issue caused by Goal113 that cannot be fixed within allowed paths;
- artifact-scope cannot be made clean without widening into forbidden zones.

Stop and commit `FAILED` only if the repository cannot build/test because of a Goal113 bug and a bounded repair fails.

## Commit / push policy

Use git only for:

- status/diff inspection;
- exact-path restore of validation side-effect churn;
- staging Goal113 allowed files;
- commit;
- push;
- final log/rev-parse verification.

Commit exactly one final status commit to `main` and push to `origin/main`.

Commit message format:

```text
GREEN Goal 113 offline geoworld alpha manual result workbench
```

or:

```text
BLOCKED Goal 113 offline geoworld alpha manual result workbench
```

or:

```text
FAILED Goal 113 offline geoworld alpha manual result workbench
```

## Final report

Final report must include:

- status: GREEN/BLOCKED/FAILED;
- commit SHA;
- push result;
- changed file groups;
- Goal113 workbench status;
- manual result present yes/no;
- preferred manual result path;
- whether any `.llmgc/manual/**` file was staged or committed;
- acceptedByCodex and humanAcceptanceStillRequired;
- WinForms visibility summary;
- validation results with counts;
- artifact-scope result;
- forbidden-zone confirmation;
- source-health summary;
- remaining debt;
- exact next human action.
