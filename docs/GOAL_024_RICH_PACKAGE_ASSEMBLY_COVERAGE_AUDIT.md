# Goal 024: Rich Package Assembly Coverage Audit

## Starting gate

This goal may start only after the user explicitly provides:

```text
capability_bundle_pipeline_inputs_verification passed
```

Goal 023 must already be reviewed from the pushed repository. Do not re-open Goal 023, Goal 022 or Goal 021 unless a concrete pushed defect is found.

## Final gate

Stop at exactly one final gate:

```text
rich_package_assembly_coverage_audit_verification
```

Leave this gate `required`, not `passed`.

Do not start Goal 025, S199, package assembly expansion, runtime feature implementation, Unity polish, WinForms UI, provider/LLM/RAG/media/Lua execution, generator-library edits, public `GamePackage` schema changes, `.sln` changes or `.csproj` changes.

## Product outcome

Goal 023 produced deterministic capability-bundle selections and generator pipeline input records. Goal 024 must now audit existing package assembly coverage against those inputs before expanding package assembly.

The concrete generator-capability improvement must be:

```text
accepted capability_bundle_pipeline_inputs_v1
  -> deterministic package assembly coverage matrix
  -> evidence-backed support / partial / sidecar-only / future-required / blocked-gap classification
  -> next package-expansion candidate plan with explicit prerequisites
  -> scope-guarded compact artifacts
```

This is an audit and planning bridge. It must not implement the package expansion itself.

## Read first

Read these before editing:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CONTEXT_INDEX.md`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/DEVELOPMENT_COMPLEXITY_STABILIZATION_POLICY.md`
8. `.devflow/artifact-scope/artifact-scope-policy.json`
9. `.devflow/scripts/check-artifact-scope.ps1`
10. `docs/GOAL_023_CAPABILITY_BUNDLE_SELECTION_TO_PIPELINE_INPUTS.md`
11. `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md`
12. `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-report.json`
13. `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`
14. `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-gap-report.json`
15. `docs/GAME_PACKAGE_FORMAT.md`
16. nearest package assembly, package validation, runtime/package loader and previous Application-layer acceptance analogs under `src/` and `tests/`.

Do not broad-scan the repository when a local package/validator seam is enough. Read only the package/runtime files needed to prove coverage claims.

## Scope

Allowed:

- New audit contract doc:
  - `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md`
- New narrow Application-layer service and models:
  - `src/LLMGameCreator.Application/Design/RichPackageAssemblyCoverageAudit/**`
- New focused tests:
  - `tests/LLMGameCreator.Tests/Application/RichPackageAssemblyCoverageAudit/**`
- New product smoke test:
  - `tests/LLMGameCreator.Tests/ProductSmoke/RichPackageAssemblyCoverageAuditSmokeTests.cs`
- One product smoke route addition in:
  - `.devflow/scripts/run-product-smoke.ps1`
- Compact root artifacts:
  - `.llmgc/procedural/rich-package-assembly-coverage-audit/**`
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Bounded handoff-regression test updates only if `check-all.ps1` requires them after state advances to Goal 024:
  - `tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/CapabilityBundlePipelineInputsAcceptanceTests.cs`
  - `tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs`
  - `tests/LLMGameCreator.Tests/Application/GameProfiles/GeneratedGameProfileContractAcceptanceTests.cs`

If any bounded handoff-regression test is edited, the change must only allow the new Goal 024 state/handoff position. Do not change Goal 021/022/023 product behavior through those tests.

Forbidden:

- Do not edit `generator-library/**`.
- Do not modify accepted Goal 021/022/023 historical artifacts outside the current Goal 024 artifact root.
- Do not change public `GamePackage` schema.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms/Runtime Preview UI work.
- Do not run or modify Unity player/build entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not implement package assembly expansion, world/entity/dialogue/item/combat generation, or runtime execution.
- Do not perform housekeeping/cleanup of tracked generated artifacts.
- Do not mark package support as complete from docs-only evidence.

## Required artifact root

All compact artifacts for this goal must be written under:

```text
.llmgc/procedural/rich-package-assembly-coverage-audit/
```

Suggested files:

```text
rich-package-assembly-coverage-matrix.json
rich-package-assembly-coverage-gap-report.json
rich-package-assembly-next-slice-plan.json
rich-package-assembly-coverage-invalid-matrix.json
rich-package-assembly-coverage-audit-report.json
rich-package-assembly-coverage-audit-report.md
rich-package-assembly-coverage-audit-verification.md
```

## Required slices

### S192: Record accepted Goal 023 and current position

Record that the user accepted:

```text
capability_bundle_pipeline_inputs_verification passed
```

Update state/queue docs so current work becomes Goal 024 and the current gate after this goal is:

```text
rich_package_assembly_coverage_audit_verification required
```

Do not mark it passed.

Queue handling:

- Goal 024 is `Rich Package Assembly Coverage Audit`.
- Move package assembly expansion implementation to the next candidate slot, normally Goal 025.
- Do not start Goal 025 or S199.

### S193: Define rich package assembly coverage audit contract v1

Create `docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md`.

It must define:

- input provenance from accepted Goal 023 generator pipeline input artifacts;
- coverage domains:
  - world/topology/maps/regions;
  - entities/actors/NPCs;
  - quests/objectives/stages;
  - dialogue/interactions;
  - items/inventory/equipment/economy/vendors/crafting;
  - combat/encounters/abilities/status/progression;
  - factions/reputation/social/work/theft/schedules;
  - assets/export/runtime presentation references;
- evidence classes:
  - `package_schema_field`;
  - `package_validator`;
  - `package_assembly_mapping`;
  - `runtime_smoke`;
  - `previous_goal_artifact`;
  - `sidecar_only`;
  - `future_required`;
  - `blocked_gap`;
- support statuses:
  - `package_supported`;
  - `package_supported_partial`;
  - `sidecar_only`;
  - `future_required`;
  - `blocked_gap`;
  - `unsupported`;
- rule: docs-only mentions do not prove package support;
- rule: coverage claims must reference concrete file/type/member/artifact ids where practical;
- rule: this audit may recommend next package-expansion work but must not implement it.

### S194: Add Application-layer coverage audit service

Add a narrow Application-layer service under:

```text
src/LLMGameCreator.Application/Design/RichPackageAssemblyCoverageAudit/
```

The service must:

- load accepted Goal 023 compact artifacts;
- physically validate Goal 023 evidence:
  - report exists;
  - `manualGate == capability_bundle_pipeline_inputs_verification`;
  - `contractProofPassed == true`;
  - `pipelineInputCount == 3`;
  - top-level diagnostics contain no `severity=error`;
  - generator inputs artifact exists and contains 3 pipeline inputs;
- inspect existing package assembly/package validation seams without mutating them;
- map Goal 023 artifact contracts, validators, runtime target ids and package assembly candidate inputs into coverage records;
- classify each record using the support statuses above;
- preserve Goal 023 `future_required` and `blocked_gap` entries rather than treating them as package-supported;
- produce deterministic coverage matrix and gap report artifacts.

Coverage evidence must be conservative. If the service cannot physically point to a package field, validator, runtime smoke or accepted artifact, it must classify as `sidecar_only`, `future_required`, `blocked_gap` or `unsupported`.

### S195: Coverage domains and next-slice candidate plan

The coverage matrix must include at least these audit domains:

```text
world
entities
quests
dialogue_interactions
items_inventory_economy
combat_progression
factions_social_work_theft_schedules
assets_runtime_export
```

For each domain, record:

- domain id;
- related profile ids;
- related Goal 023 pipeline input ids;
- candidate artifact contract ids;
- candidate package schema areas or explicit absence;
- validator ids or explicit absence;
- runtime smoke evidence or explicit absence;
- support status;
- gap ids;
- recommended next action.

Create a deterministic next-slice candidate plan artifact. It must rank candidate package-expansion work, but must not start it.

The first candidate may be `Package Assembly Expansion 1 - World And Entities` only if the audit evidence supports that sequencing. If another order is safer, record the reason.

### S196: Invalid/fake/leak matrix

Minimum invalid/fake/leak scenarios:

1. missing accepted Goal 023 report is rejected;
2. stale or mismatched previous gate is rejected;
3. copied coverage report without Goal 023 generator input artifact is rejected;
4. fewer than 3 Goal 023 pipeline inputs is rejected;
5. top-level error diagnostics in Goal 023 report are rejected;
6. docs-only `GamePackage` mention treated as package support is rejected;
7. future-required capability treated as package-supported is rejected;
8. blocked gap treated as ready-for-package-assembly is rejected;
9. public `GamePackage` schema mutation claim is rejected;
10. package assembly execution claim is rejected;
11. Unity build / LLM / RAG / provider / media / Lua execution claim is rejected;
12. generator-library mutation claim is rejected;
13. historical Goal 020/021/022/023 artifact mutation is rejected by the scope guard or equivalent verification;
14. duplicate coverage domain id is rejected;
15. missing required coverage domain is rejected;
16. Goal 025/S199 started marker is rejected.

Invalid cases should flow through shared artifact, coverage, report or scope guard validation where possible. Do not manually append diagnostics when a real helper can produce them.

### S197: Product smoke and focused tests

Add product smoke route:

```text
rich-package-assembly-coverage-audit
```

The product smoke must:

