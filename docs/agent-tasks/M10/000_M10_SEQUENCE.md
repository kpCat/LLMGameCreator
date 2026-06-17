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

Task-pack files alone cannot unlock M10.

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

| Order | Task ID | Intent | Status | Spec |
|---:|---|---|---|---|
| 1 | M10_001 | Export profile contracts. | Locked draft | `M10_001_EXPORTS.md` |
| 2 | M10_002 | Unity IR skeleton. | Locked draft | `M10_002_UNITY_IR.md` |
| 3 | M10_003 | Deterministic export package. | Locked draft | `M10_003_PACKAGE.md` |
| 4 | M10_004 | Player boundary tests. | Locked draft | `M10_004_BOUNDARY.md` |
| 5 | M10_005 | Asset reference mapping. | Locked draft | `M10_005_ASSETS.md` |

## Future proof-test categories

```text
- export profile validates known good/bad profiles;
- Unity IR contains package data only;
- repeated export is deterministic;
- player boundary test proves no editor/generator/provider dependency;
- asset references map to stable ids/paths;
- invalid asset reference produces exact diagnostic.
```

## Allowed implementation direction after unlock

```text
- Start with data-only export profile contracts.
- Keep Unity IR as a neutral, validated data contract.
- Do not create Unity project files from LLM output.
- Keep player/runtime free of editor/generator/provider dependencies.
- Add one asset mapping category at a time.
```

## Stop rules

Stop instead of executing M10 if:

```text
- runtime/package validation is not stable;
- export would require LLM-generated C#;
- player would depend on editor/generator/provider code;
- asset/provider boundary is unclear;
- deterministic export cannot be pinned;
- current-state docs do not explicitly unlock export profile work.
```

## Candidate next pack

```text
agent-task-pack-012-m4-1-execution-support-or-roadmap-freeze
```
