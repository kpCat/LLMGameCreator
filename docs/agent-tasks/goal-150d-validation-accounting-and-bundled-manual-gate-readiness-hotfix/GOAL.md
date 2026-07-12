# Goal 150D — Validation Accounting + Bundled Manual Gate Readiness Hotfix

## Identity

- Task ID: `goal-150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `29d8b50493f8af416c71a086f82af7816870ee37`
- Base message: `BLOCKED Goal 150C hermetic adaptive validation and acceptance readiness hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

The architecture and defects are already identified. Escalate to Sol only in a later task if a clean exact rerun proves a new unknown cross-cutting P0/P1.

## Pre-approval

The owner approved execution by launching this task.

- Produce an internal plan.
- Do not ask for plan confirmation.
- If base/worktree checks pass, start immediately.
- Ask no clarifying questions for conditions specified here.

## Product state to preserve

Focused GREEN behavior:

```text
equipment-only 0:
  equipmentDamageBonus=0
  no stat evidence

equipment-only 3:
  equipment/stat/total=3/0/3

custom 3/8/2/12:
  equipment/stat/total=3/6/9
  level/XP=2/12
```

Exact hashes:

```text
disabled:
e78356e5c35b777098fea4db22095419aacd69129da012f8ed72168330410221
c46826d8231951ab941f6ee1608d30273b1e186f920ea8cad58c58c25317eeeb
95d1122906521b5ebfbaf85c10061b4e2017c3a4084edf256221e878d30756b8
13/8/13

equipment default 2:
94a47ab896b425a76c2e523acef3ab87d538bb8f0c754b2402b0127e5ad82bf5
147f88ac026f006ab5fbe93dc6c7cb039e85189fcb3421a71a1fd99284d3a5c1
51bba1ffada4ce9ffccfa9132e7e7c007afcbcec8632d7de13d26ce961b3ea0d
17/13/17

all optional defaults:
ba9dbf32c8e79d4e2bf37116dd611cc7eccd7bee73f880aefeb041cce4b2ee40
19e837b8d4925b0b567c52adfb93905bc44ac6e9a13d3008726ff1be89ea49cf
ebb05a61036ddfde40b605267685ba8ab90baa01ed3b5efbb815615ae26eca5c
20/16/20

Goal150A custom:
66c6fa980123ad113a6b37e7d6d31b13d946b48df325230a44bac351660c0db3
578aa5b7b40b87015897c762cb651ef6e61f3a190e5e66fd80a6c1dd79664391
5f367569870cd8290225e06bba3570b8185c157febb823d444ff9cfa27def09e
9bbcc1573999aa3a82a257bf0c8e2d95ed8453574e8a4a7b1d91042146a01050
```

All acceptance flags remain false. Do not perform or claim human acceptance.

## Independent audit findings

### P1-A — runner deletes tracked baseline

Goal150C resets the disposable worktree and then deletes:

```text
.llmgc/procedural
.llmgc/exports
```

This removes tracked historical artifacts. The ProductSmoke baseline copy then has no source. Accepted historical source/artifact tests fail because the runner destroyed their inputs.

Required:

- preserve every tracked candidate file;
- clean only untracked disposable output;
- never delete tracked `.llmgc/procedural` or `.llmgc/exports`;
- create unique external ProductSmoke project/package/test/temp/log roots.

### P1-B — false `executed` accounting

Goal150C recorded:

```text
discovered=1734
executed=1734
passed=97
failed=18
aborted=1619
```

`executed` was actually `$terminal.Count`; 1619 not-run tests were marked Aborted after the wall-clock budget expired.

Required fields:

```text
discovered
assigned
attempted
executed
passed
failed
skipped
notRun
timedOut
missing
duplicate
```

Definitions:

- attempted: included in a launched terminal command;
- executed: terminal TRX result exists;
- notRun: never produced a terminal TRX result;
- timedOut: exact launched terminal test exceeded timeout;
- missing: reconciliation defect.

Required invariants:

```text
executed = passed + failed + skipped
discovered = executed + notRun + timedOut
```

Never count not-run tests as executed.

### P1-C — validation identity is not remote-reachable final code

Goal150C claims validation of `74b36ce2...`, while `origin/main` is `29d8b504...`. The validation SHA is not the final published commit.

Required two-commit workflow:

1. create a validation-candidate commit with all executable source, scripts, tests and pre-result docs;
2. validate that exact candidate from detached worktree;
3. create final status/evidence commit on top;
4. final commit may change only compact evidence/publication/result-routing docs;
5. push both commits;
6. prove candidate is an ancestor of `origin/main`;
7. run T0/current/spine guards at final commit.

Candidate message:

```text
VALIDATION CANDIDATE Goal 150D validation accounting and manual gate readiness
```

Final message:

