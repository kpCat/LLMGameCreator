# Goal 155 — Accepted Mechanics Release Candidate Integration & Operator Readiness

## Identity

- Task ID: `goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness`
- Repository: `https://github.com/kpCat/LLMGameCreator`
- Working copy: `C:\Users\endim\LLMGameCreator\`
- Branch: `main`
- Required base: `fc2ac34db60d2627e1cafc86493396937bf63fe4`
- Required base message: `GREEN Goal 154D all-selected precompleted quest social qualification hotfix`

This is a new isolated Codex dialog. This file is the complete instruction source.

## Recommended Codex configuration

```text
Model: GPT-5.6 Sol
Reasoning effort: Extra High
```

Reason: this is a milestone release-candidate integration pass across the accepted Goals149–154
product families. It records a real human acceptance, combines all accepted mechanics in one saved
project and one maximal interaction profile, adds a persistent typed RC summary, validates a cached
standalone payload and updates milestone/risk state. This is not another narrow hotfix.

## Pre-approval

- The owner approved execution by launching this task.
- Produce a concise internal execution plan; do not ask for confirmation.
- Do not request intermediate or final manual testing.
- Own all P0/P1 defects reproduced by the required RC matrix within this single Goal.
- P2/P3 findings are recorded as debt and do not spawn child Goals.
- Create and push exactly one final GREEN/BLOCKED/FAILED commit.
- No candidate/intermediate commits.
- Codex performs commit and push itself.

## Expected initial worktree

After this ZIP is unpacked, the only permitted untracked files are:

```text
docs/agent-tasks/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/GOAL.md
docs/agent-tasks/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/CODEX_LAUNCHER.txt
docs/agent-tasks/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/README.md
```

Required:

```text
HEAD == origin/main == fc2ac34db60d2627e1cafc86493396937bf63fe4
branch=main
tracked diff count=0
staged diff count=0
unknown dirty/untracked count=0
```

The three task files are authorized and must be committed.
Any other dirty path blocks execution. Never use destructive cleanup, reset, stash, merge or rebase.

## Unity and standalone budget

```text
Unity Editor invocation budget: 0
Unity host build budget: 0
real hidden standalone smoke budget: exactly 1
visible automated standalone launch budget: 0
```

The existing generic host cache must be reused. If the cache is incomplete or a host rebuild would
be required, publish BLOCKED without starting Unity.

## Why this Goal exists

The project has completed and received explicit human acceptance for the product slices:

```text
Goal149: equipment
Goal150 family: character attributes and level progression
Goal151: real saved-project recovery and diagnostic truth
Goal152 family: project-scoped cached Windows standalone
Goal153 family: active abilities, mana/spells and turn effects
Goal154 family: faction reputation, quest consequences and trusted dialogue reward
```

The repository now needs a release-candidate pass because more than five major product Goals have
accumulated since the previous milestone pass.

This Goal must prove that the accepted mechanics remain compatible in:

```text
one exact real user project
one maximal accepted-value interaction profile
save/reopen and last-success semantics
one canonical Runtime/replay
one cached Windows standalone
one concise operator-facing RC surface
```

It must not introduce another gameplay mechanic.

## Human acceptance to record before product work

The owner explicitly stated after successfully retrying Goal154D:

```text
Я принимаю Goals154/154A/154B/154B1/154C/154C1/154C2/154C3/154D: в реальном проекте со всеми 22 выбранными механиками и 10 настроенными параметрами социальные механики успешно собраны и воспроизведены без отключения Alchemy Focus и других профилей; репутация изменилась 0→10, квест завершился, доверенная награда была получена один раз, золото изменилось 0→10→17; значения и карточка сохранились после повторного открытия, standalone показал те же факты и переиспользовал host cache без запуска Unity Editor.
```

Accepted implementation commit:

```text
fc2ac34db60d2627e1cafc86493396937bf63fe4
```

Record this statement exactly in repository documentation/evidence.

### Acceptance flags

Preserve historical implementation outcomes, but record human acceptance for every named Goal:

```text
goal154Accepted=true
goal154AcceptedByHuman=true
goal154AcceptedByCodex=false
goal154ManualReviewPerformed=true
goal154ManualGateReady=false
goal154ManualGateStatus=ACCEPTED_BY_HUMAN

goal154aAccepted=true
goal154aAcceptedByHuman=true
goal154aAcceptedByCodex=false
goal154aManualReviewPerformed=true

goal154bAccepted=true
goal154bAcceptedByHuman=true
goal154bAcceptedByCodex=false
goal154bManualReviewPerformed=true

goal154b1Accepted=true
goal154b1AcceptedByHuman=true
goal154b1AcceptedByCodex=false
goal154b1ManualReviewPerformed=true

goal154cAccepted=true
goal154cAcceptedByHuman=true
goal154cAcceptedByCodex=false
goal154cManualReviewPerformed=true

goal154c1Accepted=true
goal154c1AcceptedByHuman=true
goal154c1AcceptedByCodex=false
goal154c1ManualReviewPerformed=true

goal154c2Accepted=true
goal154c2AcceptedByHuman=true
goal154c2AcceptedByCodex=false
goal154c2ManualReviewPerformed=true

