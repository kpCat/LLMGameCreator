# S021 Hardening Task: Provider Job Plan Merge-Readiness Fixes

## Executor decision

Use Kilo Code first. This is a narrow hardening/cleanup pass over already implemented S021 provider job plan.

Use Codex only if Kilo fails checks or produces broad rewrites.

## Branch / git policy

Work in the repository as it is currently checked out.

Do not create branches.
Do not switch branches.
Do not merge.
Do not rebase.
Do not cherry-pick.
Do not run git commands.
Branch management is handled manually by the user.

## Goal

Harden the current Provider Job Plan implementation before merge decision.

This is not a new product slice. Do not add new feature scope. Fix correctness and merge-readiness issues found during review.

## Known review findings to address

### 1. Provider job plan errors must affect materialization readiness

Current materialization integrates provider job plan diagnostics into validation report, but materialization readiness is still derived only from dry-run readiness.

Fix this so provider job plan errors affect the final `UnityArchiveMaterializationReadiness`.

Required behavior:

```text
dry-run invalid/blocked                  -> existing blocked/invalid behavior remains
request pipeline errors                  -> materialization must not report playable success
provider job plan errors                 -> materialization must not report playable success
warnings only from request/provider plan -> materialization with warnings / metadata-only according to existing dry-run category
no warnings/errors                       -> existing success behavior
```

Keep existing dry-run mapping semantics, but add a safe aggregation step after request pipeline and provider job plan are built.

Do not overcomplicate this. A small helper is enough, for example:

```text
ApplyPipelineAndProviderReadiness(...)
```

or

```text
CombineMaterializationReadiness(...)
```

Acceptance:
- if provider job plan has error diagnostics, validation report readiness is not `MaterializedPlayableContract`;
- if provider job plan has only warnings, readiness is warning-level, not blocked;
- existing normal S021 smoke still passes.

### 2. Avoid confusing diagnostic code double-prefixing

Review observed request pipeline diagnostics can become:

```text
request.request.diagnostic...
```

because pipeline diagnostic codes already start with `request.diagnostic...` and materialization prepends `request.`.

Fix this if present.

Preferred behavior:
- preserve original request pipeline diagnostic code if it already starts with `request.`;
- otherwise prefix with `request.`;
- do not break existing tests by changing unrelated codes.

Acceptance:
- materialization validation report should not contain `request.request.` diagnostic codes.

### 3. Add focused regression tests

Add focused tests for:

1. Provider job plan error changes materialization readiness away from playable success.
2. Request pipeline diagnostic prefix does not double-prefix to `request.request`.
3. Normal provider job plan smoke remains unchanged and green.

Do not add brittle snapshot files.

### 4. Keep S021 scope unchanged

Do not add S022 fulfillment scanner in this hardening task.

Do not change provider job plan JSON shape unless required by the above fixes.

## Read first

Read only these files first:

```text
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveRequestPipelineModels.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveMaterializationTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveProviderJobPlanTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveProviderJobPlanSmokeTests.cs
docs/PRODUCT_SMOKE_SCENARIOS.md
.devflow/scripts/run-product-smoke.ps1
.devflow/CURRENT_RUN.md
```

If another file is truly necessary, read only that file and explain why in the final report.

## Allowed files

```text
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveMaterializationModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveProviderJobPlanService.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveMaterializationTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveProviderJobPlanTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveProviderJobPlanSmokeTests.cs
.devflow/CURRENT_RUN.md
```

Only touch smoke docs/scripts if tests show they are inconsistent.

## Forbidden files

```text
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Scripting/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.WinForms/**
generator-library/**
LLMGameCreator.sln
*.csproj
.devflow/NEXT_TASK.md
.devflow/task-queue.json
```

Do not add NuGet packages.
Do not create Unity project files.
Do not call LLM/provider.
Do not execute generators.
Do not execute Lua.
Do not implement ComfyUI/Suno integration.
Do not change Runtime, WinForms, GamePackage schema or generator-library.

## Required checks

Focused:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveProviderJob"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveMaterialization"
```

Smoke:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-provider-job-plan
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
```

Final:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

## Stop conditions

Stop and report if:
- fix requires Runtime/GamePackage schema/WinForms changes;
- fix requires `.sln` or `.csproj` changes;
- task turns into S022 or a new feature;
- more than 8 files need changes;
- check-all fails after 2 repair attempts.

## Final report

Report in Russian:

```text
files read
files changed
how provider job plan readiness now affects materialization readiness
request diagnostic prefix behavior
tests/checks results
confirmation that Unity/Runtime/WinForms/GamePackage schema/provider/generator/LLM/Lua were not touched
recommendation: merge / needs another cleanup / reject
```
