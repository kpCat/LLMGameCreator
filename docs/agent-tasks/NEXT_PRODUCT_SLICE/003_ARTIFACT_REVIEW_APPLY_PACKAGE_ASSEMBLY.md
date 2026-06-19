# Product Slice 003 Task: Artifact Review -> Apply -> Package Assembly

## Task type

Large product implementation slice.

## Goal

Implement a narrow but real product flow:

```text
staged strict LLM artifacts
-> artifact review approval
-> apply approved baseline artifacts
-> draft GamePackage assembly
-> validation
-> save/export inspectable package output
```

This task should turn generated JSON artifacts into game/package state.

## Source-of-truth docs to read first

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/M4_1_REAL_EVALUATION_GATE_REPORT.md
docs/PRODUCT_SLICE_003_ARTIFACT_REVIEW_APPLY_PACKAGE_ASSEMBLY.md
docs/GENERATOR_PLAN_STRICT_LLM_ARTIFACT_GENERATION.md
docs/GENERATOR_PLAN_ONE_CLICK_PACKAGE_EXPORT_UI.md
src/LLMGameCreator.WinForms/Pages/ArtifactReview/ArtifactReviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/PackageExport/PackageExportPageControl.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanDraftArtifactReviewModels.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanDraftArtifactApprovalArtifactModels.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanStrictLlmArtifactGenerationArtifactService.cs
src/LLMGameCreator.GamePackage/**
```

Then search narrowly for existing artifact services, package validators, package save/export services and tests.

## Allowed files

Expected areas:

```text
src/LLMGameCreator.Application/Design/GeneratorPlans/**
src/LLMGameCreator.Application/**Package**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.WinForms/Pages/ArtifactReview/**
src/LLMGameCreator.WinForms/Pages/PackageExport/**
src/LLMGameCreator.WinForms/CompositionRoot.cs
tests/LLMGameCreator.Tests/**/Artifact*Tests.cs
tests/LLMGameCreator.Tests/**/Package*Tests.cs
tests/LLMGameCreator.Tests/**/GamePackage*Tests.cs
samples/**
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Only touch `CompositionRoot.cs` if new service registration is required.

## Forbidden files

```text
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Runtime*/**
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
.devflow/scripts/**
docs/agent-tasks/M5/**
docs/agent-tasks/M6/**
docs/agent-tasks/M8/**
docs/agent-tasks/M9/**
docs/agent-tasks/M10/**
```

Do not add NuGet packages.

## Required behavior

### 1. Reuse existing artifact review/storage model

Before implementing anything new, inspect existing review, approval and strict LLM artifact services.

Reuse current approval/review concepts if present. Do not create a parallel artifact storage system unless no usable service exists.

### 2. Artifact Review actions

Add or wire UI actions so the user can:

```text
Approve selected valid artifact
Approve all valid artifacts
Reject selected artifact or mark as not approved
Apply approved artifacts to package assembly
```

If approval already exists, add only the missing `Apply approved` path.

### 3. Baseline contract support

Support only:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

Unknown contracts should be preserved or skipped with a clear warning, not crash.

### 4. Package assembly service

Create/reuse an application service:

```text
input: approved artifact records
output: package assembly result
```

The result should include:

```text
ok/status
applied artifact count
skipped artifact count
diagnostics
draft package object or export path
provenance
```

### 5. Mapping rules

Map what existing package model supports.

If current `GamePackage` has exact fields, use them. If exact fields do not exist, do not create a huge new game schema. Use a narrow generated content/extension/preserved-artifacts section if the package model supports it. If it does not, add a minimal non-breaking structure in GamePackage for applied generated content.

Minimal desired content:

```text
package title/description from game_profile_v1
core loop/pillars from game_profile_v1
scene ids/titles/descriptions from scene_pack_v1
quest ids/titles/descriptions/steps from quest_pack_v1
mechanic ids/names/descriptions from mechanics_pack_v1
source contract/artifact provenance
```

### 6. Validation

Add focused validation:

```text
baseline artifact json parses
artifact_kind matches expected contract
ids are non-empty
ids are unique within scenes/quests/mechanics when present
package title is non-empty if game_profile_v1 was applied
provenance is present
```

Do not make generated content fail because of deeper runtime rules that are not implemented yet.

### 7. Save/export

Use existing package/project save conventions if available. If no suitable convention exists, write an inspectable draft output under the current game folder:

```text
.llmgc/package-assembly/draft-package.json
.llmgc/package-assembly/package-assembly-report.md
```

The UI should display the export path/report.

### 8. LLM isolation

No LLM calls in this task. Applying artifacts must not call LM Studio/provider.

### 9. Preserve generated artifact history

Do not delete staged artifacts after applying.

Applying should be repeatable/idempotent where practical.

### 10. Manual flow

Target manual flow after task:

```text
Capability Picker -> Save selection
LLM Artifacts -> Generate baseline contracts with Stage for review
Artifact Review -> Load/refresh
Artifact Review -> Approve all valid
Artifact Review -> Apply approved to package
Package Export or result panel -> inspect draft package/report
```

## Tests

Add focused tests:

1. `game_profile_v1` maps to package title/description/core metadata or preserved section.
2. `scene_pack_v1` maps/preserves scene list with ids.
3. `quest_pack_v1` maps/preserves quest list with ids.
4. `mechanics_pack_v1` maps/preserves mechanics list with ids.
5. duplicate ids produce validation diagnostic.
6. unknown contract is skipped/preserved with warning, not crash.
7. applying same artifacts twice is deterministic/idempotent at service level.
8. package assembly result includes provenance.
9. no provider/LLM service is required for assembly service.

Prefer application-layer tests over fragile WinForms tests.

## Focused commands

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Artifact"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Package"
```

## Required checks

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Manual verification

Report whether manual verification was done. If not, say so clearly.

Manual steps:

```text
1. Start WinForms app.
2. Open existing test project.
3. LLM Artifacts: confirm staged valid baseline artifacts exist or generate them.
4. Artifact Review: load/refresh artifacts.
5. Approve all valid baseline artifacts.
6. Apply approved artifacts to package.
7. Confirm package assembly result/report is shown.
8. Confirm output file/package state exists.
9. Confirm Package Export can see or export the assembled package if integrated.
```

## Stop conditions

Stop and report if:

- more than 20 files need changes;
- `.sln` or `.csproj` changes are required;
- Lua/runtime implementation becomes necessary;
- existing strict LLM generation/evaluation flow breaks;
- package schema requires a destructive rewrite;
- check-all fails after 2 repair attempts;
- current generated artifacts would need to be deleted/migrated destructively.

## Expected final report in Russian

Include:

- files read;
- files changed;
- artifact approval behavior;
- package assembly service behavior;
- baseline contracts supported;
- mapping/preserve strategy;
- validation diagnostics;
- save/export path;
- focused test results;
- check-devflow-state result;
- check-all result;
- manual verification status;
- remaining gaps and recommended next slice.
