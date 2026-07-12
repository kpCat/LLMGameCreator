# Goal 150B — Zero-Value Runtime Evidence + Complete Test-Suite Closure Hotfix

## Identity

- Task ID: `goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `90f278b1f3a70fdb5011e555491fb83860d00509`
- Base message: `Update generator state docs and validation flows`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Mandatory status-aware publication policy

The owner must never commit or push Codex work manually again.

For this Goal, GREEN, BLOCKED and FAILED are publishable outcomes.

### GREEN

Commit and push all complete allowlisted changes:

`GREEN Goal 150B zero-value Runtime evidence and complete-suite closure hotfix`

### BLOCKED

When implementation is coherent and the blocker is a timeout, unavailable CI, external tool failure or another non-corrupting verification condition:

1. keep the coherent implementation and honest BLOCKED artifacts;
2. commit it;
3. push it to `origin/main`.

Message:

`BLOCKED Goal 150B zero-value Runtime evidence and complete-suite closure hotfix`

Do not return BLOCKED with an uncommitted worktree merely because the overall gate is not GREEN.

### FAILED

Never push known broken product code. Restore only exact task-owned broken product paths to the required base, retain safe diagnostic/reproduction changes, then commit and push:

`FAILED Goal 150B zero-value Runtime evidence and complete-suite closure hotfix`

### Publication is mandatory

For every outcome with task-owned changes:

```powershell
git commit
git push origin main
git rev-parse HEAD
git rev-parse origin/main
git status --short
```

Final response must confirm `HEAD == origin/main` and clean worktree. Reporting a status without attempting commit/push is a protocol failure.

Only a concrete Git/auth/remote failure permits no pushed commit. Retry once, report the exact command/error, and do not tell the owner to push manually.

Copy this status-aware publication section into all future Goal files.

## Current state

Goal149 added default-off equipment. Goal150 added default-off attributes/progression. Goal150A added a generic immutable effective catalog synchronizing package mutation values, Runtime-effect expectations and Runtime playthrough args.

Focused Goal150A evidence is GREEN for:

```text
weapon=3
strength=8
per-point=2
level2 XP=12
equipment/stat/total=3/6/9
level/XP=2/12
```

Goal150A remained BLOCKED because:

`dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug`

did not finish after multiple attempts, including one over 60 minutes.

The owner manually committed/pushed the coherent BLOCKED implementation as base commit `90f278b1...`. Do not amend it.

Acceptance remains false for Goals149/150/150A. Do not perform or claim human acceptance.

## P1 defect found by independent audit

`weaponDamageBonus` permits `0..10`.

Goal150A tests value `0` only with both equipment and attributes selected. Attributes cause `DamageApplied.Args` to exist, masking equipment-only zero behavior.

Current behavior:

```text
if stat metadata exists:
    emit equipmentDamageBonus/statDamageBonus/totalAdditionalDamage
else if equipment bonus > 0:
    emit equipmentDamageBonus
else:
    emit no bonus args
```

Thus this valid independent composition cannot prove its exact Runtime effect:

```text
equipment enabled
attributes disabled
weaponDamageBonus=0
```

The evaluator expects exact `0`, but no `equipmentDamageBonus` datum exists.

## Part A — Generic zero-value Runtime evidence

Do not branch on module IDs.

Extend generic equipment bonus resolution to distinguish:

- metadata absent;
- metadata present with value zero.

Recommended shape:

```text
TryResolveEquipmentDamageBonus(...,
    out double bonus,
    out bool metadataPresent,
    out RuntimeDiagnostic? diagnostic)
```

Equivalent design is acceptable.

When an equipped item has valid numeric `combat_damage_bonus` metadata:

- `metadataPresent=true`, including `0`;
- `DamageApplied.Args["equipmentDamageBonus"]` exists;
- zero serializes deterministically as `"0"`;
- exact Runtime effect, checkpoint and replay observe it.

When metadata is absent, do not invent equipment evidence.

### Preserve hashes and positive event shape

Goal149 equipment bonus `2` hashes must remain exact. Do not add fields to the existing positive equipment-only event if that changes hashes.

For zero with metadata present, add only the minimal evidence needed to distinguish zero from absence.

### Build-result total fallback

The project build result must report honest total additional damage without duplicating gameplay:

```text
if totalAdditionalDamage event arg exists:
    use it