goal154c3Accepted=true
goal154c3AcceptedByHuman=true
goal154c3AcceptedByCodex=false
goal154c3ManualReviewPerformed=true
goal154c3HumanGatePassed=true

goal154dAccepted=true
goal154dAcceptedByHuman=true
goal154dAcceptedByCodex=false
goal154dManualReviewPerformed=true
goal154dHumanGatePassed=true

goal154FamilyAcceptedImplementationCommit=fc2ac34db60d2627e1cafc86493396937bf63fe4
goal154FamilyAcceptanceRecordedByGoal=Goal155
```

Do not rewrite historical FAILED/BLOCKED implementation statuses into GREEN. Acceptance means the
owner accepted the completed family at Goal154D, not that every intermediate attempt was successful.

## Milestone result

Goal155 is an automated release-candidate integration milestone:

```text
goal155ImplementationStatus=GREEN only when all criteria pass
goal155CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal155Accepted=false
goal155AcceptedByHuman=false
goal155AcceptedByCodex=false
goal155ManualReviewRequired=false
goal155ManualGateReady=false
goal155IndependentAuditRequired=true
```

No new human gate is created. The next action after publication is independent audit and selection of
the next major product vertical slice.

## Mandatory read-first design packet

Read at most 12 primary files:

```text
AGENTS.md
docs/GOAL_DESIGN_QUALITY_POLICY.md
docs/UNITY_EXECUTION_POLICY.md
docs/CURRENT_GENERATOR_STATE.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
GameProjectWorkspaceModels.cs
GameProjectBuildAndQualificationService.cs
GameProjectBuildHistoryReader.cs
UnifiedGameProjectWorkspaceController.cs
ProjectsPageControl.cs
```

Before production edits create:

```text
.llmgc/procedural/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/rc-design-review.json
```

Required sections:

```text
acceptedProductFamilies
existingTypedFacts
missingPersistentRcFacts
savedCurrentLastSuccessStateMachine
standaloneCorrelationContract
ownerProjectProfile
maxInteractionProfile
failureRollbackMatrix
uiCardContract
nonGoals
```

Every section must be resolved before implementation. A vague or empty design packet blocks GREEN.

## Non-goals

Do not add:

```text
new gameplay mechanics
new Runtime primitives
new FeatureModules
new parameter types
public GamePackage schema changes
Unity scripts/scenes/prefabs/settings/packages changes
new host build
clean-machine installer
provider/network/live-geodata work
Lua/generator-library changes
historical 64-failure closure
full release packaging
```

No fixed product counts or content IDs may be embedded in generic production behavior. Exact values
and IDs are allowed in regression fixtures and evidence.


## A. Typed accepted-mechanics build summary

Create a generic Application-layer projection service, naming flexible:

```text
GameProjectAcceptedMechanicsSummaryService
```

Add a typed summary, naming flexible:

```text
GameProjectAcceptedMechanicsSummary
```

Required fields:

```text
Present
Passed
SelectedMechanicCount
ConfiguredParameterCount
QualifiedAuthoringFingerprint
EquipmentDamageBonus
StatDamageBonus
TotalAdditionalDamage
AbilityDirectDamage
ManaBefore
ManaSpent
ManaRemaining
StatusTickDamage
StatusExpired
Social
CheckpointReloadPassed
FullReplayEquivalent
ActionBindingPassed
HumanFacts[]
MissingFactKinds[]
Diagnostics[]
```

### A1. Data sources

Use only typed build/Runtime result fields:

```text
GameProjectBuildResult
GameProjectSocialSummary
qualified authoring fingerprint
checkpoint/full replay/action binding flags
```

Do not parse the WinForms card or standalone localized text back into data.

### A2. Generic readiness

The projection may not branch on module IDs or exact selected-module counts.

Accepted-mechanics readiness requires data-derived presence of:

```text
equipment fact
attribute/stat fact
progression fact
ability fact
mana fact
turn-status fact
complete social fact
checkpoint reload
full replay
action binding
nonempty qualified authoring fingerprint
```

Use existing typed summaries/fields to determine presence.

A project without the complete accepted family remains buildable. Its accepted-mechanics summary is:

```text
Present=true when any integrated fact exists
Passed=false
MissingFactKinds populated
```

It must not fail an otherwise valid game build merely because the operator did not select every
accepted optional mechanic.

For the required RC profiles all facts must be present and `Passed=true`.

### A3. Human facts

Produce concise Russian facts without IDs or hashes:

```text
Механики
Настроенные параметры
Бонус оружия
Бонус характеристик
Общий дополнительный урон
Прямой урон способности
Мана
Эффект по ходам
Репутация
Золото
Сохранение и повтор
```

Example values are derived, not hardcoded:

```text
Мана: 12 → 9
Эффект по ходам: урон 1; завершён
Сохранение и повтор: пройдено
```

Append the existing social facts by meaning, avoiding duplicate rows.

### A4. Persistence

Add the typed accepted-mechanics summary to:

```text
GameProjectBuildResult
GameProjectBuildHistoryEntry
UnifiedGameProjectWorkspaceSnapshot
```

A successful build writes it to GREEN build history.

`GameProjectBuildHistoryReader` restores it only from the same hash-validated GREEN entry already
used for the last successful build.

A failed build attempt must not replace it.

Older build-history entries without the new field remain readable and expose no accepted-mechanics
summary.

Do not bump the public GamePackage schema. An additive internal build-history field is allowed.

## B. Persistent release-candidate record

Create a confined project-local service, naming flexible:

```text
GameProjectReleaseCandidateRecordService
```

Project-relative path:

```text
.llmgc/release-candidate/accepted-mechanics-rc1.json
```

Vocabulary belongs in `UnifiedGameProjectWorkspaceVocabulary`.

### B1. Record shape

Create an internal typed record:

```text
schemaVersion=accepted_mechanics_release_candidate_v1
completedAtUtc
status=GREEN
projectPackageId
projectTitle
projectVersion
qualifiedAuthoringFingerprint
packageSha256
compositionPackageSha256
finalStateHash
acceptedMechanicsSummary
hostCacheKey
hostReused
hostRebuilt
launchSmokePassed
selfCheckPassedCount
selfCheckTotalCount
standalonePackageSha256
standaloneFinalStateHash
playerAdapterModelSha256
humanFactsSha256
```

### B2. Write contract

Write the record only after a successful `BuildWindowsStandalone()`.

Require:

```text
normal build Passed=true
accepted mechanics summary Passed=true
standalone Status=GREEN
standalone package/final hashes equal the build
HostReused=true
HostRebuilt=false
LaunchSmokePassed=true
selfCheckTotalCount>0
selfCheckPassedCount=selfCheckTotalCount
actual player-adapter-model.json exists
payload package/final hashes match the build
payload HumanReviewFacts contain all accepted-mechanics human facts
```

Read the actual payload, not a prepared request or constant.

Write atomically:

```text
temporary sibling
flush/close
atomic replace/move
```

A failed build or standalone attempt must not modify the previous valid record.

### B3. Read contract

On project open, read only the confined exact record path.

Reject or ignore with causal diagnostics:

```text
invalid JSON
unsupported schema
status not GREEN
missing fingerprint
package/composition/final hash mismatch
standalone package/final mismatch
accepted summary missing/failed
HostRebuilt=true
smoke/self-check failure
payload hash mismatch when the referenced payload exists
```

Configuration status:

```text
CURRENT:
  record fingerprint equals current semantic authoring fingerprint

