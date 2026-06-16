# _TASK_TEMPLATE.md — executable agent task spec template

Use this template for every new `docs/agent-tasks/**` task spec.

```text
This task spec is executable guidance. If implementation conflicts with this file, stop and report.
```

# TASK_ID — Title

## Header

Task ID:

Milestone:

Status:

Depends on:

Unlocks:

Risk level:

Expected changed files count:

## Gate status

Allowed before current gate review:

Requires user approval:

Approval text required in NEXT_TASK.md:

## Source of truth

Source-of-truth docs:

Context budget:

Read only these docs:

Do not read:

Existing patterns to inspect:

## File boundaries

Allowed files:

Forbidden files:

Deleted files:

## API / implementation contract

New interfaces:

New classes:

New methods:

Modified classes:

Public contracts changed:

Schema changed:

New dependencies:

## Exact behavior

Input contract:

Output contract:

Success behavior:

Failure behavior:

Diagnostic codes:

Validation rules:

Security/sandbox rules:

Persistence rules:

GamePackage mutation rule:

## Proof tests

Tests to add before/with implementation:

Required pass tests:

Required fail/reject tests:

Regression tests:

Golden/snapshot fixtures:

Fake/corpus requirements:

## System gates

Build commands:

Unit/integration test commands:

Docs consistency commands:

Manifest integrity commands:

Artifact schema commands:

Package validator commands:

Runtime scenario commands:

Snapshot/golden commands:

## Stop conditions

Stop if:

## Non-goals

Non-goals:

## Expected final report

Final report must include:

## Next task pointer

On success, suggest NEXT_TASK:

On block, write BLOCKERS:
```
