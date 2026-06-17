# 003_DEVELOPMENT_ROADMAP.md — pack-driven development roadmap

This file is the stable roadmap for future task packs. It is intentionally high-level; executable details belong in individual task specs.

## Core rule

Do not generate all detailed executable packs in advance.

Use this depth model:

```text
current gate: detailed and executable
next phase: detailed but locked unless gate is open
far phases: sequence skeletons only
```

The goal is to avoid stale task specs. Every executable pack must be generated from the current repository state.

## Phase map

### M4.1 — strict real-model evaluation gate

Purpose:

```text
Prove that strict LLM artifact generation/evaluation is stable enough to proceed to Lua/module/package assembly work.
```

Exit requires evidence:

```text
- deterministic strict parser/golden/repair guardrails are green;
- real evaluation evidence exists or user explicitly accepts the risk;
- gate decision report classifies the state as pass, needs_repair, or blocked;
- docs/CURRENT_GENERATOR_STATE.md and docs/CURRENT_GENERATOR_STATE.json are updated by explicit user decision.
```

Allowed work while active:

```text
- M4.1 deterministic parser/markdown/repair/doc consistency tasks;
- real evaluation runbook and evidence manifest;
- real report import/analyzer/golden tasks;
- current-state update after user decision;
- local-agent quality hardening when execution feedback demands it.
```

Blocked while active:

```text
- M5 Lua production integration;
- M6 rich GamePackage assembly;
- M8 runtime preview repair loop;
- broad artifact contract expansion;
- Unity/export profile implementation.
```

### M5 — Lua module executor integration

Purpose:

```text
Introduce a safe, typed Lua execution path for generator modules without letting Lua mutate GamePackage or escape sandbox boundaries.
```

Entry condition:

```text
M4.1 passed in current-state docs.
```

Expected sequence:

```text
M5.000 sequence skeleton
M5.001 executor contracts
M5.002 manifest validation
M5.003 static sandbox policy
M5.004 test harness
M5.005 request/result DTOs
M5.006 manifest binding
M5.007 forbidden API goldens
M5.008 no package mutation guard
M5.009 one module family vertical slice
```

Non-negotiable constraints:

```text
- no io/os/debug/package/load/loadfile/dofile/require;
- no real filesystem/provider access from Lua;
- deterministic seed behavior;
- typed request/result envelope;
- diagnostics for capability mismatch;
- GamePackage is not mutated by Lua execution.
```

### M6 — rich GamePackage assembly

Purpose:

```text
Map reviewed/generated artifacts into GamePackage through explicit assembly contracts and validators.
```

Entry condition:

```text
M5 has a safe artifact envelope or current-state docs explicitly choose a non-Lua assembly path.
```

Expected sequence:

```text
M6.000 sequence skeleton
M6.001 mapping contracts
M6.002 artifact envelope -> package base mapping
M6.003 items/economy mapping
M6.004 scene/map mapping
M6.005 dialogue/quest mapping
M6.006 validation after assembly
M6.007 review/apply boundary
M6.008 first rich sample package
```

Non-negotiable constraints:

```text
- no direct LLM output -> GamePackage apply;
- review/validation/apply boundary stays explicit;
- package validator must reject missing references;
- runtime remains independent of editor providers;
- sample package must remain small but meaningful.
```

### M8 — runtime preview validation loop

Purpose:

```text
Validate assembled packages in headless runtime scenarios before UI/Unity/export expansion.
```

Expected sequence:

```text
M8.000 sequence skeleton
M8.001 package load smoke
M8.002 deterministic command scenario
M8.003 event/state snapshot guard
M8.004 no package mutation guard
M8.005 runtime diagnostic report
```

Constraints:

```text
- runtime does not call LLM/provider/UI;
- rendering never mutates state;
- command input -> state/events output;
- snapshots/goldens are deterministic.
```

### M9 — templates and balancing

Purpose:

```text
Introduce reusable template families, numeric balancing constraints, and progression fixtures.
```

Expected sequence:

```text
M9.000 sequence skeleton
M9.001 template family contracts
M9.002 numeric range constraints
M9.003 progression/balance fixtures
M9.004 formula diagnostics
M9.005 sample template packs
```

Constraints:

```text
- no vague balancing without assertions;
- tests pin numeric ranges and rejection diagnostics;
- generated data remains data/contracts, not C# code.
```

### M10 — export profiles and Unity IR

Purpose:

```text
Prepare export profiles and a Unity-oriented intermediate representation without moving editor/generator logic into the player.
```

Expected sequence:

```text
M10.000 sequence skeleton
M10.001 export profile contracts
M10.002 Unity IR skeleton
M10.003 deterministic export package
M10.004 player boundary tests
M10.005 asset reference mapping
```

Constraints:

```text
- Unity/player receives data, not editor workflow;
- no runtime LLM/provider calls;
- no generated C# from LLM;
- export profiles are validated and deterministic.
```

## Repair policy

If a pack or local-agent execution creates defects:

```text
1. Stop feature progression.
2. Create a focused repair/hardening pack.
3. Keep repair scope smaller than the failed task.
4. Preserve evidence in CURRENT_RUN/BLOCKERS, not in generated run artifacts.
5. Resume roadmap only after check-all is green and the review finds no boundary violation.
```

## Independence model

Packs are partially independent by design:

```text
- quality/devflow packs should be broadly independent;
- executable task packs depend on current-state gates and previous contracts;
- locked future skeletons may be replaced or amended before execution;
- repair packs may be inserted at any time;
- no pack should require executing all later packs to remain coherent.
```

This means future work should not get stuck in a long fix chain merely because skeletons exist. Skeletons guide generation; executable specs are generated or refreshed when the gate is open.
