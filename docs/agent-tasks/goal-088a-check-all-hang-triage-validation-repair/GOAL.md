# Goal 088A — Check-All Hang Triage & Region Composer Validation Repair

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Resolve the Goal 088 BLOCKED state caused by the required full `.devflow\scripts\check-all.ps1` route timing out in the non-product test leg.

Do not start new feature work. The goal is bounded validation triage and repair:

- prove whether full `check-all.ps1` passes when allowed to complete;
- if it does not pass, isolate the exact hanging/failing test namespace/test;
- if the root cause is in Goal 088 code/tests/evidence, fix it inside the allowed Goal 088 areas;
- if the root cause is unrelated historical suite behavior, do not mutate unrelated systems; commit a precise BLOCKED evidence report with the isolated cause.

Goal 088 implementation evidence may be GREEN, but the repository cannot proceed to the next feature goal until the full validation route is either passing or the blocker is precisely isolated and documented.

## Current context

Latest known state from the user report:

- Goal 087 GREEN commit: `d8cd8059c`.
- Goal 088 BLOCKED commit: `114a3280f BLOCKED Goal 088 deterministic visual region composer`.
- Goal 088 focused tests and product smoke passed.
- `check-all.ps1 -SkipTests` passed.
- Full `check-all.ps1` timed out twice, around 5 minutes and 15 minutes, hanging in the non-product `dotnet test --filter FullyQualifiedName!~ProductSmoke` leg.
- Goal 088 evidence root: `.llmgc/procedural/goal-088-deterministic-visual-region-composer/`.
- Goal 088 artifacts report GREEN implementation evidence but the final status is BLOCKED because the required full route did not complete.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current HEAD includes `114a3280f BLOCKED Goal 088 deterministic visual region composer`.
4. Confirm Goal 088 artifacts exist and report `implementationStatus=GREEN`, `accepted=false`, and manual gate `deterministic_visual_region_composer_verification required`.
5. Confirm current state docs record Goal 088 as BLOCKED / produced-for-review due to full check-all timeout.
6. Inspect current dirty state before editing. Do not stage/revert unrelated user changes.
7. Confirm no forbidden areas are already modified before work starts.
8. Record current `AlphaRuntimeBootstrap.cs` line count/hash as read-only baseline; do not modify it.

## Read first

