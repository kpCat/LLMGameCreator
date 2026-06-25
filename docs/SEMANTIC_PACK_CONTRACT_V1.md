# Semantic Pack Contract v1

Status: accepted for Goal 005.

`semantic_pack_contract_v1` is a compact authoring input for the existing semantic catalog foundation. Raw layers are not consumed directly by generation. They are validated and compiled into `.llmgc/semantic/compiled-semantic-pack.json` first.

## Layers

Supported layer kinds:

- `core`: small shipped base vocabulary.
- `genre`: audited genre/example vocabulary.
- `project`: project-specific accepted overlay.
- `imported_candidate`: untrusted imported suggestions.
- `llm_candidate`: untrusted LLM-proposed suggestions.

Active precedence is:

```text
project > genre > core
```

Candidate layers never override accepted layers and never enter active generation merely because they exist.

## Fields

Each layer declares:

- `schemaVersion`: `semantic_pack_contract_v1`.
- `layerId`: safe two-segment id such as `genre/wildland_frontier`.
- `layerKind`: one supported layer kind.
- `source`: relative provenance text, not a rooted or traversal path.
- `terms`: ordered semantic declarations.
- `relations`: ordered game-useful relation declarations.

Each term declares a safe `termId`, `kind`, `label`, `status`, optional `aliases`, `tags`, `generationHints`, `constraints` and `notes`.

Each relation declares safe `sourceTermId`, `relationKind`, `targetTermId`, `status` and optional `tags`.

## Status Policy

Supported statuses are:

- `known`
- `candidate`
- `deprecated`
- `conflict`
- `invalid`

Only `known` declarations from `core`, `genre` and `project` layers can enter the active compiled catalog. Candidate, deprecated, conflict and invalid declarations remain quarantined and are reported.

## Relation Allow-List

Goal 005 validates game-useful relations only:

- `requires`
- `excludes`
- `implies`
- `compatible_with`
- `preferred_in_tone`
- `forbidden_in_tone`
- `prefers_quest_pattern`
- `prefers_dialogue_intent`
- `prefers_interaction_family`

Relation endpoints must resolve to active compiled terms or to accepted external Goal 004 rule-pack ids supplied by the narrow composition adapter.

## Safety Rules

Unsafe ids, malformed statuses, unsupported relation kinds, rooted provenance paths and traversal-looking source paths are rejected with diagnostics.

Adding a new safe term or a relation instance is data-only. Adding a new semantic kind, relation semantics or executable gameplay primitive requires a reviewed C# contract change.
