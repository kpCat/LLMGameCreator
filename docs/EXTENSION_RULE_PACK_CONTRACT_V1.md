# Extension Rule Pack Contract v1

Status: accepted Goal 003 contract  
Scope: declaration-level data and Lua-like rule packs for generated microgame extension proofs  
Non-scope: arbitrary Lua execution, provider/LLM execution, Unity, media generation, broad runtime redesign

## Purpose

Extension rule pack v1 proves that small gameplay variations can be described as validated data instead of bespoke C# mechanics.

The contract is Lua-like in shape but not executable Lua. C# validates declarations and the scenario harness consumes only supported primitives.

## Supported declarations

- triggers: `trigger/runtime_interact_completed`, `trigger/on_goal_completed`, `trigger/on_reward_granted`
- conditions: `condition/has_inventory_item`, `condition/flag_equals`, `condition/quest_objective_completed`
- formulas: bounded arithmetic expressions with declared variables only
- actions: `action/grant_item`, `action/advance_objective`, `action/set_flag`
- rewards: `reward/item`
- quest objectives: `objective/inventory_item`, `objective/flag`
- rules: links from one trigger to condition/action/reward/objective ids

## Validator authority

The validator is declaration-only and side-effect free. It checks:

- safe slash ids;
- duplicate ids;
- safe target refs and path-like parameter values;
- known refs between rules, triggers, conditions, formulas, actions, rewards and objectives;
- unsupported trigger, condition, action, reward and objective types;
- unknown API calls;
- unsupported mutation targets;
- unsafe or invalid formulas.

The validator does not run Lua, evaluate formulas, call providers, mutate packages, mutate runtime state or call WinForms.

## Supported runtime-state consumption

The Goal 003 scenario harness may consume a validated pack through generic primitives only:

- `runtime.inventory` for declared item grants;
- `runtime.quest_objective` for declared objective progress;
- `runtime.flag` for declared flags.

Those primitives map onto existing serializable `GameRuntimeState` fields. A future mechanic that needs a new command family, a new state container, a new formula evaluator, a new interaction mode or renderer still requires an explicit C# primitive slice.

## Proof pack

The accepted proof pack is written to:

```text
.llmgc/procedural/extension-spine/extension-proof-rule-pack.json
```

It adds an inventory objective and an additional reward through data declarations:

```text
runtime-backed generated interaction
-> validated rule pack trigger
-> grant item/extension_spine_badge
-> advance objective/collect_extension_badge
-> set flag/extension_spine_rule_applied
```

This proves extensibility of the generated loop without adding a one-off C# gameplay mechanic for the specific reward or objective.

## Rejection proof

The invalid proof pack is not executed. It is validated and rejected because it includes unsafe ids/paths, unknown API calls, invalid formula text and unsupported mutation targets.

Expected report:

```text
.llmgc/procedural/extension-spine/invalid-extension-validation-report.json
```