```text
GREEN|BLOCKED|FAILED Goal 150D validation accounting and bundled manual gate readiness hotfix
```

Do not amend the candidate after validation.

### P1-D — original 64 failures were not classified

Goal150C labelled all 64 Goal150B failures as timeout because most were never reached.

Required:

- extract exact 64 failed identities from commit `07a8ea17c9ff01a319c1e610b62d020216d0605b`;
- extract exact 21 missing identities from the same commit;
- build a deduplicated closure manifest;
- cleanly run every manifest identity;
- classify only after real result or exact-test timeout;
- notRun remains unclassified and blocks readiness.

## Validation strategy

Do not run all historical ProductSmoke tests again.

Acceptance-readiness gate:

1. Goals149/150/150A/150B focused tests;
2. `check-current-goal.ps1`;
3. `check-spine-fast.ps1`;
4. one authoritative isolated non-ProductSmoke full gate via `check-all.ps1`;
5. one hermetic rerun of exact Goal150B closure set: 64 failed + 21 missing;
6. no blind all-ProductSmoke sweep.

If these pass and hashes remain exact, set `manualGateReady=true`, but do not set accepted=true.


## Command and investigation budget

### Read budget

Before editing read at most 10 primary files:

```text
AGENTS.md
this GOAL.md
active CURRENT_GENERATOR_STATE section
AUTOMATED_VALIDATION_TIERS.md
run-complete-test-suite.ps1
check-all.ps1
run-product-smoke.ps1
Goal150C dashboard/result/taxonomy
Goal150B compact complete-suite result
one representative artifact-dependent test
```

Use targeted `rg`, exact ranges and `git show`; do not read full historical state/queue/debt files.

### Command budget

- no raw monolithic all-test command;
- no second all-ProductSmoke sweep;
- no rerun of successful closure tests;
- focused command timeout: 5 minutes;
- one `check-all.ps1 -SkipRestore`, maximum 25 minutes;
- closure manifest maximum 20 minutes;
- exact heavy test timeout: maximum 8 minutes;
- maximum total validation wall clock: 45 minutes;
- maximum two simultaneous testhost processes;
- rerun an exact failure only after a new isolation hypothesis or code change;
- no unchanged 15→30→60-minute escalation;
- raw logs/TRX stay under ignored `.devflow/runs`;
- commit compact evidence only.

## Required implementation

### A. Correct disposable reset

`Reset-DisposableWorktree` must restore exact candidate commit and preserve tracked files.

Broad reset/clean is permitted only inside the throwaway detached worktree. Clean only explicit untracked output roots. Never use broad reset/clean in the user's main worktree.

### B. Targeted closure mode

Update `run-complete-test-suite.ps1` with a bounded mode equivalent to:

```text
Mode=Goal150AcceptanceClosure
ManifestPath=<json>
```

Manifest entry:

```json
{
  "testName": "...",
  "source": "goal150b_failed|goal150b_missing",
  "lane": "N|P"
}
```

Requirements:

- exact unique identities;
- final candidate discovery contains every manifest identity;
- unique ProductSmoke environment roots;
- terminal result per identity;
- split grouped commands only when needed;
- exact timeout only after exact test launch;
- no fabricated execution.

### C. Extract closure from Git history

Read old diagnostics through Git history only:

```powershell
git show 07a8ea17c9ff01a319c1e610b62d020216d0605b:<path>
```

Do not restore old raw files into main.

Record:

```text
sourceFailedCount=64
sourceMissingCount=21
uniqueClosureCount=<deduplicated>
```

### D. True taxonomy

Categories:

```text
A stale pre-final-tree evidence
B cross-shard artifact contamination
C missing environment/isolation
D real exact-test timeout
E genuine reproducible product regression
F stale historical assertion
```

Rules:

- pass after preserving tracked baseline → B or C;
- old moving current-state token but stable historical achievement → A or F;
- clean failure of valid contract → E;
- exact launched test over 8 minutes → D;
- notRun → unclassified and blocking.

`allClosureTestsClassified=true` only when every manifest identity has a real terminal result/classification.

### E. Repair only exact reproduced defects

Do not edit tests in bulk.

A test/product path may change only after exact clean reproduction. Add only that exact path to artifact scope. Never replace GREEN with BLOCKED merely to pass. Preserve accepted historical checks.

Prefer existing ProductSmoke scenario runner when a scenario contract exists.

### F. Correct result semantics

Closure result fields:

```text
manifestCount
attempted
executed
passed
failed
skipped
notRun
timedOut
missing
duplicate
```

GREEN closure:

```text
attempted=manifestCount
executed=manifestCount
passed=manifestCount
all other counts=0
```

Record non-ProductSmoke gate separately; do not merge counts into misleading totals.

### G. Manual gate readiness

GREEN automated result means:

```text
manualGateReady=true
```

It does not mean accepted.

Keep:

```text
goal149Accepted=false
goal150Accepted=false
goal150aAccepted=false
goal150bAccepted=false
acceptedByCodex=false
manualReviewPerformed=false
```

Future single user gate uses:

```text
weapon=3
strength=8
per-point=2
level2 XP=12
expected equipment/stat/total=3/6/9
expected level/XP=2/12
```

Equipment-zero remains automated.

## Required commands

Candidate T0:

```powershell
dotnet build
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150D"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150C"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150B"
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests"
```

Focused mechanics:

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
```

Repository gates:

```powershell
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
.\.devflow\scripts\check-all.ps1 -SkipRestore
```

Run the new exact closure mode once against the candidate commit.

After final status/evidence commit:

```powershell
dotnet build
dotnet test ... --filter "FullyQualifiedName~Goal150D"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
```

Do not rerun successful full/closure lanes after evidence-only final commit.

## Candidate/final source equivalence

Hash executable/validation-authority files:

```text
src/**
tests/**
.devflow/scripts/**
.devflow/artifact-scope/**
catalogs/**
docs/CURRENT_GENERATOR_STATE.*
docs/CONTEXT_INDEX.md
docs/manual-acceptance/**
```

Exclude Goal150D compact evidence. Record candidate/final manifest hashes and `sourceEquivalent=true`.

If result-value docs must differ after validation, exclude only explicit result fields and run final consistency tests.

## Compact evidence

Maximum 12 files per root:

```text
goal150d-dashboard.json
validation-candidate-proof.json
source-equivalence-proof.json
goal150b-closure-manifest-summary.json
goal150b-closure-result.json
goal150b-closure-taxonomy.json
non-product-full-gate-proof.json
focused-regression-proof.json
historical-hash-regression-proof.json
artifact-scope-proof.json
goal150d-file-index.json
goal150d-report.md
```

Raw logs/TRX remain ignored. Do not rewrite Goal150B/150C historical BLOCKED evidence.

## Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-complete-test-suite.ps1
.devflow/scripts/run-complete-test-suite.cmd
.devflow/scripts/run-goal150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix.ps1
.devflow/scripts/run-goal150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix.cmd

tests/LLMGameCreator.Tests/Devflow/RunCompleteTestSuiteScriptTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal150CHermeticAdaptiveValidationTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal150DValidationAccountingTests.cs

docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md

docs/agent-tasks/goal-150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix/
.llmgc/procedural/goal-150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix/
.llmgc/exports/goal-150d-validation-accounting-and-bundled-manual-gate-readiness-hotfix/
```

For a clean reproduced defect: record it, add only the exact path, explain, then edit.

## Forbidden

- no new gameplay mechanic;
- no Runtime/GamePackage/schema/Unity/provider/LLM/RAG/Lua change unless exact E-class P1 is proven and exact scope is updated;
- no deleted/skipped/weakened tests;
- no status rewriting for convenience;
- no fake results;
- no notRun counted as executed;
- no tracked baseline deletion;
- no raw full ProductSmoke sweep;
- no broad reset/clean in main;
- no branch/merge/rebase/cherry-pick/history rewrite;
- no Goal-number normal UI.

## Status-aware publication

The owner must not push manually.

Create candidate commit, validate it, then create final status commit. Push both together.

Final messages:

```text
GREEN Goal 150D validation accounting and bundled manual gate readiness hotfix
BLOCKED Goal 150D validation accounting and bundled manual gate readiness hotfix
FAILED Goal 150D validation accounting and bundled manual gate readiness hotfix
```

End:

```powershell
git push origin main
git rev-parse HEAD
git rev-parse origin/main
git status --short
git merge-base --is-ancestor <candidate> origin/main
```

Require:

```text
HEAD == origin/main
candidate reachable
worktree clean
```

Only concrete Git/auth failure permits no push; retry once and never ask the owner to push.

## GREEN criteria

```text
candidate reachable=true
sourceEquivalent=true
focused Goals149/150/150A/150B=true
current-goal=true
spine-fast=true
non-ProductSmoke full gate=true
closure complete=true
closure notRun/timedOut/failed/missing/duplicate=0
all closure tests classified=true
historical hashes preserved=true
artifact scope=true
manualGateReady=true
all acceptance flags=false
manualReviewPerformed=false
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- candidate SHA/message;
- final SHA/message;
- push and ancestry;
- HEAD/origin/worktree;
- command-budget compliance;
- original 64 failed/21 missing/unique closure counts;
- closure result counts;
- A/B/C/D/E/F totals;
- exact remaining failures/timeouts/notRun;
- non-ProductSmoke gate status/counts;
- focused/current/spine results;
- source equivalence hashes;
- historical hashes;
- compact artifact count/scope violations;
- manualGateReady;
- acceptance flags;
- confirmation no human acceptance claimed.
