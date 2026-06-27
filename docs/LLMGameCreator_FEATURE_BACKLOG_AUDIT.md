# LLMGameCreator Feature Backlog Audit

Status: backlog/audit reference  
Source-of-truth role: not current gate, not active goal queue

## Purpose

This document preserves wanted capability directions without turning them into
immediate implementation work. It is not an implementation plan, not the active
goal queue and not a replacement for `docs/CURRENT_GENERATOR_STATE.*` or
`docs/FULL_GENERATOR_GOAL_QUEUE.md`.

Wanted capabilities are not removed. They move into capability backlog, future
gaps and campaign planning until a bounded contract/module/integration/product
gate selects them.

## Policy

- Implement wants through contracts, modules, integrations and rare product
  vertical gates.
- Keep one executable bounded goal at a time.
- Do not split Contract, Module, Integration and Proof into separate manual
  goals by default.
- Use synthetic future-consumer fixtures when a module would otherwise overfit
  one current scenario.
- Treat live runtime LLM/RAG as an explicit non-goal, not a wanted capability.
- Runtime variation must be deterministic, reproducible and save-compatible.

## Backlog Buckets

| Bucket | Examples | Adoption path |
|---|---|---|
| Rich package assembly | World/entity data, dialogue/quest stages, items/economy, combat/progression, factions/schedules. | Package assembly campaign, bounded composite goals, Level 2/3 proof before rare vertical gate. |
| Runtime-safe variation | Phrase grammar, dialogue/event grammar, deterministic descriptions, seeded semantic choices. | Contract plus module proof with at least one real and one synthetic consumer. |
| Semantic authoring memory | Project semantic catalog, candidate quarantine, relations, provenance, generation context. | Editor/generation helper only; runtime consumes compiled data. |
| Validation and repair | Better diagnostics, invalid/fake/leak matrices, bounded repair attempts. | Contract/conformance proof before integration. |
| Review and promotion | Approval workflow, history, comparison, apply/promotion audit. | Human gate where canon or promotion is involved. |
| Export/runtime presentation | Unity/export profiles, runtime preview smoke, final player evidence. | Rare product vertical gates after modules are ready. |

## Explicit Non-Goals

- live runtime LLM/RAG path;
- runtime provider/media calls;
- runtime dependency on editor/generation tools;
- arbitrary Lua execution without an explicitly approved sandbox/manifest gate;
- public `GamePackage` schema changes without explicit migration approval;
- broad platform cleanup mixed into product goals.

## Current Campaign Link

The next package assembly direction is planned in
`docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md`. That campaign keeps desired
world, entity, quest, dialogue, item/economy, combat/progression and
faction/schedule capabilities visible while sequencing them through bounded
composite goals.
