# Codex task — GOAL 060 Full Campaign GamePackage Materialization Matrix

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
goal-060-full-campaign-gamepackage-materialization-matrix
Goal 060: Full Campaign GamePackage Materialization Matrix
```

Required goal marker / gate:

```text
full_campaign_gamepackage_materialization_matrix_verification required
```

Codex reasoning level:

```text
very high
```

## Mandatory git policy

You must commit and push the final state to `origin/main` even if the result is GREEN, BLOCKED or FAILED.

Use commit message patterns:

```text
GREEN Goal 060 full campaign gamepackage materialization matrix
BLOCKED Goal 060 full campaign gamepackage materialization matrix
FAILED Goal 060 full campaign gamepackage materialization matrix
```

Never pretend that a blocked or failed result is green. Final report must include exact status and pushed commit hash.

Forbidden git operations unless separately authorized by the user:

```text
git checkout
git switch
git merge
git rebase
git cherry-pick
git reset
git stash
git clean
git push --force
```

Allowed git operations:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit paths>
git diff --stat --cached
git diff --cached --name-only
git diff --cached --check
git add <explicit allowed paths>
git commit -m "<GREEN/BLOCKED/FAILED message>"
git rev-parse HEAD
git push origin main
```

## First step: preflight acceptance of Goal 059

Record the user handoff acceptance of Goal 059 before implementing Goal 060:

```text
full_generator_variability_regression_matrix_verification passed before Goal 060
```

Update only the normal docs quartet for this preflight:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Do not mark Goal 060 passed. Goal 060 must stop at:

```text
full_campaign_gamepackage_materialization_matrix_verification required
```

Preserve Goal 031 and Goal 032 as produced-for-review/not passed unless current docs already changed them.

## Read-first list

