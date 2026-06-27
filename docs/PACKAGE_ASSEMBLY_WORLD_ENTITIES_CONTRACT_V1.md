# Package Assembly World Entities Contract v1

Status: Goal 025 mapping contract  
Final gate: `package_assembly_world_entities_expansion_verification`

## Purpose

`package_assembly_world_entities_contract_v1` defines the bounded mapping from
accepted planning artifacts and world/entity fixture artifacts into the existing
`GamePackage` schema.

This contract expands package assembly only for world and entities. It does not
change public `GamePackage` schema, run Unity, call LLM/RAG/providers/media,
execute arbitrary Lua, edit WinForms UI, edit `generator-library`, or start
Goal 026/S206.

## Accepted Inputs

- Goal 023 generator pipeline inputs from
  `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`.
- Goal 024 coverage matrix, gap report and next-slice plan from
  `.llmgc/procedural/rich-package-assembly-coverage-audit/`.
- Approved `scene_pack_v1` fixture artifacts.
- Approved `region_pack_v1` fixture artifacts.
- Approved `entity_pack_v1` fixture artifacts.
- Approved `npc_pack_v1` fixture artifacts.

## Existing Package Targets

- `GamePackageDefinition.Game.Maps`.
- `MapDefinition.Entities`.
- `GamePackageDefinition.Game.EntityPrototypes`.
- `GamePackageDefinition.GeneratedContent.Scenes`.
- `GamePackageDefinition.GeneratedContent.Regions`.
- `GamePackageDefinition.GeneratedContent.Npcs`.
- `GeneratedContent.AppliedArtifacts`.
- `GeneratedContent.PreservedArtifacts` when a future or unsupported record
  cannot be mapped.

## Output Statuses

- `mapped_package_field`: the fixture maps into an existing package field.
- `mapped_generated_content`: the fixture maps into existing generated-content
  sidecar fields.
- `preserved_sidecar`: the fixture is retained as sidecar/provenance because no
  current package field safely supports it.
- `future_required`: Goal 023/024 marked the capability as future work.
- `blocked_gap`: Goal 023/024 marked the topology/capability as blocked.
- `rejected_invalid`: validation rejected an invalid/fake/leak mutation.

## Mapping Rules

- `scene_pack_v1` maps scenes to existing package maps and
  `GeneratedContent.Scenes`.
- `entity_pack_v1` may create or update package entity prototypes through
  existing `GameDefinition.EntityPrototypes`.
- `entity_pack_v1` may create map placements through existing
  `MapDefinition.Entities` only when a record contains deterministic
  package-safe map or scene reference plus bounded position fields.
- `npc_pack_v1` always preserves NPC sidecar evidence through
  `GeneratedContent.Npcs`.
- `npc_pack_v1` may also create package entity prototypes or map placements only
  when explicit package-safe fields are present.
- `region_pack_v1` remains generated-content or sidecar evidence unless existing
  schema safely supports the requested package field.
- Goal 023/024 unsupported, future-required and blocked topology gaps remain
  gaps or sidecars. They are not converted into package support.

## Required Proof

Goal 025 proof requires:

- one real consumer fixture derived from accepted Goal 023/024 planning inputs;
- one independent synthetic future-consumer fixture named `npc_city_walk`;
- deterministic package summaries for both consumers;
- an invalid/fake/leak matrix that rejects gate, evidence, placement,
  anti-overfit, scope and external-execution false positives.

## Non-Goals

- No public `GamePackage` schema changes.
- No Unity runtime proof.
- No product vertical gate.
- No dialogue, quest, item, economy, combat or progression package expansion.
- No live runtime LLM/RAG/provider/media/Lua path.
- No Goal 026 or S206 work.
