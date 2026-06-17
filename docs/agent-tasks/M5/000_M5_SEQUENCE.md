# 000_M5_SEQUENCE.md — M5 Lua module executor integration sequence

This file is a locked sequence skeleton. It is routing and planning guidance, not an executable task spec.

## Gate status

```text
Status: locked_until_M4_1_gate_passes
Current blocking gate: M4.1 real-model evaluation gate
```

M5 executable production work is allowed only when:

```text
- docs/CURRENT_GENERATOR_STATE.md says M4.1 passed;
- docs/CURRENT_GENERATOR_STATE.json says M4.1 passed;
- the pack author refreshes task assumptions from current source layout.
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
- runtime remains independent of editor generation providers.
```

## Existing starting specs

The following locked specs already exist and should be reviewed/refreshed before execution:

```text
M5_001_LUA_EXECUTOR_CONTRACTS.md
M5_002_LUA_MANIFEST_VALIDATION.md
M5_003_LUA_STATIC_SANDBOX_POLICY.md
```

## Planned sequence

| Order | Task ID | Intent | Status |
|---:|---|---|---|
| 1 | M5_001 | Executor contracts only: request/result/diagnostic boundary, no real execution. | Existing locked spec |
| 2 | M5_002 | Manifest validation for Lua generator modules and capabilities. | Existing locked spec |
| 3 | M5_003 | Static sandbox policy for forbidden APIs and dangerous language constructs. | Existing locked spec |
| 4 | M5_004 | Lua executor test harness with fake/minimal scripts and no package mutation. | Skeleton only |
| 5 | M5_005 | Request/result DTOs aligned with artifact envelope and deterministic seed. | Skeleton only |
| 6 | M5_006 | Manifest binding from approved module manifest to execution request. | Skeleton only |
| 7 | M5_007 | Forbidden API golden fixtures and exact diagnostic assertions. | Skeleton only |
| 8 | M5_008 | No GamePackage mutation guard across executor path. | Skeleton only |
| 9 | M5_009 | First one-module-family vertical slice producing a typed artifact envelope. | Skeleton only |

## Future proof-test categories

When converted to executable specs, M5 tasks must include exact proof tests for:

```text
- forbidden API rejection with exact diagnostic code;
- deterministic same-seed output;
- different seed changes only allowed deterministic fields;
- capability mismatch diagnostic;
- malformed manifest diagnostic;
- Lua execution result cannot mutate GamePackage;
- artifact envelope validation before any package assembly.
```

## Allowed implementation direction after unlock

```text
- Prefer small contracts before execution implementation.
- Prefer one module family before generic module orchestration.
- Prefer corpus/golden tests over real model/provider calls.
- Keep execution result as data/envelope; do not apply directly to GamePackage.
```

## Stop rules

Stop instead of executing M5 if:

```text
- M4.1 has not explicitly passed;
- the task requires schema/project/dependency changes not explicitly approved;
- Lua needs file/network/provider access;
- GamePackage mutation is required by the design;
- runtime would gain dependency on Lua generation/editor services;
- proof tests cannot pin deterministic behavior.
```

## Candidate next pack when unlocked

```text
agent-task-pack-007-m5-executable-entry-specs
```
