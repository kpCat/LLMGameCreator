# Goal 150C — Hermetic Adaptive Validation + Goals149/150 Acceptance Readiness Hotfix

## Identity

- Task ID: `goal-150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base commit: `07a8ea17c9ff01a319c1e610b62d020216d0605b`
- Required base message: `BLOCKED Goal 150B zero-value Runtime evidence and complete-suite closure hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.
Do not rely on memory from another Codex dialog.

## Recommended model

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: the architecture and primary defect are already identified. This is a bounded validation-integrity and acceptance-readiness hotfix. It requires careful repository/test isolation, but does not require a new gameplay architecture or public schema design.

Escalate to Sol only in a later separate task if the hermetic rerun proves a genuinely new cross-cutting P0/P1 product defect whose root cause remains unknown.

## Current state

Goal149 added the first real optional equipment module.

Goal150 added default-off character attributes and level progression.

Goal150A added an immutable effective FeatureModule catalog synchronizing:

```text
typed parameter values
package mutation values
Runtime-effect expected values
Runtime playthrough arguments
```

Goal150B fixed:

```text
equipment-only combat_damage_bonus=0 evidence
equipment-only total additional damage reporting
decimal expression overflow rejection
```

Goal150B focused results are GREEN:

```text
equipment-only 0 -> equipmentDamageBonus=0, no stat evidence
equipment-only 3 -> equipment/stat/total = 3/0/3
custom 3/8/2/12 -> equipment/stat/total = 3/6/9
custom progression -> level/XP = 2/12
Goal150B focused tests = 5/5
Goal150A parameterized tests = 5/5
Goal149 focused runner = 7/7
Goal150 focused runner = 1/1
```

Historical hashes remain exact:

### Modules disabled

```text
composition=e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
activated=c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb
final=95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
actions=13/8/13
```

### Equipment default 2

```text
composition=94a47ab896b425a76c2e523acef3ab87d538bb8f0c754b2402b0127e5ad82bf5
activated=147f88ac026f006ab5fbe93dc6c7cb039e85189fcb3421a71a1fd99284d3a5c1
final=51bba1ffada4ce9ffccfa9132e7e7c007afcbcec8632d7de13d26ce961b3ea0d
actions=17/13/17
```

### All optional defaults

```text
composition=ba9dbf32c8e79d4e2bf37116dd611cc7eccd7bee73f880aefeb041cce4b2ee40
activated=19e837b8d4925b0b567c52adfb93905bc44ac6e9a13d3008726ff1be89ea49cf
final=ebb05a61036ddfde40b605267685ba8ab90baa01ed3b5efbb815615ae26eca5c
actions=20/16/20
```

### Goal150A custom

```text
composition=66c6fa980123ad113a6b37e7d6d31b13d946b48df325230a44bac351660c0db3
activated=578aa5b7b40b87015897c762cb651ef6e61f3a190e5e66fd80a6c1dd79664391
final=5f367569870cd8290225e06bba3570b8185c157febb823d444ff9cfa27def09e
playthroughSignature=9bbcc1573999aa3a82a257bf0c8e2d95ed8453574e8a4a7b1d91042146a01050
```

Acceptance remains:

```text
goal149Accepted=false
goal150Accepted=false
goal150aAccepted=false
goal150bAccepted=false
acceptedByCodex=false
manualReviewPerformed=false
```

Do not perform or claim human acceptance in this Goal.

## Why Goal150B remains blocked

Goal150B introduced `run-complete-test-suite.ps1` and recorded:

```text
discovered=1736
assigned=1736
executed=1715
passed=1651
failed=64
skipped=0
missing=21
duplicate=0
aborted=4
```

The result is useful diagnostic evidence, but it is not acceptance-grade for four reasons.

### 1. It was not executed against the final committed tree

Several failures say that `docs/CONTEXT_INDEX.md` does not contain:

```text
goal150b_independent_audit_then_bundled_human_gate
```

The final Goal150B commit does contain that marker.

Therefore at least part of the sharded evidence was produced before the final docs/current-state edits and does not describe the exact committed tree.

### 2. Shards were not hermetic

The runner executes all class shards sequentially in the same repository worktree.

