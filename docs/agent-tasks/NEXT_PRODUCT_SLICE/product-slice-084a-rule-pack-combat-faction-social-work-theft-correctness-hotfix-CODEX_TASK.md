# Product Slice 084A - Goal 009 Acceptance Correctness Hotfix

## Purpose

Repair false-positive exact-binding, command-correlation, reputation-clamping and cross-scenario-isolation evidence found during external review of Goal 009.

This is a bounded correctness hotfix for S078-S084. It is not S085 or Goal 010. Keep `rule_pack_combat_faction_social_work_theft_artifact_verification` as the only final gate and do not mark it passed.

## Confirmed Review Findings

The real adapter does construct `GameRuntimeService`, `GameRuntimeStateFactory` and the production encounter/faction/dialogue/interaction/container services. Valid command execution, combat reward, work output, theft transfer and save/load evidence are real.

The gate is still rejected because these acceptance claims are currently too weak:

1. Cross-scenario isolation is derived from `ExpectedScenarioStateMarker.StartsWith("leak:")` and absence of a metadata key that no previous scenario ever writes. The invalid fixture does not inject previous scenario state/evidence, so it fails because of its marker text rather than actual leakage.
2. Command coverage checks only that `SourceDeclarationId` belongs to a selected declaration. It does not prove that command type, target, secondary target, actor, amount and value are authorized by that declaration.
3. Package binding audit checks partial ID existence but does not fully verify ability participant/resource/cost/reward bindings, dialogue node/choice outputs, work interaction-to-transaction mapping, or theft flag/reputation consequence values.
4. Faction acceptance proves `0 -> 12`, not clamping. Runtime acceptance only requires a changed faction delta/event and never compares the actual result to declaration `ExpectedAfter`.
5. Several category predicates accept any non-empty delta instead of the exact declared result. A real command affecting the wrong valid package target can therefore be misreported as declaration-backed success.

## Read First

Read only:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. this task
5. `docs/GOAL_009_RULE_PACK_COMBAT_FACTION_SOCIAL_WORK_THEFT.md`
6. the Goal 009 Application service, real runtime adapter, focused tests and smoke test
7. directly used production runtime/domain definitions only when needed to confirm exact semantics

Do not read historical packs or broad roadmaps.

## Allowed Files