- `AGENTS.md`
- `.devflow/scripts/check-all.ps1`
- `.devflow/scripts/check-devflow-state.ps1`
- `.devflow/scripts/check-artifact-scope.ps1`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-composer-report.md`
- `.llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-quality-gate-scan.json`
- `.llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-negative-proof.json`
- `.llmgc/procedural/goal-088-deterministic-visual-region-composer/visual-region-definition.json`
- `src/LLMGameCreator.Application/Design/DeterministicVisualRegionComposer/**`
- `tests/LLMGameCreator.Tests/Application/DeterministicVisualRegionComposer/**`
- `tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualRegionComposerProductSmokeTests.cs`
- Latest `.devflow/runs/*check-all*` directories if present.

## Allowed files / areas

Primary allowed areas:

- `.llmgc/procedural/goal-088a-check-all-hang-triage-validation-repair/`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `docs/agent-tasks/goal-088a-check-all-hang-triage-validation-repair/`

Conditional repair areas only if the isolated root cause is in Goal 088:

- `src/LLMGameCreator.Application/Design/DeterministicVisualRegionComposer/`
- `tests/LLMGameCreator.Tests/Application/DeterministicVisualRegionComposer/`
- `tests/LLMGameCreator.Tests/ProductSmoke/DeterministicVisualRegionComposerProductSmokeTests.cs`

Do not touch broad historical test suites unless the final status is BLOCKED and you are only documenting the isolated external cause in Goal 088A evidence.

## Forbidden files / areas

Do not change:

- public GamePackage schema;
- Runtime / Runtime.Abstractions;
- Unity files, including `AlphaRuntimeBootstrap.cs`;
- Infrastructure provider / LLM / RAG / media provider code;
- Lua / Scripting;
- generator-library;
- `.sln`;
- `.csproj`;
- package lock files;
- binary media assets;
- generated raster assets;
- prompt dumps or provider output;
- external dependencies.

Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Baseline validation

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualRegionComposer
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualRegionComposerProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
```

If these fail due to Goal 088 code/tests, repair within allowed Goal 088 files.

### 2. Run the real full check-all route carefully

Run the required full:

```powershell
.\.devflow\scripts\check-all.ps1
```

If the interactive tool wrapper times out but child `dotnet` / `testhost` processes are still active, do not immediately call it a failure. Instead:

- inspect the newest `.devflow/runs/*check-all*` directory;
- poll `test.log`, `build.log`, `summary.json` if present;
- inspect child process CPU/activity;
- let the run continue long enough to distinguish slow historical tests from an actual hang.

Recommended maximum wall-clock before declaring unresolved hang: 45 minutes.

If the full run eventually passes, record the run directory, test count, warnings, elapsed time and hashes in Goal 088A evidence.

### 3. Isolate if check-all still hangs

If full check-all does not complete:

1. Stop only stale/orphaned test processes from the timed-out check-all run. Do not kill unrelated user processes.
2. Run non-product test isolation with bounded splits, for example:
   - non-Application tests;
   - Application blocks by namespace;
   - Unity/World blocks if needed;
   - Goal 088-specific tests separately.
3. Use `dotnet test --list-tests` and `--blame-hang --blame-hang-timeout 180s` only where useful.
4. Identify the narrowest namespace/test or process state that causes the hang.

### 4. Repair if root cause is Goal 088

If the hang/failure is caused by Goal 088 code/tests/evidence:

- fix it inside the allowed Goal 088 Application/tests/evidence areas;
- avoid broad refactors;
- avoid changing historical tests;
- avoid changing check-all script unless a local Goal 088-specific test behavior requires no code change and only documentation would be misleading. Prefer not changing the script.

Common allowed fixes if proven necessary:

- make Goal 088 tests deterministic and non-blocking;
- avoid long synchronous file-generation loops in tests;
- avoid shared mutable static state;
- avoid writing historical artifact roots during full suite;
- introduce a narrow xUnit collection only for Goal 088 tests if parallel artifact writes are the cause.

### 5. Do not hide unrelated failures

If the hang is unrelated historical behavior, return BLOCKED with a precise triage artifact. Do not mark GREEN merely because Goal 088 focused tests pass.

### 6. Generate Goal 088A evidence

Create `.llmgc/procedural/goal-088a-check-all-hang-triage-validation-repair/`.

Required artifacts:

- `check-all-hang-triage-report.md`
- `check-all-run-summary.json`
- `test-isolation-matrix.json`
- `process-cleanup-log.json`
- `quality-gate-scan.json`

Evidence must include:

- starting commit;
- final commit;
- whether full check-all passed;
- check-all run directory if available;
- elapsed time;
- total test count / warnings if passed;
- isolated hang/failure namespace/test if not passed;
- whether Goal 088 code was modified;
- whether historical artifacts were restored after validation side effects;
- whether any orphaned processes were stopped;
- no heavy raw logs copied into evidence.

### 7. Docs/state update

If GREEN:

- record Goal 088A as a validation repair produced for review with `accepted=false`;
- record that the Goal 088 check-all blocker is repaired / full route passed;
- preserve Goal 088 artifact evidence as `accepted=false` and do not rewrite its report into fake GREEN if it was originally BLOCKED by commit message;
- manual gate: `goal_088_check_all_validation_repair_verification required`.

If BLOCKED:

- record the precise unresolved blocker and isolated test/namespace/process state;
- keep Goal 088 as not ready for next feature goal;
- manual gate remains blocked.

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`

### 8. Artifact scope

Update `.devflow/artifact-scope/artifact-scope-policy.json` for scenario:

`goal-088a-check-all-hang-triage-validation-repair`

Allow only Goal 088A task pack, Goal 088A evidence, docs quartet/debt register, artifact-scope policy, and conditional Goal 088 code/tests if actually repaired.

## Quality gate

GREEN only if:

- full `.devflow/scripts/check-all.ps1` passes;
- focused Goal 088 tests pass;
- CurrentState tests pass;
- artifact scope passes;
- no forbidden files changed;
- no Unity/Runtime/provider/schema/project/dependency changes;
- no binary/raster media added;
- no prompt dumps;
- no heavy logs in tracked evidence;
- no orphaned test processes left behind;
- if code changed, source formatting remains clean.

BLOCKED if:

- full check-all still hangs/fails and the root cause is not repairable within allowed files;
- the hang is isolated to unrelated historical tests and fixing it would require forbidden/broad changes;
- the root cause cannot be isolated within a reasonable time, but enough evidence is captured to avoid fake GREEN.

FAILED if:

- build/focused tests regress due to this goal and cannot be repaired inside allowed files.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualRegionComposer
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter DeterministicVisualRegionComposerProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-088a-check-all-hang-triage-validation-repair"
git diff --check
git diff --cached --check
```

Also run text hygiene scans over changed files for mojibake, absolute local paths in tracked evidence, timestamps/heavy logs, prompt dumps, and binary additions.

## Final report format

Report:

- Final status: GREEN / BLOCKED / FAILED.
- Latest commit before work.
- Latest commit after work.
- Push status.
- Preflight summary.
- Exact check-all result.
- If check-all initially timed out, how it was handled.
- Test isolation summary.
- Root cause classification: Goal088 / historical unrelated / unresolved.
- Files changed.
- Any code/test repair performed.
- Evidence artifacts created.
- Validation results.
- Artifact scope result.
- Process cleanup result.
- Evidence hygiene result.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.

## Mandatory commit / push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

- `GREEN Goal 088A check-all hang triage validation repair`
- `BLOCKED Goal 088A check-all hang triage validation repair`
- `FAILED Goal 088A check-all hang triage validation repair`

Do not rewrite history.
