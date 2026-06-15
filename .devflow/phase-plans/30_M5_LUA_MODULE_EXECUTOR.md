# 30_M5_LUA_MODULE_EXECUTOR.md — M5 Lua module executor integration

Locked until M4.1 gate explicitly passes in `docs/CURRENT_GENERATOR_STATE.md` and `.json`.

## Phase goal

Run approved deterministic Lua generator modules through sandbox, manifests and validation, producing validated artifacts without direct GamePackage mutation.

## Hard boundaries

```text
- no unrestricted Lua;
- no filesystem/network/process/OS/debug APIs;
- no direct C# GameState mutation;
- no package mutation from Lua;
- no runtime LLM/provider;
- no broad activation for all modules.
```

## TASK M5-001 — Confirm sandbox/readiness and one module family scope

Status: locked until gate passes.
Requires approval: yes.

Objective: choose exactly one safe Lua module family and verify existing sandbox/static analyzer boundaries.

Source docs:

```text
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/LUA_SCRIPTING.md
docs/SCRIPT_MANIFEST_SPEC.md
docs/LLM_LUA_CSHARP_RESPONSIBILITY_CONTRACT.md
docs/CONTEXT_INDEX.md
.devflow/CODE_QUALITY_AND_STYLE.md
```

Target areas:

```text
src/LLMGameCreator.Scripting/
src/LLMGameCreator.Application/Design/
tests/LLMGameCreator.Tests/
generator-library/manifests/ only as metadata input
```

Required checks:

```text
sandbox forbidden API tests
manifest capability check tests
deterministic output test
check-all
```

Stop on:

```text
m4_1_gate_not_passed
requires_unrestricted_lua
requires_filesystem_or_network
requires_package_mutation
requires_more_than_8_files
```

## TASK M5-002 — One approved Lua module produces validated artifact

Status: locked until M5-001 done and approved.
Requires approval: yes.

Objective: execute/capture one safe deterministic module output as an artifact, validate it, and stage it without applying to GamePackage.

Non-goals:

```text
- no broad executor;
- no runtime integration;
- no package apply;
- no hidden Lua API expansion.
```
