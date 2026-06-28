# Goal 027: Package Assembly Expansion 3 - Items, Economy And Crafting

## Starting gate

This goal may start only after the user explicitly provides:

```text
package_assembly_dialogue_quests_expansion_verification passed
```

Goal 026 must already be reviewed from the pushed repository. Do not re-open Goal 026 unless a concrete pushed defect is found.

## Goal type and proof level

Task type:

```text
module_implementation + integration_slice
```

Required proof level:

```text
Level 2/3
```

This is a bounded composite goal. Its internal phases are:

```text
Contract -> Module -> Integration -> Proof
```

Those phases are internal checkpoints of this one Goal 027. Do not split them into separate default goals unless a stop condition is hit.

## Final gate

Stop at exactly one final gate:

```text
package_assembly_items_economy_crafting_expansion_verification
```

Leave this gate `required`, not `passed`.

Do not start Goal 028 or S220. Do not create a product vertical gate in this goal.

## Product / generator outcome

Goal 025 expanded package assembly for world/entities. Goal 026 expanded package assembly for dialogue/quests. Goal 027 must now expand package assembly for item/economy/crafting data through existing public `GamePackage` schema and existing validators.

Concrete improvement:

```text
accepted Goal 023/024/025/026 evidence
  -> item/economy/crafting mapping contract
  -> bounded package assembly module expansion for items + resources + recipes + transactions/loot/inventory basics
  -> integration with existing GeneratorPlanGamePackageAssembler / EconomyDefinitionValidator
  -> real consumer + synthetic future-consumer anti-overfit fixture
  -> deterministic proof artifacts
```

This is not a full playable product vertical. It must not claim a new playable/manual runtime review result.

## Read first

