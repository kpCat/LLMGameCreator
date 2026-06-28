# Goal 028: Package Assembly Expansion 4 - Combat And Progression

## Starting gate

This goal may start only after the user explicitly provides:

```text
package_assembly_items_economy_crafting_expansion_verification passed
```

Goal 027 must already be reviewed from the pushed repository. Do not re-open Goal 027 unless a concrete pushed defect is found.

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

Those phases are internal checkpoints of this one Goal 028. Do not split them into separate default goals unless a stop condition is hit.

## Final gate

Stop at exactly one final gate:

```text
package_assembly_combat_progression_expansion_verification
```

Leave this gate `required`, not `passed`.

Do not start Goal 029 or S227. Do not create the rare product vertical gate in this goal.

## Product / generator outcome

Goals 025-027 expanded package assembly for world/entities, dialogue/quests and items/economy/crafting. Goal 028 must now expand package assembly for combat and progression data through existing public `GamePackage` schema and existing validators.

Concrete improvement:

```text
accepted Goal 023/024/025/026/027 evidence
  -> combat/progression mapping contract
  -> bounded package assembly module expansion for stats + abilities + statuses + progressions + encounters
  -> integration with existing GeneratorPlanGamePackageAssembler / EncounterDefinitionValidator
  -> real consumer + synthetic future-consumer anti-overfit fixture
  -> deterministic proof artifacts
```

This is not a full package vertical and not a playable/manual runtime review. Goal 029 remains the first planned rare product vertical gate unless the user changes direction.

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
9. `docs/GOAL_027_PACKAGE_ASSEMBLY_EXPANSION_3_ITEMS_ECONOMY_CRAFTING.md`
10. `docs/PACKAGE_ASSEMBLY_ITEMS_ECONOMY_CRAFTING_CONTRACT_V1.md`
11. `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-report.json`
12. `.llmgc/procedural/package-assembly-items-economy-crafting/package-assembly-items-economy-crafting-package-summary.json`
13. `docs/GOAL_026_PACKAGE_ASSEMBLY_EXPANSION_2_DIALOGUE_AND_QUESTS.md`
14. `docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md`
15. `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`
16. `src/LLMGameCreator.Domain/Definitions/ContentDefinitions.cs`
17. `src/LLMGameCreator.Domain/Definitions/EncounterDefinitions.cs`
18. `src/LLMGameCreator.Domain/Definitions/EconomyDefinitions.cs`
19. `src/LLMGameCreator.Application/Validation/EncounterDefinitionValidator.cs`
20. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
21. nearest tests for `GeneratorPlanGamePackageAssembler`, encounter validation, combat/progression runtime and previous package assembly goals.

Do not broad-scan the repository when the local package assembly and encounter validator seams are enough.

## Scope

Allowed:

- New mapping contract:
  - `docs/PACKAGE_ASSEMBLY_COMBAT_PROGRESSION_CONTRACT_V1.md`
- New narrow Application-layer proof/acceptance service:
  - `src/LLMGameCreator.Application/Design/PackageAssemblyCombatProgression/**`