LAST_SUCCESS:
  valid record exists but current saved authoring differs

UNKNOWN:
  valid old record exists but current fingerprint cannot be calculated

ABSENT:
  no valid record
```

Do not delete malformed records automatically.

### B4. Copy/reopen portability

The record must survive copying the complete project directory to a different short LocalAppData
path.

Project folder absolute paths must not be part of its identity or fingerprint.

On a copied project with the same package, authoring and activated hashes:

```text
record status=CURRENT
accepted mechanics facts restored
no rebuild required
```

## C. Workspace integration

Extend `UnifiedGameProjectWorkspaceSnapshot` with:

```text
AcceptedMechanics
ReleaseCandidate
ReleaseCandidateConfigurationStatus
ReleaseCandidateRecordPath
```

Naming may vary but the data must remain typed.

### C1. Open project

On `OpenProject()`:

```text
restore last successful build and AcceptedMechanics from matching GREEN history
read and validate release-candidate record
compare record fingerprint with current saved authoring
```

### C2. Build

After `BuildAndQualify()`:

```text
populate AcceptedMechanics
do not create a release-candidate record
status becomes BUILD_GREEN_STANDALONE_PENDING when summary Passed=true
```

### C3. Standalone

After `BuildWindowsStandalone()`:

```text
rebuild/qualify current project through the existing route
produce actual standalone
write RC record only after every correlation check
update snapshot to READY
```

### C4. Failure matrix

Require:

```text
failed normal build:
  prior accepted summary and RC record preserved

failed standalone:
  prior RC record preserved
  current attempt diagnostics visible

saved authoring change without build:
  prior summary/record visible as LAST_SUCCESS

return to semantically identical qualified values:
  status returns CURRENT without rebuild

successful build after a change, before standalone:
  accepted build summary CURRENT
  old RC record LAST_SUCCESS
  overall state BUILD_GREEN_STANDALONE_PENDING
```

Do not claim an old standalone record applies to a newly qualified package.

## D. Operator-facing RC card

Add one compact card to the ordinary Projects page:

```text
Принятые механики — Release Candidate
```

No new top-level page and no Goal-number controls.

### D1. States

```text
ABSENT:
  card hidden unless a complete accepted build exists

BUILD_GREEN_STANDALONE_PENDING:
  Статус: сборка пройдена; Windows RC ещё не подтверждён

READY/CURRENT:
  Статус: RC готов

LAST_SUCCESS:
  Статус: последняя успешная RC-проверка

UNKNOWN:
  Статус: последняя RC-проверка; соответствие текущим настройкам не подтверждено
