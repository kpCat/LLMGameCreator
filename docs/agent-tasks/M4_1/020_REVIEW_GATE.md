# 020_REVIEW_GATE.md — post-agent review gate for M4.1

Use this checklist after each Kilo/local-agent task branch before merging or requesting the next task.

## Hard pass conditions

```text
- check-all.ps1 passed;
- check-devflow-state.ps1 passed or only known non-fatal warning is explained;
- changed files match the task spec Allowed files;
- forbidden files were not touched;
- no M5/M6/M8/M9/M10 production implementation appeared;
- no .sln/.csproj/dependency changes unless explicitly allowed;
- no generated .devflow/runs/** artifacts are committed;
- proof tests assert exact diagnostic codes, exact golden output, or exact state as required;
- CURRENT_RUN.md explains what happened;
- NEXT_TASK.md points to one next task or a stop/review state.
```

## Warning signs

```text
- broad refactor not asked by task;
- tests only assert non-empty/success/failure without contract details;
- fixture/golden files are huge or unclear;
- markdown/golden tests depend on unstable timestamps/paths/order;
- agent rewrites unrelated files for style;
- agent changes docs to make tests pass instead of implementing the task;
- agent silently relaxes parser/validator behavior;
- agent bypasses review/apply or gate policy.
```

## Review outcomes

### Pass

```text
Merge or keep branch as clean evidence. Continue to the next queued task.
```

### Repair required

```text
Do not proceed to the next feature task. Request a focused repair pack or give Kilo a stabilization prompt.
```

### Reject branch

```text
Do not merge. Start over from a clean branch with a smaller prompt.
```

## Evidence to provide for assistant review

```text
- branch name;
- task id;
- Kilo final report;
- check-all output;
- changed file list;
- any warnings/blockers;
- whether user wants repair, next task, or gate decision.
```