else if DamageApplied exists:
    total = observed equipmentDamageBonus + observed statDamageBonus
else:
    total = 0
```

Required:

```text
equipment only +3 => equipment=3, stat=0, total=3
equipment only 0  => equipment=0, stat=0, total=0
attributes only +6 => equipment=0, stat=6, total=6
combined 3+6 => total=9
```

Disabled modules add no irrelevant summary lines.

## Part B — Complete test-suite diagnosis and exhaustive execution

Do not merely increase the timeout again.

Collect:

- `dotnet --info`;
- test SDK/xUnit versions;
- discovered test count/classes/namespaces;
- process CPU/memory;
- last completed test/shard;
- slowest classes;
- fixed artifact roots touched concurrently;
- whether xUnit collection parallelism causes contention;
- whether the suite is large or contains a nonterminating test.

Use one bounded monolithic diagnostic attempt, maximum 15 minutes.

Create:

```text
.devflow/scripts/run-complete-test-suite.ps1
.devflow/scripts/run-complete-test-suite.cmd
```

The runner must execute the complete discovered test set in isolated deterministic shards:

1. robust discovery inventory;
2. disjoint namespace/class or exact-test partition;
3. every discovered test assigned exactly once;
4. isolated testhost per shard;
5. bounded per-shard timeout;
6. one TRX per shard;
7. merged JSON summary;
8. counts for discovered/assigned/executed/passed/failed/skipped/missing/duplicate/aborted;
9. required: missing=0, duplicate=0, failed=0, aborted=0;
10. 20 slowest tests/classes;
11. exact commands/stdout/stderr for failed or slow shards;
12. no excluded Unity, WinForms, artifact, devflow, Runtime or historical regression tests.

Use at most two simultaneous testhost processes unless measurement proves another limit safer.

Preferred GREEN: monolithic command finishes. Acceptable equivalent GREEN: monolithic remains pathological but exhaustive isolated execution covers the exact discovered set with zero missing/duplicate/failed/aborted. Record both statuses honestly.

If a slow/hanging test is found, fix actual termination/isolation cause. Forbidden:

- skip attributes;
- delete tests;
- weaken assertions;
- exclude slow categories;
- mock success;
- treat timeout as pass.

If another existing test file must change, add its exact path to artifact scope with diagnostic evidence. Do not allowlist the whole tests tree.


## Part C — Expression hardening

Goal150A's decimal expression evaluation may throw `OverflowException`.

Required:

- arithmetic overflow becomes a failed binding result with deterministic diagnostic;
- no unhandled exception reaches activation;
- division-by-zero/cycle rejection remain GREEN;
- do not expand the expression language or add scripting/reflection/compiler/file/network access.

## Required tests

### Equipment-only zero

Through real composition/qualification:

```text
selected optional modules:
  feature.equipment.weapon_loadout
weaponDamageBonus=0
attributes/progression disabled
```

Assert package metadata `"0"`, weapon equipped, event arg `equipmentDamageBonus=="0"`, exact expected/actual `0`, validation/checkpoint/replay/binding GREEN, and no stat evidence invented.

### Equipment-only positive

```text
equipment only
weaponDamageBonus=3
```

Assert equipment=3, stat=0, total=3.

### No-metadata baseline

Equipment disabled: no false equipment evidence and disabled hashes unchanged.

### Existing custom regression

Re-run `3/8/2/12 => 3/6/9 and 2/12`.

### Independent parameter matrix

```text
equipment only: 0,3,10
attributes only:
  startingStrength 0,8,20
  damagePerStrengthPoint 0,0.5,2,5
