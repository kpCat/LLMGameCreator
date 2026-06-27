# Goal 025: Package Assembly Expansion 1 - World And Entities

## Starting gate

This goal may start only after the user explicitly provides:

```text
modular_contract_goal_policy_adoption_verification passed
```

The modular contract policy adoption task must already be reviewed from the pushed repository. Do not re-open Goal 024 or the process-policy task unless a concrete pushed defect is found.

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

Those phases are internal checkpoints of this one Goal 025. Do not split them into separate default goals unless a stop condition is hit.

## Final gate

Stop at exactly one final gate:

```text
package_assembly_world_entities_expansion_verification
```

Leave this gate `required`, not `passed`.

Do not start Goal 026 or S206. Do not create a product vertical gate in this goal.

## Product / generator outcome

Goal 024 audited package assembly coverage and recommended `package_assembly_expansion_1_world_and_entities` as the first expansion candidate.

Goal 025 must now implement the first bounded package assembly expansion for world and entities, using existing `GamePackage` schema and existing package assembly seams.

Concrete improvement:

```text
accepted Goal 023/024 planning artifacts
  -> world/entity mapping contract
  -> bounded package assembly module expansion for world + entities
  -> integration with existing GeneratorPlanGamePackageAssembler / validators
  -> real consumer + synthetic future-consumer anti-overfit fixture
  -> deterministic proof artifacts
```

This is not a full game package vertical. It must not claim a new playable vertical result.

## Read first

