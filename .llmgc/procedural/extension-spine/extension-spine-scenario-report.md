# Extension Spine Scenario Report

- Deterministic: true
- External execution: none
- Accepted: `true`
- Snapshot hash: `2429bc273b680e8a17583138c145c3bbe7003ca24301f64ead9556ad23e13277`
- Manual gate: `manual_extension_spine_verification`
- Proof rule pack: `rule_pack/extension_spine_inventory_objective_v1`
- Extension changed behavior: `true`
- Invalid extension rejected: `true`

## Scenarios

### base

- Accepted: `true`
- Seed/preset: `goal003-base-runtime-backed-loop` / `survival_exploration`
- Package: `Generated MVP semi_procedural_regions e144f558e3e5` / `game/generated_mvp_semi_procedural_regions_e144f558e3e5`
- Runtime path: start=`true`, move=`true`, interact=`true`
- Goal/reward/completion: progress=`true`, reward=`true`, completion=`true`
- Extension consumed: `false`

### extension_inventory_objective

- Accepted: `true`
- Seed/preset: `goal003-extension-inventory-objective` / `recover_resource`
- Package: `Generated MVP semi_procedural_regions 2b79b6099e26` / `game/generated_mvp_semi_procedural_regions_2b79b6099e26`
- Runtime path: start=`true`, move=`true`, interact=`true`
- Goal/reward/completion: progress=`true`, reward=`true`, completion=`true`
- Extension consumed: `true`
- Extension reward/objective: `item/extension_spine_badge` / `objective/collect_extension_badge`

## Data Extensible

- `triggers`
- `conditions`
- `formulas`
- `actions`
- `rewards`
- `quest objectives`
- `inventory-objective reward variation`

## Requires C# Primitive

- `new runtime command families`
- `new mutable runtime state containers`
- `new formula evaluator semantics`
- `new rendering or UI interaction modes`
- `new external providers or Lua execution`

## Diagnostics

- `info` `extension_spine.csharp_scope` target=`harness`: C# changes are limited to declaration validation, deterministic harnessing and generic runtime-state action primitives.
- `info` `extension_spine.extension_changed_behavior` target=`extension_inventory_objective`: Extension rule pack changed runtime-backed inventory objective and reward evidence.
- `info` `extension_spine.invalid_extension_rejected` target=`rule_pack/invalid_extension_spine_v1`: Invalid extension rule pack was rejected by declaration-level validation.
- `info` `extension_spine.manual_verification_required` target=`manual_extension_spine_verification`: Codex acceptance is headless; the next and only manual gate for this goal is manual extension spine verification.
- `info` `extension_spine.no_external_execution` target=`harness`: No LLM, provider, Lua, Unity or media execution was invoked.