- `src/LLMGameCreator.Application/Design/Gameplay/RulePackCombatFactionSocialWorkTheftAcceptanceService.cs`
- `tests/LLMGameCreator.Tests/Application/Gameplay/RulePackCombatFactionSocialWorkTheftRealRuntimeAdapter.cs`
- `tests/LLMGameCreator.Tests/Application/Gameplay/RulePackCombatFactionSocialWorkTheftAcceptanceTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/RulePackCombatFactionSocialWorkTheftSmokeTests.cs`
- `.llmgc/procedural/rule-pack-combat-faction-social-work-theft/*`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md` only if current gate wording requires correction

Do not edit runtime production files, public schemas, `.sln` or `.csproj` unless a focused test exposes a real blocker. Stop and report that blocker instead of broadening this hotfix.

## Required Fixes

### 1. Audit exact command authorization by declaration

For every command, validate more than `SourceDeclarationId` membership. The selected declaration must authorize the exact command shape.

At minimum validate:

- combat start: exact encounter id;
- combat ability: exact declared ability id, source participant and target participant;
- combat AI: selected encounter declaration and an active eligible AI participant;
- faction change: exact faction id and declared amount;
- social open/choice: exact dialogue and choice ids;
- work execution: exact interaction and transaction binding;
- work completion flag: exact declared flag id and supported value;
- theft open/take: exact container, item and positive declared amount;
- theft flag: exact declared theft flag id/value;
- theft reputation consequence: exact faction and declared penalty;
- no command type unrecognized by the selected declaration family.

Emit stable diagnostics such as `combat_family.audit.command_type_not_declared`, `command_target_mismatch`, `command_amount_mismatch` or similarly precise codes. Binding failure must block runtime.

Add negative fixtures where a selected valid declaration id is retained but target, secondary target, amount, flag or command type is changed to another existing valid package id. These must fail the binding audit before runtime. A missing/unselected declaration id alone is insufficient coverage.

### 2. Complete exact package binding audit

Verify selected declarations against actual package structures:

- encounter contains each exact participant once;
- participants expose their declared abilities and resources;
- ability resource target, costs and supported output/effect refs resolve;
- encounter reward item/output resolves;
- faction default/min/max support the declared before, amount, clamp and expected result;
- dialogue contains the exact declared node and choice in that node;
- declared social faction, flag and item consequences match the choice outputs;
- work interaction metadata points to the exact transaction;
- work required/reward items exist and match transaction requirements/outputs;
- work completion flag command matches the work declaration;
- theft inventory is a real container with the declared item/amount;
- theft flag, faction and reputation penalty match the declared consequence commands.

Do not count an id as audited merely because `DeclarationRuntimeIds` copied it from the declaration.

### 3. Validate exact runtime results per declaration

Replace generic `Changed` acceptance with declaration-specific expected evidence.

Require, as applicable:

- exact encounter id, turn/actor/target/ability, resource cost, health delta and action history;
- resolution scenario defeated target, inactive/resolved encounter and exact reward delta;
- exact faction before/after and clamped expected value;
- exact dialogue/node/choice history plus the declared flag/item/reputation effects;
- exact work requirement consumption/preservation semantics, wage amount and completion flag;
- exact container decrease, player increase, theft flag and declared reputation penalty;
- combined loop exact ordered command-to-declaration correlation and final state.

Application validation must reject an adapter that reports a real successful command and real delta against the wrong target or wrong amount.

### 4. Add real reputation clamping proof

The valid faction scenario must exercise a value that crosses a package min/max boundary and prove the runtime clamps to the exact declared `ExpectedAfter`.

Record structured before, requested amount, unclamped candidate, bounds and actual clamped result, or equivalent deterministic evidence. Tests and smoke must assert the clamp, not only `FactionDelta.Changed` or an event type.

Keep the theft penalty scenario separately exact.

### 5. Replace marker-based isolation with actual sequential-state proof

Run all valid scenarios sequentially through the same adapter instance and retain enough deterministic prior-scenario identity to check the next initial state.

For each scenario prove that its initial state contains none of the previous scenario's dynamic:

- active/resolved encounter state or action history;
- dialogue/history;
- reputation changes;
- reward/work/theft inventory deltas;
- flags;
- command log or scenario marker.

Return structured isolation evidence including previous scenario id/signature, current initial-state signature and unexpected retained keys/ids. Overall isolation must be computed from this evidence.

The negative fixture must actually inject or return previous-scenario state/evidence and be rejected because concrete retained fields are detected. Remove acceptance/rejection logic based solely on a scenario id or `leak:` marker prefix.

It is acceptable for the adapter to create a clean state per run; the proof must compare that clean initial state against tracked prior dynamic evidence. The negative wrapper/fixture must contaminate it with real prior evidence.

### 6. Keep failure causality honest

- Binding-invalid scenarios do not run.
- Runtime-invalid scenarios run through the real runtime and fail by its real diagnostic/event.
- Fake adapter success still fails structurally.
- Save/load mismatch remains an actual restored-state mismatch.
- `ExpectedValid`, scenario id and state-marker text remain expectation/fixture routing only and never directly decide `ActualValid`.

Overall invalid acceptance must require each named invalid scenario's intended diagnostic family, not merely any error produced later from empty runtime evidence.

### 7. Keep deterministic artifacts clean

Regenerate the same three files under:

```text
.llmgc/procedural/rule-pack-combat-faction-social-work-theft/
```

Do not include GUID temp paths, timestamps, machine names or absolute paths. Clean up any temporary snapshot directory created by tests/adapters in `finally`/`Dispose` without hiding save/load failures.

## Required Tests

Add focused behavioral tests for at least:

1. same selected declaration plus wrong existing command target is rejected before runtime;
2. same declaration plus wrong ability actor/target is rejected;
3. same declaration plus wrong amount/flag/value is rejected;
4. missing/mismatched participant ability/resource/cost/reward binding is rejected;
5. dialogue choice in the wrong node or with mismatched declared outputs is rejected;
6. work interaction mapped to the wrong existing transaction is rejected;
7. theft command amount/flag/reputation penalty mismatch is rejected;
8. faction scenario crosses a bound and proves exact clamped result;
9. successful real delta for the wrong target cannot satisfy runtime evidence;
10. valid scenarios expose declaration-specific exact result evidence;
11. valid scenarios run sequentially through one adapter and prove clean initial state against prior dynamic evidence;
12. actual injected prior encounter/dialogue/faction/inventory/flag/command evidence fails isolation;
13. removing the injected leakage makes that expected-invalid fixture fail the expected-invalid matrix;
14. existing real runtime failures, fake success, save/load mismatch and all seven valid scenarios remain covered;
15. repeated combined output remains byte/hash stable.

Tests and smoke must deserialize/assert structured evidence. Do not validate only top-level booleans or strings.

## State And Gate

Record S084A as a Goal 009 correctness hotfix, but keep:

```text
rule_pack_combat_faction_social_work_theft_artifact_verification
```

required and not passed. Do not create or recommend S085/Goal 010 in this run.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~RulePackGameplayFamily|FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario rule-pack-combat-faction-social-work-theft
.\.devflow\scripts\check-all.ps1
```

Search changed/generated files for mojibake and accidental post-Goal-009 markers. Exclude the Goal/task prohibition documents from marker search.

## Stop Conditions

Stop and report a blocker rather than weakening acceptance if:

- exact declaration execution needs a new public runtime command/state contract;
- real clamping or isolation cannot be proven through existing runtime/state seams;
- a public GamePackage schema or project-reference change is required;
- a second Application gameplay simulator would be required;
- full verification exposes an unrelated existing failure.

## Hard Limits

- No S085 or Goal 010.
- No new manual gate.
- No git commands or branch operations.
- No UI/WinForms/Designer work.
- No Unity, Lua, LLM/RAG, provider/media or generator execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No content-specific production execution branches.
- Use repository-relative Windows/PowerShell paths only; never use `/mnt`, `/home/oai`, `sandbox:/...` or fabricated `C:\mnt` paths.

## Final Report

Report:

- each repaired false-positive root cause;
- changed files;
- exact package and command-to-declaration audit rules;
- clamping before/request/bounds/after evidence;
- sequential valid-scenario isolation and actual injected-leak rejection evidence;
- declaration-specific runtime result assertions;
- focused/smoke/full verification totals;
- regenerated artifact folder and report hash;
- temp snapshot cleanup result;
- confirmation that public schemas/project files were unchanged;
- confirmation that the gate remains required and S085/Goal 010 were not started.