Historical tests and ProductSmoke tests can read and write fixed locations such as:

```text
.llmgc/procedural/
.llmgc/exports/
.devflow/runs/
project-local smoke roots
```

The runner records changed `.llmgc` paths only after execution; it does not restore a clean baseline before each shard.

Representative failures report many unrelated historical gates becoming `BLOCKED`/false in later shards. That is consistent with cross-test artifact contamination and must be separated from real product regressions.

### 3. It bypasses the repository's established isolation paths

Existing `check-all.ps1`:

- isolates ProductSmoke project/package roots;
- seeds a controlled procedural baseline;
- runs non-ProductSmoke tests separately.

Existing `run-product-smoke.ps1` sets:

```text
LLMGC_PRODUCT_SMOKE_PROJECT_DIR
LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR
```

The Goal150B complete runner directly launches all test classes without equivalent per-shard isolation.

### 4. Timeout policy is not adaptive

Every class received the same 180-second limit.

Known heavy classes can legitimately exceed that limit. Four timed-out shards account for 21 missing tests.

A timeout must trigger recursive split or isolated retry with measured allowance, not immediate classification as missing product coverage.

## Primary objective

Produce acceptance-grade validation for the exact final Goal150C commit while preserving all Goal149/150/150A/150B product behavior.

Required final decision:

```text
either:
  all relevant tests GREEN in hermetic execution
  -> Goals149/150/150A/150B are ready for one bundled human gate

or:
  a small exact set of reproducible real product regressions remains
  -> return BLOCKED with one classified list and no human gate
```

Do not repair tests merely to make counts green.
Do not update expected status from GREEN to BLOCKED unless the historical contract is demonstrably obsolete and superseded.
Do not weaken assertions.
Do not treat artifact contamination as a product defect.
Do not treat a real product defect as harmless historical noise.

## Command and investigation budget

This budget is mandatory.

### Read budget

Before the first edit, read no more than 12 primary files:

```text
AGENTS.md
this GOAL.md
docs/CONTEXT_INDEX.md relevant Goal149-150C section only
docs/CURRENT_GENERATOR_STATE.md first active section only
docs/AUTOMATED_VALIDATION_TIERS.md
.devflow/scripts/run-complete-test-suite.ps1
.devflow/scripts/check-all.ps1
.devflow/scripts/run-product-smoke.ps1
Goal150B dashboard/report/slowest summary
up to three representative failed test files
```

Use targeted `rg`, exact ranges and existing Goal150B inventory for everything else.

Do not read the full multi-thousand-line current-state history.

### Command budget

- Never repeat the unchanged monolithic `dotnet test` command.
- Do not run another blind 15/30/60-minute monolithic attempt.
- An unchanged failing command may be repeated only after code/configuration changes or with new isolation/instrumentation.
- Normal focused commands: maximum 5 minutes.
- Normal shard: maximum 5 minutes.
- A known measured heavy single class/test may receive up to 8 minutes.
- Total final exhaustive validation wall-clock budget: maximum 35 minutes.
- Maximum simultaneous testhost processes: 2.
- Retry only failing/aborted shards, never all successful shards.
- Stop recursive splitting when one exact test remains.
- Do not launch more than one full exhaustive pass after final code/docs are frozen.
- Raw logs remain outside committed evidence under `.devflow/runs`.
- Do not spend tokens narrating every shard. Record machine-readable progress and summarize only classifications.

### Hypothesis discipline

Start from existing Goal150B failure inventory.

Classify before editing:

```text
A. stale pre-final-tree evidence
B. cross-shard artifact contamination
C. missing required environment/isolation
D. timeout requiring split
E. genuinely reproducible product regression
F. genuinely stale historical assertion
```

No test or product file may be changed until its failure reproduces in a clean isolated environment at final HEAD.

## Required implementation

### A. Replace the complete-suite runner with a hermetic adaptive runner

Update:

```text
.devflow/scripts/run-complete-test-suite.ps1
.devflow/scripts/run-complete-test-suite.cmd
```

The command name may remain for compatibility, but behavior must become hermetic.

### Clean immutable source baseline

Run validation from a disposable validation worktree or equivalent exact clean snapshot at current HEAD.

