# 018_EXEC_QUEUE.md — M4.1 execution queue

This file defines the practical M4.1 execution queue after the documentation roadmap freeze.

## Primary deterministic hardening queue

Run these one at a time on a dedicated execution branch:

```text
M4_1_005 -> M4_1_006 -> M4_1_008
```

Meaning:

```text
M4_1_005: evaluation markdown/golden recommendations
M4_1_006: strict repair prompt guardrails
M4_1_008: agent task docs consistency guard
```

## Real-evaluation closure queue

When real/manual strict evaluation evidence exists:

```text
M4_1_013 -> M4_1_014 -> M4_1_015 -> M4_1_016 -> M4_1_017
```

Meaning:

```text
M4_1_013: user-facing strict evaluation runbook
M4_1_014: evidence manifest discipline
M4_1_015: report import fixture guard
M4_1_016: M4 gate closure decision
M4_1_017: final completion checklist
```

## Optional automation queue

Use only with user approval:

```text
M4_1_009 -> M4_1_012
```

Meaning:

```text
M4_1_009: named devflow gates/check-all automation
M4_1_012: overnight/local-agent run review gate
```

## Stop points

Stop and ask for review if:

```text
- check-all fails;
- task touches files outside Allowed files;
- task modifies M5/M6/M8/M9/M10 production code;
- task changes .sln/.csproj without explicit approval;
- task weakens tests or removes exact assertions;
- task cannot prove its behavior with deterministic fixtures/goldens;
- M4.1 gate decision needs user judgment.
```

## Branch naming suggestions

```text
kilo-m4-1-005
kilo-m4-1-006
kilo-m4-1-008
kilo-m4-1-gate
```
