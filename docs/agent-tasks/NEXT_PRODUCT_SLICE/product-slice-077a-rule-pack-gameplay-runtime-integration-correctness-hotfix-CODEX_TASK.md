# Product Slice 077A - Rule-Pack Gameplay Runtime Integration Correctness Hotfix

## Purpose

Repair false-positive runtime and save/load acceptance found during external review of Goal 008.

This is a bounded correctness hotfix for S071-S077. It is not S078 or Goal 009. Keep `rule_pack_gameplay_family_artifact_verification` as the only final gate and do not mark it passed.

## Confirmed Defects

The pushed Goal 008 artifacts are deterministic, but they do not yet prove the requested production runtime chain.

1. `DefaultRulePackGameplayFamilyRuntimeAdapter` reimplements use-item, equipment, recipe, transaction and flag behavior inside the Application acceptance service. It does not call the existing `GameRuntimeService`, `UseItemRuntimeService`, `EquipmentRuntimeService`, `RecipeRuntimeService` or `TransactionRuntimeService`.
2. `RuntimeStartSucceeded` is assigned `true`; the adapter does not start through the existing runtime state factory/service.
3. Save/load serializes a custom `RuntimeStateSnapshot` projection and deserializes that same projection. It does not serialize/restore `GameRuntimeState` through the existing runtime serializer/snapshot seam.
4. Binding audit lists all declarations globally but does not prove that each scenario's `SourceDeclarationIds` exists and maps to that scenario's exact package ids/commands.
5. Evidence categorization is misleading: generic inventory differences populate crafting/trade item lists even for equip/use commands. Category evidence must be attributable only to the matching command family.
6. `decl/status/focused` declares three ticks while `item/field_ration` currently applies two ticks in one selected scenario. Selected declaration/package/runtime semantics must agree or use distinct declarations.

Goal 008 therefore currently proves a second acceptance-only simulator, not execution by the existing production runtime.

## Read First

Read only:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. this task
5. `docs/GOAL_008_RULE_PACK_GAMEPLAY_FAMILY_FOUNDATIONS.md`
6. `src/LLMGameCreator.Application/Design/Gameplay/RulePackGameplayFamilyAcceptanceService.cs`
7. existing runtime entry points and serializer needed by the fix:
   - `src/LLMGameCreator.Runtime/GameRuntimeService.cs`
   - `src/LLMGameCreator.Runtime/UseItemRuntimeService.cs`
   - `src/LLMGameCreator.Runtime/EquipmentRuntimeService.cs`
   - `src/LLMGameCreator.Runtime/RecipeRuntimeService.cs`
   - `src/LLMGameCreator.Runtime/TransactionRuntimeService.cs`
   - existing runtime state serializer/snapshot-store implementation
8. Goal 008 focused and smoke tests

Do not read historical task packs or broad roadmap documents.

## Allowed Files

Primary:

- `src/LLMGameCreator.Application/Design/Gameplay/RulePackGameplayFamilyAcceptanceService.cs`
- `tests/LLMGameCreator.Tests/Application/Gameplay/RulePackGameplayFamilyAcceptanceTests.cs`
- `tests/LLMGameCreator.Tests/ProductSmoke/RulePackGameplayFamilySmokeTests.cs`
- `.llmgc/procedural/rule-pack-gameplay-family-foundations/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md` only if current gate wording needs correction

Conditionally allowed only when a focused integration test proves an existing runtime defect:

- the narrow existing Runtime service or serializer file containing that defect;
- its focused runtime regression test.

Do not edit `.sln`, `.csproj`, public GamePackage contracts, WinForms or generator-library files.

## Required Fixes

### 1. Execute through the existing production runtime

Replace the acceptance-only gameplay executor with an adapter that uses the existing runtime composition and real `GameRuntimeCommand` path.

Required commands must flow through `GameRuntimeService.Execute` or an already-established unified runtime boundary:

- `UseItem`;
- `EquipItem`;
- `CraftRecipe`;
- `ExecuteTransaction`;
- `SetFlag`.

Use the real `GameRuntimeStateFactory`, `RequirementEvaluator`, `CostConsumer`, `OutputApplier`, runtime family services and `GameRuntimeService`. Do not duplicate their gameplay semantics in Application or tests.

Because Application must not depend on the Runtime implementation project, use the established adapter boundary:

- the production acceptance service may default to an explicit unavailable adapter that cannot accept;
- focused tests and product smoke must inject a real integration adapter from the test assembly, as already done for prior headless runtime acceptance;
- alternatively use an existing composition root without introducing a forbidden project dependency.

The report must include structured proof that the real runtime boundary was used, such as adapter/runtime implementation identity plus actual runtime result diagnostics/events. A copied string alone is not sufficient; tests must exercise the real service types.

### 2. Start and mutate real runtime state

Create initial state through the existing runtime state factory/service. Scenario-specific starting inventory may be applied through existing runtime commands/helpers without bypassing the tested family commands.

For each command:

- snapshot the real `GameRuntimeState` immediately before execution;
- call the real runtime command;
- retain the real success, diagnostics and event types;
- snapshot the returned/mutated real state immediately after execution;
- derive deltas from those two states only.

Do not set `RuntimeStartSucceeded = true` unless real initialization succeeded.

### 3. Use the real runtime save/load seam

Serialize and restore the complete runtime state/session through the existing `RuntimeStateSerializer` and, where appropriate, `RuntimeSnapshotStore`.

Acceptance must compare:

- real serialized state before save;
- real restored state after load;
- exact inventory, equipment, status, flags, package/map identity and command evidence required by the scenario.

A report-only `RuntimeStateSnapshot` may remain as an evidence projection, but serializing that projection must not count as save/load proof.

Add a negative adapter/fixture with otherwise valid commands and deltas but mismatched restored runtime state. It must fail with stable save/load diagnostics.

### 4. Make binding audit scenario-exact

For every scenario verify before runtime:

- every `SourceDeclarationId` exists exactly once;
- its family matches a selected family;
- its declared package item/slot/recipe/transaction/status ids exist;
- package recipe input/output ids equal the selected recipe declaration;
- package transaction cost/output ids equal the selected transaction declaration;
- equipment item and slot match the selected equipment declaration and package compatibility rules;
- item effects reference existing declared/package statuses and flags;
- every command target/inventory/secondary target is covered by the selected declarations and exact package ids;
- unknown source ids, unrelated declarations or command targets fail before runtime.

Do not report all global declarations as audited when only a scenario subset is selected. Record the exact audited subset.

Add negative tests for an unknown `SourceDeclarationId` and for a command target not covered by the selected declaration.

### 5. Align status declaration semantics

Ensure selected status duration/effect semantics agree across declaration, package effect and runtime evidence.

Either:

- use three ticks consistently for `decl/status/focused`; or
- introduce a distinct deterministic declaration when a two-tick effect is intentional.

The audit must reject a duration mismatch. Test that the real runtime state restores the exact remaining ticks.

### 6. Keep evidence categories honest

Populate:

- crafting inputs/outputs only for a craft command;
- trade costs/outputs only for a transaction command;
- equipment delta only for equip/unequip;
- status/item-use evidence only from the corresponding runtime command;
- completion evidence only from the command that changed the flag/reward.

Generic inventory differences may remain available separately, but must not masquerade as crafting or trade evidence for other command types.

Add assertions that equip and use-item evidence have empty crafting/trade lists and `Changed = false` for unrelated categories.

### 7. Derive actual invalidity rather than hardcode it

Compute `ActualValid` for invalid fixtures through the same binding/runtime/evidence predicate used for valid scenarios. `ExpectedValid` remains expectation metadata only.

The invalid matrix passes only when actual acceptance is false and a causal error diagnostic exists. Removing the invalid mutation/condition from a fixture must make the expected-invalid matrix fail.

Preserve real rejection for:

- missing refs;
- equipment slot mismatch;
- missing crafting inputs;
- insufficient transaction cost;
- invalid status binding;
- fake runtime success;
- save/load mismatch.

## Required Tests

Add focused behavioral tests proving:

1. all valid scenarios execute through the real `GameRuntimeService` boundary;
2. default/unavailable production adapter cannot claim acceptance;
3. use, equip, craft, trade and set-flag commands return real runtime events/diagnostics and real state deltas;
4. missing inputs/cost and slot mismatch are produced by the real runtime services;
5. full `GameRuntimeState` survives the existing serializer/snapshot seam;
6. an otherwise-valid restored-state mismatch is rejected;
7. unknown selected declaration id is rejected;
8. a command target not covered by selected declarations is rejected;
9. status declaration/package/runtime duration mismatch is rejected;
10. unrelated crafting/trade delta categories remain empty for equip/use commands;
11. fake adapter copied success remains rejected;
12. removing an invalid condition causes the expected-invalid matrix to fail;
13. repeated real-runtime execution remains byte/hash stable;
14. Goal 007 and Goal 006 focused regressions remain green.

Product smoke must inject the real runtime adapter, deserialize JSON and assert actual runtime identity, real command event evidence, full-state save/load and the invalid matrix.

## Artifacts And State

Regenerate the existing three files under:

```text
.llmgc/procedural/rule-pack-gameplay-family-foundations/
```

Update state docs to record S077A correctness repair while keeping:

```text
rule_pack_gameplay_family_artifact_verification
```

Do not mark the gate passed. Do not create or recommend S078/Goal 009 in this run.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RulePackGameplayFamily|FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~SemanticRuntimeComposition|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario rule-pack-gameplay-family-foundations
.\.devflow\scripts\check-all.ps1
```

Search changed/generated files for mojibake and accidental S078/Goal 009 work.

## Stop Conditions

Stop with a blocker report instead of weakening acceptance if:

- the existing runtime cannot execute one of the required families;
- real runtime save/load cannot preserve the required state;
- fixing production behavior requires a public GamePackage/runtime contract redesign;
- a project-reference change would be required;
- full verification exposes an unrelated pre-existing failure.

Do not fall back to a second acceptance-only gameplay simulator.

## Hard Limits

- No S078 or Goal 009.
- No git commands or branch operations.
- No WinForms/UI, Unity, Lua, provider/media or generator execution.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` changes.
- No genre/project-specific C# branches.
- No unrelated refactor.
- Use repository-relative Windows/PowerShell paths only. Do not use `/mnt`, `/home/oai`, `sandbox:/...` or `C:\mnt`.

## Final Report

Report:

- how the duplicate Application gameplay executor was removed or made unavailable;
- exact real runtime services and commands used;
- exact scenario binding audit results;
- real runtime events/diagnostics and state deltas;
- full-state serializer/save-load evidence;
- negative fixtures and causal diagnostics;
- changed files;
- focused/smoke/full verification results;
- regenerated artifact folder;
- confirmation that the gate remains `rule_pack_gameplay_family_artifact_verification` and S078/Goal 009 were not started.
