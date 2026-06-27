# Package Assembly Expansion Campaign Pack

Status: plan-only campaign pack  
Scope: next 3-5 bounded package assembly expansion goals  
Non-scope: Goal 025 implementation, S199, package assembly execution

## Purpose

Use the accepted Goal 024 coverage audit to plan package assembly expansion
without starting implementation. The campaign reduces manual goal cycles by
grouping Contract, Module, Integration and Proof phases inside bounded
composite goals, then reserving a rare product vertical gate until several
modules are ready.

## Campaign Rules

- Goal 025 is plan-only until this process gate is accepted.
- Product vertical proof is not required in every goal.
- Level 2/3 proof is enough for early package assembly modules when no new
  playable/runtime-facing result is expected.
- Each foundational module needs one real consumer plus one independent or
  synthetic future-consumer fixture.
- Do not change public `GamePackage` schema, runtime, Unity, WinForms UI,
  provider/media/RAG/LLM/Lua execution or generator-library unless a future
  approved task explicitly unlocks that scope.

## Candidate Goals

| Candidate | Task type | Proof level | Bounded composite phases |
|---|---|---|---|
| Goal 025: `package_assembly_expansion_1_world_and_entities` | `module_implementation` plus `integration_slice` | Level 2/3 | Contract fields mapping review, world/entity assembly module, package validation integration, anti-overfit fixture proof. |
| Goal 026: `package_assembly_expansion_2_dialogue_and_quests` | `module_implementation` plus `integration_slice` | Level 2/3 | Dialogue/quest contract mapping, assembly module, validator/runtime-smoke conformance, second consumer fixture. |
| Goal 027: `package_assembly_expansion_3_items_economy_crafting` | `module_implementation` plus `integration_slice` | Level 2/3 | Item/economy/crafting mapping, inventory/equipment validation, synthetic vendor/crafting fixture. |
| Goal 028: `package_assembly_expansion_4_combat_progression` | `module_implementation` plus `integration_slice` | Level 2/3 | Combat/progression mapping, encounter/status validation, synthetic alternate combat/progression fixture. |
| Goal 029: `full_package_assembly_vertical` | `product_vertical_gate` | Level 4 | Combine prepared modules, produce one generated package, run validation/runtime/export smoke and manual review if needed. |

## Input Contracts

- `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md`
- `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-matrix.json`
- `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-gap-report.json`
- `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-next-slice-plan.json`
- `docs/GAME_PROFILE_CONTRACT_V1.md`
- `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md`
- current `GamePackage` schema and validators

## Output Contracts

Each module goal should produce deterministic artifacts under one current root:

```text
.llmgc/procedural/<campaign-goal-id>/
```

Expected outputs per module:

- mapping contract/report;
- assembled package-facing records or explicit unsupported/future-required gaps;
- conformance/validation report;
- invalid/fake/leak matrix;
- anti-overfit fixture report;
- final verification markdown with `accepted=false` and one manual gate.

## Anti-Overfit Fixtures

| Goal | Real consumer | Second/synthetic consumer fixture |
|---|---|---|
| Goal 025 | selected profile world/entity pipeline input | `npc_city_walk` or independent settlement/region entity fixture |
| Goal 026 | selected profile quest/dialogue pipeline input | independent rumor-board or tutorial objective fixture |
| Goal 027 | selected profile item/economy pipeline input | vendor/crafting transaction fixture |
| Goal 028 | selected profile combat/progression pipeline input | alternate encounter/status progression fixture |

The synthetic fixture proves contract generality. It does not need to implement
the future system.

## Allowed Plan-Level Scope

- package assembly contract/mapping docs;
- Application-layer assembly modules when a future goal starts;
- focused validators/tests;
- one compact artifact root per goal;
- state/context/queue updates for the active goal.

## Forbidden Plan-Level Scope

- live runtime LLM/RAG;
- provider/media execution;
- arbitrary Lua execution;
- Unity build/player changes;
- WinForms UI changes;
- public `GamePackage` schema changes without explicit migration approval;
- broad cleanup or historical artifact mutation;
- Goal 025/S199 implementation inside this process task.

## Manual Check Reduction

Manual checks can be replaced by automated or synthetic checks when the question
is contract conformance, deterministic assembly, invalid/fake/leak rejection or
anti-overfit coverage. Reserve manual gates for:

- product vertical playability;
- profile/canon approval;
- public schema migration approval;
- opening live execution surfaces.

## Stop Conditions

Stop and split when:

- a goal would touch more than 10 files;
- a second artifact family must be changed;
- proof requires public schema changes or runtime primitives not allowed by the
  task;
- invalid/fake/leak evidence cannot be produced honestly;
- two hotfix attempts fail to close the same acceptance gap.

## First Rare Product Vertical Gate

The first rare product vertical gate should be Goal 029
`full_package_assembly_vertical`, after Goals 025-028 prepare enough modules to
prove a combined package. Goal 025 should not be a broad vertical slice if
Level 2/3 proof is enough.
