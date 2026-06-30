# Codex task — Goal 061 Full Campaign Playable Review Package RC

## Assignment metadata

Repository:

```text
https://github.com/kpCat/LLMGameCreator
```

Working copy:

```text
C:\Users\endim\LLMGameCreator\
```

Branch:

```text
main
```

Composite goal id/name:

```text
goal-061-full-campaign-playable-review-package-rc
Goal 061: Full Campaign Playable Review Package RC
```

Required gate marker:

```text
full_campaign_playable_review_package_rc_verification
```

Codex reasoning level:

```text
very high
```

## Operating policy

This is an aggressive composite goal. Commit and push the final state to `origin/main` regardless of GREEN/BLOCKED/FAILED status. Do not pretend a non-green result is accepted.

Commit messages:

```text
GREEN Goal 061 full campaign playable review package rc
BLOCKED Goal 061 full campaign playable review package rc
FAILED Goal 061 full campaign playable review package rc
```

No branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Preflight

1. Confirm branch is `main`.
2. Inspect current state docs and Goal 060 artifacts.
3. Record Goal 060 as accepted by user handoff before Goal 061:

```text
full_campaign_gamepackage_materialization_matrix_verification passed before Goal 061
```

4. Do not mark Goal 061 passed.
5. If untracked task launcher/spec files from this goal exist, include them only if they are part of the explicit allowed Goal 061 package/docs and artifact-scope policy permits them; otherwise leave them untracked only if the final report explains why. Prefer committing the task/spec/scouting docs as part of the authoritative goal package if allowed.

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_061_FULL_CAMPAIGN_PLAYABLE_REVIEW_PACKAGE_RC_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_061_FULL_CAMPAIGN_PLAYABLE_REVIEW_PACKAGE_RC.md`
8. Goal 060 artifacts under `.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/`
9. Goal 059 artifacts under `.llmgc/procedural/goal-059-full-generator-variability-regression-matrix/`
10. Goal 058 artifacts under `.llmgc/procedural/goal-058-full-media-bound-generator-campaign/`
11. Goal 057 artifacts under `.llmgc/procedural/goal-057-unity-alpha-multifamily-playable-loop/`
12. Goal 056 artifacts under `.llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/`
13. Goal 055 artifacts under `.llmgc/procedural/goal-055-media-bound-playable-review-package-smoke/`
14. Goal 054 artifacts under `.llmgc/procedural/goal-054-media-materialization-review-package/`
15. Existing Application seams for Goal 056-060.
16. Existing Unity Alpha runtime bootstrap and build/proof routes:
    - `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
    - any existing Alpha build scripts used by product smoke.
17. Existing product smoke and artifact scope guard patterns.

## Allowed files / areas

You may create/edit:

```text
docs/GOAL_061_FULL_CAMPAIGN_PLAYABLE_REVIEW_PACKAGE_RC_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_061_FULL_CAMPAIGN_PLAYABLE_REVIEW_PACKAGE_RC.md
docs/agent-tasks/GOAL_061_FULL_CAMPAIGN_PLAYABLE_REVIEW_PACKAGE_RC.md
docs/agent-tasks/GOAL_061_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/FullCampaignPlayableReviewPackageRc/**
tests/LLMGameCreator.Tests/Application/FullCampaignPlayableReviewPackageRc/**
tests/LLMGameCreator.Tests/ProductSmoke/FullCampaignPlayableReviewPackageRcProductSmokeTests.cs
.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Only touch `AlphaRuntimeBootstrap.cs` narrowly for package-row selection and proof markers. Do not refactor it broadly.

## Forbidden files / areas

Forbidden unless the task becomes impossible and you commit/push BLOCKED with evidence:

```text
src/LLMGameCreator.GamePackage/** public schema changes
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
src/LLMGameCreator.Scripting/**
unity/** except AlphaRuntimeBootstrap.cs explicitly allowed above
generator-library/**
samples/**
templates/**
*.sln
*.csproj
external dependencies / NuGet additions
```

Do not call network/provider/LLM/RAG/media generators. Do not execute arbitrary Lua. Do not create an installer.

## Exact behavior

### 1. Source chain loader

Implement a BCL-only Application seam under:

```text
src/LLMGameCreator.Application/Design/FullCampaignPlayableReviewPackageRc/
```

It must load and validate:

- Goal 060 materialized package inventory and 9 physical GamePackage JSON files.
- Goal 060 runtime/Unity package proof.
- Goal 059 matrix rows and seeds.
- Goal 058 campaign proof.
- Goal 054/055 physical media and staged media manifests as needed.
- Existing Unity Alpha proof route metadata.

Reject stale/fake/missing source evidence causally.

### 2. Review package RC staging

Create a deterministic review package RC under:

```text
.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/review-package/
```

Include compact tracked files only. Heavy Unity build/log/cache outputs must remain ignored unless already tracked by policy.

The review package should include, where feasible:

- `README.md`
- `RUN_MANUAL.ps1`
- `RUN_AUTOMATED_SMOKE.ps1`
- package selection matrix JSON
- package inventory JSON
- media/StreamingAssets payload manifest
- family/seed command plan
- manual checklist
- generated scenario summaries for all 9 rows
- hashes and provenance sidecars

If scripts are not executable in CI/test environment, still validate their content, paths and referenced files deterministically.

### 3. Unity Alpha package-row selection proof

Narrowly extend `AlphaRuntimeBootstrap.cs` if needed so the existing Unity Alpha route can:

- load a Goal 061 command plan from StreamingAssets or staged payload;
- select a package row by family/seed/package id;
- emit deterministic markers:

```text
review_package_rc_loaded=true
review_package_rc_id=<id>
package_row_selected=<row id>
package_id=<package id>
family_id=<family id>
seed_id=<seed id>
package_hash_verified=true
package_media_bindings_verified=true
package_loop_started=true
package_loop_step=<ordered step id>
package_loop_completed=true
save_load_replay_verified=true
review_package_rc_proof=goal061
```

Prefer all 9 rows in automated proof. If Unity/player runtime cost makes all 9 rows too expensive, at minimum prove all 3 families plus provide a deterministic matrix plan for all 9. However GREEN should strongly prefer all 9 rows.

### 4. Runtime/save-load/replay tie-in

The Goal 061 Application proof must tie save/load/replay to package row ids and package hashes, not just family ids.

Produce:

- package-row save/load audit;
- replay determinism audit;
- preview/export payload consistency proof;
- invalid matrix.

### 5. Evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/
```

Required:

```text
source-manifest.json
review-package-rc-manifest.json
review-package-file-inventory.json
package-row-selection-matrix.json
unity-player-command-plan.json
unity-player-proof-matrix.json
package-media-binding-audit.json
save-load-replay-package-row-audit.json
manual-review-checklist.md
automated-smoke-script-manifest.json
invalid-review-package-matrix.json
full-campaign-playable-review-package-rc-report.md
```

No absolute machine paths in committed evidence. Use repo-relative paths.

### 6. Invalid/fake/leak matrix

Cover at least:

- missing Goal 060 inventory;
- stale package hash;
- missing package file;
- malformed package JSON;
- fake family/seed/package row;
- duplicate row id;
- unsafe relative path / traversal;
- missing media binding;
- stale media hash;
- fake Unity proof marker;
- provider/LLM/RAG/media-generation claim;
- Runtime/GamePackage schema broad mutation claim;
- Unity broad mutation claim outside allowed bootstrap marker route;
- nondeterministic row order;
- missing review trace;
- script path escaping review package root.

### 7. Docs/state update

Update docs quartet:

- Goal 060 accepted by user handoff before Goal 061.
- Goal 061 produced for review with `full_campaign_playable_review_package_rc_verification required`.
- Goal 061 not accepted.
- Goal 031/032 remain produced-for-review/not passed unless current docs intentionally preserve that state.

## Tests

Add focused tests for:

- source loading;
- review package manifest/inventory;
- package row selection matrix;
- script manifest validation;
- save/load/replay package-row audit;
- Unity command plan/proof parsing;
- invalid matrix.

Add exact product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/FullCampaignPlayableReviewPackageRcProductSmokeTests.cs
```

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~FullCampaignPlayableReviewPackageRc|FullyQualifiedName~Goal061"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~FullCampaignPlayableReviewPackageRcProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal061|FullyQualifiedName~FullCampaignPlayable"

.\.devflow\scripts\check-all.ps1
```

Run existing artifact-scope guard for Goal 061 if the repository has the standard script/policy route. Do not invent a new guard.

Also run a mojibake marker scan on changed text files/artifacts using the existing repo pattern.

## Bounded repairs pre-authorized

Allowed within this task if needed:

1. Update stale current-state/handoff guard tests when they only hardcode the previous latest gate and the repair preserves historical assertions.
2. Restore exact accidental historical `.llmgc/procedural/**` mutations from HEAD using `git restore --source=HEAD -- <exact paths>` only for non-Goal-061 accidental artifacts.
3. Update `.devflow/artifact-scope/artifact-scope-policy.json` to allow Goal 061 changed paths.
4. Re-run Unity build/player proof if the first attempt fails due to transient file lock or timeout.

Do not use reset/clean/stash/rebase/merge/cherry-pick/force-push.

## Stop/BLOCKED conditions

Commit/push BLOCKED if:

- physical Goal 060 package artifacts cannot be consumed honestly;
- package row selection cannot be tied to package hashes;
- Unity/player route cannot produce real markers and no honest bounded fallback remains;
- proving the goal requires public GamePackage schema changes;
- proving the goal requires broad Runtime/UI/Unity refactors outside allowed scope;
- final checks fail and cannot be repaired within allowed scope.

## Final report format

Report in Russian:

```text
Goal 061 выполнен / остановлен
Status: GREEN / BLOCKED / FAILED
Gate: full_campaign_playable_review_package_rc_verification required

Что стало реальнее:
...

Изменённые файлы:
...

Review package RC:
- package rows:
- scripts:
- physical package count:
- Unity/player proof:

Проверки:
...

Invalid/fake/leak matrix:
...

Git:
- commit:
- push:
- final status:

Ограничения:
...
```
