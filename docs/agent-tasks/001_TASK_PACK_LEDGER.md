# 001_TASK_PACK_LEDGER.md — generated task pack ledger

This file is the state ledger for generated agent-task packs. Future packs must update it instead of relying on chat memory.

## Pack 001 — agent task specification framework

Pack id: `agent-task-pack-001`

Generated purpose:

```text
Add an executable task-spec layer for local agents so future work can be driven by small technical contracts with proof tests and system gates.
```

## What this pack added

```text
docs/agent-tasks/000_INDEX.md
docs/agent-tasks/001_TASK_PACK_LEDGER.md
docs/agent-tasks/002_NEXT_PACK_REQUEST.md
docs/agent-tasks/_TASK_TEMPLATE.md
docs/agent-tasks/_TASK_READINESS_CHECKLIST.md
docs/agent-tasks/_GATE_MATRIX.md
docs/agent-tasks/_SYSTEM_GATES.md
```

First task spec batch:

```text
M4_1_001_REAL_EVALUATION_REPORT_IMPORT
M4_1_002_STRICT_OUTPUT_CORPUS_FIXTURES
M4_1_003_REPAIR_POLICY_HARDENING
M5_001_LUA_EXECUTOR_CONTRACTS
M5_002_LUA_MANIFEST_VALIDATION
M5_003_LUA_STATIC_SANDBOX_POLICY
M6_001_ARTIFACT_TO_PACKAGE_MAPPING_CONTRACTS
```

## Current active gate assumption

```text
M4.1 real-model evaluation gate
```

Allowed now:

```text
- M4.1 report import/analyzer if report exists;
- strict output corpus/fake fixture coverage;
- prompt/repair/parser/validator hardening based on real evaluation evidence;
- docs/devflow/task-spec consistency work.
```

Locked now:

```text
- M5 Lua module executor production integration;
- M6 rich GamePackage assembly;
- broad artifact contract expansion;
- runtime preview repair loop.
```

## Do not regenerate in next pack

Do not replace the framework files unless repo review finds a concrete problem.

Do not regenerate M5/M6 specs from scratch. Amend them only if repository source changes make the contracts stale.

## Next pack should cover

Preferred next generated pack after this one is applied and pushed:

```text
agent-task-pack-002-m4-1-executable-specs
```

Suggested contents:

```text
- refine M4_1 task specs after reading actual current strict generation/evaluation source files;
- add one or two additional M4_1 proof-test specs if missing;
- update NEXT_PACK_REQUEST with observed gaps;
- do not unlock M5/M6 unless current state has changed.
```

## Open questions for next pack

1. Is a real strict LLM evaluation report present in `.llmgc/generator-plans/`?
2. Which strict output extraction/parser classes currently own fenced/text-before/text-after JSON handling?
3. Are docs consistency tests already present, or should a future task add them?
4. Should `check-all.ps1` gain named optional gates for docs/manifests/runtime/snapshots after fixtures exist?
