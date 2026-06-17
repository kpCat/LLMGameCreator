# 000_M5_SEQUENCE.md — M5 Lua module executor integration sequence

This file is a locked sequence document. It is routing and planning guidance, not permission to execute M5.

## Gate status

```text
Status: locked_until_M4_1_gate_passes
Current blocking gate: M4.1 real-model evaluation gate
```

M5 executable production work is allowed only when:

```text
- docs/CURRENT_GENERATOR_STATE.md says M4.1 passed;
- docs/CURRENT_GENERATOR_STATE.json says M4.1 passed;
- the pack author refreshes task assumptions from current source layout;
- user approval is present in NEXT_TASK.md for the selected M5 task.
```

Task-pack files alone cannot unlock M5.

## Purpose

Introduce a safe, typed Lua execution path for generator modules without letting Lua mutate GamePackage or escape sandbox boundaries.

## Non-negotiable constraints

```text
- no io/os/debug/package/load/loadfile/dofile/require;
- no real filesystem/provider access from Lua;
- deterministic seed behavior;
- typed request/result envelope;
- diagnostics for capability mismatch;
- GamePackage is not mutated by Lua execution;
- runtime remains independent of editor generation providers;
- generated Lua output is artifact data, not C# code.
```

## M5 sequence

| Order | Task ID | Intent | Status |
|---:|---|---|---|
| 1 | M5_001 | Executor contracts only: request/result/diagnostic boundary, no real execution. | Existing locked spec |
| 2 | M5_002 | Manifest validation for Lua generator modules and capabilities. | Existing locked spec |
| 3 | M5_003 | Static sandbox policy for forbidden APIs and dangerous language constructs. | Existing locked spec |
| 4 | M5_004 | Lua executor test harness with fake/minimal scripts and no package mutation. | Locked draft spec |
| 5 | M5_005 | Request/result DTO alignment with artifact envelope and deterministic seed. | Locked draft spec |
| 6 | M5_006 | Manifest binding from approved module manifest to execution request. | Locked draft spec |
| 7 | M5_007 | Forbidden API golden fixtures and exact diagnostic assertions. | Locked draft spec |
| 8 | M5_008 | No GamePackage mutation guard across executor path. | Locked draft spec |
| 9 | M5_009 | First one-module-family vertical slice producing a typed artifact envelope. | Locked draft spec |

## Recommended execution after unlock

Use this order unless source review shows a better split:

```text
M5_001 -> M5_002 -> M5_003 -> M5_004 -> M5_005 -> M5_006 -> M5_007 -> M5_008 -> M5_009
```

## Future proof-test categories

When converted/refreshed for execution, M5 tasks must include exact proof tests for:

```text
- forbidden API rejection with exact diagnostic code;
- deterministic same-seed output;
- different seed changes only allowed deterministic fields;
- capability mismatch diagnostic;
- malformed manifest diagnostic;
- Lua execution result cannot mutate GamePackage;
- artifact envelope validation before any package assembly;
- runtime project remains untouched by generator executor work.
```

## Allowed implementation direction after unlock

```text
- Prefer small contracts before execution implementation.
- Prefer one module family before generic module orchestration.
- Prefer corpus/golden tests over real model/provider calls.
- Keep execution result as data/envelope; do not apply directly to GamePackage.
- Keep Lua execution under Scripting/Application ownership; Runtime must not depend on it.
```

## Stop rules

Stop instead of executing M5 if:

```text
- M4.1 has not explicitly passed;
- the task requires schema/project/dependency changes not explicitly approved;
- Lua needs file/network/provider access;
- GamePackage mutation is required by the design;
- runtime would gain dependency on Lua generation/editor services;
- proof tests cannot pin deterministic behavior;
- selected spec is stale against current source layout.
```

## Candidate next pack when unlocked

```text
agent-task-pack-007-m5-executable-entry-specs
```

If this file already contains locked M5 draft specs but M4.1 is still active, the next useful pack is either M4.1 execution support or source-refreshed M5 executable specs after M4.1 passes.
