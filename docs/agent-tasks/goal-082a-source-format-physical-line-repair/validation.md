# Validation commands

Run these commands from repo root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenUnityAlphaStreamingAssetsHandoff
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenUnityAlphaStreamingAssetsHandoffProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-082a-source-format-physical-line-repair"
git diff --check
git diff --cached --check
```

Required direct source-format checks:

```powershell
# Replace path list with all Goal082 .cs files, workspace parent files, Unity probe, and tests.
# This check must operate on raw bytes/text, not on Roslyn logical lines only.
# It must fail for files with zero LF, CR-only separators, raw physical line > 500,
# or a large file with only one physical line.
```

Also run a mojibake marker scan over all changed text files and report the result.

The exact artifact-scope command must pass. If it fails only because of unrelated local dirty files, do not claim GREEN until the final committed tracked diff passes the scenario scope or the task is honestly BLOCKED.
