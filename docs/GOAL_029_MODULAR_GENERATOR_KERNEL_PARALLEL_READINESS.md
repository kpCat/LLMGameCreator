# Goal 029: Modular Generator Kernel And Parallel Development Readiness

## Starting gate

This goal may start only after the user explicitly provides:

```text
package_assembly_combat_progression_expansion_verification passed
```

Goal 028 must already be reviewed from the pushed repository. Do not re-open Goal 028 unless a concrete pushed defect is found.

## Why this goal exists

Package assembly expansion Goals 025-028 proved world/entities, dialogue/quests, items/economy/crafting and combat/progression assembly through the existing `GeneratorPlanGamePackageAssembler` seam. They also exposed development bottlenecks:

- every package assembly module changes the shared assembler file;
- every product smoke scenario changes the shared `run-product-smoke.ps1`;
- broad `check-all.ps1` is too expensive for every module-only change;
- old state/handoff regression tests keep being updated after every active gate move;
- true parallel Codex implementation work is unsafe while active state and shared seams are not isolated.

Goal 029 must create the first real technical readiness layer for safe modular and eventually parallel development. This is not a broad rewrite and not a paper-only policy task.

## Goal type and proof level

Task type:

```text
kernel_refactor + integration_slice + process_tooling
```

Required proof level:

```text
Level 2/3
```

This is a bounded composite goal. Its internal phases are:

```text
Contract -> Module -> Integration -> Proof
```

Those phases are internal checkpoints of this one Goal 029. Do not split them into separate goals unless a stop condition is hit.

## Final gate

Stop at exactly one final gate:

```text
modular_generator_kernel_parallel_readiness_verification
```

Leave this gate `required`, not `passed`.

Do not start Goal 030 or S234. Do not create a product vertical gate.

## Product / generator outcome

Concrete improvement:

```text
package assembly and product-smoke development can move from shared-file growth
to contract-manifested modules and scenario manifests,
so future module-only changes can be validated with targeted module proof
and later adopted serially without making every Codex task touch the same files.
```

This must produce measurable technical profit, not only documentation.

