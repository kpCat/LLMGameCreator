# Validation commands

Run these commands from repository root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenReviewPackagePlayableSession
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenSpineQualityConsolidation
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenSpineQualityConsolidationProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-079-edit-driven-spine-quality-consolidation"
```

If `check-all.ps1` exceeds the local tool timeout but continues to execute, use the existing run-log pattern from Goals 076-078: poll the generated `.devflow/runs/*-check-all` directory and report the actual result. Do not call a timeout GREEN without evidence.

Also run direct hygiene scans over changed files and Goal 079 evidence:

- no mojibake markers;
- no absolute local paths such as `C:\Users` or drive-root paths in tracked evidence;
- no timestamp-like values in Goal 079 evidence except stable documented non-volatile sentinel values already used in current state;
- no scratch tamper files left under `.llmgc/procedural/goal-079-edit-driven-spine-quality-consolidation/`.
