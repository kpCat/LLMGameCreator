# Validation commands

Run these commands before final status:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter EditDrivenPlayableReviewPackageMaterialization
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter EditDrivenReviewPackagePlayableSession
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter EditDrivenReviewPackagePlayableSessionProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-078-edit-driven-review-package-playable-session"
```

If `check-all.ps1` exceeds a short tool timeout but the testhost is still consuming CPU, rerun with a longer timeout or run it as a monitored background process and report the real result. Do not claim GREEN based on a wrapper timeout.

Also run direct hygiene checks:

- Changed C# files: no minified/one-line files, max line length <= 500, no file over 1000 lines.
- Goal 078 evidence: no absolute local paths, timestamp-like values, heavy logs, or scratch tamper files.
- Changed text files: mojibake scan clean.
- `AlphaRuntimeBootstrap.cs`: size/hash inspected and unchanged.
