# Validation commands

Run these commands from the repository root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter SchemaDrivenCampaignEditValidateApplyLoop
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter EditDrivenPlayablePreviewRefresh
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter EditDrivenPlayablePreviewRefreshProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
```

Also run the artifact-scope validation used by the repo if discoverable. If the exact command is not discoverable, state that in the final report and run the closest available artifact-scope check.

Expected minimum behavior:

- restore passes
- build passes with 0 warnings / 0 errors
- focused Goal 075 filter remains green
- new Goal 076 focused tests pass
- new Goal 076 product smoke passes
- CurrentState docs tests pass
- check-all passes
- artifact scope passes for explicit Goal 076 changed paths

If global artifact scope is affected by c8343e8 docs-only planning files, do not hide it. Either repair policy in a bounded way if appropriate, or report it as P2/P3 docs/artifact-scope debt if not directly part of Goal 076.
