# _AGENT_EXECUTION_QUALITY_RULES.md — execution quality rules for local agents

This file records lessons from real local-agent execution.

It is not a task spec. It applies to all future task specs.

## Principle

A weaker model can produce good work when the task is bounded, but only if it is forced to prove behavior precisely.

Good execution is not just “tests pass”. Good execution means:

```text
- correct scope;
- exact proof tests;
- clean diff;
- preserved architecture;
- preserved style;
- useful report;
- no hidden artifacts;
- next cursor is safe.
```

## What the agent must do

For every task:

```text
1. Read the normal devflow orientation docs.
2. Read docs/agent-tasks/000_INDEX.md.
3. Read shared quality docs.
4. Read exactly one task spec.
5. Write a mini-plan in CURRENT_RUN.md.
6. Make the smallest possible change.
7. Add proof tests before or with implementation.
8. Run required checks.
9. Repair at most 2 times.
10. Update CURRENT_RUN.md and NEXT_TASK.md.
11. Stop unless explicitly allowed to continue.
```

## What the agent must not do

```text
- invent architecture;
- broaden scope;
- continue after missing approval;
- perform future locked phase work;
- weaken tests;
- add noisy generated artifacts to source;
- use real providers in tests;
- rewrite unrelated code style;
- hide uncertainty.
```

## Reporting standard

A useful report says what can be verified.

It must include:

```text
Task id:
Changed files:
Patterns inspected:
Proof tests added:
Exact diagnostic/state/order assertions:
Checks run:
Run directory:
Generated local artifacts:
Diff hygiene status:
Stop conditions checked:
Next task suggestion:
Uncertainty:
```