```

### D2. Rows

Show concise rows derived from typed summaries:

```text
Механики
Настроенные параметры
Снаряжение и характеристики
Прогрессия
Способность и мана
Эффект по ходам
Репутация
Золото
Сохранение и повтор
Windows standalone
```

Example accepted benchmark presentation:

```text
Снаряжение и характеристики    +3 / +6 / +9
Способность и мана             урон 2; 12 → 9
Эффект по ходам                1 за ход; завершён
Репутация                      0 → 10
Золото                         0 → 10 → 17
Сохранение и повтор            пройдено
Windows standalone             cache reused; проверки пройдены
```

The UI must not expose:

```text
module IDs
quest/ability/status IDs
package/final hashes
authoring fingerprint
absolute paths
raw Runtime events
```

Those remain in Technical Details/evidence.

### D3. Layout

Require:

```text
readable at 1100x720
word wrap
no clipping
no duplicate social card content
existing social card may remain, but RC card summarizes rather than repeats every social row
failed attempt does not erase the last successful RC card
fresh reopen restores the card from project-local records
```

Do not require a new human screenshot or manual gate.

## E. Standalone human facts

Refactor only when necessary so the same generic accepted-mechanics fact projection supplies:

```text
WinForms RC card
ProjectStandaloneBuildRequest.HumanReviewFacts
RC-record payload comparison
```

Avoid two independently formatted fact sets.

The existing standalone payload must continue to carry the detailed accepted facts.

Add one concise fact:

```text
Release Candidate = готов
```

only when the accepted-mechanics summary is complete and the request is being assembled for the
current qualified configuration.

Do not change Unity host code. It already renders arbitrary HumanReviewFacts.


## F. Required release-candidate profiles

Use only disposable copies. The source project is always read-only:

```text
%LOCALAPPDATA%\LLMGameCreator\Games\goal148-manual
```

Capture the source tree manifest before any copy is opened by Application services and require it to
remain byte-identical after all proofs.

### F1. Profile A — exact owner-current project

Copy the entire project, including saved authoring and build history, without resetting authoring.

Require before changes:

```text
selected mechanics=22
explicit configured parameters=10
all currently selected profile/social/combat modules remain selected
authoring configuration status=CURRENT
Goal154 accepted social values=0/10/5/10/7
Goal153 values=2/12/3/5/1
```

Do not toggle modules or rewrite parameters.

Run:

```text
open
build
repeat build
dispose/recreate controller
reopen
```

Require:

```text
both builds GREEN
AcceptedMechanics Passed=true
selected/configured counts remain 22/10
package/composition/final hashes deterministic
checkpoint/full replay/action binding GREEN
social 0→10 and gold 0→10→17
ability damage 2
mana 12→9
status tick damage 1 and expired
fresh reopen accepted summary CURRENT
source project byte-identical
```

No standalone smoke is run on Profile A.

### F2. Profile B — maximal accepted-value interaction profile

Create a separate copy from the same read-only source.

Keep every selected module from Profile A.

Set the complete accepted parameter profile:

```text
equipment:
  weaponDamageBonus=3

attributes:
  startingStrength=8
  damagePerStrengthPoint=2

progression:
  level2RequiredExperience=12

active ability:
  directDamage=2

mana/spell:
  startingMana=12
  manaCost=3

turn status:
  durationTurns=5
  tickDamage=1

social:
  startingReputation=0
  questReputationReward=10
  questFailurePenalty=5
  trustedReputationThreshold=10
  trustedGoldReward=7
