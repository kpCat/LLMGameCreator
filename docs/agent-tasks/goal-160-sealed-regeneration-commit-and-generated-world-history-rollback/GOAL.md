# Goal 160 — Sealed Regeneration Commit & Generated World History Rollback

## Identity

- Task ID: `goal-160-sealed-regeneration-commit-and-generated-world-history-rollback`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `c7788e1e872576fbc37d53550a679ebe3477c5f3`
- Required base message: `Goal159 transactional seed regeneration`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: this is a major saved-project lifecycle vertical slice. It closes the independent-audit P1
at Goal159's commit boundary, introduces a shared mutation lease for build/standalone/regeneration,
seals candidate truth, moves semantic validation inside the rollback window, and adds user-visible
generated-world history with fully qualified rollback to an earlier world. It must preserve current
FeatureModule authoring and never restore a stale package blindly.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal plan; do not ask for confirmation.
- Do not request intermediate or final manual testing.
- Own all P0/P1 defects reproduced by the mandatory Goal160 matrix inside this Goal.
- Record P2/P3 debt without creating Goal160A.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and standard push itself.

## Expected initial worktree

After unpacking, only these untracked files are allowed:

```text
docs/agent-tasks/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/GOAL.md
docs/agent-tasks/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/README.md
```

Require:

```text
HEAD == origin/main == c7788e1e872576fbc37d53550a679ebe3477c5f3
branch=main
tracked diff count=0
staged diff count=0
unknown dirty/untracked count=0
```

Any other dirt blocks execution. Never use reset, stash, merge, rebase or destructive cleanup.

## Unity and standalone budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
real hidden standalone smoke budget: exactly 1
visible automated standalone launch budget: 0
```

The one smoke is run only after a successful history rollback and ordinary rebuild.

## Goal159 independent-audit result

Record:

```text
goal159IndependentAuditResult=BLOCKED_AT_C7788E1E
goal159IndependentAuditBlocker=regeneration_commit_not_sealed_inside_shared_operation_and_semantic_rollback_boundary
goal159AuditBlocker=closed_by_goal160 only on GREEN
```

Goal159 implementation remains useful and GREEN; accepted=false; no human gate.

### Useful Goal159 foundation to preserve

```text
backward-compatible generated source v2
GenerationRequest versus ResolvedGenerationOptions
preset definition and override truth
shared deterministic artifact factory
isolated candidate generation and real qualification
typed world diff
journaled authoritative file replacement
v1→v2 successful upgrade
WinForms regeneration dialog/result
old RC retained as LAST_SUCCESS
one cache-only standalone smoke and portable recovery
```

Do not restart these systems.

## Independent-audit P1

Goal159 has three related commit-boundary holes.

### P1-A — final concurrency check is outside the transaction lock

`GameProjectSeedRegenerationService.Apply()` captures and compares truth tokens, then performs more
work and calls `GameProjectSeedRegenerationTransaction.Apply()`.

The transaction creates its own regeneration lock, but:

```text
it does not receive the expected truth tokens
it does not recheck source/authoring/package/identity/RC after acquiring the lock
normal build/standalone/save do not participate in that lock
```

A real race is possible:

```text
BuildAndQualify checks RegenerationRunning=false
Apply sets RegenerationRunning=true and sees BuildRunning=false
the build enters immediately afterward
Apply captures old truth and then replaces the newer build/edit
```

External project mutation can produce the same loss window.

### P1-B — caller preview and candidate are not sealed

Apply checks only cached:

```text
AttemptId
Status
CandidateRoot
directory exists
```

Then it uses fields from the caller-supplied preview:

```text
Diff
CandidateBuild
CandidateBuildHistoryFileName
```

and validates only candidate source truth.

The candidate package, authoring, identity, selected history entry or support files may change after
Preview. The transaction hashes the changed candidate as if it were authorized.

### P1-C — semantic validation occurs after irreversible commit

The transaction:

```text
validates candidate byte hashes
sets journal=committed
deletes backups/staging/original directories
returns success
```

Only afterward the service reopens the authoritative project and checks:

```text
TRAVEL_CURRENT
regeneration record valid
RC pending
```

If that semantic reopen fails, Apply returns failure after backups are already gone and no rollback
is possible.

These are low-probability but real project data-loss/truth risks; they block Goal159 independent audit.

## Goal160 product outcome

Goal160 closes the entire boundary and adds a visible product capability:

```text
Generated-world history
→ choose an earlier generated world
→ preview old/current world diff
→ rebuild it with current project identity and current mechanics
→ qualify accepted mechanics and generated travel
→ sealed rollback-safe apply
→ old RC LAST_SUCCESS
→ optional normal standalone build
```

World rollback never copies an old package or old authoring document directly into the project.

It restores only a strictly validated historical generation artifact set, then rebuilds it through the
current modern pipeline with current mechanics and parameters.

## Non-goals

Do not add:

```text
new Runtime primitives
new FeatureModules or parameter types
public GamePackage schema fields
Unity project/host changes
gameplay save-state migration across worlds
merging two generated worlds
arbitrary file-system snapshot/restore
history editing or deletion UI
provider/LLM/Lua/media execution
clean-machine packaging
```

Gameplay-save migration remains the next major product choice after Goal160.


## Mandatory architecture review

Read at most 18 primary files:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/UNITY_EXECUTION_POLICY.md
GameProjectSeedRegenerationService.cs
GameProjectSeedRegenerationTransaction.cs
GameProjectSeedRegenerationModels.cs
GameProjectSeedRegenerationRecordService.cs
SeededGeneratedProjectSourceService.cs
GameProjectBuildAndQualificationService.cs
UnifiedGameProjectWorkspaceController.cs
GameProjectBuildHistoryReader.cs
GameProjectReleaseCandidateRecordService.cs
GameProjectFeatureModuleAuthoringService.cs
ProjectsPageControl.cs
RegenerateGeneratedWorldDialog.cs
Goal159CandidateIsolationTests.cs
Goal159AtomicApplyRollbackTests.cs
```

