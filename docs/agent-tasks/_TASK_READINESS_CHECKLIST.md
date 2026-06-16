# _TASK_READINESS_CHECKLIST.md — task readiness gate

A task spec is not ready for autonomous local-agent execution until all applicable items are true.

## Required

```text
[ ] Task ID is stable and unique.
[ ] Current gate allows this task, or task is explicitly marked locked.
[ ] Dependencies are listed.
[ ] Source-of-truth docs are listed.
[ ] Context budget is bounded.
[ ] Allowed files are listed.
[ ] Forbidden files are listed.
[ ] Existing local patterns to inspect are listed.
[ ] Exact behavior is testable.
[ ] Failure behavior is testable.
[ ] Diagnostic codes are specified or the task says none are added.
[ ] GamePackage mutation rule is explicit.
[ ] New dependencies are explicitly forbidden or approved.
[ ] Public contract/schema changes are explicitly forbidden or approved.
[ ] At least one proof test is specified.
[ ] System gates are specified.
[ ] Stop conditions are specified.
[ ] Expected final report is specified.
[ ] Next task pointer is specified.
```

## Automatic stop

The agent must stop if:

```text
- proof tests are missing;
- task requires files outside Allowed files;
- implementation requires a schema/dependency/project change not approved by the task;
- task is locked by current gate;
- task asks to weaken validation or delete tests;
- task cannot identify a local pattern to follow.
```

## Task authoring rule

Do not write task specs like:

```text
Integrate Lua.
Improve runtime.
Make package assembly richer.
Fix generation.
```

Write task specs like:

```text
Add ILuaGeneratorModuleExecutor contract only, with request/result DTOs, diagnostic codes, no real execution, and contract tests proving GamePackage is not mutated.
```