Read these before editing:

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/CURRENT_GENERATOR_STATE.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/MODULAR_CONTRACT_GOAL_POLICY.md`
8. `docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md`
9. `docs/GOAL_026_PACKAGE_ASSEMBLY_EXPANSION_2_DIALOGUE_AND_QUESTS.md`
10. `docs/PACKAGE_ASSEMBLY_DIALOGUE_QUESTS_CONTRACT_V1.md`
11. `.llmgc/procedural/package-assembly-dialogue-quests/package-assembly-dialogue-quests-report.json`
12. `.llmgc/procedural/package-assembly-dialogue-quests/package-assembly-dialogue-quests-package-summary.json`
13. `docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md`
14. `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-report.json`
15. `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`
16. `src/LLMGameCreator.Domain/Definitions/ContentDefinitions.cs`
17. `src/LLMGameCreator.Domain/Definitions/EconomyDefinitions.cs`
18. `src/LLMGameCreator.Application/Validation/EconomyDefinitionValidator.cs`
19. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
20. nearest tests for `GeneratorPlanGamePackageAssembler`, economy validation, item/recipe/transaction runtime and previous package assembly goals.

Do not broad-scan the repository when the local package assembly and economy validator seams are enough.

## Scope

Allowed:

- New mapping contract:
  - `docs/PACKAGE_ASSEMBLY_ITEMS_ECONOMY_CRAFTING_CONTRACT_V1.md`
- New narrow Application-layer proof/acceptance service:
  - `src/LLMGameCreator.Application/Design/PackageAssemblyItemsEconomyCrafting/**`
- Minimal bounded edit to existing package assembler:
  - `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
- New focused tests:
  - `tests/LLMGameCreator.Tests/Application/PackageAssemblyItemsEconomyCrafting/**`
- New product/integration smoke test:
  - `tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyItemsEconomyCraftingSmokeTests.cs`
- One product smoke route addition:
  - `.devflow/scripts/run-product-smoke.ps1`
- Compact current artifact root:
  - `.llmgc/procedural/package-assembly-items-economy-crafting/**`
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Bounded handoff-regression test updates only if `check-all.ps1` requires them after state advances to Goal 027.

Forbidden:

- Do not change public `GamePackage` schema.
- Do not edit `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`.
- Do not edit `src/LLMGameCreator.Domain/Definitions/ContentDefinitions.cs`.
- Do not edit `src/LLMGameCreator.Domain/Definitions/EconomyDefinitions.cs`.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms UI.
- Do not change Unity build/player entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit `generator-library/**`.
- Do not mutate accepted historical `.llmgc/procedural/**` artifact families outside `.llmgc/procedural/package-assembly-items-economy-crafting/**`.
- Do not implement combat/progression package expansion in this goal except for safe existing economy references that validators require.
- Do not start Goal 028 or S220.
- Do not claim product vertical proof or manual playable review.

## Budget

Default limits:

- production implementation files: max 2;
- new Application service/model files under Goal 027 folder: max 3;
- focused test files: max 2;
- product smoke test files: max 1;
- docs/state/routing files: max 6 unless state handoff requires more;
- artifact family roots: exactly 1 current root;
- hotfix attempts: max 2.

Stop and return a split/diagnosis plan if more than 10 files must change, if package schema changes are needed, or if honest proof cannot be produced without broad runtime/Unity work.

## Required artifact root

All compact artifacts for this goal must be written under:

```text
.llmgc/procedural/package-assembly-items-economy-crafting/
```

Suggested files:

```text
package-assembly-items-economy-crafting-mapping-contract-proof.json
package-assembly-items-economy-crafting-input-fixtures.json
package-assembly-items-economy-crafting-assembly-report.json
package-assembly-items-economy-crafting-package-summary.json
package-assembly-items-economy-crafting-anti-overfit-fixtures.json
package-assembly-items-economy-crafting-invalid-matrix.json
package-assembly-items-economy-crafting-report.json
package-assembly-items-economy-crafting-report.md
package-assembly-items-economy-crafting-verification.md
goal-027-final-artifact-scope-report.json
goal-027-final-artifact-scope-report.md
```

## Required internal phases

### S213: Record accepted Goal 026 and current position

Record that the user accepted:

```text
package_assembly_dialogue_quests_expansion_verification passed
```

Update state/queue docs so current work becomes Goal 027 and the current gate after this goal is:

```text
package_assembly_items_economy_crafting_expansion_verification required
```

Queue handling:

- Goal 027 is `Package Assembly Expansion 3 - Items, Economy And Crafting`.
- Goal 028 remains future work, normally `Package Assembly Expansion 4 - Combat And Progression`.
- Do not start Goal 028 or S220.

### S214: Contract phase - items/economy/crafting package mapping contract

Create `docs/PACKAGE_ASSEMBLY_ITEMS_ECONOMY_CRAFTING_CONTRACT_V1.md`.

It must define:

- accepted inputs:
  - Goal 023 generator pipeline inputs;
  - Goal 024 coverage matrix / gap report / next-slice plan;
  - Goal 025 world/entity assembly artifacts;
  - Goal 026 dialogue/quest assembly artifacts;
  - approved `item_pack_v1`, `resource_pack_v1`, `recipe_pack_v1`, `loot_pack_v1`, `transaction_pack_v1`, `inventory_pack_v1`, and `equipment_pack_v1`-style fixture artifacts;
- existing package targets:
  - `GamePackageDefinition.Game.Items`;
  - `GamePackageDefinition.Game.Resources`;
  - `GamePackageDefinition.Game.Recipes`;
  - `GamePackageDefinition.Game.LootTables`;
  - `GamePackageDefinition.Game.Transactions`;
  - `GamePackageDefinition.Game.Inventories`;
  - `GamePackageDefinition.Game.EquipmentSlots`;
  - `GeneratedContent.Items`;
  - `GeneratedContent.AppliedArtifacts`;
  - `GeneratedContent.PreservedArtifacts` when a future/unsupported record cannot be mapped;
- output statuses:
  - `mapped_package_field`;
  - `mapped_generated_content`;
  - `preserved_sidecar`;
  - `future_required`;
  - `blocked_gap`;
  - `rejected_invalid`;
- proof requirement:
  - one real consumer fixture, preferably trade caravan vendor/crafting/economy flow or another accepted planning profile;
  - one second/synthetic consumer fixture, preferably `vendor_crafting_transaction`, proving output is not hardcoded to one scenario;
- non-goals:
  - no public schema changes;
  - no Unity runtime proof;
  - no combat/progression expansion;
  - no live runtime LLM/RAG/provider/media/Lua.

### S215: Module phase - bounded items/economy/crafting assembly expansion

Implement a bounded package assembly expansion through the existing `GeneratorPlanGamePackageAssembler` seam where possible.

Minimum mapping improvement:

- `item_pack_v1` must create/update package items through existing `GameDefinition.Items`;
- `resource_pack_v1` must create/update package resources through existing `GameDefinition.Resources`;
- `recipe_pack_v1` must create/update package recipes through existing `GameDefinition.Recipes`;
- `loot_pack_v1` must create/update package loot tables through existing `GameDefinition.LootTables`;
- `transaction_pack_v1` or `vendor_pack_v1` must create/update package transactions through existing `GameDefinition.Transactions`;
- `inventory_pack_v1` may create/update package inventories through existing `GameDefinition.Inventories` when owner and stack data are valid;
- `equipment_pack_v1` may create/update package equipment slots through existing `GameDefinition.EquipmentSlots` when slot data are valid;
- generated item sidecars under `GeneratedContent.Items` must remain present where existing behavior needs them;
- unsupported/future/blocked Goal 023/024 gaps must remain preserved as gaps or sidecars, not converted into package support.

Do not alter `GamePackageDefinition`, domain definitions or validators unless the task is stopped and a split/schema plan is returned.

### S216: Integration phase - real consumer and synthetic anti-overfit fixture

Create deterministic Goal 027 fixtures inside the acceptance service/test path, not as production runtime data.

Required consumers:

1. Real consumer:
   - derived from accepted trade caravan social/economy planning where practical;
   - uses existing package assembly seam;
   - proves at least one package item, resource, recipe, loot table, transaction and inventory/slot or equipment slot can be assembled;
   - proves transaction costs/outputs and recipe inputs/outputs validate against known item/resource ids.

2. Synthetic future-consumer fixture:
   - use id/name such as `vendor_crafting_transaction`;
   - include an independent workshop/vendor transaction and recipe shape;
   - prove the assembly output is not hardcoded to trade caravan names;
   - does not need to implement future vendor AI, economy simulation or runtime UI.

Both consumers must produce deterministic package summaries and mapping diagnostics.

### S217: Proof phase - invalid/fake/leak matrix

Minimum invalid/fake/leak scenarios:

1. missing accepted Goal 026 gate is rejected;
2. missing Goal 026 dialogue/quest evidence is rejected;
3. missing Goal 025 world/entity evidence is rejected;
4. missing Goal 023 generator input evidence is rejected;
5. public `GamePackage` schema mutation claim is rejected;
6. item missing id/name is rejected;
7. recipe output references unknown item/resource id is rejected;
8. recipe input or cost has invalid amount is rejected;
9. loot entry references unknown item/resource id is rejected;
10. transaction stock loot table references unknown loot table is rejected;
11. inventory stack references unknown item id is rejected;
12. duplicate item/resource/recipe/transaction id is rejected or deterministically de-duplicated with diagnostic;
13. future-required vendor/economy/crafting gap treated as implemented is rejected;
14. synthetic anti-overfit fixture missing is rejected;
15. output hardcoded only to trade/frontier/gothic consumer is rejected;
16. package assembly claims Unity/LLM/RAG/provider/media/Lua execution is rejected;
17. Goal 028/S220 started marker is rejected;
18. historical Goal 020-026 artifact mutation is rejected by scope guard or equivalent verification.

Invalid cases should flow through real package/economy validation where possible. Do not fake diagnostics when a real mapping/validation helper can produce them.

### S218: Product/integration smoke and focused tests

Add product smoke route:

```text
package-assembly-items-economy-crafting
```

This smoke is an automated integration smoke, not a product vertical gate.

The smoke must:

- build/write compact artifacts under `.llmgc/procedural/package-assembly-items-economy-crafting/`;
- validate report shape;
- verify `accepted=false`;
- verify final/manual gate is `package_assembly_items_economy_crafting_expansion_verification`;
- verify previous accepted gate is `package_assembly_dialogue_quests_expansion_verification passed`;
- verify no public schema changes;
- verify package assembly is bounded to items/economy/crafting;
- verify real consumer and synthetic fixture both produce deterministic outputs;
- verify future/blocked gaps are preserved;
- verify no Unity/LLM/RAG/provider/media/Lua execution claims;
- verify Goal 028/S220 not started.

Focused tests must cover at minimum:

- deterministic outputs;
- real consumer produces item/resource/recipe/loot table/transaction/inventory or equipment slot;
- synthetic `vendor_crafting_transaction` consumer produces independent output;
- transaction and recipe references validate against known item/resource ids;
- anti-overfit proof rejects single-consumer hardcoding;
- invalid matrix rejects required scenarios;
- no top-level `severity=error` diagnostics when proof passes;
- state docs record Goal 026 accepted before Goal 027.

### S219: State handoff, artifacts and final scope guard

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Current next work after Goal 027 must be:

```text
package_assembly_items_economy_crafting_expansion_verification
```

Do not mark it passed.

Final report must include:

```text
accepted=false
finalStatus=package_assembly_items_economy_crafting_expansion_verification
manualGate=package_assembly_items_economy_crafting_expansion_verification
previousAcceptedGate=package_assembly_dialogue_quests_expansion_verification passed
goal026EvidenceVerified=true
goal025EvidenceVerified=true
goal024EvidenceVerified=true
goal023EvidenceVerified=true
realConsumerPassed=true
syntheticConsumerPassed=true
antiOverfitProofPassed=true
itemsEconomyCraftingMappingWritten=true
packageSummaryWritten=true
packageAssemblyExecuted=true
productVerticalGate=false
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
unityBuildExecuted=false
llmRagProviderMediaLuaExecuted=false
scopeGuardPassed=true
invalidMatrix.passed=true
```

`packageAssemblyExecuted=true` means bounded in-memory/package-data assembly through the existing Application package assembly seam. It must not mean product vertical, Unity build, runtime playtest or public schema mutation.

Top-level diagnostics must contain no `severity=error` when `contractProofPassed=true`.

## Required verification

Run, at minimum:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~PackageAssemblyItemsEconomyCrafting|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario package-assembly-items-economy-crafting
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-027-final -AllowedPath docs/PACKAGE_ASSEMBLY_ITEMS_ECONOMY_CRAFTING_CONTRACT_V1.md -AllowedPathPrefix src/LLMGameCreator.Application/Design/PackageAssemblyItemsEconomyCrafting/ -AllowedPath src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs -AllowedPathPrefix tests/LLMGameCreator.Tests/Application/PackageAssemblyItemsEconomyCrafting/ -AllowedPath tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyItemsEconomyCraftingSmokeTests.cs -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPathPrefix .llmgc/procedural/package-assembly-items-economy-crafting/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/GOAL_027_PACKAGE_ASSEMBLY_EXPANSION_3_ITEMS_ECONOMY_CRAFTING.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-027-package-assembly-expansion-3-items-economy-crafting-CODEX_GOAL.md -AllowedPath tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/CapabilityBundlePipelineInputsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/RichPackageAssemblyCoverageAudit/RichPackageAssemblyCoverageAuditAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyWorldEntities/PackageAssemblyWorldEntitiesAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyDialogueQuests/PackageAssemblyDialogueQuestsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs
```

If stale handoff regression tests fail after state advances to Goal 027, update only the minimal allowed assertion and rerun focused tests plus `check-all.ps1`.

## Pre-final self-review

Before final report, directly inspect:

- `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-report.json`;
- `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-assembly-report.json`;
- `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-package-summary.json`;
- `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-anti-overfit-fixtures.json`;
- `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-invalid-matrix.json`;
- `docs/CURRENT_GENERATOR_STATE.json`;
- `docs/CONTEXT_INDEX.md`;
- the final artifact scope report.

Acceptance evidence table is required in final Codex response:

```markdown
| Acceptance criterion | Evidence path/test | Status |
|---|---|---|
```

## Final response requirements

The final Codex response must include:

- changed files;
- new contract/service/test/product-smoke paths;
- compact artifact paths;
- real consumer and synthetic fixture summary;
- mapping/package summary;
- generated report/artifact hashes;
- invalid matrix count;
- focused/product-smoke/check-all/scope-guard verification results;
- acceptance evidence table;
- whether final valid report has zero top-level error diagnostics;
- confirmation that `package_assembly_items_economy_crafting_expansion_verification` remains required, not passed;
- confirmation that Goal 028/S220 was not started;
- confirmation that no public `GamePackage` schema changed;
- confirmation that no product vertical gate was claimed;
- exact bounded git commands used, or confirmation none were used except through `check-artifact-scope.ps1`.