Before production edits write:

```text
.llmgc/procedural/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/architecture-review.json
```

Required resolved sections:

```text
goal159IndependentAudit
sharedOperationLease
crossProcessProjectLock
candidateSeal
callerPreviewTrustBoundary
transactionTruthRecheck
semanticValidationInsideRollbackWindow
journalStateMachine
regenerationCompatibility
worldHistoryStorage
worldHistoryValidation
historyRollbackCandidate
currentAuthoringPreservation
historyDiff
releaseCandidateTransition
uiWorkflow
crashRecovery
legacyAndV1Compatibility
nonGoals
```

Each section identifies exact types, lock order, persisted files, mutation order, rollback order and
behavioral tests.

## A. Shared project operation coordinator

Create an Application-layer service:

```text
IGameProjectOperationCoordinator
GameProjectOperationCoordinator
GameProjectOperationLease
```

A single process-wide exclusive operation is acceptable because current package/build services are
process-global. Do not claim parallel multi-project builds.

Operation kinds:

```text
authoring_save
build
standalone
regeneration_preview
regeneration_apply
world_history_rollback_preview
world_history_rollback_apply
recovery
```

### A1. In-process lease

Use an atomic/nonblocking lease:

```text
TryAcquire(projectFolder, operationKind)
```

Return causal failure rather than waiting indefinitely:

```text
project_operation.busy:<active-kind>
```

Lease carries:

```text
OperationId
OperationKind
NormalizedProjectFolder
Acquired=true
```

The lease is disposable and cannot be reused after disposal.

Support a controlled child-operation scope for the same owning operation:

```text
regeneration preview/apply
→ candidate build/repeat/reopen
```

The candidate build must not open a second unrelated top-level operation. Use an explicit owner token,
not thread ID or ambient static state.

### A2. Cross-process project lock

For every authoritative mutation operation acquire a confined lock file:

```text
.llmgc/operations/project-mutation.lock
```

Hold a `FileStream` with `FileShare.None` for the entire operation.

The lock file content may include operation ID/kind for diagnostics, but no correctness depends on
content.

Candidate roots use their own lock when built.

### A3. All mutation routes participate

Wrap at minimum:

```text
SetModuleSelected
SetParameterValue
SaveAuthoring
BuildAndQualify
BuildWindowsStandalone
PreviewGeneratedWorldRegeneration
ApplyGeneratedWorldRegeneration
world-history rollback preview/apply
OpenProject recovery
```

Rules:

```text
build and standalone reject while regeneration/rollback active
regeneration rejects while build/standalone/save active
authoring mutation rejects while long operation active
no check-then-enter race between independent flags
```

Keep existing `BuildRunning`/`RegenerationRunning` for presentation only; operation lease is authority.

### A4. Candidate builder

Add an internal builder overload accepting a valid owner/child lease, or inject a coordinator-aware
build context.

Legacy callers remain supported and acquire a normal build lease automatically.

Do not add ad hoc bypass booleans.

## B. Sealed regeneration candidate

Create:

```text
GameProjectSeedRegenerationCandidateSeal
GameProjectSeedRegenerationCandidateSealService
```

Seal fields:

```text
schemaVersion=seed_regeneration_candidate_seal_v1
AttemptId
CandidateRootIdentity
SourceRecordSha256
GenerationTreeSha256
PackageSha256
AuthoringTreeSha256
IdentitySha256
SelectedBuildHistoryFileName
SelectedBuildHistorySha256
SupportTreeSha256
QualifiedAuthoringFingerprint
SelectedModuleIdsSha256
ParameterValuesSha256
CandidatePackageSha256
CandidateCompositionSha256
CandidateFinalStateHash
CandidateSourceRequestSha256
CandidatePlanSha256
CandidateOverlaySha256
CandidateGeneratedBaseSha256
CandidateSnapshotStatus
DiffSha256
SealSha256
```

`CandidateRootIdentity` is a random operational ID, not an absolute path.

### B1. Tree hashes

A tree hash is calculated from sorted:

```text
relative path
file SHA-256
```

Excluded:

```text
build staging
standalone output
transaction journals
candidate seal temporary file
```

No timestamp/path outside the candidate root participates.

### B2. Preview publication

After candidate repeat/reopen/diff:

1. calculate the seal;
2. write it atomically under:
   ```text
   <candidate>/.llmgc/regeneration-candidate/seal.json
   ```
3. cache an internal immutable `SealedRegenerationCandidate`;
4. return a public preview containing only the seal SHA and user-facing facts.

The public preview may retain technical summaries, but Apply must not trust any caller-supplied field
other than:

```text
AttemptId
CandidateSealSha256
```

### B3. Apply validation

Apply loads the internal cached sealed candidate and verifies:

```text
caller AttemptId/seal matches cache
candidate root is the exact cached confined path
seal file valid
every tree/file/build/diff hash still matches
strict source validation passes
selected history row matches build
identity and authoring match seal
candidate snapshot still TRAVEL_CURRENT
candidate RC is not CURRENT
```

Failures:

```text
regeneration.candidate_seal_mismatch
regeneration.candidate_tampered
regeneration.candidate_history_changed
regeneration.candidate_authoring_changed
regeneration.candidate_package_changed
```

No authoritative mutation occurs.

### B4. Cached authority

All apply inputs come from the cached sealed candidate.

Never use caller-supplied:

```text
CandidateRoot
CandidateBuild
CandidateBuildHistoryFileName
Diff
ExpectedTruthTokens
```

as authority.

## C. Truth tokens inside the transaction boundary

Extend the transaction request with:

```text
ExpectedTruthTokens
ExpectedAuthoritativeInventorySha256
CandidateSealSha256
OperationLease
```

The transaction must:

1. verify the operation lease is current and belongs to the project;
2. acquire/confirm the cross-process project lock;
3. recapture source/authoring/package/identity/RC truth using a supplied typed truth reader;
4. compare every expected token;
5. calculate current authoritative inventory;
6. require exact expected inventory;
7. only then create backups and start journal application.

Failures preserve authority:

```text
regeneration.source_changed
regeneration.authoring_changed
regeneration.package_changed
regeneration.identity_changed
regeneration.release_candidate_changed
regeneration.authoritative_inventory_changed
project_operation.lease_invalid
```

This is the actual second optimistic-concurrency check. A service-level check before entering the
transaction is useful diagnostics but is not sufficient.

## D. Semantic validation inside rollback window

Create:

```text
IGameProjectSeedRegenerationCommitValidator
GameProjectSeedRegenerationCommitValidator
```

After candidate files/history/record are applied and byte hashes pass, but before journal `committed`
and before backup cleanup, validate the authoritative project directly from disk.

Required:

```text
strict source v2 CURRENT
package validates
package hash equals candidate build
authoring document package/composition/final hashes equal candidate
identity fingerprint equals expected
selected modules and parameters equal sealed candidate/current source project
new selected history row exists and matches candidate build
last regeneration record validates
GeneratedWorld restores TRAVEL_CURRENT
GeneratedWorldActivation Passed
GeneratedRegionTravel Passed
AcceptedMechanics compatibility equals candidate
old RC bytes unchanged
RC record status LAST_SUCCESS or ABSENT
overall RC BUILD_GREEN_STANDALONE_PENDING when accepted mechanics complete
no unexpected authoritative file
```

The validator must not create a new history row, build, standalone or RC record.

### D1. Transaction state

Add journal state:

```text
prepared
applying
validating
committed
rolling_back
rolled_back
```

Order:

```text
apply files
byte validation
journal=validating
semantic commit validator
journal=committed
cleanup
```

Any semantic diagnostic throws inside the transaction and triggers exact rollback.

Required diagnostic:

```text
regeneration.commit_semantic_validation_failed:<cause>
```

### D2. Service post-open

After transaction success, the service may reopen the controller for presentation.

A failure in this presentation reopen does not redefine the committed transaction, but should be
extremely unlikely because semantic validation has already passed. Return committed result with a
presentation diagnostic rather than falsely claiming rollback.

## E. Recovery

Recovery rules:

```text
prepared/applying/validating/rolling_back -> restore exact before state
committed + candidate hashes valid -> cleanup
rolled_back -> retain evidence
unknown state -> recovery_required
```

A crash during semantic validation always rolls back on next open.

Tests simulate interruption in the new `validating` state.


## F. Generated world history

Create:

```text
GeneratedWorldHistoryService
GeneratedWorldHistoryManifest
GeneratedWorldHistoryEntry
GeneratedWorldHistoryReadResult
```

Project-relative root:

```text
.llmgc/regeneration/world-history/
```

World ID:

```text
SHA-256(source-record bytes + generated-base SHA-256)
```

Use the full lowercase hash as the directory name.

### F1. Entry contents

```text
.llmgc/regeneration/world-history/<world-id>/
  manifest.json
  generation/
    seeded-project-source.json
    every required Goal156/157 sidecar
```

Do not store:

```text
package.json
authoring document
identity document
build history
RC record
standalone output
absolute paths
```

Rollback always rebuilds with current project identity and current mechanics.

### F2. Manifest

Schema:

```text
generated_world_history_v1
WorldId
SourceSchemaVersion
SourceRecordSha256
SourceRequestSha256
PlanSha256
OverlaySha256
GeneratedBasePackageSha256
Seed
Mode
PresetId
ResolvedStyleHintIds[]
ResolvedVariantIds[]
Counts
StartRegionTitle
TravelDestinationTitle
GenerationTreeSha256
CreatedByOperationKind
```

`CreatedByOperationKind`:

```text
initial_capture
regeneration_before
regeneration_after
history_rollback_before
history_rollback_after
```

This field is descriptive and excluded from WorldId.

No timestamps are required. An optional display timestamp is excluded from identity.

### F3. Strict validation

Read validates:

```text
directory name equals WorldId
manifest exact schema
generation tree hash
source record/file hashes
strict SeededGeneratedProjectSourceService validation against a confined temporary/project adapter
manifest source/request/plan/overlay/base values match source
counts/titles match regenerated plan and route
```

Do not trust only manifest hashes.

Failures:

```text
world_history.invalid_manifest
world_history.tree_hash_mismatch
world_history.source_invalid
world_history.identity_mismatch
world_history.path_escape
```

### F4. Deduplication

Archiving an already-present valid WorldId is a no-op.

An existing directory with the same ID but unequal content is corruption and blocks mutation:

```text
world_history.identity_collision
```

### F5. Capture points

A successful sealed regeneration/history rollback transaction archives:

```text
current world before mutation
candidate world after mutation
```

Both history entries are part of the same transaction and rollback inventory.

The first Goal160 operation on a valid Goal159 project therefore captures the pre-Goal160 current
world without mutating it before the transaction begins.

A plain project open does not create history.

## G. History rollback candidate

Create:

```text
GameProjectGeneratedWorldRollbackRequest
GameProjectGeneratedWorldRollbackPreview
GameProjectGeneratedWorldRollbackResult
GameProjectGeneratedWorldRollbackService
```

Request:

```text
ProjectFolder
TargetWorldId
Expected current truth tokens
Expected world-history manifest/tree hash
```

### G1. Preconditions

Require:

```text
current generated source valid
target history entry valid
target WorldId != current WorldId
current authoring/fingerprint valid
operation coordinator lease acquired
current package/document truth valid
```

Failures:

```text
world_rollback.not_generated_project
world_rollback.target_missing
world_rollback.target_invalid
world_rollback.no_semantic_change
world_rollback.current_truth_changed
project_operation.busy
```

### G2. Candidate construction

Use the same candidate clone/build infrastructure as regeneration:

1. clone current project to isolated short root;
2. replace candidate `.llmgc/generation` with exact historical generation set;
3. keep current identity;
4. keep current selected modules and parameter values;
5. clear only candidate qualified-build hashes;
6. real build/repeat/fresh reopen;
7. require `TRAVEL_CURRENT`;
8. require AcceptedMechanics according to current selection;
9. create current→historical typed diff;
10. seal candidate.

Do not restore old authoring or old build history as current truth.

### G3. Apply

Use the exact same:

```text
operation coordinator
truth-token recheck
candidate seal
journal transaction
semantic commit validator
world-history before/after capture
```

Operation kind is `world_history_rollback`.

The result record distinguishes rollback from regeneration.

### G4. Result record

Extend the last successful world-change record additively or create:

```text
.llmgc/regeneration/last-successful-world-change.json
```

Preferred schema:

```text
generated_world_change_result_v1
OperationKind=regeneration|history_rollback
AttemptId
FromWorldId
ToWorldId
old/new source/package/composition/final hashes
qualified authoring fingerprint
diff
selected history file
previous RC hash/status
```

Keep reading Goal159 `last-successful-regeneration.json` for compatibility.

