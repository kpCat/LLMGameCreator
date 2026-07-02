# Validation commands

Run these commands from repo root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenGamePackageRuntimePreviewBridge
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenGamePackageRuntimePreviewPlaythrough
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenGamePackageRuntimePreviewPlaythroughProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-081-edit-driven-gamepackage-runtime-preview-playthrough"
```

If `check-all.ps1` appears to timeout through the tool wrapper but its child testhost is still consuming CPU, use the existing project run-log pattern and continue until the wrapper passes/fails. Do not report GREEN from a timed-out wrapper.

Final hygiene checks:

- No absolute local paths in Goal 081 evidence.
- No timestamp-like values in Goal 081 evidence.
- No heavy logs or scratch/tamper files tracked.
- Mojibake marker scan over changed text files.
- `git diff --check` and `git diff --cached --check` before commit.
- Confirm `AlphaRuntimeBootstrap.cs` line count/hash unchanged.
