# Goal 008 - Rule-Pack Gameplay Family Foundations

## Purpose

Goal 008 starts only after the user/assistant accepted:

```text
connected_world_travel_state_artifact_verification passed
```

Goal 007 proved a bounded connected world/travel state and deterministic save/load. Goal 008 must prove the first reusable rule-pack gameplay family set through a headless product vertical slice:

```text
validated gameplay family declarations
-> exact package/runtime bindings
-> ordered runtime commands
-> inventory/equipment/crafting/trading/status state deltas
-> deterministic save/load evidence
-> invalid declaration/runtime rejection
```

This goal is not a full economy/combat/faction system. It is the bounded foundation slice for data/rule-pack-driven gameplay families that later generated games can compose.

## Final Gate

Stop at exactly one final gate:

```text
rule_pack_gameplay_family_artifact_verification
```

Do not create the next goal or any post-Goal-008 work in this run.

## Product Slices Inside This Goal

Use these slice ids in docs/report evidence. Class names may vary, but the behavior must be covered.

- S071: record Goal 007 gate as passed and create the gameplay-family acceptance spine.
- S072: deterministic gameplay family rule-pack/proof-pack declarations and validator.
- S073: item, inventory, item-use and equipment family materialization/runtime evidence.
- S074: crafting/recipe/resource-conversion family materialization/runtime evidence.
- S075: trading/transaction and status/effect family materialization/runtime evidence.
- S076: invalid declaration/runtime/bypass rejection, determinism and save/load regressions.
- S077: product smoke route, state docs and final verification artifacts.
- S077A: bounded correctness hotfix for the same Goal 008 artifact family; Application default runtime evidence is unavailable, test/smoke inject a real `GameRuntimeService` adapter, save/load uses runtime serializer/snapshot seams, binding audit is scenario-exact, and the final gate remains `rule_pack_gameplay_family_artifact_verification`.

## Required Outcome

A valid Goal 008 run must write deterministic artifacts under:

```text
.llmgc/procedural/rule-pack-gameplay-family-foundations/
```

Required files:

```text
rule-pack-gameplay-family-report.json
rule-pack-gameplay-family-report.md
rule-pack-gameplay-family-verification.md
```

The JSON report must be structured enough that tests can deserialize and assert fields without string contains checks.

## Non-Goals

- No Runtime Preview UI.
- No WinForms Designer work.
- No Unity, Unity archive, Unity project or Windows build work.
- No LLM, RAG, provider, ComfyUI, Suno or media execution.
- No Lua execution or generator execution.
- No GamePackage public schema redesign.
- No broad combat/factions/reputation/social/work/theft systems.
- No full economy simulation, price balancing, market networks or AI vendors.
- No genre/project/term-specific C# branches.
- No C# code generation for game-specific mechanics.

## Explicit Narrow Runtime Permission

Goal 008 is allowed to add or adjust narrow runtime/application primitives only when needed to honestly execute existing gameplay-family semantics.

Allowed narrow runtime/application work:

- item/inventory command evidence using existing runtime state/services;
- equipment slot state if an existing serializable runtime field/metadata path or narrow runtime-owned service can represent it;
- recipe/crafting command execution through existing recipe/runtime primitives where available;
- transaction/trading command execution through existing transaction/runtime primitives where available;
- status/effect evidence through existing flags/status/effects or a narrow serializable runtime-owned status state;
- deterministic save/load support for only the state introduced or used by this goal.

Do not redesign public `GamePackage` or broad runtime architecture. If a public GamePackage schema change appears necessary, stop and report a schema blocker instead of implementing it.

## Data Ownership Rules

Immutable generated/source content may include:

- gameplay family rule-pack/proof-pack sidecar;
- item/equipment/recipe/trade/status declarations;
- deterministic scenario seed/config/rules version;
- exact package item, recipe, transaction, interaction, flag/status ids.

Runtime/save-only state may include:

- inventory amounts and item ownership;
- equipped item ids by slot;
- consumed/crafted/traded item deltas;
- applied status/effect ids and remaining duration/turn evidence;
- runtime command log;
- completion/reward flags;
- save/load evidence hashes.

Runtime deltas must never be counted as immutable source/package content.

## Required Valid Scenarios

Build at least five deterministic valid scenarios. They may share one package/proof pack, but each scenario must have its own deterministic seed/evidence hash.

1. `gameplay_inventory_item_use`
   - Start with at least one usable item in inventory.
   - Execute a use-item action.
   - Prove inventory delta and applied effect/status/flag delta.

2. `gameplay_equipment_loadout`
   - Equip a valid item into an exact equipment slot.
   - Prove item/slot ids match declarations.
   - Prove equipped state survives save/load.