A Goal160 successful operation writes the new world-change record and may maintain the Goal159
regeneration record when operation kind is regeneration. Do not lie about a rollback as regeneration.

## H. World history UI

### H1. Button

Add:

```text
История миров
```

Visible only for a valid generated project.

Disabled while any project operation lease is active.

### H2. Dialog

Create:

```text
GeneratedWorldHistoryDialog
```

List entries:

```text
Текущий
Seed
Режим
Пресет
Регионы
Фракции
Персонажи
Столкновения
Задания и события
Игровой старт
Маршрут назначения
```

Order:

```text
current first
then WorldId ordinal or a stable recorded sequence
```

Do not rely on filesystem creation time for correctness.

### H3. Rollback flow

Selecting a noncurrent entry enables:

```text
Проверить и восстановить
```

Flow:

```text
build sealed candidate
show current→target diff
confirm automatically within the already approved action
apply transaction
refresh same project
```

Do not ask for a second low-level confirmation after candidate qualification.

### H4. Card

Extend the generated-world card with compact rows:

```text
Сохранённых миров
Текущий мир
Последнее изменение мира
Источник последнего изменения
Windows standalone
```

After rollback apply before standalone:

```text
Последнее изменение мира    восстановление из истории
Windows standalone          требуется повторная проверка
```

After standalone:

```text
Windows standalone          подтверждён
```

No IDs/hashes/paths.

Technical Details expose world IDs/hashes, operation kind, candidate seal and transaction state.

## I. Release Candidate semantics

After regeneration or history rollback apply:

```text
old RC bytes unchanged
RC record status LAST_SUCCESS
overall BUILD_GREEN_STANDALONE_PENDING when accepted mechanics complete
```

After ordinary standalone:

```text
new RC CURRENT
```

World history entries never contain or restore an RC record.

## J. Portable copy

After rollback and standalone, copying the complete project restores without execution:

```text
current source v2 CURRENT
world history entries valid
current WorldId identified
last world-change record valid
GeneratedWorld TRAVEL_CURRENT
activation/travel Passed
AcceptedMechanics current
RC CURRENT
```

No build/Runtime/Unity starts.


## K. Required behavioral tests

Create at least 54 Goal160 tests; at least 48 behavioral.

### Shared operation lease

1. build lease rejects regeneration preview;
2. regeneration preview lease rejects build;
3. regeneration apply lease rejects standalone;
4. standalone lease rejects authoring save/module/parameter change;
5. two simultaneous build entries cannot pass check-then-enter;
6. cross-process lock file rejects second process/service;
7. child candidate build works only with matching owner token;
8. disposed/foreign lease rejected;
9. operation lock released after success;
10. operation lock released after exception.

### Candidate seal

11. Preview writes exact seal;
12. unchanged candidate seal validates;
13. caller-modified preview Diff ignored/rejected;
14. caller-modified history filename ignored/rejected;
15. caller-modified build hashes ignored/rejected;
16. candidate package tamper rejected before mutation;
17. candidate authoring tamper rejected;
18. candidate identity tamper rejected;
19. candidate generation tamper rejected;
20. candidate selected history tamper rejected;
21. candidate support-file tamper rejected;
22. seal path/root substitution rejected;
23. failed seal leaves authoritative tree unchanged.

### Truth recheck and semantic rollback

24. truth tokens rechecked after transaction lock acquired;
25. mutation between service check and transaction entry rejected;
26. authoritative inventory mismatch rejected;
27. RC mutation in race window rejected;
28. authoring/build race cannot be overwritten;
29. journal enters validating before commit;
30. semantic validator success commits;
31. semantic source failure rolls back;
32. semantic package/document mismatch rolls back;
33. semantic history mismatch rolls back;
34. semantic identity mismatch rolls back;
35. semantic RC truth failure rolls back;
36. rollback restores exact before hashes;
37. committed backups are cleaned only after semantic pass;
38. presentation reopen failure returns committed result with diagnostic;
39. crash in validating state recovers before state.

### Goal159 compatibility

40. Goal159 successful regeneration remains GREEN through sealed path;
41. v1→v2 behavior preserved;
42. semantic no-op remains zero-write;
43. candidate diff values preserved;
44. exactly one new history row remains;
45. old RC remains LAST_SUCCESS after apply;
46. Goal159 journal v1 recovery remains readable or is explicitly migrated additively.

### World history storage

47. current and candidate worlds archived atomically;
48. WorldId deterministic from source/base;
49. duplicate exact archive is no-op;
50. same-ID unequal archive rejected;
51. manifest/tree/source full validation;
52. tampered history sidecar rejected;
53. path escape rejected;
54. history contains no package/authoring/identity/RC files;
55. plain open creates no history.

### World rollback candidate/apply