```

The exact module/parameter IDs are regression-fixture inputs only. Production code remains generic.

Expected explicit configured parameter count:

```text
14
```

Run:

```text
save
dispose/recreate
reopen
build
repeat build
fresh reopen
```

Require:

```text
selected mechanics remain 22
configured parameters=14
equipment/stat/total bonuses=3/6/9
progression level2 required amount=12
ability direct damage=2
mana 12→9
five turn-status ticks of 1 and expiry
social reputation 0→10
quest gold 0→10
trusted reward +7
final gold 17
all-selected precompleted-quest path remains truthful
checkpoint/full replay/action binding GREEN
repeat build deterministic
AcceptedMechanics Passed=true
ReleaseCandidateConfigurationStatus before standalone=BUILD_GREEN_STANDALONE_PENDING
```

### F3. Profile B standalone

Before standalone:

```text
verify complete host cache
hash host executable and file set
assert zero Unity processes
```

Call `BuildWindowsStandalone()` exactly once.

Require:

```text
Status=GREEN
HostReused=true
HostRebuilt=false
Unity process count before/after=0
LaunchSmokePassed=true
selfCheckPassedCount=selfCheckTotalCount>0
host executable/file-set hashes unchanged
actual payload package/final hashes match the build
actual payload contains every accepted-mechanics fact
Release Candidate=готов
RC record written atomically
snapshot ReleaseCandidate status=READY/CURRENT
```

No second real standalone smoke is allowed.

### F4. Portable copy

After Profile B RC record is complete, copy the whole disposable project to a second short LocalAppData
path.

Without build or standalone:

```text
open copied project
```

Require:

```text
AcceptedMechanics restored
RC record restored
configuration status=CURRENT
human facts byte-equivalent
no absolute source path dependency
no build/Runtime/Unity execution
```

### F5. Last-success and rollback matrix

On a separate Profile B copy with a valid RC record:

1. Change one accepted parameter and save without build:
   ```text
   accepted summary/RC record remain visible
   status=LAST_SUCCESS
   ```
2. Return to the semantically identical accepted value and save:
   ```text
   status=CURRENT without rebuild
   ```
3. Change to a valid different value and build, but do not standalone:
   ```text
   build summary CURRENT
   old RC record LAST_SUCCESS
   overall RC state=BUILD_GREEN_STANDALONE_PENDING
   ```
4. Attempt an invalid value:
   ```text
   build rejected
   last successful build summary preserved
   previous RC record preserved
   activated package unchanged
   ```
5. Inject a failed capturing standalone service:
   ```text
   previous RC record byte-identical
   failure diagnostics visible
   no Unity process
   ```

### F6. Incomplete composition

Create a disposable project with only required core modules.

Require:

```text
normal build remains GREEN
AcceptedMechanics Passed=false
MissingFactKinds nonempty
RC card does not claim READY
no RC record written
default-off historical package/final hashes remain unchanged where existing fixtures define them
```

This proves the RC layer is an optional readiness projection, not a new build requirement.

## G. Behavioral test matrix

Create at least 28 Goal155 tests; at least 24 must be behavioral.

A behavioral test invokes real projection/history/record/controller/WinForms/standalone services and
asserts resulting files, state or process plans. Reflection/source-string tests do not count.

Required tests:

### Human acceptance ledger

1. exact owner acceptance statement is recorded byte-for-byte;
2. all named Goal154-family acceptance flags are true/by-human and never by-Codex;
3. historical intermediate implementation statuses remain honest;
4. accepted implementation commit equals the required base.

### Accepted-mechanics projection

5. complete Profile A build produces Passed summary;
6. complete Profile B build produces Passed summary;
7. Profile B exact `3/6/9`, `2`, `12→9`, status tick/expiry and social facts;
8. missing equipment fact is reported without failing build;
9. missing ability/mana/status/social facts are classified independently;
10. replay/action-binding failure makes summary not Passed;
11. human facts contain no IDs/hashes/paths;
12. summary is deterministic under repeated build.

### History/current truth

13. GREEN build history persists summary;
14. fresh reopen restores summary before rebuild;
15. failed build preserves prior summary;
16. old history without summary remains readable;
17. saved semantic change returns LAST_SUCCESS;
18. return to same semantic values returns CURRENT;
19. new build before standalone reports BUILD_GREEN_STANDALONE_PENDING.

### RC record

20. successful standalone writes one valid atomic record;
21. record reader validates package/composition/final/fingerprint;
22. malformed/unsupported/mismatched records are rejected causally;
23. failed standalone does not replace prior record;
24. copied project restores CURRENT record without execution;
25. absolute project folder does not affect record identity;
26. old record becomes LAST_SUCCESS after saved change;
27. new build without new standalone cannot claim old record CURRENT;
28. incomplete composition never writes record.

### WinForms

29. Profile A build-pending card is readable;
30. Profile B READY card shows integrated values;
31. LAST_SUCCESS/UNKNOWN headings are truthful;
32. failed attempts preserve the visible prior card;
33. fresh reopen restores card;
34. card contains no raw IDs/hashes/paths;
35. layout remains readable at normal project-page dimensions.

### Standalone

36. exactly one real hidden smoke;
37. host reused and never rebuilt;
38. zero Unity process starts;
39. actual payload contains all Profile B facts plus `Release Candidate=готов`;
40. payload and normal build hashes correlate;
41. host files remain unchanged;
42. copied record does not trigger a smoke.

### Regressions

43. exact owner Profile A remains 22/10 and deterministic;
44. all-current selectable optional composition remains GREEN;
45. Goal154D explicit/precompleted paths remain GREEN;
46. Goals153C, Goal150 custom profile and Goal149 equipment regressions remain GREEN;
47. default-off baseline remains unaffected;
48. source `goal148-manual` is byte-identical.

Do not claim counts from this list unless the corresponding test is actually discovered and executed.

## H. Validation strategy

### Required focused commands

```powershell
dotnet build

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj -c Debug --no-build --list-tests --filter "FullyQualifiedName~Goal155"
# require discovered >=28 and behavioral >=24

dotnet test ... --filter "FullyQualifiedName~Goal155"
dotnet test ... --filter "FullyQualifiedName~Goal154D"
dotnet test ... --filter "FullyQualifiedName~Goal154C3"
dotnet test ... --filter "FullyQualifiedName~Goal153C"
dotnet test ... --filter "FullyQualifiedName~Goal150AParameterizedRuntimeContractSynchronization"
dotnet test ... --filter "FullyQualifiedName~Goal149"
dotnet test ... --filter "FullyQualifiedName~CapabilityDrivenRuntimePlaythrough"
dotnet test ... --filter "FullyQualifiedName~UnifiedGameProjectWorkspace"
dotnet test ... --filter "FullyQualifiedName~ProjectsPage"
dotnet test ... --filter "FullyQualifiedName~ProjectStandaloneBuild"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleLibrary"
dotnet test ... --filter "FullyQualifiedName~FeatureModuleCertification"
dotnet test ... --filter "FullyQualifiedName~RuntimeNarrative"
```

Run:

```powershell
.\.devflow\scripts\run-capability-runtime-equipment-slice.ps1
.\.devflow\scripts\run-character-attributes-level-progression-slice.ps1
.\.devflow\scripts\check-current-goal.ps1
```

Then run the Profile A/Profile B/portable/rollback matrix and exactly one Profile B hidden smoke.

Run artifact-scope validation last.

### Forbidden validation

Do not run:

```text
full suite
85-case historical closure
all-ProductSmoke
Unity host build
visible automatic standalone launch
unchanged timed-out command retry
timeout escalation loop
```

A zero-match test filter is a failure.


## I. Evidence contract

Create exactly 12 files in each mirrored root:

```text
goal155-dashboard.json
goal154-human-acceptance-record.json
rc-design-review.json
owner-project-integration-proof.json
max-interaction-profile-proof.json
build-summary-persistence-proof.json
release-candidate-record-proof.json
winforms-rc-card-proof.json
standalone-payload-proof.json
focused-regression-proof.json
artifact-scope-proof.json
goal155-report.md
```

Roots:

```text
.llmgc/procedural/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/
.llmgc/exports/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/
```

Procedural/export twins must be byte-identical by relative name and SHA-256.

### Dashboard fields

```text
status
candidateStatus
goal154FamilyAccepted
goal154AcceptedImplementationCommit
goal155TestsDiscovered
goal155BehavioralTestsPassed

