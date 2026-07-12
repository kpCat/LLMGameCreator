# Goal 150E — Historical Test Identity Reconciliation + Bundled Manual Gate Readiness Hotfix

## Identity

- Task ID: `goal-150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `cd5a2aa910790755cd0e0470fabdf3b85d61f201`
- Base message: `BLOCKED Goal 150D validation accounting and bundled manual gate readiness hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: High
```

Reason: the gameplay architecture is already correct. The remaining blocker is precise historical/current xUnit identity reconciliation plus a bounded targeted validation closure.

## Pre-approval

The owner approved execution by launching this task.

- Produce a concise internal plan.
- Do not ask the owner to confirm it.
- After base/worktree checks, begin immediately.
- Ask no clarifying question already answered here.

## Current product state

Goals149/150/150A/150B gameplay behavior remains focused GREEN:

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

Preserve exact hashes:

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

all optional:
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

## Independent audit of Goal150D

Goal150D published four candidate commits instead of one and then a final commit containing only `FINAL_STATUS.md`.

The current blocker is not proof of a gameplay regression:

```text
historical Goal150B failed identities = 64
historical Goal150B missing identities = 21
historical unique closure identities = 85
candidate discovery did not contain every historical full display identity
```

### Root cause: historical display identity is not a stable current identity

The wrapper compares full historical TRX/list-test strings with current `--list-tests` output using exact string equality.

Test identities may drift because of:

- renamed test method;
- changed xUnit theory display formatting;
- changed argument rendering;
- intentionally replaced validation contract.

Concrete repository example:

```text
Goal150B:
LLMGameCreator.Tests.Devflow.RunCompleteTestSuiteScriptTests.
Complete_suite_runner_declares_exhaustive_disjoint_bounded_contract

Current:
LLMGameCreator.Tests.Devflow.RunCompleteTestSuiteScriptTests.
Complete_suite_runner_declares_hermetic_adaptive_bounded_contract
```

The test coverage was replaced/renamed; the old full identity cannot be required to appear literally in current discovery.

### Additional current runner defects

1. Closure discovery writes `discovered=$manifest.Count` even when some manifest identities are absent from candidate discovery.
2. `attempted=$attempts.Count` counts commands, not unique tests included in launched commands.
3. `timedOut` counts unique classes from timed-out attempts rather than exact unresolved test identities.
4. A full identity filter uses `FullyQualifiedName~Class.Method`, but result reconciliation still expects the old full display identity.
5. Goal150D final status did not update current-state source-of-truth or compact evidence.
6. The candidate workflow produced multiple `VALIDATION CANDIDATE` commits. This Goal permits exactly one candidate commit.

## Primary objective

Resolve every historical Goal150B closure identity to an executable current test contract, run the bounded closure, then complete acceptance-readiness validation.

Required final result is either:

```text
GREEN:
  all historical identities resolved or validly superseded
  all mapped current tests pass
  non-ProductSmoke gate passes
  focused/current/spine gates pass
  hashes preserved
  manualGateReady=true
```

or:

```text
BLOCKED:
  exact unresolved identities, exact failures or exact timeouts listed
  manualGateReady=false
