# Validation

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-083-visual-adult-layer-context-integration"
git diff --check
git diff --cached --check
```

Also scan changed Markdown/JSON files for:
- mojibake;
- absolute local paths;
- timestamps/heavy logs in tracked evidence;
- accidental prompt dumps;
- accidental binary/media additions.
