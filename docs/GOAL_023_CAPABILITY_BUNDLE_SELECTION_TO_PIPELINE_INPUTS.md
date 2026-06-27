# Goal 023: Capability Bundle Selection To Pipeline Inputs

## Starting gate

This goal may start only after the user explicitly provides:

```text
development_complexity_stabilization_verification passed
```

Goal 022 must already be reviewed from the pushed repository. Do not re-open Goal 022 or Goal 021 unless a concrete pushed defect is found.

## Final gate

Stop at exactly one final gate:

```text
capability_bundle_pipeline_inputs_verification
```

Leave this gate `required`, not `passed`.

Do not start Goal 024, S192, package assembly expansion, Unity polish, WinForms UI, provider/LLM/RAG/media/Lua execution, generator-library edits, public GamePackage schema changes, `.sln` changes or `.csproj` changes.

## Product outcome

Goal 021 accepted `game_profile_v1`. Goal 022 added artifact-scope governance. Goal 023 must now turn accepted game profiles into deterministic capability bundle selections and concrete generation pipeline inputs.

The concrete generator-capability improvement must be:

```text
accepted game_profile_v1
  -> deterministic capability bundle selection
  -> concrete generator pipeline input records
  -> explicit supported_now / future_required / blocked_gap separation
  -> scope-guarded compact artifacts for the next package-assembly goals
```

