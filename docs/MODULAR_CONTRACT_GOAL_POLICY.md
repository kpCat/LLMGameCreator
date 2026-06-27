# Modular Contract Goal Policy

Status: active process policy  
Final adoption gate: `modular_contract_goal_policy_adoption_verification`

## Purpose

LLMGameCreator uses modular contracts, bounded composite goals and rare product
vertical gates so future work reduces manual goal cycles instead of multiplying
them.

Contract, Module, Integration and Proof are internal phases of one bounded
composite goal by default. They are not separate default goals.

## Task Types

| Task type | Use when | Typical proof |
|---|---|---|
| `audit_only` | Existing evidence must be inspected or classified before implementation. | Level 0 or Level 1 |
| `contract_only` | A durable artifact/data/process contract is needed before code work. | Level 0 or Level 1 |
| `module_implementation` | One bounded module or service can be implemented without a new vertical product gate. | Level 2 |
| `integration_slice` | Existing modules must be wired together through an existing seam. | Level 3 |
| `product_vertical_gate` | Several prepared pieces must prove a new playable, simulatable or runtime-facing result. | Level 4 |
| `bounded_hotfix` | A defect in the current accepted family must be repaired without opening the next goal. | Matching failing proof level |

## Proof Levels

| Level | Name | Required evidence |
|---|---|---|
| 0 | Docs/contract proof | Contract doc, scope boundaries, source-of-truth routing and explicit non-goals. |
| 1 | Conformance proof | Validator, schema check, docs/state guard or artifact-shape test proves conformance. |
| 2 | Module proof | Focused tests prove one module emits deterministic, validated output for real inputs. |
| 3 | Integration proof | Existing seams consume the module output and reject invalid/fake/leak cases. |
| 4 | Product vertical proof | End-to-end product smoke or equivalent runtime-facing proof shows a new playable/simulatable outcome. |

## Composite Goal Rule

One bounded composite goal may include:

```text
Contract -> Module -> Integration -> Proof
```

Those phases are internal checkpoints, not separate manual goals by default.
Split them into separate goals only when one condition is true:

- budget is exceeded;
- scope creeps beyond the goal allowlist;
- architecture risk is too high;
- the result cannot be proven honestly inside one bounded composite goal;
- the user explicitly asks for a split.

The split decision must say what evidence is missing and which phase becomes the
next bounded goal.

## Product Vertical Gate Rule

A product vertical gate is rare and intentional. It is not required for every
goal.

Use a product vertical gate after multiple modules or integrations are ready and
a new playable, simulatable or runtime-facing result must be proven. Do not
create a product vertical gate merely because a contract or module was added.

## Anti-Overfit Rule

A foundational module cannot be accepted through one consumer scenario only.

Require at least:

- one real consumer; and
- one second consumer shape, which may be a synthetic future-consumer fixture.

Example: pathfinding cannot be proven only by a caravan route. It also needs an
`npc_city_walk` synthetic consumer fixture or another independent consumer
shape. The synthetic fixture does not need to implement the future system, but
it must prove the output contract is not hardcoded to one scenario.

## Runtime LLM/RAG Decision

Optional live runtime LLM/RAG is not a target mode for LLMGameCreator.

Runtime must not depend on live LLM/RAG, media providers, asset generators,
WinForms UI, external generation tools or arbitrary Lua execution.

Allowed:

- pre-runtime LLM/RAG as an editor/generation authoring helper only;
- contract-bound drafts that are validated before promotion.

Required runtime approach for variable dialogue, quest text, descriptions,
events and large/infinite worlds:

```text
compiled semantic catalog
+ seed
+ rule packs
+ phrase/dialogue/event grammar
+ deterministic runtime-safe variation
+ validation
+ save-compatible runtime deltas
```

Runtime consumes compiled/validated data, semantic catalogs, rules, seeds and
grammars. Runtime variation must be deterministic, reproducible and
save-compatible.

Forbidden:

- planning live runtime LLM/RAG as an optional product path;
- runtime calls to provider/media/RAG/LLM systems;
- runtime dependency on unvalidated generated code.

## Goal Budget Limits

Default limits for one bounded composite goal:

- implementation files: 8 maximum unless the task records a bounded exception;
- docs/state files: 6 maximum unless source-of-truth routing requires more;
- artifact family roots: 1 current root;
- new production concepts: 2 maximum;
- focused tests: 1-5, scaled to actual contract risk;
- product smoke routes: 1 maximum unless explicitly justified;
- hotfix attempts: 2 maximum.

Stop and return a split plan when:

- more than 10 files must change;
- more than one independent artifact family must change;
- public `GamePackage` schema would change without explicit approval;
- Unity, provider/media/RAG/LLM/Lua execution is needed but not allowed;
- evidence remains missing after the bounded self-fix protocol.

## Hotfix Policy

```text
0 hotfixes = excellent
1 hotfix = normal
2 hotfixes = maximum
after the second hotfix, stop blind repair and return diagnosis/split plan
```

Hotfixes must stay in the current accepted artifact family unless the user
explicitly authorizes a broader repair.

## Pre-Final Self-Review Protocol

Before the final Codex report:

1. reread acceptance criteria;
2. map each criterion to a concrete evidence path or test;
3. directly inspect artifacts, not only test output;
4. confirm final gate status;
5. confirm next goal/slice was not started;
6. confirm forbidden files were not changed;
7. confirm public `GamePackage` schema did not change unless allowed;
8. confirm Unity/LLM/RAG/provider/media/Lua did not run unless allowed;
9. if evidence is missing, do one bounded self-fix;
10. if evidence is still missing after self-fix, stop and return diagnosis instead of claiming done.

Final Codex reports for goal/process work must include:

```markdown
| Acceptance criterion | Evidence path/test | Status |
|---|---|---|
```

## Source Of Truth

Current gate and routing remain in:

- `AGENTS.md`;
- `docs/CONTEXT_INDEX.md`;
- `docs/CURRENT_GENERATOR_STATE.md`;
- `docs/CURRENT_GENERATOR_STATE.json`;
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`.

This policy constrains future task shaping. It does not implement Goal 025,
S199, package assembly expansion, runtime features, Unity work, provider/media
execution, RAG/LLM calls, Lua execution or public `GamePackage` schema changes.
