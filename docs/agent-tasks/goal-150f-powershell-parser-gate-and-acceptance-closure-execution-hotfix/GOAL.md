# Goal 150F — PowerShell Parser Gate + Acceptance Closure Execution Hotfix

## Identity

- Task ID: `goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `a952e2918601804c47eafaf7a53f880f9aadac49`
- Base message: `BLOCKED Goal 150E historical test identity reconciliation and bundled manual gate readiness hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Terra
Reasoning effort: Medium
```

Reason: the remaining work is a narrow PowerShell syntax/runner correction and execution of an already reconciled validation closure. No new gameplay or architecture is allowed. Medium is intentionally selected to reduce limit usage.

## Pre-approval

The owner approved execution by launching this task.

- Give a concise internal plan.
- Do not ask for plan confirmation.
- Start immediately after base/worktree checks.
- Do not stop and create another task merely because a pre-publication parser/smoke defect is found; repair it inside this task before commit.

## Why Goal150E blocked

Goal150E preflight completed:

```text
historical identities=85
resolved=85
unresolved=0
ambiguous=0
R1 exact=85
R2/R3/R4=0
```

The closure never started because candidate script:

```text
.devflow/scripts/run-complete-test-suite.ps1
```

has a PowerShell parser error around line 194:

```text
Missing closing '}' in statement block or type definition.
```

This should have been detected before commit by:

```powershell
[System.Management.Automation.Language.Parser]::ParseFile(...)
```

The failure is validation infrastructure, not a demonstrated gameplay defect.

A second known validation-infrastructure defect exists in:

```text
.devflow/scripts/check-artifact-scope.ps1
```

Its baseline comparison can fail PowerShell parameter binding when deletion allowlists are empty. Empty exact/prefix deletion collections must be accepted normally.

## Product behavior to preserve

No product code changes are expected.

Focused behavior remains:

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

All acceptance flags remain false. GREEN means only `manualGateReady=true`.

## Critical process simplification

### No validation-candidate commit

Do not create any `VALIDATION CANDIDATE` commit in Goal150F.

Goal150F uses:

1. an uncommitted frozen source manifest;
2. parser/contract/plan/smoke checks;
3. the real mapped closure;
4. `check-all` only after closure GREEN;
5. one final status commit;
6. final build/parser/current/spine checks.

The validated executable-source manifest must equal the executable-source manifest in the final commit. Compact evidence/current-state result fields may be added after validation.

This removes the candidate-immutability trap that converted a missing brace into another Goal.

### Repair loop

Before the real closure begins, at most two validation-runner repair iterations are allowed.

Each iteration must end with:

```text
all changed PS1 parse clean
Goal150F contract tests GREEN
closure PlanOnly GREEN
one-test closure smoke GREEN
artifact-scope baseline smoke GREEN
```

After those five gates pass, freeze runner code. Do not modify it during the real closure.

## Exact required repair

### 1. Rewrite the closure lane-plan block

Do not make a one-character brace patch only.

Rewrite the block around the current line 194 in conventional expanded PowerShell syntax:

```powershell
if ($Mode -eq 'Goal150AcceptanceClosure') {
    $methodGroups = @(
        $inventory |
            Group-Object -Property {
                $className = $_.className
                $methodName = Get-MethodName -TestName $_.name
                "$className.$methodName"
            } |
            Sort-Object -Property Name
    )

    foreach ($methodGroup in $methodGroups) {
        $members = @($methodGroup.Group)
        $lanePlan += [pscustomobject]@{
            ...
        }
    }
}
else {
    foreach ($lane in @('N', 'P')) {
        ...
    }
}
```

Equivalent readable syntax is allowed.

Requirements:

- balanced braces verified by PowerShell AST parser;
- no nested `else { foreach (...) {` compact form;
- no nested command invocation inside an interpolated expression when a named intermediate variable is clearer;
- behavior unchanged except syntax correctness.

### 2. Fix empty deletion allowlists

In `check-artifact-scope.ps1`, parameters receiving deletion allowlists must accept empty arrays.

Use one safe approach:

```powershell
[AllowEmptyCollection()]
[string[]]$DeletedExactAllowed = @()
```

and equivalent for prefix list, or make them optional with defaults.

Required tests:

```text
empty deletion exact list accepted
empty deletion prefix list accepted
non-empty deletion policy still enforced
undeclared deletion still rejected
```

Do not weaken artifact-scope enforcement.

## Mandatory pre-publication gates

### A. Parse every changed PowerShell file

Use PowerShell's parser, not regex or brace counting:

```powershell
$tokens = $null
$errors = $null
[System.Management.Automation.Language.Parser]::ParseFile(
    $path,
    [ref]$tokens,
    [ref]$errors
) | Out-Null
```

Fail with file, line, column and message if any parser error exists.

At minimum parse:

```text
run-complete-test-suite.ps1
check-artifact-scope.ps1
run-goal150e...ps1
run-goal150f...ps1
```

### B. Real parser regression tests

Add tests that invoke the PowerShell AST parser on the production scripts.

String-contains tests are insufficient.

### C. PlanOnly smoke

Regenerate the reconciled 85-identity map and run closure `-PlanOnly`.

Require:

```text
historicalIdentityCount=85
resolvedHistoricalIdentityCount=85
unresolved=0
ambiguous=0
currentExecutionCaseCount=85
```

### D. One-test closure smoke

Before the real 85-test closure, create a temporary one-entry reconciliation manifest using one known fast, current, non-ProductSmoke test.

Run the exact production closure path.

Require:

```text
attemptedExecutionCaseCount=1
executedCaseCount=1
passedCaseCount=1
all failure/notRun/timeout/missing/duplicate counts=0
```

This smoke must use:

- disposable worktree;
- tracked baseline preservation;
- real discovery;
- real TRX parsing;
- real result reconciliation.

### E. Artifact-scope baseline smoke

Run the real artifact-scope command with:

- an empty deletion allowlist;
- the Goal150F scenario;
- a baseline ref.

It must not throw parameter-binding errors.

## Command budget

```text
read-first: maximum 6 primary files
PowerShell parse gate: under 1 minute
contract tests: under 3 minutes
PlanOnly: under 2 minutes
one-test closure smoke: under 3 minutes
real 85-case closure: maximum 20 minutes
check-all: one run, maximum 25 minutes
total validation: maximum 45 minutes
maximum two testhost processes
no all-ProductSmoke sweep
no raw monolithic test
no rerun of successful closure cases
```

## Real closure

After all pre-publication gates pass:

1. freeze runner and scope script;
2. regenerate reconciliation map;
3. execute exactly the 85 mapped current cases once;
4. use unique ProductSmoke environment roots;
5. preserve tracked historical artifacts;
6. produce correct accounting.

Required closure GREEN:

```text
historicalIdentityCount=85
resolvedHistoricalIdentityCount=85
unresolved=0
ambiguous=0

currentExecutionCaseCount=85
attemptedExecutionCaseCount=85
executedCaseCount=85
passedCaseCount=85
failedCaseCount=0
skippedCaseCount=0
notRunCaseCount=0
timedOutCaseCount=0
missingResultCount=0
duplicateResultCount=0
```

If current theory expansion legitimately produces more than 85 execution cases, record the larger exact current-case count and require all current cases to pass.

Do not classify a case until a real terminal result exists.

## Validation after closure GREEN

Run:

```powershell
dotnet build
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --filter "FullyQualifiedName~Goal150F"
dotnet test ... --filter "FullyQualifiedName~Goal150E"
dotnet test ... --filter "FullyQualifiedName~Goal150D"
dotnet test ... --filter "FullyQualifiedName~Goal150C"
dotnet test ... --filter "FullyQualifiedName~Goal150B"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronizationTests"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
.\.devflow\scripts\check-spine-fast.ps1
```

Then exactly one:

```powershell
.\.devflow\scripts\check-all.ps1 -SkipRestore
```

Do not run all ProductSmoke tests.

## Source freeze/equivalence

Before real closure, write a SHA-256 manifest over:

```text
.devflow/scripts/**
.devflow/artifact-scope/**
tests/**
src/**
catalogs/**
```

After final commit, recompute it.

Required:

```text
validatedSourceManifestSha256 == finalSourceManifestSha256
sourceEquivalent=true
```

Current-state and compact evidence files are excluded.

## Final commit only

Create exactly one Goal150F commit:

```text
GREEN Goal 150F PowerShell parser gate and acceptance closure execution hotfix
```

or honest `BLOCKED` / `FAILED`.

Do not create candidate commits.

Codex must push it itself.

After commit:

```powershell
git push origin main
git rev-parse HEAD
git rev-parse origin/main
git status --short
```

Then run final lightweight guards:

```text
PowerShell AST parse gate
dotnet build
Goal150F tests
check-current-goal
check-spine-fast
source-manifest equivalence
```

Required:

```text
HEAD == origin/main
worktree clean
```

## Current-state

Update source-of-truth honestly.

If GREEN:

```text
manualGateReady=true
nextAction=independent_goal150f_audit_then_one_bundled_human_gate
```

If BLOCKED:

```text
manualGateReady=false
nextAction=<one exact blocker, not generic validation wording>
```

Keep all acceptance flags false until the owner performs the manual gate.

## Compact evidence

Maximum 8 files per root:

```text
goal150f-dashboard.json
powershell-parser-proof.json
closure-smoke-proof.json
closure-result.json
focused-and-full-gates-proof.json
source-equivalence-proof.json
artifact-scope-proof.json
goal150f-report.md
```

Raw logs/TRX remain only under ignored `.devflow/runs`.

Do not rewrite historical Goal150B/C/D/E evidence.

## Allowed paths

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/check-artifact-scope.ps1
.devflow/scripts/run-complete-test-suite.ps1
.devflow/scripts/run-complete-test-suite.cmd
.devflow/scripts/run-goal150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix.ps1
.devflow/scripts/run-goal150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix.cmd

tests/LLMGameCreator.Tests/Devflow/RunCompleteTestSuiteScriptTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal150EHistoricalIdentityReconciliationTests.cs
tests/LLMGameCreator.Tests/Devflow/RunGoal150FPowerShellParserGateTests.cs
tests/LLMGameCreator.Tests/Devflow/ArtifactScopeDeletionAllowlistTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/manual-acceptance/capability-driven-runtime-playthrough-and-equipment-featuremodule-vertical-slice.md
docs/manual-acceptance/character-attributes-and-level-progression-featuremodules-vertical-slice.md

docs/agent-tasks/goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix/
.llmgc/procedural/goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix/
.llmgc/exports/goal-150f-powershell-parser-gate-and-acceptance-closure-execution-hotfix/
```

No production source changes are authorized.

## Forbidden

- no Runtime/GamePackage/Application/WinForms/Unity gameplay changes;
- no test deletion, skipping or weakening;
- no fake parser or fake closure result;
- no all-ProductSmoke sweep;
- no raw monolithic suite;
- no tracked baseline deletion;
- no candidate commit;
- no more than one final Goal150F commit;
- no branch/merge/rebase/cherry-pick;
- no broad reset/clean in main;
- no history rewrite;
- no user manual gate during this task.

## GREEN criteria

```text
all changed PS1 parse clean
artifact-scope baseline with empty deletion lists passes
one-test real closure smoke passes
85 historical identities resolved
all mapped current closure cases pass
focused Goals149/150/150A/150B pass
current-goal passes
spine-fast passes
check-all non-ProductSmoke passes
historical hashes preserved
sourceEquivalent=true
artifact scope passes
manualGateReady=true
all acceptance flags=false
manualReviewPerformed=false
one final commit pushed
```

## Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- parser error root cause and exact repair;
- all changed PS1 parser results;
- artifact-scope empty-list result;
- one-test closure smoke counts;
- real closure accounting;
- exact remaining failures, if any;
- focused/current/spine/check-all;
- historical hashes;
- source manifest hashes/equivalence;
- one final commit SHA/message;
- push/HEAD/origin/worktree;
- compact artifact count/scope violations;
- manualGateReady;
- acceptance flags;
- confirmation no human acceptance claimed.
