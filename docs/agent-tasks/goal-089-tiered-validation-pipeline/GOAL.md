# Goal 089 — Tiered Validation Pipeline & Check-All Runtime Control

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Add a tiered validation pipeline so future Codex goals do not depend on manually or blindly waiting for the full historical `check-all.ps1` route every time.

This goal must not weaken `check-all.ps1`. It must preserve full historical validation as the highest tier, while adding practical lower tiers for current-goal validation, fast spine regression and observed full-run monitoring.

## Why this goal exists

Goal 088A proved that Goal 088 was not hung and full `check-all.ps1` passed, but it took about 18 minutes in the non-product test leg and roughly 1110 seconds wall clock. That is too expensive and brittle as a mandatory per-goal route.

The goal is to make validation usable:

- fast current-goal gate for every feature goal;
- medium spine-fast gate for recent/high-value regression coverage;
- full check-all route for consolidation, milestone and shared/core-risk work;
- clear heartbeat/logging so long full runs are not mistaken for hangs;
- no manual user-run requirement.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes:
   - `114a3280f BLOCKED Goal 088 deterministic visual region composer`
   - `9bfdff86e GREEN Goal 088A check-all hang triage validation repair`
4. Read Goal 088A evidence and confirm:
   - full check-all passed;
   - 1235 non-product tests passed;
   - wrapper wall-clock was recorded;
   - root cause was classified as long-running historical suite / timeout budget, not Goal 088 code hang.
5. Inspect current dirty state before edits. Do not stage/revert unrelated user work.
6. Confirm this can be implemented without product code changes.

## Read first

- `AGENTS.md`
- `.devflow/scripts/check-all.ps1`
- `.devflow/scripts/check-artifact-scope.ps1`
- `.devflow/scripts/check-devflow-state.ps1` if present
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-088a-check-all-hang-triage-validation-repair/check-all-hang-triage-report.md`
- `.llmgc/procedural/goal-088a-check-all-hang-triage-validation-repair/check-all-run-summary.json`
- `.llmgc/procedural/goal-088a-check-all-hang-triage-validation-repair/test-isolation-matrix.json`
- `.llmgc/procedural/goal-088a-check-all-hang-triage-validation-repair/quality-gate-scan.json`
- Current latest visual stack docs:
  - `docs/context/DEEPSEARCH_VISUAL_STACK_SYNTHESIS.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`

## Allowed files / areas

You may change only:

- `.devflow/scripts/check-current-goal.ps1`
- `.devflow/scripts/check-spine-fast.ps1`
- `.devflow/scripts/check-all-observed.ps1`
- `.devflow/scripts/check-all.ps1` only if adding non-breaking optional parameters/heartbeat without weakening existing default behavior
- `.devflow/validation-profiles/`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/VALIDATION_PIPELINE.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-089-tiered-validation-pipeline/`
- `docs/agent-tasks/goal-089-tiered-validation-pipeline/`

## Forbidden files / areas

Do not change:

- Application product code.
- WinForms product code.
- Runtime / Runtime.Abstractions.
- Unity files.
- Public GamePackage schema.
- Infrastructure provider / LLM / RAG / media provider code.
- Lua / Scripting.
- generator-library.
- `.sln`
- `.csproj`
- package lock files.
- binary media assets.
- generated visual artifacts unrelated to Goal 089 evidence.

Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Preserve full check-all

Do not remove tests from `check-all.ps1`.
Do not make `check-all.ps1` weaker by default.
Do not silently skip slow/historical tests from full mode.

If `check-all.ps1` is edited, default behavior must remain equivalent to the current full route unless the caller explicitly opts into a new mode/timeout/heartbeat behavior.

### 2. Add current-goal validation script

Add `.devflow/scripts/check-current-goal.ps1`.

It should support parameters such as:

- `-Scenario`
- `-FocusedFilter`
- `-ProductSmokeFilter`
- `-SkipRestore`
- `-SkipBuild`
- `-SkipArtifactScope`
- `-TimeoutMinutes`
- `-HeartbeatSeconds`
- `-DryRun`

Expected default current-goal route:

1. restore unless skipped;
2. build unless skipped;
3. run focused test filter when provided;
4. run product smoke filter when provided;
5. run CurrentState tests;
6. run artifact scope for the scenario when provided;
7. run `git diff --check`;
8. write a deterministic summary JSON under `.devflow/runs/<run-id>-check-current-goal/`.

The script must print heartbeat/progress lines for long steps.

### 3. Add spine-fast validation script

Add `.devflow/scripts/check-spine-fast.ps1`.

It should validate recent high-value spine coverage without full historical slow suites.

Minimum coverage:
- restore/build;
- CurrentState;
- latest visual stack focused filters:
  - `VisualAssetContractRatingMetadata`
  - `VisualPartPackRuleStack`
  - `DeterministicVisualMicrotileMaterializer`
  - `DeterministicVisualMapPatchComposer`
  - `DeterministicVisualRegionComposer`
