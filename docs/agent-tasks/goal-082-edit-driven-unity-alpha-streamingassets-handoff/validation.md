# Validation commands

Required commands:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenGamePackageRuntimePreviewBridge
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenGamePackageRuntimePreviewPlaythrough
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenUnityAlphaStreamingAssetsHandoff
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenUnityAlphaStreamingAssetsHandoffProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-082-edit-driven-unity-alpha-streamingassets-handoff"
```

Also run:

```powershell
git diff --check
git diff --cached --check
```

Evidence hygiene checks:

- no absolute local paths in tracked Goal082 evidence;
- no timestamp-like values or heavy logs in tracked Goal082 evidence;
- no scratch/tamper temp files left in tracked artifacts;
- no mojibake markers in changed text files;
- no new minified / CR-only / zero-LF source files;
- `AlphaRuntimeBootstrap.cs` hash and line count unchanged.