progression only: 1,12,1000
```

Do not add unrelated modules merely to expose event args.

## Historical hashes

Preserve exactly.

Disabled:

```text
composition=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
activated=c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb
final=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
actions=13/8/13
```

Equipment default 2:

```text
composition=94a47ab896b425a76c2e523acef3ab87d538bb8f0c754b2402b0127e5ad82bf5
activated=147f88ac026f006ab5fbe93dc6c7cb039e85189fcb3421a71a1fd99284d3a5c1
final=51bba1ffada4ce9ffccfa9132e7e7c007afcbcec8632d7de13d26ce961b3ea0d
actions=17/13/17
```

All optional defaults:

```text
composition=ba9dbf32c8e79d4e2bf37116dd611cc7eccd7bee73f880aefeb041cce4b2ee40
activated=19e837b8d4925b0b567c52adfb93905bc44ac6e9a13d3008726ff1be89ea49cf
final=ebb05a61036ddfde40b605267685ba8ab90baa01ed3b5efbb815615ae26eca5c
actions=20/16/20
```

Goal150A custom:

```text
composition=66c6fa980123ad113a6b37e7d6d31b13d946b48df325230a44bac351660c0db3
activated=578aa5b7b40b87015897c762cb651ef6e61f3a190e5e66fd80a6c1dd79664391
final=5f367569870cd8290225e06bba3570b8185c157febb823d444ff9cfa27def09e
playthroughSignature=9bbcc1573999aa3a82a257bf0c8e2d95ed8453574e8a4a7b1d91042146a01050
```

Do not rewrite historical Goal149/150/150A artifacts.

## Required commands

```powershell
dotnet build
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150B"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests"
.\.devflow\scripts\run-capability-driven-runtime-playthrough-equipment-featuremodule-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\run-goal150a-parameterized-runtime-contract-synchronization-hotfix.ps1
.\.devflow\scripts\run-complete-test-suite.ps1
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
```

Use exact existing script names if spelling differs. Known visual smoke debt 084–088 may remain only when the existing wrapper classifies it as known and exits PASSED.

## Required artifacts

Write byte-identical evidence under:

```text
.llmgc/procedural/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/
.llmgc/exports/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/
```

At minimum:

```text
goal150b-dashboard.json
goal150b-root-cause.json
equipment-only-zero-runtime-proof.json
equipment-only-positive-runtime-proof.json
no-equipment-metadata-baseline-proof.json
parameter-independence-matrix-proof.json
goal150a-custom-regression-proof.json
expression-overflow-negative-proof.json
complete-suite-discovery.json
complete-suite-shard-plan.json
complete-suite-result.json
complete-suite-slowest-tests.json
monolithic-suite-diagnostic.json
default-hash-regression-proof.json
historical-artifact-integrity-proof.json
artifact-scope-proof.json
publication-proof.json
goal150b-file-index.json
goal150b-report.md
```

Use executable evidence and SHA-256 file index. Old Goal149/150/150A artifacts remain unchanged.

GREEN dashboard requires:

```text
status=GREEN
equipmentOnlyZeroPassed=true
equipmentOnlyPositivePassed=true
noMetadataBaselinePassed=true
parameterIndependencePassed=true
goal150aCustomRegressionPassed=true
expressionOverflowRejected=true
fullDiscoveredTestSetCovered=true
completeSuiteMissingCount=0
completeSuiteDuplicateCount=0
completeSuiteFailedCount=0
completeSuiteAbortedShardCount=0
defaultHashesPreserved=true
historicalArtifactsPreserved=true
artifactScopePassed=true
goal149Accepted=false
goal150Accepted=false
goal150aAccepted=false
goal150bAccepted=false
acceptedByCodex=false
manualReviewPerformed=false
passed=true
```

Incomplete exhaustive coverage => BLOCKED/passed=false, but coherent changes still must be committed and pushed.

## Current state and manual gate

Update current state honestly:

- Goal150A was manually pushed because its task incorrectly allowed publication only on GREEN;
- Goal150B establishes mandatory status-aware Codex publication;
- record equipment-only zero fix status;
- record monolithic and exhaustive-sharded suite separately;
- record absent GitHub Actions if still absent;
- all acceptance flags false;
- next step after independent audit is one bundled human gate, not another proof-only Goal.

Do not rewrite Goal150A historical dashboard.

Do not perform manual review. Future checklist remains one custom case:

```text
weapon=3
strength=8
per-point=2
level2 XP=12
expected equipment/stat/total=3/6/9
expected level/XP=2/12
```

Equipment zero remains automated and adds no user clicks.

## Allowed paths

Production:

```text
src/LLMGameCreator.Runtime/EncounterRuntimeService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleEffectiveValueExpression.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/FeatureModuleParameterBindingService.cs
```

Runtime scope is only generic equipment metadata presence/event evidence.

Test infrastructure:

```text
tests/LLMGameCreator.Tests/LLMGameCreator.Tests.csproj
tests/LLMGameCreator.Tests/xunit.runner.json
tests/LLMGameCreator.Tests/Goal150BTestCollectionConfiguration.cs
```

New tests:

```text
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal150BZeroValueRuntimeEvidenceTests.cs
tests/LLMGameCreator.Tests/Runtime/Goal150BEquipmentZeroEvidenceRuntimeTests.cs
tests/LLMGameCreator.Tests/Devflow/RunCompleteTestSuiteScriptTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal150BZeroValueRuntimeEvidenceCompleteSuiteClosureScriptTests.cs
```

Scripts:

```text
.devflow/scripts/run-complete-test-suite.ps1
.devflow/scripts/run-complete-test-suite.cmd
.devflow/scripts/run-goal150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix.ps1
.devflow/scripts/run-goal150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix.cmd
```

Task/docs/evidence:

```text
.devflow/artifact-scope/artifact-scope-policy.json
docs/agent-tasks/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.json
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
.llmgc/procedural/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/
.llmgc/exports/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/
```

A diagnostically identified existing test file may be changed only after adding that exact path to artifact scope with evidence. No broad test-tree prefix.

## Forbidden

Unless explicitly allowed:

```text
src/LLMGameCreator.GamePackage/
src/LLMGameCreator.Domain/
src/LLMGameCreator.Generation/
src/LLMGameCreator.AssetPipeline/
src/LLMGameCreator.Scripting/
src/LLMGameCreator.Runtime.Abstractions/
src/LLMGameCreator.WinForms/
unity/
samples/
generator-library/
provider/
LLM/
RAG/
ProjectSettings/
Packages/
```

Also forbidden:

- old Goal149/150/150A artifacts;
- accepted manual project files;
- user workspace files;
- new normal UI page/tab;
- module-ID Runtime/Application dispatch;
- broad formatting/refactoring;
- powerset logic.

Changed generic production C# must not branch on module IDs, parameter IDs, `stat/strength` or `progression/character_level`. These identifiers are allowed only in JSON/tests/docs/evidence.

## Git discipline

Before editing:

```powershell
git status --short
git rev-parse HEAD
git rev-parse origin/main
```

Require:

`HEAD == origin/main == 90f278b1f3a70fdb5011e555491fb83860d00509`

No branch, merge, rebase, cherry-pick, broad reset/clean/restore, or unrelated staging. Stage exact allowlisted paths.

End with mandatory commit/push for GREEN/BLOCKED/safe FAILED, `HEAD == origin/main`, clean worktree.

## Final report

Return exactly GREEN, BLOCKED or FAILED and include:

- commit SHA/message;
- push result;
- HEAD/origin/worktree;
- zero-value cause/result;
- equipment-only 0 and 3;
- combined 3/8/2/12;
- overflow rejection;
- monolithic suite status;
- exhaustive discovered/executed/passed/failed/skipped/missing/duplicate/aborted counts;
- slowest test/shard;
- historical hashes;
- artifact-scope result;
- acceptance flags;
- confirmation no manual review was claimed.

A report without a pushed commit when task-owned changes exist is invalid.
