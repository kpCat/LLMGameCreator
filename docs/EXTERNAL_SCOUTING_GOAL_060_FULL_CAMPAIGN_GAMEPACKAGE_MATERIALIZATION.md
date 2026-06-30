# External scouting — Goal 060 Full Campaign GamePackage Materialization Matrix

## Decision

No new external dependencies for Goal 060.

Goal 060 must consume the already accepted/produced evidence chain and materialize package-compatible artifacts through existing repository contracts and validators. This is a domain integration and proof task, not a generic serialization, schema, ECS, graph, or Unity-package adoption task.

## Considered and deferred

- JSON schema tooling / NJsonSchema / JsonSchema.Net: deferred. Goal 060 needs existing GamePackage validator/assembler compatibility, not a second external schema authority.
- MessagePack / binary serialization: deferred. Goal 060 evidence must remain deterministic, reviewable and compact JSON plus existing package/runtime serializer proof.
- Tiled/TMX/LDtk/Unity asset tools: deferred. Goal 060 should not adopt a map-editor/export ecosystem before package materialization and runtime consumption are proven.
- ECS libraries: deferred. Runtime ownership exists through current runtime state and Unity Alpha proof path; adopting ECS would be a separate architecture decision.
- Additional Unity packages: rejected for this goal. Reuse the repo-local Unity Alpha path and existing deterministic manifests.

## Required posture

- BCL-only Application seam.
- No public GamePackage schema changes.
- No external package additions.
- Use existing validators and package/runtime/Unity proof paths where available.
- If a real package materialization path cannot be proven, commit/push a BLOCKED result with evidence explaining the missing seam rather than fabricating a green report.
