# 000_M9_SEQUENCE.md — M9 templates and balancing sequence

This file is a locked sequence skeleton. It is routing and planning guidance, not an executable task spec.

## Gate status

```text
Status: locked_until_generation_and_package_validation_paths_are_stable
```

M9 executable work is allowed only when:

```text
- generated/assembled packages have deterministic validation;
- runtime preview or equivalent smoke coverage exists;
- current-state docs explicitly allow template/balancing expansion.
```

## Purpose

Introduce reusable template families, numeric balancing constraints, and progression fixtures.

## Non-negotiable constraints

```text
- no vague balancing without assertions;
- tests pin numeric ranges and rejection diagnostics;
- generated data remains data/contracts, not C# code;
- template expansion must remain deterministic under seed;
- formula constraints must fail visibly with diagnostic codes.
```

## Planned sequence

| Order | Task ID | Intent | Status |
|---:|---|---|---|
| 1 | M9_001 | Template family contracts. | Skeleton only |
| 2 | M9_002 | Numeric range constraints. | Skeleton only |
| 3 | M9_003 | Progression/balance fixtures. | Skeleton only |
| 4 | M9_004 | Formula diagnostics. | Skeleton only |
| 5 | M9_005 | Sample template packs. | Skeleton only |

## Future proof-test categories

```text
- valid template expands to expected data envelope;
- invalid numeric range is rejected with exact diagnostic;
- progression fixture remains deterministic;
- formula diagnostic identifies formula/path/id;
- sample template pack validates as package input;
- balancing tests assert ranges, not vague non-empty output.
```

## Stop rules

Stop instead of executing M9 if:

```text
- package assembly/validation is not stable;
- generated output would become C# code;
- balancing cannot be expressed as exact assertions;
- task would require broad schema changes without approval.
```