## Read first

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/CURRENT_GENERATOR_STATE.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/MODULAR_CONTRACT_GOAL_POLICY.md`
8. `docs/PACKAGE_ASSEMBLY_EXPANSION_CAMPAIGN_PACK.md`
9. `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`
10. `.devflow/artifact-scope/artifact-scope-policy.json`
11. `.devflow/scripts/run-product-smoke.ps1`
12. `.devflow/scripts/check-all.ps1`
13. `.devflow/scripts/check-artifact-scope.ps1`
14. `docs/GOAL_028_PACKAGE_ASSEMBLY_EXPANSION_4_COMBAT_PROGRESSION.md`
15. `docs/PACKAGE_ASSEMBLY_COMBAT_PROGRESSION_CONTRACT_V1.md`
16. `.llmgc/procedural/package-assembly-combat-progression/package-assembly-combat-progression-report.json`
17. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
18. package assembly acceptance services/tests from Goals 025-028.

Do not broad-scan unrelated Unity/UI/provider/runtime files.

## Scope

Allowed:

- New contracts/docs:
  - `docs/MODULE_CONTRACT_MANIFEST_V1.md`
  - `docs/PRODUCT_SMOKE_SCENARIO_MANIFEST_V1.md`
  - `docs/PARALLEL_CANDIDATE_DEVELOPMENT_POLICY.md`
- New narrow Application kernel/readiness service/models:
  - `src/LLMGameCreator.Application/Design/ModularGeneratorKernel/**`
- Minimal bounded edit to existing assembler:
  - `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
- Product smoke scenario manifest directory:
  - `.devflow/product-smoke-scenarios/**`
- Bounded update to product smoke runner:
  - `.devflow/scripts/run-product-smoke.ps1`
- Optional bounded update to check-all or devflow scripts only if needed to expose a module-tier verification mode:
  - `.devflow/scripts/check-all.ps1`
  - `.devflow/scripts/check-artifact-scope.ps1`
- New focused tests:
  - `tests/LLMGameCreator.Tests/Application/ModularGeneratorKernel/**`
  - `tests/LLMGameCreator.Tests/Devflow/ProductSmokeScenarioManifestTests.cs`
- New product/integration smoke test:
  - `tests/LLMGameCreator.Tests/ProductSmoke/ModularGeneratorKernelReadinessSmokeTests.cs`
- Compact current artifact root:
  - `.llmgc/procedural/modular-generator-kernel-parallel-readiness/**`
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Bounded handoff-regression test updates only if `check-all.ps1` requires them after state advances to Goal 029.

Forbidden:

- Do not start Goal 030 or S234.
- Do not implement a new gameplay package expansion module.
- Do not create a product vertical gate.
- Do not change public `GamePackage` schema.
- Do not edit `src/LLMGameCreator.GamePackage/GamePackageDefinition.cs`.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms UI.
- Do not change Unity build/player entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not edit `generator-library/**`.
- Do not mutate accepted historical `.llmgc/procedural/**` artifact families outside `.llmgc/procedural/modular-generator-kernel-parallel-readiness/**`.
- Do not broadly migrate all package assembly code in one pass.
- Do not remove `check-all.ps1`; it becomes campaign/kernel-level proof, not dead code.

## Budget

This is a critical architecture/readiness goal, but must remain bounded.

Default limits:

- production implementation files: max 4;
- devflow script/config files: max 5;
- docs/state files: max 7 unless state handoff requires one more;
- focused test files: max 4;
- product smoke test files: max 1;
- artifact roots: exactly 1 current root;
- migrated package assembly modules/wrappers: 2 maximum;
- hotfix attempts: max 2.

Stop and return a split/diagnosis plan if:
- public schema changes are needed;
- the registry requires rewriting all existing package assembly mappers;
- `run-product-smoke.ps1` cannot remain backward compatible;
- module-only verification cannot be honestly proven;
- more than 15 files need material changes.

## Required artifact root

All compact artifacts for this goal must be written under:

```text
.llmgc/procedural/modular-generator-kernel-parallel-readiness/
```

Suggested files:

```text
module-contract-manifest-proof.json
product-smoke-scenario-manifest-proof.json
package-assembly-module-registry-report.json
module-compatibility-matrix.json
module-absence-behavior-report.json
parallel-candidate-policy-proof.json
modular-generator-kernel-invalid-matrix.json
modular-generator-kernel-readiness-report.json
modular-generator-kernel-readiness-report.md
modular-generator-kernel-readiness-verification.md
goal-029-final-artifact-scope-report.json
goal-029-final-artifact-scope-report.md
```

## Required internal phases

### S227: Record accepted Goal 028 and current position

Record that the user accepted:

```text
package_assembly_combat_progression_expansion_verification passed
```

Update state/queue docs so current work becomes Goal 029 and the current gate after this goal is:

```text
modular_generator_kernel_parallel_readiness_verification required
```

Queue handling:

- Goal 029 is `Modular Generator Kernel And Parallel Development Readiness`.
- Goal 030 remains future work.
- Do not start Goal 030 or S234.

### S228: Contract phase - module and smoke scenario manifests

Create `docs/MODULE_CONTRACT_MANIFEST_V1.md`.

It must define a deterministic module manifest with at least:

- `moduleId`;
- `moduleKind`;
- `version`;
- `ownedSourceRoots`;
- `ownedArtifactRoot`;
- `inputContracts`;
- `outputContracts`;
- `requiredKernelCapabilities`;
- `requiredDependencies`;
- `optionalDependencies`;
- `absenceBehavior`;
- `validators`;
- `focusedTestFilter`;
- `productSmokeScenario`;
- `forbiddenRuntimeDependencies`;
- `deterministicHashRules`.

Create `docs/PRODUCT_SMOKE_SCENARIO_MANIFEST_V1.md`.

It must define a deterministic product smoke scenario manifest with at least:

- `scenarioId`;
- `testFilter`;
- `artifactRoot`;
- `ownedModuleId`;
- `expectedReportPath`;
- `forbiddenPaths`;
- `timeoutPolicy`;
- `isProductVerticalGate`;
- `allowedForModuleOnlyVerification`.

Create `docs/PARALLEL_CANDIDATE_DEVELOPMENT_POLICY.md`.

It must define:

- one active state writer only;
- parallel Codex work must be candidate work unless explicitly adopted;
- candidate tasks must not change `CURRENT_GENERATOR_STATE.*`, `CONTEXT_INDEX.md`, `FULL_GENERATOR_GOAL_QUEUE.md`;
- candidate tasks must not claim accepted gates;
- candidates must own source roots and artifact roots;
- serial adoption task updates state once;
- conflict resolution order:
  1. rebase candidate onto accepted main;
  2. rerun module compatibility matrix;
  3. accept/reject candidate in serial adoption;
  4. never auto-merge contradictory module manifests.

### S229: Module phase - registry and optional module seam

Add a narrow Application-layer registry under:

```text
src/LLMGameCreator.Application/Design/ModularGeneratorKernel/
```

Minimum required types/behavior:

- deterministic module manifest model/reader/validator;
- deterministic product smoke scenario manifest model/reader/validator;
- package assembly module registry abstraction or service;
- compatibility matrix builder;
- absence behavior evaluator.

Do not attempt a full plugin runtime or dynamic assembly loading. This is static/manifested modularity within the repo.

Minimum package assembly readiness proof:

- wrap or register 2 existing package assembly module shapes:
  - world/entities;
  - dialogue/quests;
- prove a registered module can be discovered and its manifest validated;
- prove an absent optional module does not crash compatibility checking and is reported as `absent_optional` or equivalent;
- prove a missing required module is rejected.

It is acceptable for the existing `GeneratorPlanGamePackageAssembler` to remain the actual package assembler, but it must gain a stable registry/manifest seam so future package module work can avoid editing its core switch for every new module.

### S230: Integration phase - product smoke scenario manifests

Create `.devflow/product-smoke-scenarios/`.

Add manifests for at least:

```text
modular-generator-kernel-readiness
package-assembly-world-entities
package-assembly-dialogue-quests
```

Optional if low-risk:

```text
package-assembly-items-economy-crafting
package-assembly-combat-progression
```

Update `.devflow/scripts/run-product-smoke.ps1` so it:

- first checks `.devflow/product-smoke-scenarios/<scenario>.json`;
- runs the manifest's test filter when found;
- validates expected report/artifact root when possible;
- falls back to existing hardcoded routing for scenarios not yet migrated;
- stays backward compatible with existing scenario commands;
- does not require a new hardcoded case for `modular-generator-kernel-readiness`.

This is the key measurable profit: future scenarios can be added with a manifest file instead of editing the shared script.

### S231: Verification tiers and module-only proof

Define module-only verification rules in docs and compact artifacts.

Required tiers:

```text
Tier 1: module proof
  focused module tests
  product smoke scenario manifest test
  module compatibility matrix
  artifact scope guard

Tier 2: kernel proof
  all module/smoke manifests parse
  registry tests
  compatibility matrix
  selected smoke set
  ordinary tests if kernel changed

Tier 3: campaign proof
  check-all
  selected cross-module smokes
  used after several modules or before adoption

Tier 4: product vertical proof
  rare playable/simulatable/runtime-facing gate
```

Do not remove `check-all.ps1`. The goal is to document and implement enough support that future module-only goals can avoid `check-all` unless they modify kernel/shared files.

### S232: Invalid/fake/leak matrix

Minimum invalid/fake/leak scenarios:

1. missing accepted Goal 028 gate is rejected;
2. missing Goal 028 compact report is rejected;
3. malformed module manifest is rejected;
4. duplicate module id is rejected;
5. unknown input contract id is rejected;
6. unknown output contract id is rejected;
7. required dependency missing is rejected;
8. optional dependency missing is accepted with `absent_optional` diagnostic;
9. forbidden runtime dependency is rejected;
10. module declares artifact root outside owned root is rejected;
11. product smoke scenario manifest references forbidden path is rejected;
12. product smoke scenario manifest missing test filter is rejected;
13. module-only verification claims product vertical gate is rejected;
14. candidate task attempts active state docs mutation is rejected by policy proof;
15. hardcoded smoke route required for new manifest scenario is rejected;
16. historical Goal 020-028 artifact mutation is rejected by scope guard or equivalent verification.

Invalid cases should flow through real manifest/registry/policy/scope validation where possible. Do not fake diagnostics when a real helper can produce them.

### S233: Product/integration smoke, focused tests, state handoff and final scope guard

Add product smoke route through manifest:

```text
modular-generator-kernel-readiness
```

The smoke must:

- build/write compact artifacts under `.llmgc/procedural/modular-generator-kernel-parallel-readiness/`;
- validate module manifest contract proof;
- validate product smoke scenario manifest proof;
- validate compatibility matrix;
- prove module present and module absent behavior;
- prove a manifest-defined scenario can run without adding a hardcoded `run-product-smoke.ps1` case for that scenario;
- verify no product vertical gate is claimed;
- verify no public schema/project/Unity/provider/LLM/RAG/media/Lua/generator-library change claims;
- verify Goal 030/S234 not started.

Focused tests must cover at minimum:

- module manifest parser/validator;
- product smoke scenario manifest parser/validator;
- duplicate module id rejection;
- optional module absence behavior;
- required module absence rejection;
- scenario manifest run path;
- `run-product-smoke.ps1` backward compatibility for one existing hardcoded route;
- compatibility matrix deterministic output;
- state docs record Goal 028 accepted before Goal 029;
- final report has no top-level `severity=error` diagnostics when proof passes.

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Final report must include:

```text
accepted=false
finalStatus=modular_generator_kernel_parallel_readiness_verification
manualGate=modular_generator_kernel_parallel_readiness_verification
previousAcceptedGate=package_assembly_combat_progression_expansion_verification passed
goal028EvidenceVerified=true
moduleManifestContractWritten=true
smokeScenarioManifestContractWritten=true
parallelCandidatePolicyWritten=true
moduleRegistryWritten=true
moduleCompatibilityMatrixWritten=true
optionalModuleAbsenceHandled=true
requiredModuleMissingRejected=true
manifestSmokeScenarioExecuted=true
runProductSmokeHardcodedRouteNotRequiredForNewManifestScenario=true
moduleOnlyVerificationTierDefined=true
productVerticalGate=false
publicGamePackageSchemaChanged=false
projectFilesChanged=false
generatorLibraryChanged=false
unityBuildExecuted=false
llmRagProviderMediaLuaExecuted=false
scopeGuardPassed=true
invalidMatrix.passed=true
```

Top-level diagnostics must contain no `severity=error` when `contractProofPassed=true`.

## Required verification

Run, at minimum:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~ModularGeneratorKernel|FullyQualifiedName~ProductSmokeScenarioManifest|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario modular-generator-kernel-readiness
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-029-final -AllowedPath docs/MODULE_CONTRACT_MANIFEST_V1.md -AllowedPath docs/PRODUCT_SMOKE_SCENARIO_MANIFEST_V1.md -AllowedPath docs/PARALLEL_CANDIDATE_DEVELOPMENT_POLICY.md -AllowedPathPrefix src/LLMGameCreator.Application/Design/ModularGeneratorKernel/ -AllowedPath src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs -AllowedPathPrefix .devflow/product-smoke-scenarios/ -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPath .devflow/scripts/check-all.ps1 -AllowedPath .devflow/scripts/check-artifact-scope.ps1 -AllowedPathPrefix tests/LLMGameCreator.Tests/Application/ModularGeneratorKernel/ -AllowedPath tests/LLMGameCreator.Tests/Devflow/ProductSmokeScenarioManifestTests.cs -AllowedPath tests/LLMGameCreator.Tests/ProductSmoke/ModularGeneratorKernelReadinessSmokeTests.cs -AllowedPathPrefix .llmgc/procedural/modular-generator-kernel-parallel-readiness/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/GOAL_029_MODULAR_GENERATOR_KERNEL_PARALLEL_READINESS.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-029-modular-generator-kernel-parallel-readiness-CODEX_GOAL.md -AllowedPath tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/CapabilityBundlePipelineInputsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/RichPackageAssemblyCoverageAudit/RichPackageAssemblyCoverageAuditAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyWorldEntities/PackageAssemblyWorldEntitiesAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyDialogueQuests/PackageAssemblyDialogueQuestsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyItemsEconomyCrafting/PackageAssemblyItemsEconomyCraftingAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/PackageAssemblyCombatProgression/PackageAssemblyCombatProgressionAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs
```

If `check-all.ps1` is still slow, it must still pass for Goal 029 because this goal changes kernel/devflow behavior. Future module-only goals may use the new Tier 1 rule only after this gate is accepted.

If stale handoff regression tests fail after state advances to Goal 029, update only the minimal allowed assertion and rerun focused tests plus `check-all.ps1`.

## Pre-final self-review

Before final response, directly inspect:

- `.llmgc/procedural/modular-generator-kernel-parallel-readiness/modular-generator-kernel-readiness-report.json`;
- `.llmgc/procedural/modular-generator-kernel-parallel-readiness/module-compatibility-matrix.json`;
- `.llmgc/procedural/modular-generator-kernel-parallel-readiness/product-smoke-scenario-manifest-proof.json`;
- `.llmgc/procedural/modular-generator-kernel-parallel-readiness/module-absence-behavior-report.json`;
- `.llmgc/procedural/modular-generator-kernel-parallel-readiness/modular-generator-kernel-invalid-matrix.json`;
- `docs/CURRENT_GENERATOR_STATE.json`;
- `docs/CONTEXT_INDEX.md`;
- final artifact scope report.

Acceptance evidence table is required in final Codex response:

```markdown
| Acceptance criterion | Evidence path/test | Status |
|---|---|---|
```

## Final response requirements

The final Codex response must include:

- changed files;
- new contract/kernel/script/test/product-smoke paths;
- compact artifact paths;
- module registry summary;
- smoke scenario manifest summary;
- module compatibility matrix summary;
- module absence behavior summary;
- verification tier summary;
- generated artifact/report hashes;
- invalid matrix count;
- focused/product-smoke/check-all/scope-guard verification results;
- acceptance evidence table;
- whether final valid report has zero top-level error diagnostics;
- confirmation that `modular_generator_kernel_parallel_readiness_verification` remains required, not passed;
- confirmation that Goal 030/S234 was not started;
- confirmation that no public `GamePackage` schema changed;
- confirmation that no product vertical gate was claimed;
- exact bounded git commands used, or confirmation none were used except through `check-artifact-scope.ps1`.