ownerSelectedMechanicCount
ownerConfiguredParameterCount
ownerBuildPassed
ownerRepeatBuildDeterministic
ownerFreshReopenCurrent

benchmarkSelectedMechanicCount
benchmarkConfiguredParameterCount
benchmarkEquipmentDamageBonus
benchmarkStatDamageBonus
benchmarkTotalAdditionalDamage
benchmarkAbilityDirectDamage
benchmarkManaBefore
benchmarkManaRemaining
benchmarkStatusTickDamage
benchmarkStatusExpired
benchmarkReputationBefore
benchmarkReputationAfter
benchmarkGoldAfterQuest
benchmarkGoldAfterClaim
benchmarkCheckpointReloadPassed
benchmarkFullReplayEquivalent
benchmarkActionBindingPassed

acceptedMechanicsSummaryPersisted
releaseCandidateRecordWritten
releaseCandidateRecordCurrent
portableCopyRecordCurrent
failedBuildPreservedLastSuccess
failedStandalonePreservedRecord

hostCacheKey
hostReused
hostRebuilt
hostFileSetHashUnchanged
unityProcessStartCount
hiddenSmokeInvocationCount
hiddenSmokePassed
standaloneSelfChecksPassed
actualPayloadAcceptedFactsPassed

sourceProjectByteIdentical
goal154dRegressionPassed
goal153cRegressionPassed
goal150RegressionPassed
goal149RegressionPassed
defaultOffRegressionPassed

artifactScopeViolationCount
goal155Accepted=false
goal155ManualReviewRequired=false
goal155IndependentAuditRequired=true
```

No required GREEN field may be:

```text
null
PARTIAL
NOT_EXECUTED
unverified constant
source-string-only assertion
```

### Human acceptance evidence

`goal154-human-acceptance-record.json` must include:

```text
status=ACCEPTED_BY_HUMAN
acceptedAtRepositoryBase=fc2ac34db60d2627e1cafc86493396937bf63fe4
exactStatement=<exact user statement>
acceptedGoalIds=[154,154A,154B,154B1,154C,154C1,154C2,154C3,154D]
acceptedByCodex=false
manualInputCommitted=false
recordedByGoal=155
```

The statement is repository documentation of the human decision. Do not create or claim an external
signature.

## J. Documentation and source-of-truth

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md
```

Update the relevant Goal154 manual files and create:

```text
docs/manual-acceptance/goal154-family-human-acceptance.md
docs/manual-acceptance/goal155-accepted-mechanics-release-candidate.md
```

At minimum update:

```text
goal154-faction-reputation-social-consequences.md
goal154a-social-lifecycle-runtime-proof.md
goal154b-executable-social-runtime-core.md
goal154b1-quest-reward-preservation.md
goal154c-saved-project-winforms-standalone-social-closure.md
goal154c1-persisted-social-result-real-project-standalone.md
goal154c2-authoring-fingerprint-final-closure.md
goal154c3-final-publication-closure.md
goal154d-all-selected-precompleted-quest-hotfix.md
```

### Required current-state publication on GREEN