56. target current world rejected as no-op;
57. missing/invalid target rejected;
58. rollback candidate uses historical generation and current authoring;
59. rollback preserves current selected modules/parameters;
60. rollback preserves current identity;
61. rollback candidate build/repeat/reopen TRAVEL_CURRENT;
62. rollback candidate RC not CURRENT;
63. rollback diff current→target correct;
64. sealed rollback apply succeeds;
65. rollback applies target source/package/build;
66. old histories retained and exactly one new GREEN history added;
67. before/after worlds remain archived;
68. rollback last world-change record says history_rollback;
69. old RC bytes retained and LAST_SUCCESS;
70. rollback failure matrix exact;
71. rollback concurrency race rejected;
72. rollback fresh reopen TRAVEL_CURRENT.

### History/UI/RC

73. history dialog lists current and previous worlds;
74. current entry cannot be restored;
75. list values data-derived and no technical IDs in primary UI;
76. result card shows restoration and standalone pending;
77. technical details show seal/world/transaction IDs;
78. standalone after rollback changes RC to CURRENT;
79. result card then says standalone confirmed.

### Standalone/portable

80. exactly one hidden standalone smoke after rollback;
81. host reused/not rebuilt and Unity zero;
82. actual payload target historical world/travel facts;
83. payload accepted mechanics facts;
84. payload hashes match rollback build;
85. portable copy restores history/current world/change record/travel/accepted/RC without execution.

### Regressions

86. Goal159 v1/v2/regeneration regressions GREEN;
87. Goal158 travel regressions GREEN;
88. Goal157 provenance/two-lane regressions GREEN;
89. Goal156 creation/custom-base regressions GREEN;
90. Goal155A/155 regressions GREEN;
91. Goal154D/153C/150/149 regressions GREEN;
92. Runtime transition/legacy interaction regressions GREEN;
93. procedural generation/preview regressions GREEN;
94. Goal142 and goal148-manual byte-identical.

Do not claim list counts unless discovered/executed.

## L. Focused validation

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal160"
# require >=54 total and >=48 behavioral

dotnet test ... --filter "FullyQualifiedName~Goal160"
dotnet test ... --filter "FullyQualifiedName~Goal159"
dotnet test ... --filter "FullyQualifiedName~Goal158"
dotnet test ... --filter "FullyQualifiedName~Goal157"
dotnet test ... --filter "FullyQualifiedName~Goal156"
dotnet test ... --filter "FullyQualifiedName~Goal155A"
dotnet test ... --filter "FullyQualifiedName~Goal155"
dotnet test ... --filter "FullyQualifiedName~Goal154D"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization"
dotnet test ... --filter "FullyQualifiedName~Goal149"

dotnet test ... --filter "FullyQualifiedName~DefaultGameRuntime"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~ProjectLifecycle"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
dotnet test ... --filter "FullyQualifiedName~ProceduralGameKernel"
dotnet test ... --filter "FullyQualifiedName~GeneratedPackageMvp"

.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run:

```text
real Goal159-style regeneration through sealed transaction
archive old/new worlds
rollback to old world through real candidate build
repeat/reopen
failure and race injection matrix
exactly one hidden standalone smoke after rollback
portable-copy proof
```

Run artifact scope last.

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
visible automatic standalone launch
unchanged failed command retry
timeout escalation loop
```

A zero-match filter is failure.

## M. Evidence

Create exactly 14 files in each mirrored root:

```text
goal160-dashboard.json
architecture-review.json
goal159-independent-audit-finding.json
shared-operation-lease-proof.json
candidate-seal-proof.json
transaction-truth-recheck-proof.json
semantic-commit-rollback-proof.json
world-history-storage-proof.json
world-history-rollback-candidate-proof.json
world-history-rollback-apply-proof.json
history-ui-rc-proof.json
standalone-portability-proof.json
artifact-scope-proof.json
goal160-report.md
```

Roots:

```text
.llmgc/procedural/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/
.llmgc/exports/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/
```

Twins byte-identical.

### Dashboard fields

```text
status
candidateStatus
goal160TestsDiscovered
goal160BehavioralTestsPassed

goal159IndependentAuditBlockerRecorded
goal159AuditBlockerClosed

sharedOperationLeasePassed
buildRegenerationRaceRejected
standaloneRegenerationRaceRejected
crossProcessLockPassed

candidateSealWritten
candidateSealValidated
callerPreviewMutationRejected
candidatePackageTamperRejected
candidateAuthoringTamperRejected
candidateHistoryTamperRejected
candidateSupportTamperRejected

transactionTruthRecheckInsideLockPassed
authoritativeInventoryRecheckPassed
journalValidatingStatePassed
semanticValidationInsideRollbackPassed
semanticFailureRollbackPassed
validatingCrashRecoveryPassed

