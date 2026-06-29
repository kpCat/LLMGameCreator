# Goal 036 Spec — Lua Sandbox Execution Gate

## Goal

Create a deterministic Application-layer Lua sandbox execution gate that decides whether selected Goal 035 Lua module manifests are eligible for future execution under a narrow host API policy.

This goal does **not** execute Lua. It does **not** parse Lua. It does **not** generate Lua source. It creates the gate that a future executor adapter must pass through.

## Why this comes now

Goal 035 added manifest records, host API groups and dependency planning. The next risk is allowing Lua-like modules into execution without a hard sandbox boundary. Goal 036 must prevent that by adding a deny-first execution gate and evidence before any interpreter dependency or runtime adapter is selected.

## Required concepts

- `LuaSandboxExecutionPolicy`
- `LuaSandboxExecutionRequest`
- `LuaSandboxBudget`
- `LuaSandboxHostBinding`
- `LuaSandboxDecision`
- `LuaSandboxDryRunPlan`
- `LuaSandboxProbeStep`
- `LuaSandboxTrace`
- `LuaSandboxRepairPlan`
- `LuaSandboxDiagnostic`

## Required statuses

Use deterministic status values equivalent to:

- `ready_for_future_executor`
- `dry_run_only`
- `needs_repair`
- `blocked_no_executor`
- `rejected`

No status may imply that Lua source was executed.

## Required scenario coverage

Use the same scenario family as Goals 031-035:

- `frontier_survival`
- `gothic_intrigue`
- `caravan_trade`
- `metamodule_kingdoms`

The metamodule scenario must keep the 7 kingdoms / 112 species-archetype slot scale visible in either request counts or manifest selection counts.

## Evidence folder

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-036-lua-sandbox-execution-gate/
```

Required files:

```text
lua-sandbox-policy-summary.json
lua-host-binding-matrix.json
lua-sandbox-execution-requests.json
lua-sandbox-decision-frontier.json
lua-sandbox-decision-gothic.json
lua-sandbox-decision-caravan.json
lua-sandbox-decision-metamodule.json
lua-sandbox-dry-run-trace-matrix.json
lua-sandbox-repair-plan-matrix.json
invalid-lua-sandbox-diagnostics-matrix.json
lua-sandbox-execution-gate-report.md
```

Evidence requirements:

- stable ordering;
- no absolute paths;
- no timestamps unless existing repo convention requires deterministic timestamp fields;
- no large logs;
- JSON must parse;
- report must contain `lua_sandbox_execution_gate_verification required`.

## Boundary

Allowed:

- Application-layer contracts and deterministic services;
- tests;
- compact evidence;
- state/handoff docs;
- task/spec/scouting docs.

Forbidden unless explicitly overridden by a later goal:

- real Lua execution;
- Lua parser;
- generated Lua source;
- interpreter NuGet/package dependency;
- Runtime/UI/Unity/GamePackage schema/provider/LLM/RAG/media/generator-library changes.
