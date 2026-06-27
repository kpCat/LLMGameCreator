# Product Slice 205A: Package Assembly World/Entities Check-All Proof Completion

Bounded verification/hotfix for Goal 025 only. This is not a new `/goal`.

## Starting context

The user has accepted:

```text
modular_contract_goal_policy_adoption_verification passed
```

Goal 025 currently stops at:

```text
package_assembly_world_entities_expansion_verification required
```

Do not mark the gate passed.

Do not start Goal 026 or S206.

## Problem found in pushed repository review

Goal 025 report and compact artifacts are product-correct-looking, but final verification is incomplete because `check-all.ps1` did not pass. State docs explicitly record:

```text
check-all.ps1: build phase reached 0 warnings / 0 errors, but ordinary test phase did not complete before two tool timeouts; no pass is claimed
```

The gate cannot be accepted until this is resolved or reported as a real blocker with exact logs.

## Read first

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/CURRENT_GENERATOR_STATE.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/MODULAR_CONTRACT_GOAL_POLICY.md`
8. `docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md`
9. `docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md`
10. `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-report.json`
11. `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-package-summary.json`
12. `.llmgc/procedural/package-assembly-world-entities/package-assembly-world-entities-invalid-matrix.json`
13. `.llmgc/procedural/package-assembly-world-entities/goal-025-final-artifact-scope-report.json`

## Scope

Allowed:

- Complete verification evidence for Goal 025.
- Rerun:
  - focused Goal 025 tests;
  - `package-assembly-world-entities` product smoke;
  - `check-all.ps1`;
  - final artifact scope guard.
- If `check-all.ps1` passes, update only state/context docs if they currently record the timeout:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- If an actual Goal 025 defect is discovered, fix only within the original Goal 025 allowed scope:
  - `src/LLMGameCreator.Application/Design/PackageAssemblyWorldEntities/**`
  - `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs`
  - `tests/LLMGameCreator.Tests/Application/PackageAssemblyWorldEntities/**`
  - `tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyWorldEntitiesSmokeTests.cs`
  - `.llmgc/procedural/package-assembly-world-entities/**`
  - `.devflow/scripts/run-product-smoke.ps1` only if the product smoke route itself is defective.

Forbidden:

- Do not start Goal 026 or S206.
- Do not mark `package_assembly_world_entities_expansion_verification` passed.
- Do not change public `GamePackage` schema.
- Do not change `.sln` or `.csproj`.
- Do not edit `generator-library/**`.
- Do not add WinForms UI.
- Do not modify Unity/player/build entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not mutate historical `.llmgc/procedural/**` artifact families outside `.llmgc/procedural/package-assembly-world-entities/**`.
- Do not perform broad cleanup.

## Required verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~PackageAssemblyWorldEntities|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario package-assembly-world-entities
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-025-final -AllowedPath docs/PACKAGE_ASSEMBLY_WORLD_ENTITIES_CONTRACT_V1.md -AllowedPathPrefix src/LLMGameCreator.Application/Design/PackageAssemblyWorldEntities/ -AllowedPath src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs -AllowedPathPrefix tests/LLMGameCreator.Tests/Application/PackageAssemblyWorldEntities/ -AllowedPath tests/LLMGameCreator.Tests/ProductSmoke/PackageAssemblyWorldEntitiesSmokeTests.cs -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPathPrefix .llmgc/procedural/package-assembly-world-entities/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/GOAL_025_PACKAGE_ASSEMBLY_EXPANSION_1_WORLD_AND_ENTITIES.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-025-package-assembly-expansion-1-world-and-entities-CODEX_GOAL.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/product-slice-205a-package-assembly-world-entities-check-all-proof-CODEX_TASK.md -AllowedPath tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/CapabilityBundlePipelineInputsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/RichPackageAssemblyCoverageAudit/RichPackageAssemblyCoverageAuditAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs
```

If `check-all.ps1` times out again, do not claim completion. Report:

- exact command;
- timeout duration/tool limit;
- last visible log lines;
- whether build reached 0 warnings / 0 errors;
- whether any tests failed before timeout;
- whether the timeout looks environmental or caused by a hanging test;
- recommended next bounded action.

If `check-all.ps1` fails with a real test/build defect, fix it only within the allowed scope and rerun required verification.

## Final response requirements

Report:

- changed files, if any;
- exact check-all result;
- focused/product-smoke/check-all/scope-guard results;
- whether final Goal 025 report still has zero top-level error diagnostics;
- confirmation that `package_assembly_world_entities_expansion_verification` remains required, not passed;
- confirmation that Goal 026/S206 were not started;
- confirmation that public GamePackage schema, generator-library, Unity, UI, provider/LLM/RAG/media/Lua were not changed/executed;
- exact bounded git usage, or confirmation none was used except through `check-artifact-scope.ps1`.
