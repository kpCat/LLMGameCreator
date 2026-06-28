# Package Assembly Items Economy Crafting Contract v1

Status: Goal 027 mapping contract  
Final gate: `package_assembly_items_economy_crafting_expansion_verification`

## Purpose

`package_assembly_items_economy_crafting_contract_v1` defines the bounded
mapping from accepted planning, coverage, world/entity and dialogue/quest
artifacts into the existing `GamePackage` schema for items, resources,
recipes, loot tables, transactions, inventories and equipment slots.

This contract expands package assembly only for item/economy/crafting package
data. It does not change public `GamePackage` schema, run Unity, call
LLM/RAG/providers/media, execute arbitrary Lua, edit WinForms UI, edit
`generator-library`, start Goal 028/S220, or create a product vertical gate.

## Accepted Inputs

- Goal 023 generator pipeline inputs from
  `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`.
- Goal 024 coverage matrix, gap report and next-slice plan from
  `.llmgc/procedural/rich-package-assembly-coverage-audit/`.
- Goal 025 world/entity assembly artifacts from
  `.llmgc/procedural/package-assembly-world-entities/`.
- Goal 026 dialogue/quest assembly artifacts from
  `.llmgc/procedural/package-assembly-dialogue-quests/`.
- Approved `item_pack_v1`, `resource_pack_v1`, `recipe_pack_v1`,
  `loot_pack_v1`, `transaction_pack_v1`, `inventory_pack_v1` and
  `equipment_pack_v1` fixture artifacts.

## Existing Package Targets

- `GamePackageDefinition.Game.Items`.
- `GamePackageDefinition.Game.Resources`.
- `GamePackageDefinition.Game.Recipes`.
- `GamePackageDefinition.Game.LootTables`.
- `GamePackageDefinition.Game.Transactions`.
- `GamePackageDefinition.Game.Inventories`.
- `GamePackageDefinition.Game.EquipmentSlots`.
- `GeneratedContent.Items`.
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

- `item_pack_v1` maps items into `GameDefinition.Items` and keeps generated
  item sidecars in `GeneratedContent.Items`.
- `resource_pack_v1` maps resources into `GameDefinition.Resources`.
- `recipe_pack_v1` maps deterministic recipe inputs, costs and outputs into
  `GameDefinition.Recipes`.
- `loot_pack_v1` maps loot tables and entries into `GameDefinition.LootTables`.
- `transaction_pack_v1` and `vendor_pack_v1` map bounded vendor/shop records
  into `GameDefinition.Transactions`.
- `inventory_pack_v1` maps valid owner/stack data into
  `GameDefinition.Inventories`.
- `equipment_pack_v1` maps valid slot data into
  `GameDefinition.EquipmentSlots`.
- Goal 023/024 future-required vendor AI, economy simulation, richer crafting
  rules and unavailable runtime UI gaps remain gaps or sidecars. They are not
  converted into package support.

## Required Proof

Goal 027 proof requires:

- one real consumer fixture derived from accepted trade-caravan
  social/economy planning inputs;
- one independent synthetic future-consumer fixture named
  `vendor_crafting_transaction`;
- deterministic package summaries for both consumers;
- an invalid/fake/leak matrix that rejects gate, evidence, missing id/name,
  broken recipe/loot/transaction/inventory references, anti-overfit, scope and
  external-execution false positives.

## Non-Goals

- No public `GamePackage` schema changes.
- No Unity runtime proof.
- No product vertical gate.
- No combat or progression package expansion.
- No live runtime LLM/RAG/provider/media/Lua path.
- No Goal 028 or S220 work.
