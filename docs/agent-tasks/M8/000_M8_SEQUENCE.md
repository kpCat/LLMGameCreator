# 000_M8_SEQUENCE.md — M8 runtime preview validation loop sequence

This file is a locked sequence skeleton. It is routing and planning guidance, not an executable task spec.

## Gate status

```text
Status: locked_until_package_assembly_path_exists
```

M8 executable work is allowed only when:

```text
- a small assembled GamePackage exists;
- package validation is green;
- runtime boundaries are still clean;
- current-state docs or a gate decision explicitly selects runtime preview validation as next work.
```

## Purpose

Validate assembled packages in headless runtime scenarios before UI/Unity/export expansion.

## Non-negotiable constraints

```text
- runtime does not call LLM/provider/UI;
- rendering never mutates state;
- command input -> state/events output;
- snapshots/goldens are deterministic;
- runtime preview does not repair packages by calling generation services;
- package data is validated before runtime scenario execution.
```

## Planned sequence

| Order | Task ID | Intent | Status |
|---:|---|---|---|
| 1 | M8_001 | Package load smoke for a validated sample package. | Skeleton only |
| 2 | M8_002 | Deterministic command scenario. | Skeleton only |
| 3 | M8_003 | Event/state snapshot guard. | Skeleton only |
| 4 | M8_004 | Runtime no package mutation guard. | Skeleton only |
| 5 | M8_005 | Runtime diagnostic report for failed scenarios. | Skeleton only |

## Future proof-test categories

```text
- loading valid package succeeds;
- loading invalid package reports deterministic diagnostic;
- same command sequence gives same final state/events;
- runtime does not mutate package definition;
- runtime remains headless and provider-free;
- snapshot/golden is stable and small.
```

## Stop rules

Stop instead of executing M8 if:

```text
- no validated sample package exists;
- runtime would need LLM/provider/UI dependency;
- task requires package schema change without approval;
- deterministic scenario cannot be pinned.
```
