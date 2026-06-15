# 10_M4_1_EVALUATION_STABILIZATION.md — M4.1 strict LLM evaluation stabilization

Read only when working on current M4.1 gate, strict generation evaluation, parser/repair/validator hardening.

## Phase goal

Before expanding contracts, Lua execution or rich assembly, prove strict LLM generation quality through real evaluation reports and deterministic fake/corpus tests.

## Gate rule

M5/M6/M8 remain blocked until current state explicitly says M4.1 gate passed.

## TASK M4EVAL-001 — Import/analyze real evaluation report

Status: ready when user provides evaluation JSON/report or existing report path is present.

Objective: summarize pass rate, repair recovery, diagnostic hot spots, quality warnings and recommendations without provider calls.

Allowed before M4.1 gate: yes.
Requires approval: no if report already exists and no DB/schema change is needed.

Source docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/CURRENT_GENERATOR_STATE.md
docs/CONTEXT_INDEX.md
.devflow/MODELING_STRATEGY.md
.devflow/CODE_QUALITY_AND_STYLE.md
```

Target areas:

```text
src/LLMGameCreator.Application/
src/LLMGameCreator.WinForms/Pages/ only if UI mapping is explicitly needed
tests/LLMGameCreator.Tests/
.devflow/CURRENT_RUN.md
```

Non-goals:

```text
- no LLM/provider calls;
- no GamePackage mutation;
- no artifact contract expansion;
- no DB schema change without approval.
```

Implementation notes:

```text
- Prefer a small service or analyzer over UI-first code.
- If UI changes are needed, keep UI thin over service/presenter.
- Tests should use saved JSON/report fixture, not real provider.
```

Required checks:

```text
dotnet build
dotnet test
focused analyzer/import tests
check-all
```

Stop on:

```text
requires_db_schema_change
requires_provider_call
requires_more_than_8_files
report_format_unclear_without_sample
```

Next candidate: M4HARDEN-001 if report shows concrete diagnostic hot spots.

## TASK M4HARDEN-001 — One bounded strict generation hardening

Status: ready only with concrete evidence: evaluation hot spot, failing fixture, or repeated diagnostic.

Objective: make one bounded improvement in raw output extraction, parse diagnostics, repair guidance or contract validation.

Allowed before M4.1 gate: yes.
Requires approval: no if bounded and no schema/contract expansion.

Source docs:

```text
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md
docs/PROMPT_AND_ARTIFACT_CONTRACT_HARDENING.md
.devflow/MODELING_STRATEGY.md
.devflow/CODE_QUALITY_AND_STYLE.md
```

Target areas:

```text
src/LLMGameCreator.Application/Design/
src/LLMGameCreator.Generation/
tests/LLMGameCreator.Tests/
tests/fixtures/ if needed
```

Non-goals:

```text
- no broad new contracts;
- no M5/M6;
- no runtime LLM;
- no real LLM in tests;
- no parser rewrite without focused fixture.
```

Implementation notes:

```text
- Start by adding or identifying a failing fake/corpus fixture.
- Fix the smallest layer that owns the failure.
- Validate repaired output again after repair.
- Add stable diagnostic code for failures.
```

Required checks:

```text
dotnet build
dotnet test
fake LLM/parser/repair tests
check-all
```

Stop on:

```text
needs_schema_change
needs_more_than_8_files
no_repro_fixture
fix_requires_contract_expansion
```

Next candidate: repeat M4HARDEN-001 for another independent hot spot, or update current state only after user reviews real results.