```text
goal154Accepted=true
goal154AcceptedByHuman=true
goal154AcceptedByCodex=false
goal154ManualReviewPerformed=true
goal154ManualGateReady=false
goal154ManualGateStatus=ACCEPTED_BY_HUMAN

goal154aAccepted=true
goal154aAcceptedByHuman=true
goal154aAcceptedByCodex=false
goal154aManualReviewPerformed=true

goal154bAccepted=true
goal154bAcceptedByHuman=true
goal154bAcceptedByCodex=false
goal154bManualReviewPerformed=true

goal154b1Accepted=true
goal154b1AcceptedByHuman=true
goal154b1AcceptedByCodex=false
goal154b1ManualReviewPerformed=true

goal154cAccepted=true
goal154cAcceptedByHuman=true
goal154cAcceptedByCodex=false
goal154cManualReviewPerformed=true

goal154c1Accepted=true
goal154c1AcceptedByHuman=true
goal154c1AcceptedByCodex=false
goal154c1ManualReviewPerformed=true

goal154c2Accepted=true
goal154c2AcceptedByHuman=true
goal154c2AcceptedByCodex=false
goal154c2ManualReviewPerformed=true

goal154c3Accepted=true
goal154c3AcceptedByHuman=true
goal154c3AcceptedByCodex=false
goal154c3ManualReviewPerformed=true
goal154c3HumanGatePassed=true

goal154dAccepted=true
goal154dAcceptedByHuman=true
goal154dAcceptedByCodex=false
goal154dManualReviewPerformed=true
goal154dHumanGatePassed=true

goal154FamilyAcceptedImplementationCommit=fc2ac34db60d2627e1cafc86493396937bf63fe4
goal154FamilyAcceptanceRecordedByGoal=Goal155

goal155ImplementationStatus=GREEN
goal155CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal155Accepted=false
goal155AcceptedByHuman=false
goal155AcceptedByCodex=false
goal155ManualReviewRequired=false
goal155ManualGateReady=false
goal155IndependentAuditRequired=true

goal155OwnerProfile=22/10
goal155MaxInteractionProfile=22/14
goal155MaxInteractionDamage=3/6/9
goal155MaxInteractionAbilityManaStatus=2/12->9/1/expired
goal155MaxInteractionSocial=0->10/0->10->17
goal155ReleaseCandidateRecordPassed=true
goal155PortableCopyPassed=true
goal155HostReused=true
goal155HostRebuilt=false
goal155UnityProcessStartCount=0
goal155HiddenSmokeInvocationCount=1
goal155ArtifactScopeViolationCount=0

nextAction=independent_goal155_audit_and_select_next_major_product_vertical_slice
```

Remove stale active prose saying Goal154 acceptance is pending. Historical failed-attempt prose may
remain only when explicitly labeled historical.

### Release risk publication

On GREEN state clearly:

```text
Goals149–154 accepted-mechanics RC integration has no known P0/P1 from the required matrix.
Historical 64-test closure remains validation debt and is not reopened.
Clean-machine install and final release packaging remain future milestone work.
Goal155 creates no human gate.
```

P2/P3 findings go to the debt register with owner, impact and defer rule.

## K. Artifact scope

Initially allowed:

```text
.devflow/artifact-scope/artifact-scope-policy.json
.devflow/scripts/run-goal155-accepted-mechanics-rc.ps1
.devflow/scripts/run-goal155-accepted-mechanics-rc.cmd

src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectWorkspaceModels.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectAcceptedMechanicsSummaryService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectReleaseCandidateRecordService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildAndQualificationService.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/GameProjectBuildHistoryReader.cs
src/LLMGameCreator.Application/Design/UnifiedGameProjectWorkspace/UnifiedGameProjectWorkspaceController.cs

src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.cs
src/LLMGameCreator.WinForms/Pages/Projects/ProjectsPageControl.Designer.cs

tests/LLMGameCreator.Tests/Application/Goal155/Goal155HumanAcceptanceLedgerTests.cs
tests/LLMGameCreator.Tests/Application/Goal155/Goal155AcceptedMechanicsProjectionTests.cs
tests/LLMGameCreator.Tests/Application/Goal155/Goal155OwnerAndBenchmarkProjectTests.cs
tests/LLMGameCreator.Tests/Application/Goal155/Goal155ReleaseCandidateRecordTests.cs
tests/LLMGameCreator.Tests/Application/Goal155/Goal155WinFormsRcCardTests.cs
tests/LLMGameCreator.Tests/Application/Goal155/Goal155StandaloneAndPortabilityTests.cs
tests/LLMGameCreator.Tests/Application/UnifiedGameProjectWorkspace/Goal155AcceptedMechanicsWorkspaceTests.cs
tests/LLMGameCreator.Tests/Application/ProjectStandaloneBuild/Goal155ReleaseCandidatePayloadTests.cs

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/MILESTONE_GATES.md
docs/RELEASE_RISK_REGISTER.md
docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md

docs/manual-acceptance/goal154-faction-reputation-social-consequences.md
docs/manual-acceptance/goal154a-social-lifecycle-runtime-proof.md
docs/manual-acceptance/goal154b-executable-social-runtime-core.md
docs/manual-acceptance/goal154b1-quest-reward-preservation.md
docs/manual-acceptance/goal154c-saved-project-winforms-standalone-social-closure.md
docs/manual-acceptance/goal154c1-persisted-social-result-real-project-standalone.md
docs/manual-acceptance/goal154c2-authoring-fingerprint-final-closure.md
docs/manual-acceptance/goal154c3-final-publication-closure.md
docs/manual-acceptance/goal154d-all-selected-precompleted-quest-hotfix.md
docs/manual-acceptance/goal154-family-human-acceptance.md
docs/manual-acceptance/goal155-accepted-mechanics-release-candidate.md

docs/agent-tasks/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/
.llmgc/procedural/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/
.llmgc/exports/goal-155-accepted-mechanics-release-candidate-integration-and-operator-readiness/
```

If a concrete compile/test failure proves one additional existing Application/WinForms/test path is
required, record the exact reason and add only that exact path.