goal159RegenerationCompatibilityPassed

worldHistoryEntryCount
currentWorldArchived
candidateWorldArchived
worldHistoryValidationPassed
worldHistoryDedupPassed
worldHistoryTamperRejected

rollbackTargetWorldId
rollbackCandidateBuildPassed
rollbackCandidateRepeatDeterministic
rollbackCandidateFreshReopenTravelCurrent
rollbackAuthoringPreserved
rollbackIdentityPreserved
rollbackDiffPassed
rollbackAtomicApplyPassed
rollbackOneNewHistoryAdded
rollbackOldHistoryPreserved
rollbackOldRcLastSuccess
rollbackWorldChangeRecordPassed

historyUiPassed
standalonePendingAfterRollback

hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
actualPayloadRollbackWorldFactsPassed
actualPayloadAcceptedFactsPassed
releaseCandidateRecordCurrent
portableCopyCurrent

goal159RegressionPassed
goal158RegressionPassed
goal157RegressionPassed
goal156RegressionPassed
goal155aRegressionPassed
goal155RegressionPassed
goal154dRegressionPassed
goal153cRegressionPassed
goal150RegressionPassed
goal149RegressionPassed
proceduralLegacyRegressionPassed
goal142SourceByteIdentical
sourceGoal148ByteIdentical

artifactScopeViolationCount
goal160Accepted=false
goal160ManualReviewRequired=false
goal160IndependentAuditRequired=true
```

No required GREEN field may be null/PARTIAL/NOT_EXECUTED/unverified constant.


## N. State and docs

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal159-transactional-seed-regeneration.md
```

Create:

```text
docs/manual-acceptance/goal160-generated-world-history-rollback.md
```

No human gate.

Required GREEN state:

```text
goal159IndependentAuditResult=BLOCKED_AT_C7788E1E
goal159IndependentAuditBlocker=regeneration_commit_not_sealed_inside_shared_operation_and_semantic_rollback_boundary
goal159AuditBlocker=closed_by_goal160

goal159ImplementationStatus=GREEN
goal159CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal159Accepted=false
goal159IndependentAuditRequired=false

goal160ImplementationStatus=GREEN
goal160CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal160Accepted=false
goal160AcceptedByHuman=false
goal160AcceptedByCodex=false
goal160ManualReviewRequired=false
goal160ManualGateReady=false
goal160IndependentAuditRequired=true

goal160SharedOperationLeasePassed=true
goal160CandidateSealPassed=true
goal160TransactionTruthRecheckPassed=true
goal160SemanticCommitRollbackPassed=true
goal160WorldHistoryPassed=true
goal160WorldRollbackPassed=true
goal160AuthoringPreservationPassed=true
goal160IdentityPreservationPassed=true
goal160OldRcLastSuccessPassed=true
goal160HostReused=true
goal160HostRebuilt=false
goal160UnityProcessStartCount=0
goal160HiddenSmokeInvocationCount=1
goal160PortableCopyPassed=true
goal160ArtifactScopeViolationCount=0

nextAction=independent_goal160_audit_and_plan_generated_gameplay_save_migration
```

Release risk statement:

```text
Regeneration and history rollback now share one operation lease, sealed candidate truth and semantic
validation inside the rollback window. Historical worlds contain only strict generation artifacts and
are rebuilt with current mechanics before apply. Gameplay save-state migration between generated
worlds remains the next product decision.
```

Do not rewrite historical Goal159 evidence. Record the audit blocker in current docs only.

## O. Text integrity

Scan actual changed/task/evidence/docs files:

```text
valid UTF-8
no NUL
no forbidden C0 except CR/LF/TAB
no mojibake markers
no escaped Cyrillic where policy forbids
no absolute candidate/source paths in committed evidence
```

Historical evidence immutable.

## P. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal160-world-history-rollback.ps1
.devflow/scripts/run-goal160-world-history-rollback.cmd

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectOperationCoordinator.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidateRecordService.cs
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/GameProjectFeatureModuleAuthoringService.cs

