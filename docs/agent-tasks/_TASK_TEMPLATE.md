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

## Shared quality rules

Applicable shared docs:

```text
docs/agent-tasks/_TEST_QUALITY_RULES.md
docs/agent-tasks/_FIXTURE_AND_GOLDEN_RULES.md
docs/agent-tasks/_DIFF_HYGIENE_RULES.md
docs/agent-tasks/_AGENT_EXECUTION_QUALITY_RULES.md
```

Task-specific quality emphasis:

```text
- exact diagnostic assertions:
- fixture/golden discipline:
- diff hygiene risk:
- existing style preservation:
```

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

Generated/local files policy:

```text
Generated run outputs/logs/TRX/build artifacts may be referenced by path in reports but must not be intended source changes.
```

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

Required exact assertions:

```text
- diagnostic code:
- severity:
- target/path:
- state:
- count:
- order:
- no mutation:
```

Regression tests:

Golden/snapshot fixtures:

Fake/corpus requirements:

Weak-test warning:

```text
Assert.False/Assert.Single/Assert.NotEmpty alone are not proof tests.
```

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

```text
- proof test cannot assert exact behavior;
- required files are outside Allowed files;
- task requires future locked phase work;
- task requires broad refactor;
- task requires test weakening;
- task requires generated run artifacts as source without explicit approval.
```

## Non-goals

Non-goals:

## Expected final report

Final report must include:

```text
- changed files;
- proof tests added;
- exact assertion contracts;
- fixtures/goldens added or changed;
- focused test result;
- check-all result;
- diff hygiene status;
- generated local artifacts and whether they are intended source changes;
- next task pointer;
- risks/uncertainties.
```

## Next task pointer

On success, suggest NEXT_TASK:

On block, write BLOCKERS:
