# Goal 169C — Post-Fix Immutable Standalone, RC & Portable Closure

## Identity

- Task ID: `goal-169c-post-fix-immutable-standalone-rc-and-portable-closure`
- Repo: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `91bef55bad9740897876f15893a93d596fa44800`
- Required base message: `BLOCKED Goal 169B package bound event proof nested combat replay and payload closure`
- Model: GPT-5.6 Terra
- Reasoning: High

This is a new isolated Codex dialog. This file is the complete instruction source.

Goal169B closed six implementation findings but consumed its one smoke before Player launch:
the legacy parser rejected a multiline JSON human fact. The published base already contains a
single-line `base64:` UTF-8 authority fix and a real non-smoke payload self-check where structural
checks and legacy compatibility pass. Goal169C supplies the fresh post-fix immutable proof.
No new campaign feature belongs here.

## 1. Publication and initial state

- Plan is approved by launching the task.
- Do not ask for confirmation or manual testing.
- No intermediate commits.
- Publish exactly one honest GREEN/BLOCKED/FAILED commit and push origin/main.
- Never leave publication to the user.

After unpacking only these files may be untracked:

```text
docs/agent-tasks/goal-169c-post-fix-immutable-standalone-rc-and-portable-closure/GOAL.md
docs/agent-tasks/goal-169c-post-fix-immutable-standalone-rc-and-portable-closure/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-169c-post-fix-immutable-standalone-rc-and-portable-closure/README.md
```

Require:

```text
HEAD == origin/main == 91bef55bad9740897876f15893a93d596fa44800
branch=main
no other changes
```

Never reset, revert, stash, merge or rebase.

## 2. Budgets

```text
Unity Editor starts=0
Unity host builds=0
Goal169/169A/169B smoke reruns=0
new Goal169C cached hidden smoke=exactly 1
Goal169C corrective retry=0
visible/manual launch=0
max concurrent testhost=2
```

## 3. Classification and audit intake

Create mirrored `scaffold-classification.json` before edits.
Classify touched files as KEEP_AND_COMPLETE / REFACTOR / REPLACE / REMOVE_AS_UNUSED.

Default classification:

```text
Goal169B production = KEEP_AND_COMPLETE
Goal169B failed smoke/forensics = immutable retained input
Goal169C tests/runner/evidence/docs = new continuation artifacts
```

Record:

```text
goal169bImplementationCommit=91bef55bad9740897876f15893a93d596fa44800
goal169bIndependentAuditResult=BLOCKED_AT_91BEF55B
goal169bCodeFindingsClosed=true
goal169bPublicationBlocker=standalone.payload.human_facts_parse_mismatch
goal169bPostSmokeFix=single_line_base64_utf8_json_authority
goal169bPostFixLegacySelfCheckPassed=true
goal169cRequiredAction=fresh_post_fix_immutable_standalone_rc_portable_proof
```

Goal169/169A/169B remain accepted=false; no human gate.

## 4. Architecture/preflight review

Read no more than 16 primary files:

```text
GeneratedCampaignRegionalEventPayloadAuthorityService.cs
GameProjectGeneratedCampaignRegionalEventQualificationService.cs
GeneratedCampaignRegionalEventReplayService.cs
GeneratedCampaignRegionalEventCorrelationService.cs
GeneratedCampaignRegionalEventDefinitionAuthorityService.cs
GameProjectBuildAndQualificationService.cs
GameProjectBuildHistoryReader.cs
ProjectStandaloneBuildService.cs (read-only)
ProjectStandalonePayloadSelfCheckService.cs (read-only)
Goal169BStandaloneSmokeTests.cs
Goal169BMigrationAndPayloadTests.cs
Goal169B runner/dashboard/payload proof
current state/roadmap/gates
```

Create mirrored `architecture-review.json` proving:

```text
Base64 authority is single-line and contains no quote/CR/LF outside encoded bytes
DeserializeHumanFact roundtrips exact AuthoritySha256
real assembled payload structural self-check passes
legacy parser compatibility passes
FrameCategory roundtrips event/route/replay/sequence/command
no speculative production change is needed
smoke starts only after every non-smoke gate
```

Use honest terminology:

```text
immutable_payload_history_package_correlation
```

Do not claim full component signatures are recomputed from reduced adapter frames alone. They are
recomputed from persisted typed history frames and correlated with authority carried by payload.

## 5. Goal169C non-smoke tests

Create >=28 Goal169C tests, >=24 behavioral, including:

1. fact begins `base64:`;
2. fact contains no quote/CR/LF;
3. UTF-8 Base64 decode;
4. schema exact;
5. AuthoritySha256 recomputes;
6. six event IDs;
7. 24 signatures;
8. 24 frame-count keys and nested-trace keys;
9. real assembled payload self-check GREEN;
10. legacy parser GREEN;
11. frame parser roundtrip;
12. nested combat represented;
13. exact ID sets;
14. actual package authority;
15. strict absent profile;
16. typed migration definitions;
17. failed Goal169B run remains unpublished;
18. failed forensics readable;
19. Goal169/169A retained inputs unchanged;
20. source sidecar/host baselines captured.

Production changes are forbidden unless a concrete pre-smoke behavioral failure proves a real defect.
Record root cause, minimal path and new regression before adding a production path.

## 6. Mandatory pre-smoke gate

Run `dotnet build`, Goal169C discovery/tests, then focused filters:

```text
Goal169B = 72/72 with old smoke disabled
Goal169A = 60/60 with old smoke disabled
Goal169 = 108/108 with old smoke disabled
Goal168 focused
Goal167=94
Goal166=59
Goal165=55
Goal164=61
Goal163–157 focused
GeneratedCampaign
GeneratedGameplaySave
RuntimeSimulator
UnifiedGameProjectWorkspace
GameProjectOperationCoordinator
ProjectStandaloneBuild
```

Also run:

```powershell
.\.devflow\scriptsun-capability-runtime-equipment-slice.ps1
.\.devflow\scriptsun-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Zero-match is failure.

Do not run full suite, Goal168 85-case closure, all-ProductSmoke or Unity host build.

## 7. Retained immutable snapshot

Before the new smoke capture exact file/tree hashes for:

```text
Goal169 pointer/run/payload/history/RC
Goal169A pointer/run/payload/history/RC
Goal169B failed staged run and forensics
cached host
Goal142
Goal148
generation source sidecars
```

After smoke require every retained hash unchanged.
The new run must use a distinct immutable run directory.

## 8. Exactly one Goal169C smoke

Create a dedicated Goal169C smoke test and environment flag. Do not invoke Goal169B smoke.

After all non-smoke gates pass, invoke exactly once.

Required pre-launch:

```text
all structural self-checks GREEN
legacy parser GREEN
no human_facts_parse_mismatch
HostReused=true
HostRebuilt=false
Unity Editor starts=0
```

Required launch/publication:

```text
standalone actually launched
exit=0
smoke markers GREEN
Player log present
immutable run-status GREEN
new current pointer published
```

If anything fails: no retry, no post-smoke correction, honest BLOCKED/FAILED publication.

## 9. Immutable post-smoke proof

Discard all in-memory fixture/build/event objects and force GC.
Then read only:

```text
current pointer
run status
selected v7 build history
actual payload game package
player-adapter-model.json
player-adapter-frames.json
Base64 replay authority
RC record
```

Require:

### Pointer/run/package

```text
attempt IDs exact
package/final hashes exact
actual payload package SHA exact
run GREEN
payload/legacy checks GREEN
smoke exit=0
host reused/no rebuild
```

### Regional events

Run package-backed correlation using selected history and actual payload package:

```text
six events
strict proof schema
24 signatures
exact ID sets
actual definitions exact
nested combat traces present
final state exact
```

### Base64 authority

Require one fact with label `regional-event-strict-replay-authority-v1`.
Decode and verify:

```text
single line
valid Base64 UTF-8 JSON
AuthoritySha256 recomputed
package/final/inventory hashes exact
six IDs
24 signatures
24 frame-count keys
24 nested trace keys
authority signatures canonical-equal selected history
```

### Frames

Every frame identity parses:

```text
generated-regional-event-frame-v1
event ID
route kind
replay index
sequence index
command identity
```

Require exact event IDs, both routes and both replays per event, contiguous sequences, exact commands,
and nested combat identities. Recompute signatures from selected typed history frames and compare to
payload authority.

### RC/portable

```text
RC record/configuration CURRENT
RC package/final/standalone hashes exact
portable all-selectable CURRENT without operational pointer
portable core-only campaign current without false RC readiness
```

## 10. Evidence

Create exactly 15 byte-identical files in each root:

```text
.llmgc/procedural/goal-169c-post-fix-immutable-standalone-rc-and-portable-closure/
.llmgc/exports/goal-169c-post-fix-immutable-standalone-rc-and-portable-closure/
```

Files:

```text
goal169c-dashboard.json
architecture-review.json
scaffold-classification.json
goal169b-independent-audit-finding.json
base64-authority-preflight-proof.json
legacy-parser-proof.json
retained-inputs-before-proof.json
goal169c-smoke-proof.json
immutable-run-publication-proof.json
immutable-payload-history-package-correlation-proof.json
rc-portability-proof.json
retained-inputs-after-proof.json
regression-immutability-proof.json
artifact-scope-proof.json
goal169c-report.md
```

Dashboard must measure test counts, Base64/legacy checks, retained hashes, smoke invocation/retry,
host/Unity/launch/exit/self-check, pointer/run publication, event/signature/frame/nested counts,
immutable correlation, RC/portable, regressions, protected bytes, scope and accepted=false.
No required GREEN field may be null, PARTIAL or NOT_EVALUATED.

## 11. Docs/state

Update generator current state/index/queue/gates/risks/debt/strategy/roadmap and Goal169B acceptance.
Create:

```text
docs/manual-acceptance/goal169c-post-fix-immutable-standalone-rc-portable.md
```

GREEN state:

```text
goal169bImplementationStatus=BLOCKED
goal169bCandidateStatus=BLOCKED_AT_91BEF55B
goal169bAccepted=false
goal169bIndependentAuditRequired=false
goal169bCodeFindings=closed
goal169bPublicationBlocker=closed_by_goal169c