- Minimal bounded edit to existing package assembler:
  - `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
- New focused tests:
  - `tests/LLMGameCreator.Tests/Application/PackageAssemblyCombatProgression/**`
- New product/integration smoke test:
  - `tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyCombatProgressionSmokeTests.cs`
- One product smoke route addition:
  - `.devflow/scripts/run-product-smoke.ps1`
- Compact current artifact root:
  - `.llmgc/procedural/package-assembly-combat-progression/**`
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Bounded handoff-regression test updates only if `check-all.ps1` requires them after state advances to Goal 028.

Forbidden:

- Do not change public `GamePackage` schema.
- Do not edit `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`.
- Do not edit `src/LLMGameCreator.Domain/Definitions/ContentDefinitions.cs`.
- Do not edit `src/LLMGameCreator.Domain/Definitions/EncounterDefinitions.cs`.
- Do not edit `src/LLMGameCreator.Domain/Definitions/EconomyDefinitions.cs`.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms UI.
- Do not change Unity build/player entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit `generator-library/**`.
- Do not mutate accepted historical `.llmgc/procedural/**` artifact families outside `.llmgc/procedural/package-assembly-combat-progression/**`.
- Do not implement full vertical playable package composition in this goal.
- Do not start Goal 029 or S227.
- Do not claim product vertical proof or manual playable review.

## Budget

Default limits:

- production implementation files: max 2;
- new Application service/model files under Goal 028 folder: max 3;
- focused test files: max 2;
- product smoke test files: max 1;
- docs/state/routing files: max 6 unless state handoff requires more;
- artifact family roots: exactly 1 current root;
- hotfix attempts: max 2.

Stop and return a split/diagnosis plan if more than 10 files must change, if package schema changes are needed, or if honest proof cannot be produced without broad runtime/Unity work.

## Required artifact root

All compact artifacts for this goal must be written under:

```text
.llmgc/procedural/package-assembly-combat-progression/
```

Suggested files:

```text
package-assembly-combat-progression-mapping-contract-proof.json
package-assembly-combat-progression-input-fixtures.json
package-assembly-combat-progression-assembly-report.json
package-assembly-combat-progression-package-summary.json
package-assembly-combat-progression-anti-overfit-fixtures.json
package-assembly-combat-progression-invalid-matrix.json
package-assembly-combat-progression-report.json
package-assembly-combat-progression-report.md
package-assembly-combat-progression-verification.md
goal-028-final-artifact-scope-report.json
goal-028-final-artifact-scope-report.md
```

## Required internal phases

### S220: Record accepted Goal 027 and current position

Record that the user accepted:

```text
package_assembly_items_economy_crafting_expansion_verification passed
```

Update state/queue docs so current work becomes Goal 028 and the current gate after this goal is:

```text
package_assembly_combat_progression_expansion_verification required
```

Queue handling:

- Goal 028 is `Package Assembly Expansion 4 - Combat And Progression`.
- Goal 029 remains future work, normally `Full Package Assembly Vertical`.
- Do not start Goal 029 or S227.

### S221: Contract phase - combat/progression package mapping contract

Create `docs/PACKAGE_ASSEMBLY_COMBAT_PROGRESSION_CONTRACT_V1.md`.

It must define:

- accepted inputs:
  - Goal 023 generator pipeline inputs;
  - Goal 024 coverage matrix / gap report / next-slice plan;
  - Goal 025 world/entity assembly artifacts;
  - Goal 026 dialogue/quest assembly artifacts;
  - Goal 027 item/economy/crafting assembly artifacts;
  - approved `stat_pack_v1`, `ability_pack_v1`, `status_pack_v1`, `progression_pack_v1`, `encounter_pack_v1` and `combat_pack_v1`-style fixture artifacts;
- existing package targets:
  - `GamePackageDefinition.Game.Stats`;
  - `GamePackageDefinition.Game.Abilities`;
  - `GamePackageDefinition.Game.Statuses`;
  - `GamePackageDefinition.Game.Progressions`;
  - `ProgressionDefinition.Stages`;
  - `GamePackageDefinition.Game.Encounters`;
  - `EncounterDefinition.Participants`;
  - `EncounterDefinition.Actions`;
  - `GeneratedContent.Encounters`;
  - `GeneratedContent.Mechanics`;
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
  - one real consumer fixture, preferably frontier survival encounter/progression flow or another accepted planning profile;
  - one second/synthetic consumer fixture, preferably `alternate_encounter_status_progression`, proving output is not hardcoded to one scenario;
- non-goals:
  - no public schema changes;
  - no Unity runtime proof;
  - no full vertical package composition;
  - no live runtime LLM/RAG/provider/media/Lua.

### S222: Module phase - bounded combat/progression assembly expansion

Implement a bounded package assembly expansion through the existing `GeneratorPlanGamePackageAssembler` seam where possible.

Minimum mapping improvement:

- `stat_pack_v1` must create/update package stats through existing `GameDefinition.Stats`;
- `ability_pack_v1` or compatible mechanics input must create/update package abilities through existing `GameDefinition.Abilities`;
- `status_pack_v1` must create/update package statuses through existing `GameDefinition.Statuses`;
- `progression_pack_v1` must create/update package progressions and stages through existing `GameDefinition.Progressions`;
- `encounter_pack_v1` or compatible combat input must create/update package encounters, participants and actions through existing `GameDefinition.Encounters`;
- generated encounter/mechanic sidecars under `GeneratedContent.Encounters` and `GeneratedContent.Mechanics` must remain present where existing behavior needs them;
- unsupported/future/blocked Goal 023/024 gaps must remain preserved as gaps or sidecars, not converted into package support.

Do not alter `GamePackageDefinition`, domain definitions or validators unless the task is stopped and a split/schema plan is returned.

### S223: Integration phase - real consumer and synthetic anti-overfit fixture

Create deterministic Goal 028 fixtures inside the acceptance service/test path, not as production runtime data.

Required consumers:

1. Real consumer:
   - derived from accepted frontier survival / combat planning where practical;
   - uses existing package assembly seam;
   - proves at least one package stat, ability, status, progression with stage, encounter, participant and action can be assembled;
   - proves encounter participant/action references validate against known stat/resource/ability/entity/loot ids where applicable.

2. Synthetic future-consumer fixture:
   - use id/name such as `alternate_encounter_status_progression`;
   - include an independent encounter/progression/status shape;
   - prove the assembly output is not hardcoded to frontier/survival names;
   - does not need to implement future tactical AI, Unity combat view or runtime UI.

Both consumers must produce deterministic package summaries and mapping diagnostics.

### S224: Proof phase - invalid/fake/leak matrix

Minimum invalid/fake/leak scenarios:

1. missing accepted Goal 027 gate is rejected;
2. missing Goal 027 items/economy/crafting evidence is rejected;
3. missing Goal 026 dialogue/quest evidence is rejected;
4. missing Goal 025 world/entity evidence is rejected;
5. missing Goal 023 generator input evidence is rejected;
6. public `GamePackage` schema mutation claim is rejected;
7. stat missing id/name is rejected;
8. ability references unknown resource id is rejected;
9. ability cost has invalid amount is rejected;
10. progression stage has invalid required amount is rejected;
11. encounter participant references unknown entity prototype id is rejected;
12. encounter participant/action references unknown ability id is rejected;
13. encounter loot table references unknown loot table id is rejected;
14. duplicate stat/ability/status/progression/encounter id is rejected or deterministically de-duplicated with diagnostic;
15. future-required combat/progression/status gap treated as implemented is rejected;
16. synthetic anti-overfit fixture missing is rejected;
17. output hardcoded only to frontier/survival consumer is rejected;
18. package assembly claims Unity/LLM/RAG/provider/media/Lua execution is rejected;
19. Goal 029/S227 started marker is rejected;
20. historical Goal 020-027 artifact mutation is rejected by scope guard or equivalent verification.

Invalid cases should flow through real package/encounter validation where possible. Do not fake diagnostics when a real mapping/validation helper can produce them.

### S225: Product/integration smoke and focused tests

Add product smoke route:

```text
package-assembly-combat-progression
```

This smoke is an automated integration smoke, not a product vertical gate.

The smoke must:

- build/write compact artifacts under `.llmgc/procedural/package-assembly-combat-progression/`;
- validate report shape;
- verify `accepted=false`;
- verify final/manual gate is `package_assembly_combat_progression_expansion_verification`;
- verify previous accepted gate is `package_assembly_items_economy_crafting_expansion_verification passed`;
- verify no public schema changes;
- verify package assembly is bounded to combat/progression;
- verify real consumer and synthetic fixture both produce deterministic outputs;
- verify future/blocked gaps are preserved;
- verify no Unity/LLM/RAG/provider/media/Lua execution claims;
- verify Goal 029/S227 not started.

Focused tests must cover at minimum:

- deterministic outputs;
- real consumer produces stat/ability/status/progression/encounter/participant/action;
- synthetic `alternate_encounter_status_progression` consumer produces independent output;
- encounter references validate against known package ids;
- anti-overfit proof rejects single-consumer hardcoding;
- invalid matrix rejects required scenarios;
- no top-level `severity=error` diagnostics when proof passes;
- state docs record Goal 027 accepted before Goal 028.

### S226: State handoff, artifacts and final scope guard

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Current next work after Goal 028 must be:

```text
package_assembly_combat_progression_expansion_verification
```

Do not mark it passed.

Final report must include:

```text
accepted=false
finalStatus=package_assembly_combat_progression_expansion_verification
manualGate=package_assembly_combat_progression_expansion_verification
previousAcceptedGate=package_assembly_items_economy_crafting_expansion_verification passed
goal027EvidenceVerified=true
goal026EvidenceVerified=true
goal025EvidenceVerified=true
goal024EvidenceVerified=true
goal023EvidenceVerified=true
realConsumerPassed=true
syntheticConsumerPassed=true
antiOverfitProofPassed=true
combatProgressionMappingWritten=true
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
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~PackageAssemblyCombatProgression|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario package-assembly-combat-progression
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-028-final -AllowedPath docs/PACKAGE_ASSEMBLY_COMBAT_PROGRESSION_CONTRACT_V1.md -AllowedPathPrefix src/LLMGameCreator.Application/Design/PackageAssemblyCombatProgression/ -AllowedPath src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs -AllowedPathPrefix tests/LLMGameCreator.Tests/Application/PackageAssemblyCombatProgression/ -AllowedPath tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyCombatProgressionSmokeTests.cs -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPathPrefix .llmgc/procedural/package-assembly-combat-progression/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/GOAL_028_PACKAGE_ASSEMBLY_EXPANSION_4_COMBAT_PROGRESSION.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-028-package-assembly-expansion-4-combat-progression-CODEX_GOAL.md -AllowedPath tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/CapabilityBundlePipelineInputsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/RichPackageAssemblyCoverageAudit/RichPackageAssemblyCoverageAuditAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyWorldEntities/PackageAssemblyWorldEntitiesAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyDialogueQuests/PackageAssemblyDialogueQuestsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyItemsEconomyCrafting/PackageAssemblyItemsEconomyCraftingAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs
```

If stale handoff regression tests fail after state advances to Goal 028, update only the minimal allowed assertion and rerun focused tests plus `check-all.ps1`.

## Pre-final self-review

Before final report, directly inspect:

- `.llmgc/procedural/package-assembly-combat-progression/package-assembly-combat-progression-report.json`;
- `.llmgc/procedural/package-assembly-combat-progression/package-assembly-combat-progression-assembly-report.json`;
- `.llmgc/procedural/package-assembly-combat-progression/package-assembly-combat-progression-package-summary.json`;
- `.llmgc/procedural/package-assembly-combat-progression/package-assembly-combat-progression-anti-overfit-fixtures.json`;
- `.llmgc/procedural/package-assembly-combat-progression/package-assembly-combat-progression-invalid-matrix.json`;
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
- confirmation that `package_assembly_combat_progression_expansion_verification` remains required, not passed;
- confirmation that Goal 029/S227 was not started;
- confirmation that no public `GamePackage` schema changed;
- confirmation that no product vertical gate was claimed;
- exact bounded git commands used, or confirmation none were used except through `check-artifact-scope.ps1`.