- corresponding product smokes when practical;
- source-format / artifact-scope / docs state checks if those are already available.

It should write a deterministic summary JSON under `.devflow/runs/<run-id>-check-spine-fast/`.

### 4. Add observed full check-all wrapper

Add `.devflow/scripts/check-all-observed.ps1`.

This script should run the existing full `check-all.ps1` with:
- a configurable timeout budget;
- heartbeat/progress output;
- process cleanup diagnostics;
- summary of run directory, elapsed time, test count, warnings and exit status;
- no raw TRX/log copy into tracked evidence by default.

The goal is not to make full check-all faster; the goal is to make it observable and not mistaken for a silent hang.

### 5. Add validation profiles

Add `.devflow/validation-profiles/validation-tiers.json`.

It must define at least:

- `current-goal`
- `spine-fast`
- `full`
- `full-observed`

For each tier include:
- intended use;
- required for which future task types;
- expected commands;
- whether it is mandatory for every Codex goal;
- whether it may replace full check-all;
- approximate runtime guidance from Goal 088A evidence.

### 6. Update docs and future task policy

Create `docs/VALIDATION_PIPELINE.md`.

It must state:

- `check-current-goal.ps1` is the default required validation route for ordinary feature goals;
- `check-spine-fast.ps1` is recommended after visual/world/gameplay spine changes;
- full `check-all.ps1` / `check-all-observed.ps1` is required for consolidation/milestone/shared/core-risk changes, not every small feature;
- full check-all remains authoritative when run;
- user should not be asked to manually run check-all;
- future Codex task packs should request tiered validation based on scope.

Update `CURRENT_GENERATOR_STATE.*`, `CONTEXT_INDEX.md`, `FULL_GENERATOR_GOAL_QUEUE.md`, debt register.

Manual gate:
`tiered_validation_pipeline_verification required`

Goal 089 must be `accepted=false`.

### 7. Generate evidence

Create `.llmgc/procedural/goal-089-tiered-validation-pipeline/`.

Recommended artifacts:
- `tiered-validation-pipeline-report.md`
- `validation-tier-matrix.json`
- `script-inventory.json`
- `observed-check-all-baseline.json`
- `current-goal-dry-run-summary.json`
- `spine-fast-dry-run-summary.json`
- `quality-gate-scan.json`

Evidence must prove:
- full check-all was not weakened;
- scripts exist;
- dry-run or lightweight sanity route works;
- validation tiers are documented;
- full route remains available;
- future policy no longer requires manual user-run of full check-all for every goal.

## Quality gate

GREEN only if:

- no product code changed;
- no Runtime/Unity/schema/provider/Lua/generator-library/project files changed;
- `check-all.ps1` default behavior is not weakened;
- new scripts exist and have basic self-check/dry-run evidence;
- validation tiers are documented;
- current-state docs route future validation correctly;
- no binary/heavy run logs are committed;
- evidence is deterministic;
- build and CurrentState pass;
- artifact scope passes.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-current-goal.ps1 -Scenario "goal-089-tiered-validation-pipeline" -FocusedFilter "CurrentState" -ProductSmokeFilter "" -DryRun
.\.devflow\scripts\check-spine-fast.ps1 -DryRun
.\.devflow\scripts\check-all-observed.ps1 -DryRun
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-089-tiered-validation-pipeline"
git diff --check
git diff --cached --check
```

Also run full check-all when feasible with an observed timeout budget:

```powershell
.\.devflow\scripts\check-all-observed.ps1 -TimeoutMinutes 45 -HeartbeatSeconds 60
```

If the full observed run is skipped due to wall-clock budget, do not claim it passed. Record it honestly and rely on Goal 088A baseline plus dry-run proof. GREEN is allowed if full check-all default behavior was not modified and core validation passes; BLOCKED if script behavior cannot be proven.

## Stop / block conditions

Return BLOCKED if:
- implementing the tiered pipeline requires product code changes;
- full check-all default behavior cannot be preserved;
- scripts cannot be made deterministic;
- artifact scope cannot be satisfied;
- validation state cannot be represented without broad docs churn.

Return FAILED if:
- build/current-state/artifact-scope regress due to this goal and cannot be fixed inside allowed files.

## Final report format

Report:
- Final status.
- Latest commit before/after.
- Push status.
- Files changed.
- Whether check-all default behavior was preserved.
- Scripts added/changed.
- Validation tier matrix summary.
- Full check-all observed baseline handling.
- Validation commands and results.
- Artifact scope result.
- Evidence hygiene.
- Remaining debt.
- Final git status.
- Git commands used and why.

## Mandatory commit/push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:
- `GREEN Goal 089 tiered validation pipeline`
- `BLOCKED Goal 089 tiered validation pipeline`
- `FAILED Goal 089 tiered validation pipeline`
