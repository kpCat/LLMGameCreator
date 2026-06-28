# Goal 026: Package Assembly Expansion 2 - Dialogue And Quests

## Starting gate

This goal may start only after the user explicitly provides:

```text
package_assembly_world_entities_expansion_verification passed
```

Goal 025 must already be reviewed from the pushed repository. Do not re-open Goal 025 unless a concrete pushed defect is found.

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

Those phases are internal checkpoints of this one Goal 026. Do not split them into separate default goals unless a stop condition is hit.

## Final gate

Stop at exactly one final gate:

```text
package_assembly_dialogue_quests_expansion_verification
```

Leave this gate `required`, not `passed`.

Do not start Goal 027 or S213. Do not create a product vertical gate in this goal.

## Product / generator outcome

Goal 025 expanded package assembly for world/entities through the existing `GeneratorPlanGamePackageAssembler` seam. Goal 026 must now expand package assembly for dialogue and quests using the existing public `GamePackage` schema.

Concrete improvement:

```text
accepted Goal 023/024/025 evidence
  -> dialogue/quest mapping contract
  -> bounded package assembly module expansion for dialogue + quests
  -> integration with existing GeneratorPlanGamePackageAssembler / NarrativeDefinitionValidator
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
9. `docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md`
10. `docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md`
11. `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-report.json`
12. `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-package-summary.json`
13. `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`
14. `src/LLMGameCreator.Domain/Definitions/ContentDefinitions.cs`
15. `src/LLMGameCreator.Domain/Definitions/DialogueDefinitions.cs`
16. `src/LLMGameCreator.Application/Validation/NarrativeDefinitionValidator.cs`
17. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
18. nearest tests for `GeneratorPlanGamePackageAssembler`, narrative validation, quest/dialogue runtime and previous package assembly goals.

Do not broad-scan the repository when the local package assembly and narrative validator seams are enough.

## Scope

Allowed:

- New mapping contract:
  - `docs/PACKAGE_ASSEMBLY_DIALOGUE_QUESTS_CONTRACT_V1.md`
- New narrow Application-layer proof/acceptance service:
  - `src/LLMGameCreator.Application/Design/PackageAssemblyDialogueQuests/**`
- Minimal bounded edit to existing package assembler:
  - `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
- New focused tests:
  - `tests/LLMGameCreator.Tests/Application/PackageAssemblyDialogueQuests/**`
- New product/integration smoke test:
  - `tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyDialogueQuestsSmokeTests.cs`
- One product smoke route addition:
  - `.devflow/scripts/run-product-smoke.ps1`
- Compact current artifact root:
  - `.llmgc/procedural/package-assembly-dialogue-quests/**`
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Bounded handoff-regression test updates only if `check-all.ps1` requires them after state advances to Goal 026.

Forbidden:

- Do not change public `GamePackage` schema.
- Do not edit `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`.
- Do not edit `src/LLMGameCreator.Domain/Definitions/ContentDefinitions.cs`.
- Do not edit `src/LLMGameCreator.Domain/Definitions/DialogueDefinitions.cs`.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms UI.
- Do not change Unity build/player entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit `generator-library/**`.
- Do not mutate accepted historical `.llmgc/procedural/**` artifact families outside `.llmgc/procedural/package-assembly-dialogue-quests/**`.
- Do not implement item/economy/crafting/combat package expansion in this goal except for safe existing quest/dialogue references that validators require.
- Do not start Goal 027 or S213.
- Do not claim product vertical proof or manual playable review.

## Budget

Default limits:

- production implementation files: max 2;
- new Application service/model files under Goal 026 folder: max 3;
- focused test files: max 2;
- product smoke test files: max 1;
- docs/state/routing files: max 6 unless state handoff requires more;
- artifact family roots: exactly 1 current root;
- hotfix attempts: max 2.

Stop and return a split/diagnosis plan if more than 10 files must change, if package schema changes are needed, or if honest proof cannot be produced without broad runtime/Unity work.

## Required artifact root

All compact artifacts for this goal must be written under:

```text
.llmgc/procedural/package-assembly-dialogue-quests/
```

Suggested files:

```text
package-assembly-dialogue-quests-mapping-contract-proof.json
package-assembly-dialogue-quests-input-fixtures.json
package-assembly-dialogue-quests-assembly-report.json
package-assembly-dialogue-quests-package-summary.json
package-assembly-dialogue-quests-anti-overfit-fixtures.json
package-assembly-dialogue-quests-invalid-matrix.json
package-assembly-dialogue-quests-report.json
package-assembly-dialogue-quests-report.md
package-assembly-dialogue-quests-verification.md
goal-026-final-artifact-scope-report.json
goal-026-final-artifact-scope-report.md
```

## Required internal phases

### S206: Record accepted Goal 025 and current position

Record that the user accepted:

```text
package_assembly_world_entities_expansion_verification passed
```

Update state/queue docs so current work becomes Goal 026 and the current gate after this goal is:

```text
package_assembly_dialogue_quests_expansion_verification required
```

Queue handling:

- Goal 026 is `Package Assembly Expansion 2 - Dialogue And Quests`.
- Goal 027 remains future work, normally `Package Assembly Expansion 3 - Items, Economy And Crafting`.
- Do not start Goal 027 or S213.

### S207: Contract phase - dialogue/quest package mapping contract

Create `docs/PACKAGE_ASSEMBLY_DIALOGUE_QUESTS_CONTRACT_V1.md`.

It must define:

- accepted inputs:
  - Goal 023 generator pipeline inputs;
  - Goal 024 coverage matrix / gap report / next-slice plan;
  - Goal 025 world/entity package assembly artifacts;
  - approved `dialogue_pack_v1` and `quest_pack_v1`-style fixture artifacts;
- existing package targets:
  - `GamePackageDefinition.Game.Dialogues`;
  - `DialogueDefinition.Nodes`;
  - `DialogueNodeDefinition.Choices`;
  - `DialogueChoiceDefinition.StartQuestId`;
  - `DialogueChoiceDefinition.AdvanceQuestId`;
  - `GamePackageDefinition.Game.Quests`;
  - `QuestDefinition.Objectives`;
  - `QuestDefinition.Stages`;
  - `GeneratedContent.Dialogues`;
  - `GeneratedContent.Quests`;
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
  - one real consumer fixture, preferably Gothic mystery dialogue/quest clue flow or another accepted planning profile;
  - one second/synthetic consumer fixture, preferably `rumor_board_tutorial`, proving output is not hardcoded to one scenario;
- non-goals:
  - no public schema changes;
  - no Unity runtime proof;
  - no item/economy/combat expansion;
  - no live runtime LLM/RAG/provider/media/Lua.

### S208: Module phase - bounded dialogue/quest assembly expansion

Implement a bounded package assembly expansion through the existing `GeneratorPlanGamePackageAssembler` seam where possible.

Minimum mapping improvement:

- `quest_pack_v1` must create/update package quests through existing `GameDefinition.Quests`;
- `quest_pack_v1` must support deterministic quest stages using existing `QuestDefinition.Stages`;
- `quest_pack_v1` must support deterministic objectives using existing `QuestObjectiveDefinition`;
- `dialogue_pack_v1` must create/update package dialogues through existing `GameDefinition.Dialogues`;
- `dialogue_pack_v1` must support deterministic dialogue nodes and choices using existing `DialogueDefinition.Nodes`;
- dialogue choices may reference existing quest ids via `StartQuestId`, `AdvanceQuestId`, or `SetQuestStageId` when the fixture explicitly declares those links and validators can prove the target quest exists;
- generated content sidecars under `GeneratedContent.Dialogues` and `GeneratedContent.Quests` must remain present where existing behavior needs them;
- unsupported/future/blocked Goal 023/024 gaps must remain preserved as gaps or sidecars, not converted into package support.

Do not alter `GamePackageDefinition`, domain definitions or validators unless the task is stopped and a split/schema plan is returned.

### S209: Integration phase - real consumer and synthetic anti-overfit fixture

Create deterministic Goal 026 fixtures inside the acceptance service/test path, not as production runtime data.

Required consumers:

1. Real consumer:
   - derived from accepted gothic mystery / trade / frontier planning where practical;
   - uses existing package assembly seam;
   - proves at least one package quest, one quest stage, one quest objective, one package dialogue, one dialogue node and one dialogue choice can be assembled;
   - proves a dialogue choice can link to a package quest when the fixture declares the link.

2. Synthetic future-consumer fixture:
   - use id/name such as `rumor_board_tutorial`;
   - include an independent tutorial/rumor-board quest-dialogue shape;
   - prove the assembly output is not hardcoded to gothic/trade/frontier names;
   - does not need to implement future rumor system, UI or runtime scheduler.

Both consumers must produce deterministic package summaries and mapping diagnostics.

### S210: Proof phase - invalid/fake/leak matrix

Minimum invalid/fake/leak scenarios:

1. missing accepted Goal 025 gate is rejected;
2. missing Goal 025 world/entity evidence is rejected;
3. missing Goal 023 generator input evidence is rejected;
4. public `GamePackage` schema mutation claim is rejected;
5. dialogue references unknown start node is rejected;
6. dialogue choice references unknown target node is rejected;
7. dialogue choice references unknown quest id is rejected;
8. quest stage references unknown next stage is rejected;
9. duplicate quest id is rejected or deterministically de-duplicated with diagnostic;
10. duplicate dialogue id is rejected or deterministically de-duplicated with diagnostic;
11. future-required dialogue graph / condition gap treated as implemented is rejected;
12. synthetic anti-overfit fixture missing is rejected;
13. output hardcoded only to gothic/trade/frontier consumer is rejected;
14. package assembly claims Unity/LLM/RAG/provider/media/Lua execution is rejected;
15. Goal 027/S213 started marker is rejected;
16. historical Goal 020-025 artifact mutation is rejected by scope guard or equivalent verification.

Invalid cases should flow through real package/narrative validation where possible. Do not fake diagnostics when a real mapping/validation helper can produce them.

### S211: Product/integration smoke and focused tests

Add product smoke route:

```text
package-assembly-dialogue-quests
```

This smoke is an automated integration smoke, not a product vertical gate.

The smoke must:

- build/write compact artifacts under `.llmgc/procedural/package-assembly-dialogue-quests/`;
- validate report shape;
- verify `accepted=false`;
- verify final/manual gate is `package_assembly_dialogue_quests_expansion_verification`;
- verify previous accepted gate is `package_assembly_world_entities_expansion_verification passed`;
- verify no public schema changes;
- verify package assembly is bounded to dialogue/quests;
- verify real consumer and synthetic fixture both produce deterministic outputs;
- verify future/blocked gaps are preserved;
- verify no Unity/LLM/RAG/provider/media/Lua execution claims;
- verify Goal 027/S213 not started.

Focused tests must cover at minimum:

- deterministic outputs;
- real consumer produces quest/stage/objective/dialogue/node/choice;
- dialogue choice can link to a known quest id;
- synthetic `rumor_board_tutorial` consumer produces independent output;
- anti-overfit proof rejects single-consumer hardcoding;
- invalid matrix rejects required scenarios;
- no top-level `severity=error` diagnostics when proof passes;
- state docs record Goal 025 accepted before Goal 026.

### S212: State handoff, artifacts and final scope guard

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Current next work after Goal 026 must be:

```text
package_assembly_dialogue_quests_expansion_verification
```

Do not mark it passed.

Final report must include:

```text
accepted=false
finalStatus=package_assembly_dialogue_quests_expansion_verification
manualGate=package_assembly_dialogue_quests_expansion_verification
previousAcceptedGate=package_assembly_world_entities_expansion_verification passed
goal025EvidenceVerified=true
goal024EvidenceVerified=true
goal023EvidenceVerified=true
realConsumerPassed=true
syntheticConsumerPassed=true
antiOverfitProofPassed=true
dialogueQuestMappingWritten=true
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
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~PackageAssemblyDialogueQuests|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario package-assembly-dialogue-quests
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-026-final -AllowedPath docs/PACKAGE_ASSEMBLY_DIALOGUE_QUESTS_CONTRACT_V1.md -AllowedPathPrefix src/LLMGameCreator.Application/Design/PackageAssemblyDialogueQuests/ -AllowedPath src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs -AllowedPathPrefix tests/LLMGameCreator.Tests/Application/PackageAssemblyDialogueQuests/ -AllowedPath tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyDialogueQuestsSmokeTests.cs -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPathPrefix .llmgc/procedural/package-assembly-dialogue-quests/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/GOAL_026_PACKAGE_ASSEMBLY_EXPANSION_2_DIALOGUE_AND_QUESTS.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-026-package-assembly-expansion-2-dialogue-and-quests-CODEX_GOAL.md -AllowedPath tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/CapabilityBundlePipelineInputsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/RichPackageAssemblyCoverageAudit/RichPackageAssemblyCoverageAuditAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyWorldEntities/PackageAssemblyWorldEntitiesAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs
```

If stale handoff regression tests fail after state advances to Goal 026, update only the minimal allowed assertion and rerun focused tests plus `check-all.ps1`.

## Pre-final self-review

Before final report, directly inspect:

- `.llmgc/procedural/package-assembly-dialogue-quests/package-assembly-dialogue-quests-report.json`;
- `.llmgc/procedural/package-assembly-dialogue-quests/package-assembly-dialogue-quests-assembly-report.json`;
- `.llmgc/procedural/package-assembly-dialogue-quests/package-assembly-dialogue-quests-package-summary.json`;
- `.llmgc/procedural/package-assembly-dialogue-quests/package-assembly-dialogue-quests-anti-overfit-fixtures.json`;
- `.llmgc/procedural/package-assembly-dialogue-quests/package-assembly-dialogue-quests-invalid-matrix.json`;
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
- confirmation that `package_assembly_dialogue_quests_expansion_verification` remains required, not passed;
- confirmation that Goal 027/S213 was not started;
- confirmation that no public `GamePackage` schema changed;
- confirmation that no product vertical gate was claimed;
- exact bounded git commands used, or confirmation none were used except through `check-artifact-scope.ps1`.