- build/write compact artifacts under `.llmgc/procedural/rich-package-assembly-coverage-audit/`;
- validate report shape;
- verify `accepted=false`;
- verify final/manual gate is `rich_package_assembly_coverage_audit_verification`;
- verify previous accepted gate is `capability_bundle_pipeline_inputs_verification passed`;
- verify Goal 023 report and generator inputs are physically checked;
- verify all required coverage domains exist;
- verify future-required and blocked gaps are not classified as package-supported;
- verify no package assembly/Unity/LLM/RAG/provider/media/Lua/package-schema execution claims;
- verify product smoke does not mutate historical artifact families.

Focused tests must cover at minimum:

- deterministic output;
- Goal 023 evidence physical validation;
- all required coverage domains present;
- docs-only support rejected;
- future-required and blocked gaps preserved;
- next-slice plan is deterministic and does not start Goal 025/S199;
- invalid matrix rejects required fake/leak scenarios;
- state docs record Goal 023 accepted before Goal 024;
- final report has no top-level `severity=error` diagnostics when proof passes.

### S198: State handoff, artifacts and final scope guard

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Current next work after Goal 024 must be:

```text
rich_package_assembly_coverage_audit_verification
```

Do not mark it passed.

Final report must include:

```text
accepted=false
finalStatus=rich_package_assembly_coverage_audit_verification
manualGate=rich_package_assembly_coverage_audit_verification
previousAcceptedGate=capability_bundle_pipeline_inputs_verification passed
goal023EvidenceVerified=true
coverageDomainCount>=8
coverageMatrixWritten=true
gapReportWritten=true
nextSlicePlanWritten=true
packageAssemblyExecuted=false
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
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~RichPackageAssemblyCoverageAudit|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario rich-package-assembly-coverage-audit
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-024-final -AllowedPath docs/RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT_V1.md -AllowedPathPrefix src/LLMGameCreator.Application/Design/RichPackageAssemblyCoverageAudit/ -AllowedPathPrefix tests/LLMGameCreator.Tests/Application/RichPackageAssemblyCoverageAudit/ -AllowedPath tests/LLMGameCreator.Tests/ProductSmoke/RichPackageAssemblyCoverageAuditSmokeTests.cs -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPathPrefix .llmgc/procedural/rich-package-assembly-coverage-audit/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/GOAL_024_RICH_PACKAGE_ASSEMBLY_COVERAGE_AUDIT.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-024-rich-package-assembly-coverage-audit-CODEX_GOAL.md -AllowedPath tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/CapabilityBundlePipelineInputsAcceptanceTests.cs -AllowedPath tests/LLMGameCreator.Tests/Devflow/DevelopmentComplexityStabilizationTests.cs -AllowedPath tests/LLMGameCreator.Tests/Application/GameProfiles/GeneratedGameProfileContractAcceptanceTests.cs
```

If `check-all.ps1` fails because a stale handoff regression test still assumes Goal 023 is the latest completed goal, update only the minimal allowed handoff regression assertion and rerun the focused test plus `check-all.ps1`.

If `check-all.ps1` fails because of a real product or scope defect, fix within this goal. If it fails because of an environmental blocker, stop and report exact logs.

## Anti-false-positive review

Before final response, directly inspect:

- `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-audit-report.json`;
- `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-matrix.json`;
- `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-gap-report.json`;
- `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-next-slice-plan.json`;
- `.llmgc/procedural/rich-package-assembly-coverage-audit/rich-package-assembly-coverage-invalid-matrix.json`;
- `docs/CURRENT_GENERATOR_STATE.json`;
- `docs/CONTEXT_INDEX.md`;
- the final artifact scope report produced by `check-artifact-scope.ps1`.

Checks:

- final report has `accepted=false`;
- final/manual gate is `rich_package_assembly_coverage_audit_verification`;
- previous accepted gate is `capability_bundle_pipeline_inputs_verification passed`;
- Goal 023 evidence was physically verified;
- all required coverage domains are present;
- future-required and blocked gaps are not treated as package-supported;
- docs-only evidence is not accepted as package support;
- next-slice plan is a recommendation only and does not start Goal 025/S199;
- no package assembly or Unity build is claimed;
- no LLM/RAG/provider/media/Lua execution is claimed;
- public GamePackage schema and generator-library are unchanged;
- historical artifacts are not mutated;
- scope guard passes with zero violations;
- no S199/Goal 025 work started except queue text;
- mojibake markers absent in changed text files.

## Final response requirements

The final Codex response must include:

- changed files;
- new contract/service/test/product-smoke paths;
- compact artifact paths;
- coverage domain summary;
- top package support gaps;
- next-slice candidate plan summary;
- generated matrix/gap/plan/report hashes;
- invalid matrix count;
- focused/product-smoke/check-all/scope-guard verification results;
- whether final valid report has zero top-level error diagnostics;
- confirmation that `rich_package_assembly_coverage_audit_verification` remains required, not passed;
- confirmation that Goal 025/S199 was not started;
- exact bounded git commands used, or confirmation none were used except through `check-artifact-scope.ps1`.
