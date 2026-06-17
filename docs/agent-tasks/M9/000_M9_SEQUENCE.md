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
- current-state docs explicitly allow template/balancing expansion;
- task assumptions are refreshed from current source layout before execution.
```

Task-pack files alone cannot unlock M9.

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
| 1 | M9_001 | Template family contracts. | Locked draft |
| 2 | M9_002 | Numeric range constraints. | Locked draft |
| 3 | M9_003 | Progression/balance fixtures. | Locked draft |
| 4 | M9_004 | Formula diagnostics. | Locked draft |
| 5 | M9_005 | Sample template packs. | Locked draft |

## Draft spec files

```text
M9_001_TEMPLATES.md
M9_002_RANGES.md
M9_003_PROGRESSION.md
M9_004_FORMULAS.md
M9_005_SAMPLE_PACKS.md
```

## Future proof-test categories

```text
- valid template expands to expected data envelope;
- invalid numeric range is rejected with exact diagnostic;
- progression fixture remains deterministic;
- formula diagnostic identifies formula/path/id;
- sample template pack validates as package input;
- balancing tests assert ranges, not vague non-empty output.
```

## Allowed implementation direction after unlock

```text
- define contracts before broad template expansion;
- keep each template family small and data-only;
- prefer deterministic fixtures/goldens over subjective balance claims;
- use exact numeric bounds and exact diagnostics;
- keep sample template packs human-readable and small.
```

## Stop rules

Stop instead of executing M9 if:

```text
- package assembly/validation is not stable;
- generated output would become C# code;
- balancing cannot be expressed as exact assertions;
- task would require broad schema changes without approval;
- template expansion would bypass artifact/package review boundaries.
```

## Candidate next pack

```text
agent-task-pack-011-next-step-by-gate-state
```
