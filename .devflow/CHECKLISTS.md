# CHECKLISTS.md

## Pre-task checklist

- [ ] Read `.devflow/LOCAL_AGENT_ROLE.md`
- [ ] Read `.devflow/AUTONOMOUS_RUNBOOK.md`
- [ ] Read `.devflow/STOP_CONDITIONS.md`
- [ ] Read `.devflow/NEXT_TASK.md`
- [ ] Read task entry in `.devflow/TASK_GRAPH.json`
- [ ] Read `AGENTS.md`
- [ ] Read `docs/CONTEXT_INDEX.md`
- [ ] Read `docs/CURRENT_GENERATOR_STATE.md`
- [ ] Confirm task is not blocked
- [ ] Confirm max changed files
- [ ] Confirm required checks
- [ ] Identify 2-3 local analogs for code task

## Patch checklist

- [ ] Scope unchanged
- [ ] No broad refactor
- [ ] No schema change
- [ ] No new dependency
- [ ] No runtime LLM/provider/UI dependency
- [ ] No UI direct JSON write
- [ ] No real LLM call in tests
- [ ] Stable diagnostics for new failures
- [ ] Tests/fixtures added if required by matrix

## Completion checklist

- [ ] `check-all.ps1` run or reason recorded
- [ ] Build result recorded
- [ ] Test result recorded
- [ ] Simulation/modeling result recorded if applicable
- [ ] `CURRENT_RUN.md` updated
- [ ] `BLOCKERS.md` updated if blocked
- [ ] `NEXT_TASK.md` updated only if safe
- [ ] Final report follows `RUN_REPORT_TEMPLATE.md`