Read these before editing:

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/MODULAR_CONTRACT_GOAL_POLICY.md`
8. `docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md`
9. `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md`
10. `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-audit-report.json`
11. `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-matrix.json`
12. `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-next-slice-plan.json`
13. `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md`
14. `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`
15. `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`
16. `src/LLMGameCreator.Domain/Definitions/GameDefinitions.cs`
17. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
18. nearest tests for `GeneratorPlanGamePackageAssembler`, generated package MVP, package validation and runtime smoke.

Do not broad-scan the repository when the local package assembly seam is enough.

## Scope

Allowed:

- New mapping contract:
  - `docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md`
- New narrow Application-layer proof/acceptance service:
  - `src/LLMGameCreator.Application/Design/PackageAssemblyWorldEntities/**`
- Minimal bounded edit to existing package assembler:
  - `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
- New focused tests:
  - `tests/LLMGameCreator.Tests/Application/PackageAssemblyWorldEntities/**`
- New product/integration smoke test:
  - `tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyWorldEntitiesSmokeTests.cs`
- One product smoke route addition:
  - `.devflow/scripts/run-product-smoke.ps1`
- Compact current artifact root:
  - `.llmgc/procedural/package-assembly-world-entities/**`
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Bounded handoff-regression test updates only if `check-all.ps1` requires them after state advances to Goal 025.

Forbidden:

- Do not change public `GamePackage` schema.
- Do not edit `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`.
- Do not edit `src/LLMGameCreator.Domain/Definitions/GameDefinitions.cs`.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms UI.
- Do not change Unity build/player entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit `generator-library/**`.
- Do not mutate accepted historical `.llmgc/procedural/**` artifact families.
- Do not implement dialogue/quest/item/economy/combat package expansion in this goal except where existing world/entity fixture data needs minimal references.
- Do not start Goal 026 or S206.
- Do not claim product vertical proof or manual playable review.

## Budget

Default limits:

- production implementation files: max 2;
- new Application service/model files under the Goal 025 folder: max 3;
- focused test files: max 2;
- product smoke test files: max 1;
- docs/state/routing files: max 6 unless state handoff requires more;
- artifact family roots: exactly 1 current root;
- hotfix attempts: max 2.

Stop and return a split/diagnosis plan if more than 10 files must change, if package schema changes are needed, or if honest proof cannot be produced without broad runtime/Unity work.

## Required artifact root

All compact artifacts for this goal must be written under:

```text
.llmgc/procedural/package-assembly-world-entities/
```

Suggested files:

```text
package-assembly-world-entities-mapping-contract-proof.json
package-assembly-world-entities-input-fixtures.json
package-assembly-world-entities-assembly-report.json
package-assembly-world-entities-package-summary.json
package-assembly-world-entities-anti-overfit-fixtures.json
package-assembly-world-entities-invalid-matrix.json
package-assembly-world-entities-report.json
package-assembly-world-entities-report.md
package-assembly-world-entities-verification.md
goal-025-final-artifact-scope-report.json
goal-025-final-artifact-scope-report.md
```

## Required internal phases

### S199: Record accepted policy gate and current position

Record that the user accepted:

```text
modular_contract_goal_policy_adoption_verification passed
```

Update state/queue docs so current work becomes Goal 025 and the current gate after this goal is:

```text
package_assembly_world_entities_expansion_verification required
```

Queue handling:

- Goal 025 is `Package Assembly Expansion 1 - World And Entities`.
- Goal 026 remains future work, normally `Package Assembly Expansion 2 - Dialogue And Quests`.
- Do not start Goal 026 or S206.

### S200: Contract phase - world/entity package mapping contract

Create `docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md`.

It must define:

- accepted inputs:
  - Goal 023 generator pipeline inputs;
  - Goal 024 coverage matrix / gap report / next-slice plan;
  - approved `scene_pack_v1`, `region_pack_v1`, `entity_pack_v1`, `npc_pack_v1`-style fixture artifacts;
- existing package targets:
  - `GamePackageDefinition.Game.Maps`;
  - `MapDefinition.Entities`;
  - `GamePackageDefinition.Game.EntityPrototypes`;
  - `GamePackageDefinition.GeneratedContent.Scenes`;
  - `GamePackageDefinition.GeneratedContent.Regions`;
  - `GamePackageDefinition.GeneratedContent.Npcs`;
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
  - one real consumer fixture derived from current accepted profile/planning artifacts;
  - one second/synthetic consumer fixture, preferably `npc_city_walk`, proving the output contract is not hardcoded to one scenario;
- non-goals:
  - no public schema changes;
  - no Unity runtime proof;
  - no dialogue/item/combat expansion;
  - no live runtime LLM/RAG/provider/media/Lua.

### S201: Module phase - bounded world/entity assembly expansion

Implement a bounded package assembly expansion through the existing `GeneratorPlanGamePackageAssembler` seam where possible.

Minimum allowed mapping improvement:

- `entity_pack_v1` may create/update package entity prototypes through existing `GameDefinition.EntityPrototypes`.
- `entity_pack_v1` may create map placements through existing `MapDefinition.Entities` when an entity record contains deterministic map/scene/position fields.
- `npc_pack_v1` may keep generated NPC sidecar evidence through existing `GeneratedContent.Npcs`; it may also create package entity prototypes or map placements only when the fixture has explicit package-safe map/entity fields.
- `region_pack_v1` remains generated content / sidecar unless existing schema safely supports the requested package field.
- unsupported/future/blocked topology gaps from Goal 023/024 must remain preserved as gaps or sidecars, not converted into package support.

Do not alter `GamePackageDefinition` or domain schema.

If existing assembler already supports some of this, add only the missing bounded behavior and proof artifacts. Do not rewrite the assembler.

### S202: Integration phase - real consumer and synthetic anti-overfit fixture

Create deterministic Goal 025 fixtures inside the acceptance service/test path, not as production runtime data.

Required consumers:

1. Real consumer:
   - derived from accepted frontier/trade/gothic world/entity profile planning where practical;
   - uses the existing package assembly seam;
   - proves at least one map, one entity prototype and one map placement can be assembled.

2. Synthetic future-consumer fixture:
   - use id/name such as `npc_city_walk`;
   - include an independent city/settlement/region entity shape;
   - prove the assembly output is not hardcoded to frontier caravan/survival names;
   - does not need to implement city simulation, schedules or pathfinding runtime.

Both consumers must produce deterministic package summaries and mapping diagnostics.

### S203: Proof phase - invalid/fake/leak matrix

Minimum invalid/fake/leak scenarios:

1. missing accepted modular policy gate is rejected;
2. missing Goal 024 coverage audit evidence is rejected;
3. missing Goal 023 generator input evidence is rejected;
4. public `GamePackage` schema mutation claim is rejected;
5. entity placement references unknown map id is rejected;
6. entity placement references unknown prototype id is rejected;
7. duplicate entity prototype id is rejected or deterministically de-duplicated with diagnostic;
8. out-of-bounds map placement is rejected;
9. blocked topology gap treated as package-supported is rejected;
10. future-required region graph/chunk gap treated as implemented is rejected;
11. synthetic anti-overfit fixture missing is rejected;
12. output hardcoded only to frontier/caravan consumer is rejected;
13. package assembly claims Unity/LLM/RAG/provider/media/Lua execution is rejected;
14. Goal 026/S206 started marker is rejected;
15. historical Goal 020-024 artifact mutation is rejected by scope guard or equivalent verification.

Invalid cases should flow through real helper validation where possible. Do not fake diagnostics when a real mapping/validation helper can produce them.

### S204: Product/integration smoke and focused tests

Add product smoke route:

```text
package-assembly-world-entities
```

This smoke is an automated integration smoke, not a product vertical gate.

The smoke must:

- build/write compact artifacts under `.llmgc/procedural/package-assembly-world-entities/`;
- validate report shape;
- verify `accepted=false`;
- verify final/manual gate is `package_assembly_world_entities_expansion_verification`;
- verify previous accepted gate is `modular_contract_goal_policy_adoption_verification passed`;
- verify no public schema changes;
- verify package assembly is bounded to world/entities;
- verify real consumer and synthetic fixture both produce deterministic outputs;
- verify future/blocked gaps are preserved;
- verify no Unity/LLM/RAG/provider/media/Lua execution claims;
- verify Goal 026/S206 not started.

Focused tests must cover at minimum:

- deterministic outputs;
- real consumer produces map/entity prototype/map placement;
- synthetic `npc_city_walk` consumer produces independent output;
- anti-overfit proof rejects single-consumer hardcoding;
- invalid matrix rejects required scenarios;
- no top-level `severity=error` diagnostics when proof passes;
- state docs record modular policy gate accepted before Goal 025.

### S205: State handoff, artifacts and final scope guard

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Current next work after Goal 025 must be:

```text
package_assembly_world_entities_expansion_verification
```

Do not mark it passed.

Final report must include:

```text
accepted=false
finalStatus=package_assembly_world_entities_expansion_verification
manualGate=package_assembly_world_entities_expansion_verification
previousAcceptedGate=modular_contract_goal_policy_adoption_verification passed
goal024EvidenceVerified=true
goal023EvidenceVerified=true
realConsumerPassed=true
syntheticConsumerPassed=true
antiOverfitProofPassed=true
worldEntityMappingWritten=true
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

`packageAssemblyExecuted=true` here means bounded in-memory/package-data assembly through the existing Application package assembly seam. It must not mean full product vertical, Unity build, runtime playtest or public schema mutation.

Top-level diagnostics must contain no `severity=error` when `contractProofPassed=true`.

## Required verification

Run, at minimum:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~PackageAssemblyWorldEntities|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario package-assembly-world-entities
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-025-final -AllowedPath docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md -AllowedPathPrefix src/LLMGameCreator.Application/Design/PackageAssemblyWorldEntities/ -AllowedPath src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs -AllowedPathPrefix tests/LLMGameCreator.Tests/Application/PackageAssemblyWorldEntities/ -AllowedPath tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyWorldEntitiesSmokeTests.cs -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPathPrefix .llmgc/procedural/package-assembly-world-entities/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-025-package-assembly-expansion-1-world-and-entities-CODEX_GOAL.md -AllowedPath tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/CapabilityBundlePipelineInputsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/RichPackageAssemblyCoverageAudit/RichPackageAssemblyCoverageAuditAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs
```

If stale handoff regression tests fail after state advances to Goal 025, update only the minimal allowed assertion and rerun focused tests plus `check-all.ps1`.

## Pre-final self-review

Before final report, directly inspect:

- `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-report.json`;
- `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-assembly-report.json`;
- `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-package-summary.json`;
- `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-anti-overfit-fixtures.json`;
- `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-invalid-matrix.json`;
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
- confirmation that `package_assembly_world_entities_expansion_verification` remains required, not passed;
- confirmation that Goal 026/S206 was not started;
- confirmation that no public `GamePackage` schema changed;
- confirmation that no product vertical gate was claimed;
- exact bounded git commands used, or confirmation none were used except through `check-artifact-scope.ps1`.
