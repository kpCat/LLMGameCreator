# Package Assembly Combat Progression Contract v1

Status: Goal 028 mapping contract  
Final gate: `package_assembly_combat_progression_expansion_verification`

## Purpose

`package_assembly_combat_progression_contract_v1` defines the bounded mapping
from accepted planning, coverage, world/entity, dialogue/quest and
items/economy/crafting artifacts into the existing `GamePackage` schema for
stats, abilities, statuses, progressions and encounters.

This contract expands package assembly only for combat/progression package data.
It does not change public `GamePackage` schema, run Unity, call
LLM/RAG/providers/media, execute arbitrary Lua, edit WinForms UI, edit
`generator-library`, start Goal 029/S227, or create a product vertical gate.

## Accepted Inputs

- Goal 023 generator pipeline inputs from
  `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`.
- Goal 024 coverage matrix, gap report and next-slice plan from
  `.llmgc/procedural/rich-package-assembly-coverage-audit/`.
- Goal 025 world/entity assembly artifacts from
  `.llmgc/procedural/package-assembly-world-entities/`.
- Goal 026 dialogue/quest assembly artifacts from
  `.llmgc/procedural/package-assembly-dialogue-quests/`.
- Goal 027 item/economy/crafting assembly artifacts from
  `.llmgc/procedural/package-assembly-items-economy-crafting/`.
- Approved `stat_pack_v1`, `ability_pack_v1`, `status_pack_v1`,
  `progression_pack_v1`, `encounter_pack_v1` and `combat_pack_v1` fixture
  artifacts.

## Existing Package Targets

- `GamePackageDefinition.Game.Stats`.
- `GamePackageDefinition.Game.Abilities`.
- `GamePackageDefinition.Game.Statuses`.
- `GamePackageDefinition.Game.Progressions`.
- `ProgressionDefinition.Stages`.
- `GamePackageDefinition.Game.Encounters`.
- `EncounterDefinition.Participants`.
- `EncounterDefinition.Actions`.
- `GeneratedContent.Encounters`.
- `GeneratedContent.Mechanics`.
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
- `blocked_gap`: Goal 023/024 marked the capability as blocked.
- `rejected_invalid`: validation rejected an invalid/fake/leak mutation.

## Mapping Rules

- `stat_pack_v1` maps stats into `GameDefinition.Stats`.
- `ability_pack_v1` maps abilities, costs, effects and generated mechanic
  sidecars into `GameDefinition.Abilities` and `GeneratedContent.Mechanics`.
- `status_pack_v1` maps statuses into `GameDefinition.Statuses`.
- `progression_pack_v1` maps progressions and deterministic stages into
  `GameDefinition.Progressions`.
- `encounter_pack_v1` maps encounters, participants, actions and generated
  encounter sidecars into `GameDefinition.Encounters` and
  `GeneratedContent.Encounters`.
- `combat_pack_v1` may carry compatible ability and encounter records through
  the same existing package targets.
- Goal 023/024 future-required combat AI, tactical presentation, deep
  progression, status runtime semantics and unavailable runtime UI gaps remain
  gaps or sidecars. They are not converted into package support.

## Required Proof

Goal 028 proof requires:

- one real consumer fixture derived from accepted frontier survival combat
  planning inputs;
- one independent synthetic future-consumer fixture named
  `alternate_encounter_status_progression`;
- deterministic package summaries for both consumers;
- an invalid/fake/leak matrix that rejects gate, evidence, missing id/name,
  broken ability/progression/encounter references, anti-overfit, scope and
  external-execution false positives.

## Non-Goals

- No public `GamePackage` schema changes.
- No Unity runtime proof.
- No full package vertical gate.
- No Goal 029 or S227 work.
- No live runtime LLM/RAG/provider/media/Lua path.