Read first, in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_060_FULL_CAMPAIGN_GAMEPACKAGE_MATERIALIZATION_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_060_FULL_CAMPAIGN_GAMEPACKAGE_MATERIALIZATION.md`
8. Goal 059 artifacts under `.llmgc/procedural/goal-059-full-generator-variability-regression-matrix/`
9. Goal 058 artifacts under `.llmgc/procedural/goal-058-full-media-bound-generator-campaign/`
10. Goal 057/056 Unity proof artifacts if needed for Alpha command markers.
11. Goal 047/043 package/dry-run/family artifacts if needed for materialization source facts.
12. Existing package assembly contracts and tests:
    - `docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md`
    - `docs/PACKAGE_ASSEMBLY_DIALOGUE_QUESTS_CONTRACT_V1.md`
    - `docs/PACKAGE_ASSEMBLY_ITEMS_ECONOMY_CRAFTING_CONTRACT_V1.md`
    - `docs/PACKAGE_ASSEMBLY_COMBAT_PROGRESSION_CONTRACT_V1.md`
    - existing `Design/PackageAssembly*/` folders
    - existing `GeneratorPlanGamePackageAssembler` usage if present
13. Existing GamePackage validation and runtime preview/export smoke tests by targeted search, not broad repository reading.
14. Existing Unity Alpha bootstrap and recent Goal 056-059 proof runner patterns.
15. Existing `.devflow/artifact-scope/artifact-scope-policy.json` and check-artifact-scope usage.

Do not read the entire repository unless a targeted search shows the needed files are elsewhere.

## Allowed files / areas

Allowed to create/edit:

```text
docs/GOAL_060_FULL_CAMPAIGN_GAMEPACKAGE_MATERIALIZATION_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_060_FULL_CAMPAIGN_GAMEPACKAGE_MATERIALIZATION.md
docs/agent-tasks/GOAL_060_FULL_CAMPAIGN_GAMEPACKAGE_MATERIALIZATION.md
docs/agent-tasks/GOAL_060_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/FullCampaignGamePackageMaterialization/**
tests/LLMGameCreator.Tests/Application/FullCampaignGamePackageMaterialization/**
tests/LLMGameCreator.Tests/ProductSmoke/FullCampaignGamePackageMaterializationProductSmokeTests.cs
.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Narrow optional allowed area only if existing package assembler extension is required and no public schema change is needed:

```text
src/LLMGameCreator.Application/**/GeneratorPlanGamePackageAssembler*.cs
src/LLMGameCreator.Application/**/PackageAssembly*.cs
```

If you touch optional package assembly files, final report must explain exactly why and prove no public schema change occurred.

## Forbidden files / areas

Do not change:

```text
src/LLMGameCreator.GamePackage/** public schema/model definitions
src/LLMGameCreator.Runtime/** except no changes are expected
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
src/LLMGameCreator.Scripting/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Do not add external dependencies.

Do not call providers, network, LLM/RAG, media generators, or arbitrary Lua.

Do not create fake packages that bypass existing validators.

## Exact behavior

### 1. Source chain loading

Create a BCL-only Application seam:

```text
src/LLMGameCreator.Application/Design/FullCampaignGamePackageMaterialization/
```

It must load and verify source facts from Goal 059 and, as needed, Goal 058/047/043/040/039/038/037/034 evidence.

The source loader must verify:

- relative paths only;
- expected gate ids;
- family ids;
- seed ids;
- source hashes;
- row ids;
- deterministic ordering.

### 2. Package materialization plan

Build a package materialization plan for the Goal 059 matrix:

- 3 families;
- 3 seeds;
- 9 rows;
- trace every package plan row to Goal 059 row id and source family/seed.

A plan row must include:

- row id;
- family id;
- seed id;
- source campaign hash;
- selected package assembly domains;
- package id;
- expected runtime loop kind;
- expected preview/export profile;
- blocked/future-required gaps if any.

### 3. Real GamePackage artifacts

Materialize real GamePackage JSON artifacts through existing schema-compatible paths.

Preferred target:

```text
9 validator-clean package JSON files, one per family x seed row.
```

Minimum GREEN target if current schema cannot honestly support all nine:

```text
at least one validator-clean package JSON per family, plus causal blocked/future-required records for non-materializable rows.
```

Each materialized package must:

- be physically written under Goal 060 artifacts;
- be valid JSON;
- pass existing GamePackage/package validation path;
- include traceable metadata/source refs only through allowed existing fields or sidecar manifests;
- not require public schema changes.

Do not mutate public GamePackage schema.

### 4. Runtime consumption proof

For each family with a materialized package, prove at least one state-changing runtime loop through existing runtime-owned state seams.

Expected proof families:

- map_panel_rpg: traversal/NPC/quest/event/item progress;
- survival_sandbox: hazard/resource/collect/consume/craft/survival transition;
- first_person_grid_dungeon: grid/dungeon traversal/orientation/encounter/progression pressure.

Prefer using existing runtime serializer/snapshot roundtrip when available.

If direct runtime execution is impossible without forbidden Runtime changes, produce a BLOCKED result unless an already accepted Application-layer runtime adapter path exists and is used honestly.

### 5. Preview/export payload proof

Build package-bound preview/export payloads that consume the materialized package inventory, not just prior Goal 059 matrix JSON.

Proof must include:

- package-bound preview payloads;
- package-bound export payloads;
- package id/family/seed trace;
- hash/provenance ledger;
- package immutability audit.

### 6. Unity Alpha package consumption proof

Narrowly extend Unity Alpha only if needed through:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Unity proof must be package-consumption oriented, not just campaign-marker replay.

Expected markers include:

```text
package_matrix_loaded=true
package_materialization_goal=goal060
package_family=<family>
package_seed=<seed>
package_id=<id>
package_validation_passed=true
package_runtime_loop_completed=true
```

GREEN requires real Unity/editor/player proof if the existing route can run. If Unity route is unavailable but package/runtime proof is strong, commit/push BLOCKED with exact reason and all non-Unity progress.

### 7. Evidence writer

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/
```

Required artifacts:

```text
source-campaign-matrix-manifest.json
package-materialization-plan.json
materialized-package-inventory.json
package-validation-matrix.json
runtime-consumption-matrix.json
preview-export-package-payloads.json
unity-package-consumption-command-plan.json
unity-package-consumption-proof.json
invalid-package-materialization-diagnostics-matrix.json
full-campaign-gamepackage-materialization-report.md
artifact-scope-report.json
```

If physical packages are generated, include them under:

```text
packages/<family>/<row-id>/game-package.json
```

No timestamps unless an existing deterministic convention exists. No absolute paths. No heavy build logs. No large binary artifacts.

### 8. Invalid/fake/leak matrix

Cover at least these cases:

- missing Goal 059 source;
- stale Goal 059 hash;
- fake matrix row id;
- duplicate package id;
- invalid family id;
- invalid seed id;
- package JSON malformed;
- package validation failure;
- package source trace mismatch;
- schema mutation claim;
- Runtime/UI/Unity broad mutation claim;
- provider/network/LLM/RAG/media-generation claim;
- arbitrary Lua execution claim;
- unsafe path;
- nondeterministic ordering;
- fake Unity marker;
- missing runtime transition proof;
- package immutability breach.

Every invalid case must have a stable diagnostic code and causal explanation.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/FullCampaignGamePackageMaterialization/**
```

Suggested tests:

- source loading tests;
- package materialization plan tests;
- package inventory/validation tests;
- runtime consumption matrix tests;
- preview/export payload tests;
- Unity command plan/proof tests;
- invalid matrix tests;
- evidence tests.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/FullCampaignGamePackageMaterializationProductSmokeTests.cs
```

## Validation commands

Run from `C:\Users\endim\LLMGameCreator\`:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~FullCampaignGamePackageMaterialization|FullyQualifiedName~Goal060"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~FullCampaignGamePackageMaterializationProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal060|FullyQualifiedName~FullCampaignGamePackage"

.\.devflow\scripts\check-all.ps1
```

Run artifact scope guard using the existing repo command/pattern. Do not invent a new guard script.

Run mojibake scan on changed text files/artifacts.

If Unity proof is part of product smoke, preserve heavy Unity logs/build outputs as ignored and commit only compact evidence.

## Bounded repairs pre-authorized

Allowed bounded repairs without stopping:

1. Update stale current-state/handoff guard tests if they hardcode the previous latest gate and fail only because Goal 060 is now current.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates them outside Goal 060.
3. Add Goal 060 artifact-scope allowlist entry.
4. Narrowly update AlphaRuntimeBootstrap markers for Goal 060 package consumption only.

Do not use forbidden git operations for repairs.

## Stop / status rules

Commit/push GREEN only if:

- package materialization proof is real;
- validation is real;
- runtime consumption proof is real;
- Unity proof is real if claimed;
- check-all is green;
- artifact scope guard is green;
- no forbidden areas were touched.

Commit/push BLOCKED if:

- public GamePackage schema changes are required;
- no existing validator/materializer path can produce real package artifacts;
- Unity route cannot run after non-Unity proofs are complete;
- runtime consumption requires forbidden Runtime changes.

Commit/push FAILED if:

- implementation is incomplete or tests cannot be stabilized and the result is not a coherent blocked proof.

## Final report format

Report in Russian:

```text
Goal 060 выполнен / заблокирован / не выполнен
Status: GREEN / BLOCKED / FAILED
Gate: full_campaign_gamepackage_materialization_matrix_verification required

Что стало реальнее:
...

Изменённые файлы:
...

Materialized packages:
<count, families, seeds, validation status>

Runtime/Unity proof:
<runtime proof, Unity proof, markers, exit codes if available>

Evidence artifacts:
...

Проверки:
...

Invalid/fake/leak matrix:
...

Bounded repairs:
...

Git:
<commit hash, push result, final status>

Ограничения:
<schema/runtime/UI/provider/LLM/RAG/Lua/media/external deps not touched>

Следующий разумный шаг:
...
```
