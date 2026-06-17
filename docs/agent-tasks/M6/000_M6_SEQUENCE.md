# 000_M6_SEQUENCE.md — M6 rich GamePackage assembly sequence

This file is a locked sequence skeleton. It is routing and planning guidance, not an executable task spec.

## Gate status

```text
Status: locked_until_M4_1_gate_passes_and_artifact_envelope_is_stable
Current blocking gate: M4.1 real-model evaluation gate
```

M6 executable production work is allowed only when:

```text
- M4.1 has explicitly passed in current-state docs;
- either M5 has a safe artifact envelope, or current-state docs explicitly choose a non-Lua assembly path;
- package assembly assumptions are refreshed from current source layout.
```

Task-pack files alone cannot unlock M6.

## Purpose

Map reviewed/generated artifacts into GamePackage through explicit assembly contracts and validators.

## Non-negotiable constraints

```text
- no direct LLM output -> GamePackage apply;
- review/validation/apply boundary stays explicit;
- package validator must reject missing references;
- runtime remains independent of editor providers;
- sample package remains small but meaningful;
- assembly diagnostics must be stable and machine-readable.
```

## Existing starting specs

The following locked spec already exists and should be reviewed/refreshed before execution:

```text
M6_001_ARTIFACT_TO_PACKAGE_MAPPING_CONTRACTS.md
```

## Planned sequence

| Order | Task ID | Intent | Status |
|---:|---|---|---|
| 1 | M6_001 | Artifact-to-package mapping contracts only. | Existing locked spec |
| 2 | M6_002 | Artifact envelope -> package base mapping. | Locked draft spec |
| 3 | M6_003 | Items/economy mapping. | Locked draft spec |
| 4 | M6_004 | Scene/map mapping. | Locked draft spec |
| 5 | M6_005 | Dialogue/quest mapping. | Locked draft spec |
| 6 | M6_006 | Package validation after assembly. | Locked draft spec |
| 7 | M6_007 | Review/apply boundary for assembled package changes. | Locked draft spec |
| 8 | M6_008 | First rich sample package vertical slice. | Locked draft spec |

## Locked draft specs

```text
M6_002_BASE_MAPPING.md
M6_003_ITEMS_MAPPING.md
M6_004_SCENE_MAPPING.md
M6_005_QUEST_MAPPING.md
M6_006_VALIDATION.md
M6_007_REVIEW_APPLY.md
M6_008_SAMPLE_SLICE.md
```

These files are planning contracts only while M4.1/M5 remain unresolved. Before any of them becomes executable, refresh the allowed files, local source patterns, diagnostics, and proof tests from current source.

## Future proof-test categories

When converted to executable specs, M6 tasks must include exact proof tests for:

```text
- mapping valid artifact envelope to expected package fragment;
- rejecting missing/unknown references;
- rejecting duplicate ids;
- preserving review/apply boundary;
- no runtime/provider dependencies in package assembly;
- package validator passes for the sample package;
- deterministic assembly output order.
```

## Allowed implementation direction after unlock

```text
- Start with contracts and small mapping functions.
- Add one artifact family at a time.
- Validate after each assembly step.
- Keep sample package small and human-readable.
- Do not build a broad generator pipeline in one task.
```

## Stop rules

Stop instead of executing M6 if:

```text
- M4.1 has not explicitly passed;
- artifact envelope is not stable enough to map;
- implementation would bypass review/apply boundary;
- task requires direct LLM output -> package mutation;
- package schema change is needed but not explicitly approved;
- proof tests cannot pin exact mapping behavior.
```

## Candidate next pack after M5 entry

```text
agent-task-pack-009-m6-artifact-to-package-mapping-contracts
```