```

No broad historical ProductSmoke sweep.

## Command and investigation budget

### Read budget

Before editing read at most 8 primary files:

```text
AGENTS.md
this GOAL.md
active CURRENT_GENERATOR_STATE section
run-complete-test-suite.ps1
run-goal150d...ps1
RunCompleteTestSuiteScriptTests.cs at Goal150B and current
Goal150D FINAL_STATUS.md
AUTOMATED_VALIDATION_TIERS.md
```

Use targeted `rg`, `git show` and exact source ranges for everything else.

### Command budget

- no raw monolithic all-test;
- no all-ProductSmoke sweep;
- reconciliation preflight: maximum 3 minutes;
- focused command: maximum 5 minutes;
- exact closure execution: maximum 20 minutes;
- one `check-all.ps1 -SkipRestore`: maximum 25 minutes;
- total validation budget: maximum 45 minutes;
- exact heavy test timeout: maximum 8 minutes;
- maximum two testhost processes;
- rerun only exact failures after a new hypothesis/code change;
- successful closure tests are never rerun;
- raw logs/TRX remain under ignored `.devflow/runs`;
- compact committed evidence only.

## Required workflow

### Phase 1 — uncommitted reconciliation preflight

Before creating a candidate commit:

1. extract the historical Goal150B closure identities through Git history;
2. run current test discovery from the current working tree;
3. construct a reconciliation map;
4. run Goal150E contract tests;
5. produce a raw untracked preflight report under `.devflow/runs`.

Do not create the validation-candidate commit until:

```text
historicalSourceCount=85
unresolvedHistoricalIdentityCount=0
ambiguousHistoricalIdentityCount=0
```

This phase may not execute the expensive closure tests.

### Phase 2 — exactly one validation candidate

After preflight passes, create exactly one commit:

```text
VALIDATION CANDIDATE Goal 150E historical test identity reconciliation and manual gate readiness
```

Do not create a second candidate commit.

If a runner defect is discovered after candidate validation begins:

- do not amend or create another candidate;
- publish final BLOCKED status;
- defer code correction to a new task.

### Phase 3 — validate exact candidate

From a detached disposable worktree at the candidate commit:

1. run mapped closure once;
2. run focused Goals149/150/150A/150B;
3. run current-goal and spine-fast;
4. if closure is GREEN, run one `check-all.ps1 -SkipRestore`;
5. preserve exact hashes.

### Phase 4 — final status commit

Create one final GREEN/BLOCKED/FAILED commit on top.

The final commit may change only:

- compact Goal150E evidence;
- current-state/queue/gate/risk/manual-readiness docs;
- final publication proof/status file.

Push candidate and final commit together.

## Canonical identity model

Each historical source identity must be parsed into:

```text
historicalFullIdentity
historicalClass
historicalMethod
historicalDisplaySuffix
source=goal150b_failed|goal150b_missing
```

Current discovery index:

```text
currentFullIdentity
currentClass
currentMethod
currentDisplaySuffix
```

Canonical method key:

```text
Class.Method
```

### Resolution rules

Apply in order.

#### R1 — exact full identity

One current exact identity matches.

```text
resolution=exact
```

#### R2 — canonical method match

No exact match, but current discovery has one or more cases with the same `Class.Method`.

This covers xUnit theory display/argument formatting drift.

```text
resolution=canonical_method
```

Execute every current discovered case for that method.

#### R3 — explicit rename/supersession

No current `Class.Method` match.

Inspect the exact test source history between Goal150B commit and candidate:

```powershell
git diff 07a8ea17... <candidate> -- <exact test source path>
git show 07a8ea17...:<path>
```

A rename/supersession may be recorded only when:

- old and new tests are in the same test class/source concern, or a clearly named replacement class;
- the old method disappeared;
- the replacement asserts the same or stronger contract;
- a human-readable rationale is recorded;
- the replacement current test exists in discovery.

```text
resolution=explicit_rename
classification=F stale historical assertion/identity
```

Do not hardcode rename dispatch in the generic runner. Store explicit mappings in a task-local JSON alias manifest consumed by the runner.

#### R4 — retired obsolete test without replacement

Allowed only if the historical assertion solely checked an old moving validation implementation and its stable product contract is covered elsewhere.

Requires:

- exact source diff proof;
- replacement coverage list;
- current stable contract proof;
- no loss of gameplay/runtime/save/replay/hash coverage.

This should be rare.

```text
resolution=retired_with_replacement_coverage
classification=F
```

A retired test does not execute, but its replacement coverage must execute and pass.

#### Unresolved/ambiguous

Blocks candidate creation.

No fuzzy name matching, edit distance or guess-based mapping.

## Closure execution manifest

The execution manifest must distinguish historical sources from current cases:

```json
{
  "historicalIdentity": "...",
  "resolution": "exact|canonical_method|explicit_rename|retired_with_replacement_coverage",
  "currentExecutionIdentities": ["..."],
  "classification": "A|B|C|D|E|F",
  "rationale": "..."
}
```

A historical identity may map to multiple current theory cases.

Deduplicate current execution identities before launch.

Required summary:

```text
historicalIdentityCount=85
resolvedHistoricalIdentityCount=85
exactCount
canonicalMethodCount
explicitRenameCount
retiredWithCoverageCount
unresolvedCount=0
ambiguousCount=0
currentExecutionCaseCount=<N>
```

## Runner accounting corrections

Update closure mode fields:

```text
historicalIdentityCount
resolvedHistoricalIdentityCount
currentExecutionCaseCount
attemptedExecutionCaseCount
executedCaseCount
passedCaseCount
failedCaseCount
skippedCaseCount
notRunCaseCount
timedOutCaseCount
missingResultCount
duplicateResultCount
```

Definitions:

- attemptedExecutionCaseCount: unique current identities included in launched commands;
- executedCaseCount: terminal TRX result exists;
- timedOutCaseCount: exact current identity was launched alone, timed out and has no terminal result;
- notRunCaseCount: never launched;
- missingResultCount: launched non-timeout command produced no expected result;
- duplicateResultCount: more than one terminal result for same current identity.

Required invariants:

```text
executedCaseCount = passedCaseCount + failedCaseCount + skippedCaseCount
currentExecutionCaseCount =
  executedCaseCount + notRunCaseCount + timedOutCaseCount + missingResultCount