Preferred:

```text
git worktree add --detach <temp-validation-root> HEAD
```

Requirements:

- never alter branch refs;
- never commit from the validation worktree;
- never use the user's main worktree as a mutable shard workspace;
- delete the disposable worktree after validation;
- use exact cleanup commands only inside the disposable worktree;
- preserve the user's main worktree and local project folders.

If `git worktree` cannot be used, create a byte-exact repository snapshot excluding `.git`, `bin`, `obj`, `.devflow/runs` and user workspace roots, and prove source hashes match HEAD.

### Per-shard reset

Before every shard:

1. restore the disposable worktree to exact HEAD;
2. remove only untracked/generated files inside allowlisted disposable output roots;
3. create unique:
   - ProductSmoke project root;
   - ProductSmoke package-output root;
   - test results root;
   - temp root when supported;
4. seed the procedural baseline exactly as established by `check-all.ps1`;
5. set required environment variables;
6. run the shard;
7. retain raw output under the external `.devflow/runs/<run>/...` directory;
8. discard disposable worktree mutations before the next shard.

Do not use broad reset/clean in the user's main worktree.
Broad reset/clean is permitted only in the disposable detached validation worktree and only because it is intentionally throwaway.

### Separate validation lanes

Do not mix all tests into one undifferentiated lane.

Required lanes:

#### Lane N — non-ProductSmoke

Use the repository's existing authoritative semantics:

```text
FullyQualifiedName!~ProductSmoke
```

Partition adaptively only when needed.

#### Lane P — ProductSmoke

Run ProductSmoke tests with isolated environment roots.

Prefer existing scenario manifests and `run-product-smoke.ps1` where a scenario exists.

For ProductSmoke tests without a scenario manifest, invoke the exact test with the same isolated environment contract.

#### Lane F — focused current product

Run Goals149/150/150A/150B focused tests and wrappers separately.

The historical Goal150A wrapper is allowed to return its committed historical `BLOCKED` result; this is not a current regression. Its focused tests must pass.

#### Lane S — current spine

Run:

```text
check-current-goal.ps1
check-spine-fast.ps1
```

### Adaptive partitioning

Initial partition should be by deterministic namespace/class groups, not one class per shard.

On fail or timeout:

1. rerun that group in a fresh baseline;
2. split the group in half;
3. recurse until exact failing classes/tests are identified;
4. for a single timeout test, retry once with up to 8 minutes if historical timing justifies it;
5. classify the exact outcome.

Successful groups are never rerun during the same final pass.

### Coverage accounting

Discovery must be performed on the frozen final tree.

Produce exact counts:

```text
discovered
assigned
executed
passed
failed
skipped
missing
duplicate
aborted
```

Every discovered test must be represented exactly once in final accounting.

A test executed during diagnostic split must not be double-counted. Final result should use one terminal result per discovered test.

Required GREEN:

```text
missing=0
duplicate=0
failed=0
aborted=0
```

### Failure taxonomy

For every initially failing Goal150B test, record:

```text
original shard/test
original failure
clean isolated outcome
classification
final outcome
changed path, if any
```

The taxonomy must demonstrate whether the original 64 failures were:

- stale evidence;
- artifact contamination;
- missing environment;
- timeout;
- real regression;
- stale assertion.

## B. Repair only cleanly reproduced defects

### Real product regression

If a test still fails against a clean baseline and the current product violates a valid historical/current contract:

- fix the narrow product cause;
- add focused regression coverage;
- rerun only affected shards;
- preserve generic architecture.

### Stale historical assertion

A historical test may be updated only when:

- it asserts an obsolete current-next-goal/status token rather than historical behavior;
- the superseding contract is explicit in current-state;
- the test still verifies the original historical achievement;
- the edit replaces moving-current-state coupling with a stable historical assertion.

Do not mechanically replace `"GREEN"` with `"BLOCKED"`.
Do not remove checks for accepted historical gates.

### Fixed artifact contamination

When a test incorrectly depends on shared mutable repository roots:

- prefer injection/environment-isolated roots;
- avoid modifying accepted historical artifacts;
- avoid requiring a particular test execution order.

## C. Correct current-state routing

