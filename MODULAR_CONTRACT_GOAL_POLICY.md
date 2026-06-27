# Modular Contract Goal Policy

Status: proposed development-process policy  
Scope: Codex goal design, modular contracts, proof levels, batching strategy, self-review and hotfix discipline  
Non-scope: production code, GamePackage schema changes, runtime implementation, Unity implementation

## 1. Purpose

LLMGameCreator should continue growing toward a full game-generation platform without every new goal becoming a broad vertical rewrite across Application, Runtime, tests, product smoke, state docs, artifacts and verification reports.

The target development mode is:

```text
modular contracts + bounded implementation + conformance proof + rare product vertical gates
```

This policy is intended to reduce:

- Codex context bloat;
- long-running goals;
- repeated hotfix loops;
- accidental broad coupling;
- false-positive “done” reports;
- manual verification after every small step.

It must not reduce quality, remove gates, or block future user wishes.

## 2. Core rule

A goal should be as small as possible, but not so small that the user spends more time issuing tasks than reviewing meaningful progress.

A goal may be phased, but it must have an explicit proof level and budget.

```text
Every goal must declare:
- goal type;
- required proof level;
- allowed phases;
- file/artifact/test budget;
- final gate;
- forbidden scope;
- stop conditions.
```

## 3. Goal types

### audit-only

Used when the next safe action is to inspect coverage, gaps, compatibility or risk.

Allowed:
- docs;
- compact audit artifacts;
- read-only inspection;
- no implementation expansion.

Forbidden by default:
- production feature implementation;
- schema changes;
- Unity;
- LLM/RAG/provider/media execution;
- arbitrary Lua execution.

### contract-only

Used when a new data/IR/artifact boundary must be defined.

Allowed:
- contract document;
- sample fixture;
- validator shape;
- conformance expectations.

Forbidden by default:
- runtime behavior;
- UI;
- provider execution;
- broad package assembly.

### module implementation

Used when one bounded module implements one known contract.

Allowed:
- one Application/Domain/Runtime/Generation module area;
- focused tests;
- no broad integration.

Forbidden by default:
- multiple unrelated modules;
- new public schemas unless approved;
- UI;
- Unity;
- external provider calls.

### integration slice

Used when output from one module must be consumed by the next stage.

Allowed:
- one upstream contract;
- one downstream consumer;
- focused integration test;
- compact diagnostic artifact.

Forbidden by default:
- full product vertical;
- multi-family expansion;
- unrelated polish.

### product vertical gate

Used only when the user should receive a new playable, simulatable, runtime-facing or export-facing result.

Allowed:
- bounded multi-layer work;
- smoke route;
- acceptance artifacts;
- final manual review if automation cannot prove the result.

Product vertical gates are expensive and should be rare.

### bounded hotfix

Used only to repair correctness gaps inside the same gate and artifact family.

Forbidden:
- new goals;
- new capability domains;
- new UI;
- schema changes;
- broad refactor;
- unrelated cleanup.

After two hotfix attempts, stop and produce a diagnosis/split plan.

## 4. Phased composite goal

A single goal may contain phases, but phases must remain bounded.

Recommended phase order:

```text
Phase 1: Contract
Phase 2: Module implementation
Phase 3: Integration
Phase 4: Proof
```

Not every goal needs all phases.

A product vertical proof is not required in every goal. It is required only when the goal claims product-level progress.

## 5. Proof levels

Every goal must declare the highest proof level required.

### Level 0 — docs/contract proof

The contract, boundary or audit decision is documented.

### Level 1 — conformance proof

Fixtures, validators or golden examples prove the contract shape.

### Level 2 — module proof

A module consumes the input contract and produces the output contract.

### Level 3 — integration proof

The output is consumed by the next pipeline stage.

### Level 4 — product vertical proof

The user can generate, simulate, play, preview, export or otherwise evaluate a runtime-facing result.

## 6. Anti-overfit rule for foundational modules

A foundational module must not be accepted only because it works for one product scenario.

A foundational module must provide at least two consumer shapes:

```text
1. primary consumer fixture;
2. second consumer fixture or synthetic consumer fixture.
```

If the second consumer is not implemented yet, the synthetic fixture must prove that the output contract is not hardcoded to the primary consumer.

Example:

```text
Pathfinding must not be proven only by caravan routing.
It should also have an npc_city_walk_consumer_fixture or equivalent synthetic fixture proving that PathPlan can be consumed by future NPC walking.
```

If the future consumer needs a genuinely new primitive, declare it as `future_required` or `blocked_gap` instead of pretending it is supported.

## 7. Supported capability envelope

Each module must state:

```text
supported_now:
  - what this module actually supports

future_required:
  - what is planned but not supported yet

blocked_gap:
  - what cannot work until another contract/runtime/schema/validator exists

non_goals:
  - what this task must not implement
```

This prevents a module from silently becoming over-specialized or over-claiming support.

## 8. Goal budget policy

Default budget for a normal implementation goal:

```text
Max implementation files: 8
Max docs/state files: 4
Max artifact family roots: 1
Max new production concepts: 1
Max product smoke route changes: 1
Max focused tests: 3
Max hotfix attempts: 2
Full check-all: final verification only
```

A goal must stop and return a split plan if it needs:

- more than 8-10 implementation files;
- more than one artifact family;
- a public GamePackage schema change;
- a new project or dependency;
- broad UI + runtime + generation changes together;
- Unity/player changes not explicitly allowed;
- provider/LLM/RAG/media/Lua execution not explicitly allowed.

## 9. Batching strategy

Do not pack many independent goals into one giant Codex run.

Recommended batching:

```text
Campaign plan:
  plans 3-5 goals together at Pro/architect level.

Codex execution:
  implements one bounded goal or one phased composite goal.

Milestone gate:
  after several module/integration goals, run one product vertical gate.
```

A giant goal may reduce manual task issuing, but usually increases:

- context bloat;
- missed constraints;
- hidden coupling;
- hotfix count;
- review difficulty;
- false-positive completion.

Use large composite goals only when the work has one artifact family, one main contract family and one selected product path.

## 10. Codex context discipline

Each goal must keep read-first docs small.

Default read-first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. current goal/task file
5. only directly relevant contract docs

Avoid reading historical task packs, old prompts and archived reports unless the current task explicitly needs them.

Each long goal must produce a compact self-handoff at the end:

```text
- changed files;
- current gate;
- evidence paths;
- tests;
- unresolved gaps;
- next allowed action;
- no-git confirmation.
```

## 11. Pre-final Codex self-review protocol

Before final report, Codex must:

1. Re-read the active goal acceptance criteria.
2. Map each criterion to an evidence path, artifact or test.
3. Inspect generated artifacts directly, not only test output.
4. Confirm the final gate status is exactly the requested status.
5. Confirm no next goal/slice was started.
6. Confirm forbidden files and artifact families were not changed.
7. Confirm public GamePackage schema was not changed unless explicitly allowed.
8. Confirm Unity/LLM/RAG/provider/media/Lua execution did not occur unless explicitly allowed.
9. Confirm state/context docs were updated only when required.
10. Perform at most one bounded self-fix if evidence is missing.
11. If evidence is still missing, stop and return diagnosis instead of claiming completion.

Final report must include:

```text
| Acceptance criterion | Evidence path/test | Status |
|---|---|---|

Changed files:
- ...

Tests:
- command: pass/fail/not run + reason

Artifacts inspected:
- ...

Gate:
- final gate status

Scope:
- forbidden changes: none / diagnosis
- next goal started: no
- git commands: not run unless explicitly requested
```

## 12. Manual verification policy

Manual verification should be the exception, not the default.

Manual review is appropriate for:

- actual playability acceptance;
- profile/canon approval;
- new capability domain approval;
- major architecture decision;
- visual/audio quality that automation cannot judge.

Automation should cover:

- schema validity;
- deterministic output;
- reference closure;
- package validation;
- runtime smoke;
- save/load;
- invalid/fake/leak rejection;
- artifact scope guard;
- conformance fixtures.

## 13. Semantic/runtime variation stance

Semantic packs may support both pre-runtime generation and runtime variability, but these are different modes.

Allowed by default:

```text
compiled semantic catalog + seed + rule packs
-> deterministic generation before package assembly
-> GamePackage
-> runtime
```

Allowed later as a controlled deterministic runtime primitive:

```text
compiled semantic catalog + seed + runtime-safe generator rules
-> on-demand chunk/event/dialogue/text variation
-> runtime state/save deltas
```

Not allowed as core runtime authority:

```text
runtime calls LLM/RAG/provider
-> unvalidated text/gameplay directly mutates live state
```

If optional live LLM/RAG runtime assistance is ever added, it must be off by default, editor-approved, quarantined, validated and non-authoritative.

## 14. Success criteria for this policy

This policy is working when:

- most goals touch fewer files;
- hotfix count drops;
- product vertical gates are clearer and less frequent;
- manual checks are reserved for real acceptance;
- future wishes become capability backlog entries instead of chaotic broad goals;
- new modules are reusable across more than one scenario;
- Codex final reports contain evidence tables rather than vague completion claims.
