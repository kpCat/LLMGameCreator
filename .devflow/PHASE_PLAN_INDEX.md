# PHASE_PLAN_INDEX.md — маршрутизатор фазовых планов

Этот файл нужен, чтобы агент не читал все планы разработки одновременно.

## Правило чтения

```text
Read this index, then read exactly one phase plan file relevant to NEXT_TASK/current gate.
Do not read all phase-plans in one run.
```

## Current gate

Текущий known gate: M4.1 real-model evaluation gate.

До явного обновления `docs/CURRENT_GENERATOR_STATE.md` и `.json` нельзя выполнять M5/M6/M8 как production work.

## Phase plans

| Phase | File | Read when |
|---|---|---|
| Devflow baseline | `.devflow/phase-plans/00_DEVFLOW_BASELINE.md` | baseline, script hardening, local agent setup |
| M4.1 evaluation stabilization | `.devflow/phase-plans/10_M4_1_EVALUATION_STABILIZATION.md` | real evaluation report, strict parser/repair/validator hardening |
| Simulation/observability foundation | `.devflow/phase-plans/20_SIMULATION_OBSERVABILITY_FOUNDATION.md` | raw-output corpus, fake clients, diagnostics bundle |
| M5 Lua module executor | `.devflow/phase-plans/30_M5_LUA_MODULE_EXECUTOR.md` | only after M4.1 gate explicitly passes |
| M6 rich GamePackage assembly | `.devflow/phase-plans/40_M6_RICH_GAMEPACKAGE_ASSEMBLY.md` | only after M4.1 gate explicitly passes |
| M8 runtime preview validation | `.devflow/phase-plans/50_M8_RUNTIME_PREVIEW_VALIDATION.md` | only after assembly path is stable or user explicitly starts infrastructure |
| M9 template families/balancing | `.devflow/phase-plans/60_M9_TEMPLATE_FAMILIES_AND_BALANCING.md` | after at least one family lifecycle is stable |
| M10 export profiles/Unity IR | `.devflow/phase-plans/70_M10_EXPORT_PROFILES_AND_UNITY_IR.md` | after package generation is stable |

## If NEXT_TASK is unclear

1. Check `docs/CURRENT_GENERATOR_STATE.md`.
2. Prefer the lowest-numbered phase not blocked by current gate.
3. If no task is safely available, stop and write `.devflow/BLOCKERS.md`.

## Do not execute future phases early

Future phase files are allowed to exist as planning material. Their existence is not approval to execute them.