The current Goal150B state incorrectly says the next step is a bundled human gate despite:

```text
failed=64
missing=21
aborted=4
```

Until hermetic validation is GREEN, current state must say:

```text
nextProductGoal=complete_goal150c_hermetic_validation_then_independent_audit
manualGateReady=false
```

Only if Goal150C is GREEN may it become:

```text
nextAction=independent_goal150c_audit_then_one_bundled_human_gate
manualGateReady=true
```

All acceptance flags remain false until the owner actually performs the gate.

## D. Compact evidence policy

Goal150B committed roughly 1,839 evidence files, including duplicated raw logs and TRX files.

Goal150C must not repeat this.

### Commit only compact artifacts

Under both procedural/export roots commit at most:

```text
goal150c-dashboard.json
validation-discovery-summary.json
validation-lane-plan.json
validation-result.json
validation-failure-taxonomy.json
validation-slowest-summary.json
focused-regression-proof.json
historical-hash-regression-proof.json
artifact-scope-proof.json
publication-policy-proof.json
goal150c-file-index.json
goal150c-report.md
```

Target: no more than 12 files per root.

Raw logs, TRX and per-shard command output remain only in:

```text
.devflow/runs/goal150c-*/
```

They must be ignored/untracked and referenced from compact summaries by relative local path.

### Goal150B bulk evidence cleanup

Remove tracked Goal150B raw diagnostics from both old artifact roots:

```text
logs/**
trx/**
dotnet-info.txt
```

Do not delete Goal150B compact dashboard/report/root-cause/proofs/results.

Before deletion, write in Goal150C evidence:

```text
sourceCommit=07a8ea17c9ff01a319c1e610b62d020216d0605b
removedRawDiagnosticFileCount
removedRawDiagnosticAggregateBytes
retainedCompactGoal150BFiles
historicalRetrievalAvailableViaGit=true
```

Do not rewrite Goal150B compact proof contents or pretend Goal150B was GREEN.

If deleting old tracked raw diagnostic files would cause a protected historical-integrity guard to fail, update that guard narrowly to distinguish compact immutable proof from removable diagnostic retention. Do not weaken gameplay/hash protections.

## Required focused preservation checks

Re-run and preserve:

```text
Goal150B equipment-only 0 and 3
Goal150B overflow
Goal150A 3/8/2/12
Goal149 equipment default
Goal150 attributes/progression defaults
checkpoint reload
full replay
action binding
project identity
transactional activation
incremental certification
```

## Historical hash requirements

All four hash groups from Current state must remain exact.

Any mismatch is P1 and blocks human acceptance.

## Required commands

Use existing exact names where they differ.

### T0

```powershell
dotnet build
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150C"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150B"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests"
```

### Focused regressions

```powershell
.\.devflow\scripts\run-capability-driven-runtime-playthrough-equipment-featuremodule-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
```

Run the Goal150A/150B wrapper only as historical-status diagnostics. Do not require the old Goal150A dashboard to become GREEN.

### Current validation

```powershell
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\run-complete-test-suite.ps1
```

No raw monolithic test command.

## Required tests for the runner itself

Add tests proving:

1. validation executes from a disposable exact-HEAD snapshot;
2. main worktree files remain byte-identical;
3. a shard mutation does not reach the next shard;
4. ProductSmoke gets unique project/package roots;
5. final discovery is performed after docs/code are frozen;
6. recursive splitting identifies one failing test without rerunning successful groups;
7. timeout split works;
8. terminal result accounting has no duplicates;
9. raw logs/TRX remain under ignored `.devflow/runs`;
10. compact artifact file count is bounded;
11. current-state route cannot claim manual readiness when validation is incomplete.

## Artifact scope

Create a Goal150C scenario in:

```text
.devflow/artifact-scope/artifact-scope-policy.json
```

Initially allowed:

```text
.devflow/scripts/run-complete-test-suite.ps1
.devflow/scripts/run-complete-test-suite.cmd
.devflow/scripts/run-goal150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix.ps1
.devflow/scripts/run-goal150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix.cmd
.devflow/artifact-scope/artifact-scope-policy.json

tests/LLMGameCreator.Tests/Devflow/RunCompleteTestSuiteScriptTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal150CHermeticAdaptiveValidationTests.cs

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md

docs/agent-tasks/goal-150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix/
.llmgc/procedural/goal-150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix/
.llmgc/exports/goal-150c-hermetic-adaptive-validation-and-acceptance-readiness-hotfix/
```