This is a generator planning bridge, not a Unity, UI, LLM, Lua or package-schema goal.

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
10. `docs/GOAL_021_GENERATED_GAME_PROFILE_CONTRACT_REFRESH.md`
11. `docs/GAME_PROFILE_CONTRACT_V1.md`
12. `.llmgc/procedural/generated-game-profile-contract/generated-game-profile-contract-report.json`
13. `.llmgc/procedural/generated-game-profile-contract/generated-game-profile-contract-pipeline-plan.json`
14. `samples/game-profiles/*.game-profile.json`
15. `docs/GENERATOR_PLAN_CAPABILITY_SELECTION_PICKER.md`
16. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionService.cs`
17. `src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanCapabilitySelectionModels.cs`
18. `tests/LLMGameCreator.Tests/Design/GeneratorPlanCapabilitySelectionServiceTests.cs`
19. `generator-library/atlas/game_form_factor_taxonomy.json`
20. `generator-library/atlas/game_system_variant_taxonomy.json`
21. `generator-library/atlas/feature_bundle_map.json`
22. `generator-library/atlas/capability_atlas.json`
23. `generator-library/atlas/artifact_contracts.json`

`generator-library/**` is read-only in this goal. If the atlas has missing bundles/contracts/validators, record those as gaps. Do not patch the atlas in this goal.

## Scope

Allowed:

- New contract doc:
  - `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md`
- New narrow Application-layer service and models:
  - `src/LLMGameCreator.Application/Design/CapabilityBundlePipelineInputs/**`
- New focused tests:
  - `tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/**`
- New product smoke test:
  - `tests/LLMGameCreator.Tests/ProductSmoke/CapabilityBundlePipelineInputsSmokeTests.cs`
- One product smoke route addition in:
  - `.devflow/scripts/run-product-smoke.ps1`
- Compact root artifacts:
  - `.llmgc/procedural/capability-bundle-pipeline-inputs/**`
- State/routing docs:
  - `docs/CURRENT_GENERATOR_STATE.json`
  - `docs/CURRENT_GENERATOR_STATE.md`
  - `docs/CONTEXT_INDEX.md`
  - `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Forbidden:

- Do not edit `generator-library/**`.
- Do not modify accepted Goal 021 artifacts under `.llmgc/procedural/generated-game-profile-contract/**`.
- Do not modify accepted Goal 020/Unity historical artifacts.
- Do not change public `GamePackage` schema.
- Do not change `.sln` or `.csproj`.
- Do not add WinForms/Runtime Preview UI work.
- Do not run or modify Unity player/build entrypoints.
- Do not invoke LLM/RAG/providers/media generation/arbitrary Lua.
- Do not implement package assembly expansion, world/entity/dialogue/item/combat generation, or runtime execution beyond deterministic planning artifacts.
- Do not perform housekeeping/cleanup of tracked generated artifacts.

## Required artifact root

All compact artifacts for this goal must be written under:

```text
.llmgc/procedural/capability-bundle-pipeline-inputs/
```

Suggested files:

```text
capability-bundle-pipeline-inputs-profile-requests.json
capability-bundle-pipeline-inputs-selection.json
capability-bundle-pipeline-inputs-generator-inputs.json
capability-bundle-pipeline-inputs-gap-report.json
capability-bundle-pipeline-inputs-invalid-matrix.json
capability-bundle-pipeline-inputs-report.json
capability-bundle-pipeline-inputs-report.md
capability-bundle-pipeline-inputs-verification.md
```

## Required slices

### S185: Record accepted Goal 022 and current position

Record that the user accepted:

```text
development_complexity_stabilization_verification passed
```

Update state/queue docs so current work becomes Goal 023 and the current gate after this goal is:

```text
capability_bundle_pipeline_inputs_verification required
```

Do not mark it passed.

Queue handling:

- Goal 023 is `Capability Bundle Selection To Pipeline Inputs`.
- Move richer package assembly work to the next candidate slot, normally Goal 024.
- Do not start Goal 024 or S192.

### S186: Define capability-bundle pipeline input contract v1

Create `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md`.

It must define at least:

- source accepted profile id;
- source profile hash or source file refs;
- selected atlas variant ids:
  - presentation mode;
  - world topology;
  - actor model;
  - inventory model;
  - combat model;
  - progression model;
  - pathfinding profile;
  - NPC behavior model;
- selected feature bundle ids;
- selected runtime target ids;
- resolved capability ids;
- resolved artifact contract ids;
- resolved validator ids;
- resolved prompt context template ids;
- concrete generation pipeline input records for the next goals;
- supported-now capability ids;
- future-required capability ids;
- blocked/impossible compatibility gaps;
- provenance to Goal 021 profile artifacts and Goal 022 scope policy;
- no runtime dependency on LLM/RAG/provider/media/arbitrary Lua.

The contract must be planning/data oriented. It must not mutate `GamePackage`.

### S187: Add profile-to-capability-selection adapter

Add a narrow Application-layer service under:

```text
src/LLMGameCreator.Application/Design/CapabilityBundlePipelineInputs/
```

The service must:

- load the three accepted sample profiles from `samples/game-profiles/`;
- load the existing generator-plan capability atlas through the existing atlas reader/service where possible;
- reuse `GeneratorPlanCapabilitySelectionService` or its atlas reader instead of inventing a parallel capability resolver;
- derive deterministic `GeneratorPlanCapabilitySelectionRequest` records from each profile;
- map profile fields to required selector fields:
  - direct: `presentationMode`, `worldTopology`, `actorModel`;
  - derived: inventory model, combat model, progression model, pathfinding profile, NPC behavior model;
  - bundle ids: deterministic from core bundles plus profile capability ids and atlas-resolved bundle/provider relationships;
  - runtime targets: deterministic from profile runtime/export target;
- produce one profile request artifact per accepted profile or one combined deterministic request artifact;
- distinguish direct atlas errors from future-required or blocked gaps.

Important compatibility rule:

Existing accepted profiles must not be silently rewritten to satisfy the older atlas. If a profile choice is accepted by `game_profile_v1` but incompatible or missing in the existing atlas, record a `blocked_gap` or `future_required` entry. Do not edit Goal 021 profiles or atlas data to hide the mismatch.

### S188: Build concrete generation pipeline input records

For each valid accepted profile, write a concrete pipeline input record that includes at minimum:

- `profileId`;
- `gameFamilyId`;
- `selectionId`;
- selected feature bundle ids;
- resolved capability ids;
- resolved artifact contract ids;
- resolved validator ids;
- resolved prompt context template ids;
- resolved runtime target ids;
- expected downstream generation stages;
- package assembly candidate inputs for future goals, expressed as ids/contracts only;
- `supportedNowCapabilityIds`;
- `futureRequiredCapabilityIds`;
- `blockedGapIds`;
- `readyForPackageAssemblyPlanning` boolean;
- deterministic diagnostics.

The record is a planning input, not a package or runtime output. It must not claim package assembly, Unity build, LLM execution, Lua execution or provider/media execution.

### S189: Gap and invalid/fake/leak matrix

Write a gap report and invalid matrix under the goal artifact root.

Gap report must include:

- atlas incompatibilities between accepted profile ids and current atlas ids;
- missing artifact contracts or validators;
- future-required capabilities from Goal 021 preserved as future-required;
- selected capability ids that have no current bundle/contract/validator route;
- explanation that gaps are planning blockers or future-required items, not false successful support.

Minimum invalid/fake/leak scenarios:

1. missing accepted Goal 022 gate is rejected;
2. missing accepted Goal 021 profile artifacts are rejected;
3. copied capability-selection report without profile files is rejected;
4. unknown profile id is rejected;
5. duplicate profile id is rejected through shared profile-set validation;
6. unknown feature bundle id is rejected;
7. unknown runtime target id is rejected;
8. presentation/topology incompatibility is not accepted as complete;
9. future capability marked `supported_now` is rejected;
10. generated pipeline input claims package assembly already ran;
11. generated pipeline input claims Unity build already ran;
12. generated pipeline input claims LLM/RAG/provider/media/Lua execution;
13. public GamePackage schema mutation claim is rejected;
14. generator-library mutation claim is rejected;
15. cross-family leakage maps gothic/trade to frontier-only bundle/package ids;
16. historical Goal 021/020 artifact mutation is rejected by the scope guard or equivalent verification.

Invalid cases should flow through shared profile, atlas, selector, gap or scope guard validation where possible. Do not manually append diagnostics when a real helper can produce them.

### S190: Product smoke and focused tests

Add product smoke route:

```text
capability-bundle-pipeline-inputs
```

The product smoke must:

- build/write the compact artifacts under `.llmgc/procedural/capability-bundle-pipeline-inputs/`;
- validate report shape;
- verify `accepted=false`;
- verify final/manual gate is `capability_bundle_pipeline_inputs_verification`;
- verify previous accepted gate is `development_complexity_stabilization_verification passed`;
- verify 3 accepted profiles are processed;
- verify future-required capabilities remain future-required;
- verify no Unity/LLM/RAG/provider/media/Lua/package-schema execution claims;
- verify product smoke does not mutate historical artifact families.

Focused tests must cover at minimum:

- deterministic output;
- three accepted profiles processed;
- profile-to-selector requests include concrete variant ids;
- selected feature bundle ids are deterministic and non-empty or produce explicit causal gap diagnostics;
- concrete generator pipeline input records include contracts/validators/runtime targets/prompt context templates or explicit gaps;
- gothic/trade future capabilities are not marked supported;
- atlas incompatibilities are not accepted as complete;
- invalid matrix rejects required fake/leak scenarios;
- state docs record Goal 022 accepted before Goal 023;
- final report has no top-level `severity=error` diagnostics when proof passes.

### S191: State handoff, artifacts and final scope guard

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`

Current next work after Goal 023 must be:

```text
capability_bundle_pipeline_inputs_verification
```

Do not mark it passed.

Final report must include:

```text
accepted=false
finalStatus=capability_bundle_pipeline_inputs_verification
manualGate=capability_bundle_pipeline_inputs_verification
previousAcceptedGate=development_complexity_stabilization_verification passed
profileCount=3
pipelineInputCount=3
capabilitySelectionStarted=true
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
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CapabilityBundlePipelineInputs|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario capability-bundle-pipeline-inputs
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario goal-023-final -AllowedPath docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md -AllowedPathPrefix src/LLMGameCreator.Application/Design/CapabilityBundlePipelineInputs/ -AllowedPathPrefix tests/LLMGameCreator.Tests/Application/CapabilityBundlePipelineInputs/ -AllowedPath tests/LLMGameCreator.Tests/ProductSmoke/CapabilityBundlePipelineInputsSmokeTests.cs -AllowedPath .devflow/scripts/run-product-smoke.ps1 -AllowedPathPrefix .llmgc/procedural/capability-bundle-pipeline-inputs/ -AllowedPath docs/CURRENT_GENERATOR_STATE.json -AllowedPath docs/CURRENT_GENERATOR_STATE.md -AllowedPath docs/CONTEXT_INDEX.md -AllowedPath docs/FULL_GENERATOR_GOAL_QUEUE.md -AllowedPath docs/GOAL_023_CAPABILITY_BUNDLE_SELECTION_TO_PIPELINE_INPUTS.md -AllowedPath docs/agent-tasks/NEXT_PRODUCT_SLICE/goal-023-capability-bundle-selection-to-pipeline-inputs-CODEX_GOAL.md
```

If `check-all.ps1` fails because of a real defect, fix within scope. If it fails because of an environmental blocker, stop and report the blocker with exact logs.

## Anti-false-positive review

Before final response, directly inspect:

- `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-report.json`;
- `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-selection.json`;
- `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-generator-inputs.json`;
- `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-gap-report.json`;
- `.llmgc/procedural/capability-bundle-pipeline-inputs/capability-bundle-pipeline-inputs-invalid-matrix.json`;
- `docs/CURRENT_GENERATOR_STATE.json`;
- `docs/CONTEXT_INDEX.md`;
- the final artifact scope report produced by `check-artifact-scope.ps1`.

Checks:

- final report has `accepted=false`;
- final/manual gate is `capability_bundle_pipeline_inputs_verification`;
- previous accepted gate is `development_complexity_stabilization_verification passed`;
- three accepted profiles produce three deterministic pipeline input records;
- future-required capabilities from Goal 021 remain future-required, not supported-now;
- atlas incompatibility or missing atlas support is explicit, not treated as complete;
- no package assembly or Unity build is claimed;
- no LLM/RAG/provider/media/Lua execution is claimed;
- public GamePackage schema and generator-library are unchanged;
- historical artifacts are not mutated;
- scope guard passes with zero violations;
- no S192/Goal 024 work started except queue text;
- mojibake markers absent in changed text files.

## Final response requirements

The final Codex response must include:

- changed files;
- new contract/service/test/product-smoke paths;
- compact artifact paths;
- selected profiles;
- selected feature bundles or explicit gap summary for each profile;
- generated selection/generator-input/gap/report hashes;
- invalid matrix count;
- focused/product-smoke/check-all/scope-guard verification results;
- whether final valid report has zero top-level error diagnostics;
- confirmation that `capability_bundle_pipeline_inputs_verification` remains required, not passed;
- confirmation that Goal 024/S192 was not started;
- exact bounded git commands used, or confirmation none were used except through `check-artifact-scope.ps1`.