### Forbidden without a newly reproduced RC P0/P1

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.Application/Design/FeatureModuleComposition/**
src/LLMGameCreator.Application/Design/FeatureModuleAuthoring/**
src/LLMGameCreator.Application/Design/ProjectStandaloneBuild/**
catalogs/feature-modules/**
unity/**
samples/**
generator-library/**
provider/**
LLM/**
RAG/**
public GamePackage schema
```

If the RC matrix reproduces a P0/P1 requiring a forbidden path:

```text
record the reproduction
expand to the exact file only
implement and prove it in Goal155
do not create Goal155A
```

A public schema, Unity host rebuild, provider/network dependency or incompatible saved-project
migration is an external blocker and must publish BLOCKED rather than being forced into scope.

## L. Text and evidence integrity

Scan the actual candidate commit files:

```text
valid UTF-8
no NUL
no forbidden C0 controls except CR/LF/TAB
no mojibake markers
no escaped Cyrillic in user-facing JSON/Markdown where repository policy forbids it
no absolute disposable/source paths in committed evidence
```

The exact human acceptance statement must retain its original Cyrillic and punctuation.

Historical evidence roots remain immutable.

## M. Command budget

```text
read-first and RC design packet: maximum 10 minutes
acceptance ledger/current-state foundation: maximum 6 minutes
typed summary/history/record integration: maximum 18 minutes
WinForms RC card and fact consolidation: maximum 10 minutes
behavioral test implementation: maximum 20 minutes
Profile A/Profile B/portable/rollback matrix: maximum 12 minutes
one cached smoke and payload inspection: maximum 5 minutes
focused regressions/evidence/docs/artifact scope: maximum 14 minutes
target wall clock: 75 minutes
maximum two concurrent testhost processes
Unity process count: 0
```

Rules:

```text
write test inventory before production edits
prepare evidence/publication script before long external proofs
no unchanged command repetition
no timeout escalation loop
after a failure run only the exact failing class/test
do not defer docs/evidence/artifact scope after product code
do not commit generated disposable projects or standalone outputs
```

## N. Publication

Create exactly one final commit:

```text
GREEN Goal 155 accepted mechanics release candidate integration and operator readiness
```

or honest:

```text
BLOCKED Goal 155 accepted mechanics release candidate integration and operator readiness
FAILED Goal 155 accepted mechanics release candidate integration and operator readiness
```

Codex performs commit and standard push.

Required final state:

```text
HEAD == origin/main
worktree clean
exactly one commit from required base
three Goal155 task files tracked
source goal148-manual byte-identical
Unity process start count=0
HostRebuilt=false
hidden smoke invocation count=1 only on GREEN
Goal154 family accepted by human
Goal155 accepted=false
Goal155 manual review required=false
```

A coherent BLOCKED/FAILED result with task-owned changes is still committed and pushed.

## O. GREEN criteria

```text
exact Goal154 human acceptance recorded
all named Goal154-family accepted flags correct
historical implementation statuses preserved

Goal155 discovered tests >=28
Goal155 behavioral tests >=24
all Goal155 tests pass

Profile A exact owner project 22/10 passes build/repeat/reopen
Profile A source remains byte-identical

Profile B exact accepted values persist as 22/14
Profile B proves 3/6/9 equipment/stat/total
Profile B proves ability 2, mana 12→9, status tick 1 and expiry
Profile B proves reputation 0→10 and gold 0→10→17
Profile B checkpoint/full replay/action binding pass
Profile B repeat build deterministic

typed AcceptedMechanics summary persists and restores
failed build preserves last success
saved/current/last-success semantics are truthful

exactly one Profile B cached hidden standalone smoke
HostReused=true
HostRebuilt=false
Unity process starts=0
self-checks all pass
actual payload contains all accepted facts and RC-ready fact
host file set unchanged

atomic RC record written
record validates build/standalone/payload correlation
portable copied project restores CURRENT record without execution
failed standalone preserves previous record
new build without standalone cannot reuse old record as CURRENT

WinForms RC card is concise, readable and free of IDs/hashes/paths
incomplete composition remains buildable and cannot claim RC READY

Goal154D/153C/150/149/default-off regressions pass
historical 64-failure closure is not reopened
12+12 evidence files are byte-identical
new text integrity passes
current-state/queue/milestone/risk/debt/manual docs are coherent
artifact scope violation count=0

goal155CandidateStatus=GREEN_ACCEPTABLE_CANDIDATE
goal155Accepted=false
goal155ManualReviewRequired=false
one final commit pushed
```

## P. Final report

Return GREEN, BLOCKED or FAILED and include:

- model/reasoning used;
- initial worktree inventory;
- RC design review result;
- exact Goal154 human acceptance record and flags;
- Goal155 discovered/behavioral test counts;
- Profile A 22/10 build/repeat/reopen facts;
- Profile B 22/14 integrated values;
- accepted-mechanics typed summary and history recovery;
- RC record write/read/current/last-success/portable-copy results;
- WinForms RC card states and rows;
- failed build/failed standalone preservation;
- host key/hash/reuse, Unity count and exact smoke count;
- actual standalone payload facts;
- focused regression counts;
- source-project immutability;
- evidence mirror/text integrity;
- artifact scope;
- Goal155 candidate/acceptance/manual-review state;
- final SHA/push/HEAD/worktree;
- confirmation no new human gate was created.
