# Validation commands

Run these commands from the repository root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenSpineQualityConsolidation
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenSpineQualityConsolidationProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-079a-source-format-line-ending-guard"
```

Also run a raw-byte source-format scan after repairs and report:

- scanned C# file count
- files with zero LF bytes
- files with CR-only line endings
- raw max physical line length when splitting only on LF
- logical max line length when splitting on CRLF/LF/CR
- files over 1000 logical lines
- minified/one-line files

The raw-byte scan must include at least the Goal 074-079 edit-driven/Application/WinForms/test paths listed in allowed-files.md.
