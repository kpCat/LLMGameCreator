# 50_M8_RUNTIME_PREVIEW_VALIDATION.md — M8 runtime preview validation loop

Locked until package assembly path is stable or user explicitly starts infrastructure-only smoke work.

## Phase goal

Run generated/assembled packages through deterministic runtime smoke scenarios and convert failures into diagnostics/repair artifacts without runtime LLM calls.

## TASK M8-001 — Runtime smoke scenario fixture runner

Status: locked until gate/assembly stable unless user explicitly approves infrastructure task.
Requires approval: yes.

Objective: add fixture-based runtime smoke scenarios: load, validate, start, wait, optional command, serialize, deserialize, wait.

Source docs:

```text
docs/VALIDATION_STRATEGY.md
docs/CONTEXT_INDEX.md
.devflow/MODELING_STRATEGY.md
.devflow/CODE_QUALITY_AND_STYLE.md
```

Target areas:

```text
tests/LLMGameCreator.Tests/
tests/fixtures/runtime_smoke/
src/LLMGameCreator.Runtime/ only if existing service needs tiny seam
src/LLMGameCreator.Runtime.Abstractions/ only with approval
```

Required checks:

```text
runtime smoke fixture tests
serialization roundtrip test
check-all
```

Stop on:

```text
requires_runtime_architecture_change
requires_llm_or_provider_call
requires_more_than_8_files
requires_package_auto_repair
```

## TASK M8-002 — Runtime smoke failures become diagnostics

Status: locked until M8-001 done.
Requires approval: yes.

Objective: map runtime smoke failures into validation/diagnostic report entries that can be included in diagnostics bundle.

Non-goals:

```text
- no auto-repair;
- no UI-first implementation;
- no generated package mutation;
- no final player UX.
```