goal169cImplementationStatus=GREEN
goal169cCandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal169cAccepted=false
goal169cAcceptedByHuman=false
goal169cAcceptedByCodex=false
goal169cManualGateReady=false
goal169cIndependentAuditRequired=true

goal169cBase64AuthorityPassed=true
goal169cLegacyParserPassed=true
goal169cImmutableCorrelationPassed=true
goal169cHiddenSmokeInvocationCount=1
goal169cCorrectiveSmokeRetryCount=0
goal169cHostReused=true
goal169cHostRebuilt=false
goal169cUnityEditorProcessStartCount=0
goal169cStandaloneExitCode=0
goal169cReleaseCandidateCurrent=true
goal169cPortableAllSelectablePassed=true
goal169cPortableCoreOnlyPassed=true
goal169cArtifactScopeViolationCount=0

nextAction=independent_goal169c_audit_then_plan_next_visible_campaign_slice
```

No human gate.

## 12. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal169c-post-fix-standalone-closure.ps1
.devflow/scripts/run-goal169c-post-fix-standalone-closure.cmd
tests/LLMGameCreator.Tests/Application/Goal169C/**
tests/LLMGameCreator.Tests/Application/Goal169B/Goal169BStandaloneSmokeTests.cs
tests/LLMGameCreator.Tests/Application/Goal169B/Goal169BMigrationAndPayloadTests.cs
tests/LLMGameCreator.Tests/Application/Goal169B/Goal169BEvidenceCaptureTests.cs
listed generator docs/manual acceptance
docs/agent-tasks/goal-169c-post-fix-immutable-standalone-rc-and-portable-closure/**
.llmgc/procedural/goal-169c-post-fix-immutable-standalone-rc-and-portable-closure/**
.llmgc/exports/goal-169c-post-fix-immutable-standalone-rc-and-portable-closure/**
```

Production application code is not initially allowed.
One exact production path may be added only after a concrete pre-smoke failure and documented root
cause/regression.

Forbidden:

```text
Runtime/Runtime.Abstractions
GamePackage/Domain
FeatureModule catalogs
ProceduralGameKernel*
GeneratedPackageMvp*
source sidecars
Unity
ProjectStandaloneBuild implementation
RC implementation
cached host
retained Goal169/169A/169B outputs
```

Scope violations=0.

## 13. Command budget

```text
classification/architecture 10m
tests/preflight 18m
regressions 20m
retained snapshots 8m
one smoke 16m
immutable proof 12m
evidence/docs/scope/publication 16m
target 100m
```

No smoke before all non-smoke gates are GREEN.

## 14. Publication

GREEN message:

```text
GREEN Goal 169C post fix immutable standalone rc and portable closure
```

or honest BLOCKED/FAILED.

Final:

```text
one commit from 91bef55bad9740897876f15893a93d596fa44800
push origin/main
HEAD==origin/main
clean worktree
three task files tracked
retained outputs unchanged
Goal169C smoke=1
retry=0
host reused/no rebuild
Unity=0
exit=0 on GREEN
protected bytes unchanged
accepted=false
no human gate
```

GREEN requires Base64/legacy real preflight, all regressions, one successful cached smoke,
immutable pointer/run/history/package/payload correlation, 24 signatures, exact frame identities,
nested combat proof, RC/portable GREEN, >=28/>=24 tests, 15+15 evidence, text integrity, scope0,
one pushed commit.

Final report must include model/base, classification, audit intake, preflight, retained hashes,
smoke and publication, immutable correlation, event/signature/frame/nested proof, RC/portable,
tests/evidence/scope, SHA/message/push, HEAD==origin/main, clean worktree, and confirmation Codex
committed and pushed for every status.
