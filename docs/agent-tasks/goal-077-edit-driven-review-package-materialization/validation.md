# Validation commands

Run the following commands from repository root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenPlayablePreviewRefresh
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenPlayableReviewPackageMaterialization
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter EditDrivenPlayableReviewPackageMaterializationProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-077-edit-driven-review-package-materialization"
```

If check-all is slow due old Unity proof tests, do not kill it early. Poll logs/processes and report the actual final result. If it exceeds a bounded local environment limit after clear evidence that Goal 077 focused/product smoke tests pass and the old slow area is unrelated, commit as BLOCKED with evidence instead of claiming GREEN.

Also run:

- A changed-file mojibake marker scan over changed text files.
- An absolute path/timestamp/heavy-log scan over Goal 077 tracked evidence.
- A source-format/readability scan proving no new one-line/minified C# files, no C# line over 500, and no new C# file over 1000 lines.
- A forbidden-path scan over the final staged diff.
