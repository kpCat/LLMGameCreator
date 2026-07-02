# Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenSpineQualityConsolidation
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenGamePackageRuntimePreviewBridge
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenGamePackageRuntimePreviewBridgeProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-080-edit-driven-gamepackage-runtime-preview-bridge"
```

Also run a raw source-format scan equivalent to Goal 079A over all changed C# files and the Goal 074-080 guarded C# scope.

If `check-all.ps1` times out in the runner harness but is still active, do not mark it as passed blindly. Inspect run logs/processes and rerun with enough time or isolate unrelated slow historical suites. Final report must distinguish tool timeout from actual validation failure/pass.
