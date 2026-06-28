# Package Assembly Dialogue Quests Contract v1

Status: Goal 026 mapping contract  
Final gate: `package_assembly_dialogue_quests_expansion_verification`

## Purpose

`package_assembly_dialogue_quests_contract_v1` defines the bounded mapping from
accepted planning, coverage and Goal 025 world/entity artifacts into the
existing `GamePackage` schema for dialogue and quests.

This contract expands package assembly only for dialogue and quests. It does
not change public `GamePackage` schema, run Unity, call LLM/RAG/providers/media,
execute arbitrary Lua, edit WinForms UI, edit `generator-library`, start Goal
027/S213, or create a product vertical gate.

## Accepted Inputs

- Goal 023 generator pipeline inputs from
  `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`.
- Goal 024 coverage matrix, gap report and next-slice plan from
  `.llmgc/procedural/rich-package-assembly-coverage-audit/`.
- Goal 025 world/entities report and package summary from
  `.llmgc/procedural/package-assembly-world-entities/`.
- Approved `dialogue_pack_v1` fixture artifacts.
- Approved `quest_pack_v1` fixture artifacts.

## Existing Package Targets

- `GamePackageDefinition.Game.Dialogues`.
- `DialogueDefinition.Nodes`.
- `DialogueNodeDefinition.Choices`.
- `DialogueChoiceDefinition.StartQuestId`.
- `DialogueChoiceDefinition.AdvanceQuestId`.
- `DialogueChoiceDefinition.SetQuestStageId`.
- `GamePackageDefinition.Game.Quests`.
- `QuestDefinition.Objectives`.
- `QuestDefinition.Stages`.
- `GeneratedContent.Dialogues`.
- `GeneratedContent.Quests`.
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

- `quest_pack_v1` maps quests into `GameDefinition.Quests`.
- `quest_pack_v1` maps deterministic objectives into
  `QuestDefinition.Objectives`.
- `quest_pack_v1` maps deterministic stages into `QuestDefinition.Stages`.
- `dialogue_pack_v1` maps dialogues into `GameDefinition.Dialogues`.
- `dialogue_pack_v1` maps nodes and choices into existing dialogue fields.
- Dialogue choices may reference known quest ids through `StartQuestId`,
  `AdvanceQuestId` and `SetQuestStageId` when the fixture declares those links.
- `GeneratedContent.Dialogues` and `GeneratedContent.Quests` remain present as
  sidecar/provenance records.
- Goal 023/024 future-required dialogue graph, quest graph, condition and reward
  gaps remain gaps or sidecars. They are not converted into package support.

## Required Proof

Goal 026 proof requires:

- one real consumer fixture derived from accepted gothic/trade/frontier planning
  inputs;
- one independent synthetic future-consumer fixture named
  `rumor_board_tutorial`;
- deterministic package summaries for both consumers;
- an invalid/fake/leak matrix that rejects gate, evidence, dialogue graph,
  quest stage, quest link, anti-overfit, scope and external-execution false
  positives.

## Non-Goals

- No public `GamePackage` schema changes.
- No Unity runtime proof.
- No product vertical gate.
- No item, economy, crafting, combat or progression package expansion.
- No live runtime LLM/RAG/provider/media/Lua path.
- No Goal 027 or S213 work.