```

`attempted` is not the number of commands.

### Result matching

Do not reconcile TRX rows back to historical display strings.

Reconcile by current execution identities/canonical method membership generated by the resolution map.

Use exact current-discovery identities as the terminal result keys.

## Classification

For each historical identity:

```text
A stale pre-final-tree evidence
B cross-shard artifact contamination
C missing required environment/isolation
D real exact-test timeout
E genuine reproducible product regression
F stale historical assertion/identity
```

- exact/canonical current tests pass after tracked baseline preservation → B or C based on original failure;
- explicit rename/supersession → F;
- exact current test fails valid contract → E;
- exact current test times out at 8 minutes → D;
- unexecuted replacement → unclassified and blocking.

`allHistoricalIdentitiesClassified=true` only when all 85 are resolved and all required current coverage has terminal outcomes.

## ProductSmoke isolation

Keep tracked `.llmgc/procedural` and `.llmgc/exports` candidate inputs intact.

For each ProductSmoke execution set unique external:

```text
LLMGC_PRODUCT_SMOKE_PROJECT_DIR
LLMGC_PRODUCT_SMOKE_PACKAGE_OUTPUT_DIR
test results
TEMP/TMP
logs/TRX
```

Prefer existing scenario runner/manifest when available.

No test may depend on previous closure execution order.

## Required validation gates

### Pre-candidate T0

```powershell
dotnet build
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150E"
dotnet test ... --filter "FullyQualifiedName~Goal150D"
```

### Candidate focused

```powershell
dotnet test ... --filter "FullyQualifiedName~Goal150C"
dotnet test ... --filter "FullyQualifiedName~Goal150B"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests"
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
```

### Candidate closure

Run exactly one mapped closure pass.

### Non-ProductSmoke full gate

Only after closure passes:

```powershell
.\.devflow\scripts\check-all.ps1 -SkipRestore
```

Do not separately rerun the same non-ProductSmoke suite.

### Final status guards

After final commit:

```powershell
dotnet build
dotnet test ... --filter "FullyQualifiedName~Goal150E"
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
```

Do not rerun successful closure/check-all after evidence-only final changes.

## Current-state update

Goal150D final commit failed to update source-of-truth.

Goal150E final status must update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
```

If GREEN:

```text
manualGateReady=true
nextAction=independent_goal150e_audit_then_one_bundled_human_gate
```

If BLOCKED:

```text
manualGateReady=false
nextAction=resolve_exact_goal150e_remaining_identity_or_test_blockers
```

Keep all acceptance flags false.

## Compact evidence

Maximum 10 files per root:

```text
goal150e-dashboard.json
historical-identity-reconciliation-summary.json
historical-identity-reconciliation-map.json
closure-execution-result.json
closure-classification-summary.json
non-product-full-gate-proof.json
focused-regression-proof.json
historical-hash-regression-proof.json
artifact-scope-proof.json
goal150e-report.md
```

Raw details remain in `.devflow/runs`.

Do not rewrite Goal150B/C/D historical evidence.

## Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-complete-test-suite.ps1
.devflow/scripts/run-complete-test-suite.cmd
.devflow/scripts/run-goal150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix.ps1
.devflow/scripts/run-goal150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix.cmd

tests/LLMGameCreator.Tests/Devflow/RunCompleteTestSuiteScriptTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal150DValidationAccountingTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal150EHistoricalIdentityReconciliationTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md

docs/agent-tasks/goal-150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix/
.llmgc/procedural/goal-150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix/
.llmgc/exports/goal-150e-historical-test-identity-reconciliation-and-manual-gate-readiness-hotfix/
```

Task-local alias manifest may be placed under the task directory.

If an exact current test genuinely fails:

1. record it;
2. add only exact path;
3. explain classification;
4. edit only that path.

No broad prefixes.

## Forbidden

- no new gameplay mechanics;
- no Runtime/GamePackage/schema/Unity/provider/LLM/RAG/Lua changes unless exact E-class product P1 is proven and exact scope is expanded;
- no test deletion/skip/weakening;
- no arbitrary fuzzy identity mapping;
- no old GREEN→BLOCKED rewrite for convenience;
- no fake results;
- no notRun counted as executed;
- no tracked baseline deletion;
- no all-ProductSmoke sweep;
- no broad reset/clean in main;
- no branch/merge/rebase/cherry-pick/history rewrite;
- no more than one validation-candidate commit;
- no Goal-number normal UI.

## Publication policy

The owner must not push manually.

Candidate:

```text
VALIDATION CANDIDATE Goal 150E historical test identity reconciliation and manual gate readiness
```

Final:

```text
GREEN Goal 150E historical test identity reconciliation and bundled manual gate readiness hotfix
BLOCKED Goal 150E historical test identity reconciliation and bundled manual gate readiness hotfix
FAILED Goal 150E historical test identity reconciliation and bundled manual gate readiness hotfix
```

Push both together.

Verify:

```powershell
git push origin main
git rev-parse HEAD
git rev-parse origin/main
git merge-base --is-ancestor <candidate> origin/main
git status --short
```

Require:

```text
HEAD == origin/main
candidate reachable
worktree clean
```

Only concrete Git/auth failure permits no push; retry once and never ask owner to push.

## GREEN criteria

```text
oneCandidateCommit=true
candidateReachable=true
historicalIdentityCount=85
resolvedHistoricalIdentityCount=85
unresolved=0
ambiguous=0
all required current execution cases passed
closure notRun/timedOut/failed/missing/duplicate=0
all historical identities classified=true
focused Goals149/150/150A/150B=true
current-goal=true
spine-fast=true
check-all non-ProductSmoke=true
historical hashes preserved=true
artifact scope=true
manualGateReady=true
all acceptance flags=false
manualReviewPerformed=false
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- preflight reconciliation counts;
- exact unresolved/ambiguous identities if any;
- mapping counts R1/R2/R3/R4;
- one candidate SHA/message;
- final SHA/message;
- push/ancestry/HEAD/worktree;
- command-budget compliance;
- closure current-case accounting;
- A/B/C/D/E/F totals;
- exact remaining failures/timeouts/notRun;
- focused/current/spine/check-all status;
- historical hashes;
- compact artifact count/scope violations;
- current-state updated;
- manualGateReady;
- acceptance flags;
- confirmation no human acceptance claimed.
