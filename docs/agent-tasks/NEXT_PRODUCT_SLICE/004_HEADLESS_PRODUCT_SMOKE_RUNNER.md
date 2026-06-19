# Product Slice 004 Task: Headless Product Smoke Runner

## Task type

Large product integration test / smoke automation slice.

## Goal

Add a headless smoke runner for the baseline strict artifact -> package assembly flow.

The runner must prove this path without UI and without LLM:

```text
fixture approved artifacts
-> GeneratorPlanGamePackageAssemblyService / assembler
-> exported package.json
-> validation assertions
-> report
```

## Recommended Codex reasoning level

High.

Do not use Max/Ultra on first attempt.
Do not use Low/Medium because this task can easily miss fixture/service/export/report boundaries.

## Source-of-truth docs to read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/M4_1_REAL_EVALUATION_GATE_REPORT.md
docs/PRODUCT_SLICE_003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md
docs/PRODUCT_SLICE_004_HEADLESS_PRODUCT_SMOKE_RUNNER.md
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyValidator.cs
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
tests/LLMGameCreator.Tests/Design/GeneratorPlanGamePackageAssemblyPipelineTests.cs
```

Then search narrowly for existing package repository/test helpers and devflow script patterns.

## Allowed files

```text
tests/LLMGameCreator.Tests/Design/GeneratorPlanGamePackageAssemblyPipelineTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/**
tests/fixtures/product-smoke/**
.devflow/scripts/run-product-smoke.ps1
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Application/GamePackage production files are allowed only for small fixes if the new smoke test exposes a real defect:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyService.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssemblyValidator.cs
src/LLMGameCreator.GamePackage/GamePackageDefinition.cs
```

## Forbidden files

```text
src/LLMGameCreator.Runtime*/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/Generation/**
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
docs/agent-tasks/M5/**
docs/agent-tasks/M6/**
docs/agent-tasks/M8/**
docs/agent-tasks/M9/**
docs/agent-tasks/M10/**
```

Do not add NuGet packages.

## Required behavior

### 1. Add deterministic baseline approved artifact fixture

Create either JSON fixture files or a strongly typed test fixture factory.

Must include approved artifacts for:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

Each artifact must have:

```text
artifact_id
contract_id / expected contract
artifact_kind
content_json
capability_selection_id
audit/generated metadata if supported by existing models
```

Use the existing approved artifact models. Do not invent a parallel fixture schema if the existing type is easy to construct in tests.

### 2. Add ProductSmoke test

Create focused test(s), preferably under:

```text
tests/LLMGameCreator.Tests/ProductSmoke/
```

Test name should include:

```text
BaselineStrictArtifactsPackageAssemblySmoke
```

The test must:

1. Create/load the baseline approved artifact set fixture.
2. Run package assembly through the real application service or assembler/validator pipeline.
3. Export package JSON to a temp folder.
4. Assert:
   - result.Ok is true or no error diagnostics;
   - package JSON file exists;
   - manifest title is not empty;
   - generatedContent.profile.title is not empty;
   - generatedContent.scenes count >= 1;
   - generatedContent.quests count >= 1;
   - generatedContent.mechanics count >= 1;
   - appliedArtifacts includes all four baseline contracts;
   - provenance/content hash exists for all applied baseline artifacts;
   - no LLM/provider dependency is required.

If current production code does not map all four baseline artifacts correctly, fix the package assembly mapping narrowly.

### 3. Add product smoke script

Add:

```text
.devflow/scripts/run-product-smoke.ps1
```

Minimum behavior:

```powershell
param(
  [string]$Scenario = "baseline-strict-package-assembly"
)
```

For `baseline-strict-package-assembly`, run:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
```

The script should:
- create `.devflow/runs/<timestamp>-product-smoke/`;
- write a small `product-smoke-summary.md`;
- write a small `product-smoke-summary.json`;
- return non-zero exit code on failure;
- not call git;
- not call LLM;
- not open UI.

Do not make the script overly complex.

### 4. Add docs

Add/update:

```text
docs/PRODUCT_SMOKE_SCENARIOS.md
```

Include:
- scenario name;
- what it validates;
- command;
- expected output;
- no-LLM guarantee;
- how it relates to manual UI tests.

### 5. Current state update

Update current state docs to say:

```text
Product Slice 004 adds headless smoke automation for baseline artifact package assembly.
Manual checking remains needed only for UI-specific changes and major new runtime gates.
```

Do not mark Unity/runtime complete.

## Tests

Required tests:

1. baseline smoke test with all four contracts.
2. package JSON has non-empty generatedContent.profile.
3. package JSON includes scenes/quests/mechanics.
4. package JSON includes applied artifact provenance for all four baseline contracts.
5. product smoke does not require LLM/provider types.

## Focused commands

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Package"
```

## Required checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

Manual UI verification is not required for this slice unless production code changes affect UI unexpectedly.

If not performed, report clearly:

```text
Manual UI verification not required for this headless smoke slice.
```

## Stop conditions

Stop and report if:

- `.sln` or `.csproj` changes are required;
- runtime/Lua/UI changes become necessary;
- product smoke requires LLM/provider calls;
- package assembly needs broad schema rewrite;
- check-all fails after 2 repair attempts.

## Expected final report in Russian

Include:

- files read;
- files changed;
- fixture strategy;
- product smoke command;
- product smoke output path;
- package assertions covered;
- whether all four baseline contracts are verified;
- focused test results;
- run-product-smoke result;
- check-devflow-state result;
- check-all result;
- remaining gaps and recommended next slice.