3. `gameplay_crafting_recipe`
   - Consume declared input item/resource amounts.
   - Produce declared output item.
   - Prove before/after amounts and command success.

4. `gameplay_trading_transaction`
   - Execute a deterministic buy/sell/barter transaction.
   - Prove cost is consumed and output/reward is received.
   - Reject insufficient-cost variant in invalid scenarios.

5. `gameplay_status_effect_chain`
   - Apply a status/effect through a valid item/use/trade/craft chain.
   - Prove status/effect id, source command, duration/remaining-turn or equivalent runtime-owned evidence.
   - Prove save/load restores the exact status/effect evidence.

The overall valid acceptance should also include a combined scenario, or one of the valid scenarios must execute a complete ordered loop:

```text
collect/start inventory
-> equip
-> craft
-> trade
-> use/apply status
-> completion evidence
```

## Required Invalid Scenarios

Add deterministic invalid scenarios that fail for real diagnostics or failed runtime commands:

1. `invalid_missing_item_or_recipe_ref`
   - A recipe/use/trade/equipment declaration references an unknown item/recipe id.

2. `invalid_equipment_slot_mismatch`
   - An item is equipped into an incompatible or missing slot.

3. `invalid_crafting_missing_inputs`
   - Craft command lacks required input amounts.

4. `invalid_trade_insufficient_cost`
   - Trade/transaction command lacks required currency/item cost.

5. `invalid_status_or_effect_binding`
   - Status/effect declaration is missing, unsafe or not bound to a runtime-owned effect.

6. `invalid_fake_runtime_success`
   - A fake adapter/evidence object copies selected ids and says success but lacks required command/state deltas.

Invalid declaration scenarios must not count as runnable. Invalid runtime scenarios must not count as accepted.

`ExpectedValid = false` remains expectation metadata and must never itself cause actual rejection.

## Required Evidence In The Report

For every valid scenario include:

- scenario id, seed and deterministic hash;
- selected gameplay family ids;
- source declaration ids and exact package/runtime ids;
- package validation/audit result;
- ordered runtime command list;
- inventory before/after amounts;
- equipment slot before/after state;
- recipe/crafting input/output deltas;
- trade/transaction cost/output deltas;
- status/effect before/after and duration/remaining evidence;
- completion/reward evidence if present;
- runtime state hash and restored runtime state hash;
- save/load roundtrip boolean;
- diagnostics.

For invalid scenarios include:

- scenario id and invalid kind;
- expected/actual validity;
- stable error diagnostic codes;
- whether runtime was attempted;
- relevant missing/mismatched ids.

For the overall report include:

- `accepted`;
- `manualGate = rule_pack_gameplay_family_artifact_verification`;
- `goal007GateRecorded = true`;
- `externalExecution` flags all false;
- valid/invalid scenario counts;
- package/rule-pack validation result;
- gameplay runtime execution result;
- save/load result;
- invalid rejection result;
- deterministic replay result;
- remaining primitive limits.

## Acceptance Criteria

Goal 008 is accepted only when:

- Goal 007 gate is recorded as passed from the user prompt.
- At least five valid gameplay-family scenarios pass.
- At least six invalid gameplay-family scenarios fail for real diagnostics or failed runtime evidence.
- Every valid package/rule binding is audited before runtime.
- Every valid runtime command changes runtime-owned state or produces a proven no-op diagnostic where appropriate; no report-only success.
- Inventory/equipment/crafting/trading/status evidence uses exact ids from the selected declarations.
- Fake adapter/evidence success is rejected.
- Missing refs, mismatched slots, missing inputs, insufficient cost and invalid status binding are rejected.
- Save/load restores exact gameplay-family state evidence.
- Repeated identical builds are byte/hash stable.
- Product smoke deserializes JSON and asserts critical structured fields.
- `check-all.ps1` passes.
- No next-slice or next-goal work is created.

## Expected Final State Update

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md` if current-next-work wording must move to the new gate.

The state docs must record:

```text
goal_008_rule_pack_gameplay_family_foundations
```

The active gate must become:

```text
rule_pack_gameplay_family_artifact_verification
```

Do not mark that gate passed. Do not recommend the next goal until the user/assistant accepts the Goal 008 gate after reviewing the pushed code.

## Stop Conditions

Stop and report a blocker instead of weakening acceptance if:

- a required gameplay family cannot be honestly represented by existing package/runtime primitives or narrow runtime-owned state;
- connected runtime evidence would require a public GamePackage schema redesign;
- save/load cannot preserve inventory/equipment/crafting/trading/status evidence;
- invalid scenarios can only be rejected by expectation metadata instead of real diagnostics;
- full verification exposes an unrelated pre-existing failure;
- the implementation would require UI, Unity, provider/media, arbitrary Lua or project-file changes.

Do not convert unsupported gameplay families into report-only success.