Allowed deletion prefixes only:

```text
.llmgc/procedural/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/logs/
.llmgc/procedural/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/trx/
.llmgc/exports/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/logs/
.llmgc/exports/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/trx/
```

Allowed exact old files for deletion:

```text
.llmgc/procedural/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/dotnet-info.txt
.llmgc/exports/goal-150b-zero-value-runtime-evidence-and-complete-suite-closure-hotfix/dotnet-info.txt
```

### Exact-path expansion policy

When a clean isolated rerun proves a real product/test defect:

1. record it in the failure taxonomy;
2. add only the exact required file path to artifact scope;
3. explain the classification;
4. edit only that exact path;
5. do not add broad production/test prefixes.

## Forbidden shortcuts

- no raw monolithic full-suite repeat;
- no test deletion or skip;
- no weaker assertions;
- no status replacement solely to get GREEN;
- no fake TRX/result generation;
- no reuse of dirty shard workspaces;
- no dependence on test order;
- no branch/merge/rebase/cherry-pick;
- no broad reset/clean in the user's main worktree;
- no new gameplay mechanic;
- no new normal UI page/tab;
- no Runtime/GamePackage schema/provider/LLM/RAG/Lua/Unity change unless a clean reproducible P1 explicitly proves it necessary and a later separate task authorizes it.

## Status-aware publication policy

The owner must not commit or push manually.

### GREEN

Commit:

```text
GREEN Goal 150C hermetic adaptive validation and acceptance readiness hotfix
```

### BLOCKED

If coherent runner/current-state improvements are complete but exact reproducible failures remain:

```text
BLOCKED Goal 150C hermetic adaptive validation and acceptance readiness hotfix
```

Commit and push the coherent BLOCKED state.

### FAILED

Do not push known broken product code.
Restore exact broken task-owned production paths, retain safe diagnostics, then commit/push:

```text
FAILED Goal 150C hermetic adaptive validation and acceptance readiness hotfix
```

For all statuses with task-owned changes:

```powershell
git commit
git push origin main
git rev-parse HEAD
git rev-parse origin/main
git status --short
```

Required final state:

```text
HEAD == origin/main
worktree clean
```

A report without a pushed commit when task-owned changes exist is invalid.

## Dashboard

GREEN requires:

```text
status=GREEN
validatedCommit=<final Goal150C commit/tree identity or documented pre-commit content identity>
validationSnapshotMatchesFinalSources=true
hermeticSnapshot=true
mainWorktreeUnchangedByValidation=true
nonProductLanePassed=true
productSmokeLanePassed=true
focusedLanePassed=true
spineLanePassed=true
discovered=assigned=executed
failed=0
missing=0
duplicate=0
aborted=0
initialGoal150BFailureCount=64
allInitialFailuresClassified=true
goal149HashesPreserved=true
goal150HashesPreserved=true
goal150aCustomHashesPreserved=true
goal149Accepted=false
goal150Accepted=false
goal150aAccepted=false
goal150bAccepted=false
goal150cAccepted=false
acceptedByCodex=false
manualReviewPerformed=false
manualGateReady=true
passed=true
```

If any exact real regression remains:

```text
status=BLOCKED
manualGateReady=false
passed=false
```

## Final report

Return exactly one status:

```text
GREEN
BLOCKED
FAILED
```

Include:

- commit SHA/message;
- push result;
- `HEAD == origin/main`;
- clean worktree;
- model actually used;
- command budget compliance;
- original Goal150B counts;
- final hermetic counts;
- classification totals by A/B/C/D/E/F;
- exact remaining failures, if any;
- lanes and durations;
- slowest terminal tests;
- Goal149/150/150A/150B focused results;
- historical hashes;
- removed raw diagnostic file count/bytes;
- committed compact artifact count;
- artifact-scope violations;
- manualGateReady;
- acceptance flags;
- confirmation no human acceptance was claimed.