src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationTransaction.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationRecordService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCandidateSealService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationCommitValidator.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedWorldHistoryModels.cs
src/LLMGameCreator.Application/Generation/Procedural/GeneratedWorldHistoryService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedWorldRollbackService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectGeneratedWorldChangeRecordService.cs
src/LLMGameCreator.Application/Generation/Procedural/GameProjectSeedRegenerationDiffService.cs
src/LLMGameCreator.Application/Generation/Procedural/SeededGeneratedProjectSourceService.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/Projects/GeneratedWorldHistoryDialog.cs
src/LLMGameCreator.WinForms/Pages/Projects/GeneratedWorldHistoryDialog.Designer.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal160/Goal160OperationLeaseTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160CandidateSealTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160SemanticCommitRollbackTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160WorldHistoryStorageTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160WorldRollbackCandidateTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160WorldRollbackApplyTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160WorkspaceUiTests.cs
tests/LLMGameCreator.Tests/Application/Goal160/Goal160StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159CandidateIsolationTests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159AtomicApplyRollbackTests.cs
tests/LLMGameCreator.Tests/Application/Goal159/Goal159CrashRecoveryTests.cs
tests/LLMGameCreator.Tests/WinForms/UnifiedGameProjectWorkspaceTests.cs
tests/LLMGameCreator.Tests/ProjectLifecycleTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
docs/GENERATOR_STRATEGY_RESET_PLAYABLE_PROCEDURAL_GENERATOR.md
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/manual-acceptance/goal159-transactional-seed-regeneration.md
docs/manual-acceptance/goal160-generated-world-history-rollback.md

docs/agent-tasks/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/
.llmgc/procedural/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/
.llmgc/exports/goal-160-sealed-regeneration-commit-and-generated-world-history-rollback/
```

One exact additional existing Application/WinForms/test path may be added only after a concrete
compile/test failure and with recorded reason.

Forbidden without a newly reproduced Goal160 P0/P1:

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.GamePackage/**
catalogs/feature-modules/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
```

No Runtime, GamePackage or FeatureModule semantics change is expected.

## Q. Command budget

```text
read-first/architecture review: 14 minutes
operation coordinator and candidate seal: 24 minutes
transaction truth recheck and semantic validation: 26 minutes
world-history store/validation: 20 minutes
rollback candidate/apply integration: 26 minutes
controller/UI/history/RC: 18 minutes
behavioral tests: 34 minutes
real regeneration→rollback matrix + one smoke: 16 minutes
regressions/evidence/docs/artifact scope: 20 minutes
target wall clock: 160 minutes
maximum two concurrent testhost processes
Unity process count: 0
```

Rules:

```text
write complete test inventory before production edits
write publication/evidence script before long external proof
no unchanged command repetition
no timeout escalation
after failure run only exact class/test
P0/P1 fixed inside Goal160
P2/P3 debt only
do not defer evidence/docs/artifact scope
```

## R. Publication

Create exactly one final commit:

```text
GREEN Goal 160 sealed regeneration commit and generated world history rollback
```

or honest BLOCKED/FAILED.

Codex performs standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal160 task files tracked
Goal142 and goal148-manual unchanged
Unity starts=0
HostRebuilt=false
hidden smoke=1 only on GREEN
Goal154 acceptance unchanged
Goal155 RC milestone passed
Goal159 accepted=false/no human gate
Goal160 accepted=false/no human gate
```

## S. GREEN criteria

```text
Goal159 P1 recorded and closed
Goal160 tests >=54 / behavioral >=48 / all pass

all project mutations share one operation lease
race between build/standalone/authoring/regeneration/rollback rejected
candidate preview sealed and caller/candidate tamper rejected
truth tokens/inventory rechecked inside locked transaction
semantic validation occurs before committed/cleanup
semantic failure and validating crash restore exact before hashes

Goal159 regeneration behavior preserved through sealed path
current/candidate worlds archived atomically
history entries strict, deterministic, deduplicated and tamper-detecting
history rollback rebuilds target world with current mechanics/identity
rollback candidate repeat/reopen TRAVEL_CURRENT
sealed rollback apply exact
old histories retained; one new GREEN history
old RC bytes retained/LAST_SUCCESS
world-change record truthful
UI history/rollback/card truthful

one cached hidden standalone smoke after rollback
HostReused=true / HostRebuilt=false / Unity=0
payload rollback-world/travel/accepted facts correlate
new RC CURRENT
portable copy restores history/current world/change record/travel/accepted/RC without execution

Goal159/158/157/156/155A/155/154D/153C/150/149 and procedural regressions GREEN
14+14 evidence mirrored
text integrity GREEN
artifact scope 0
goal160CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
one final commit pushed
```

## T. Final report

Return GREEN/BLOCKED/FAILED and include:

- model/reasoning;
- architecture review;
- exact Goal159 P1 reproduction;
- operation lease/race matrix;
- candidate seal fields and tamper matrix;
- in-transaction truth/inventory recheck;
- semantic validation/journal/rollback/recovery;
- Goal159 regeneration compatibility;
- world-history entry count/IDs/validation;
- selected rollback target and diff;
- rollback candidate build/repeat/reopen;
- authoring/identity preservation;
- apply/history/RC results;
- UI/card states;
- host/Unity/smoke;
- actual payload/new RC/portable copy;
- regressions;
- source/baseline immutability;
- evidence/text/artifact scope;
- state/no-human-gate;
- final SHA/push/HEAD/worktree.
