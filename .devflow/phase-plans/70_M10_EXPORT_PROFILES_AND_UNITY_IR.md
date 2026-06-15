# 70_M10_EXPORT_PROFILES_AND_UNITY_IR.md — M10 export profiles and Unity IR

Locked until package generation is stable.

## Phase goal

Define validated export profiles and Unity-facing IR as data, not arbitrary generated Unity code.

## TASK M10-001 — Export profile / Unity IR contract design

Status: future locked.
Requires approval: yes.

Objective: define a small validated IR/export profile contract that consumes stable GamePackage data.

Source docs:

```text
docs/ROADMAP_TO_FULL_GENERATOR.md
docs/GAME_PACKAGE_FORMAT.md
.devflow/FINAL_ACCEPTANCE_CRITERIA.md
.devflow/CODE_QUALITY_AND_STYLE.md
```

Target areas:

```text
docs/
src/LLMGameCreator.Application/ only if validator/dry-run is approved
tests/LLMGameCreator.Tests/
```

Required checks:

```text
IR schema validation test
asset/prefab binding refs test
export dry-run report test
check-all
```

Stop on:

```text
arbitrary_unity_csharp_generation
full_unity_project_rewrite
bypassing_gamepackage
requires_more_than_8_files
```
