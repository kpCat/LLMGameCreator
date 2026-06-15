# 40_M6_RICH_GAMEPACKAGE_ASSEMBLY.md — M6 rich GamePackage assembly

Locked until M4.1 gate explicitly passes.

## Phase goal

Expand assembly from narrow approved artifacts into richer GamePackage domains where schema already supports it.

## Hard boundaries

```text
- no schema change without explicit migration task;
- no arbitrary artifact writes;
- no direct apply without validation/dry-run/audit;
- no auto-approval;
- no LLM/runtime coupling.
```

## TASK M6-001 — Assembly coverage map from approved artifacts to existing schema

Status: locked until gate passes.
Requires approval: yes.

Objective: document and test one capability family mapping from approved artifacts to existing GamePackage schema.

Source docs:

```text
docs/GAME_PACKAGE_FORMAT.md
docs/GENERATOR_PLAN_ARTIFACT_REVIEW_UI.md
docs/VALIDATION_STRATEGY.md
docs/CONTEXT_INDEX.md
.devflow/CODE_QUALITY_AND_STYLE.md
```

Target areas:

```text
src/LLMGameCreator.Application/
src/LLMGameCreator.GamePackage/
tests/LLMGameCreator.Tests/
tests/fixtures/package_assembly/ if created
```

Required checks:

```text
fixture artifacts -> assembly -> package validation
invalid refs rejection test
check-all
```

Stop on:

```text
m4_1_gate_not_passed
needs_schema_change
requires_more_than_8_files
auto_apply_without_review
```

## TASK M6-002 — One family baseline-valid assembled package

Status: locked until M6-001 done.
Requires approval: yes.

Objective: produce a baseline-valid package from approved fixture artifacts for one chosen gameplay family.

Non-goals:

```text
- no broad family expansion;
- no schema migration;
- no Unity/export work;
- no runtime repair loop.
```
