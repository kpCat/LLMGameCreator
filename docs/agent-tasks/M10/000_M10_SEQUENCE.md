# 000_M10_SEQUENCE.md — M10 export profiles and Unity IR sequence

This file is a locked sequence skeleton. It is routing and planning guidance, not an executable task spec.

## Gate status

```text
Status: locked_until_export_profiles_are_explicitly_unlocked
```

M10 executable work is allowed only when:

```text
- package generation/validation/runtime preview are stable enough;
- current-state docs explicitly allow export profile work;
- Unity/player boundary rules are reviewed against current source layout.
```

## Purpose

Prepare export profiles and a Unity-oriented intermediate representation without moving editor/generator logic into the player.

## Non-negotiable constraints

```text
- Unity/player receives data, not editor workflow;
- no runtime LLM/provider calls;
- no generated C# from LLM;
- export profiles are validated and deterministic;
- asset references are data contracts, not provider calls from player;
- editor generation remains outside player/runtime.
```

## Planned sequence

| Order | Task ID | Intent | Status |
|---:|---|---|---|
| 1 | M10_001 | Export profile contracts. | Skeleton only |
| 2 | M10_002 | Unity IR skeleton. | Skeleton only |
| 3 | M10_003 | Deterministic export package. | Skeleton only |
| 4 | M10_004 | Player boundary tests. | Skeleton only |
| 5 | M10_005 | Asset reference mapping. | Skeleton only |

## Future proof-test categories

```text
- export profile validates known good/bad profiles;
- Unity IR contains package data only;
- repeated export is deterministic;
- player boundary test proves no editor/generator/provider dependency;
- asset references map to stable ids/paths;
- invalid asset reference produces exact diagnostic.
```

## Stop rules

Stop instead of executing M10 if:

```text
- runtime/package validation is not stable;
- export would require LLM-generated C#;
- player would depend on editor/generator/provider code;
- asset/provider boundary is unclear;
- deterministic export cannot be pinned.
```
