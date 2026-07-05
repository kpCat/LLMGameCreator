# Goal 120A Clean Unity Editor Noise Empty-Status Hotfix

## Summary

Goal120A fixes the cleanup script UX bug where clean `git status --porcelain=v1 --untracked-files=all` output could be
treated as `$null` and fail binding into `Get-CleanupTargets`.

The script now normalizes status output into a preserved string array, allows empty status collections in
`Get-CleanupTargets`, and prints `Final status:` through an explicit final status array.

## Supported Commands

```text
.devflow/scripts/clean-unity-editor-noise.ps1 -DryRun
.devflow/scripts/clean-unity-editor-noise.ps1 -Apply
.devflow/scripts/clean-unity-editor-noise.cmd
```

## Scope

Changed scope is limited to the cleanup script, its focused source-contract test, current-state/queue docs,
artifact-scope policy and Goal120A compact evidence.

No Unity source/settings/package files, Runtime, public schema, provider, Lua, generator-library, solution/project files
or dependency files are part of this hotfix.

## Safety

- No broad `git clean`.
- Staged files are refused by default.
- Untracked removals remain bounded to Unity editor noise.
- `ProjectVersion.txt` remains the only restore target.
- `.cs`, `.json`, `.md`, `.unity` and `.prefab` are never removal targets.

## Validation

- `dotnet restore`: passed.
- `dotnet build .\LLMGameCreator.sln -c Debug`: passed, with existing warnings only.
- Filtered tests `CleanUnityEditorNoiseScript|AcceptedAlphaUnityPlayableProjection`: 6/6 passed.
- `.devflow/scripts/clean-unity-editor-noise.ps1 -DryRun`: exit 0, printed `Final status:`.
- `.devflow/scripts/clean-unity-editor-noise.ps1 -Apply`: exit 0, printed `Final status:`.
- `CurrentState` guard after context-index repair: 16/16 passed.
- `.devflow/scripts/check-current-goal.ps1`: passed after the context-index repair.
- `.devflow/scripts/check-spine-fast.ps1`: wrapper passed; historical product-smoke assertions are still printed by the wrapper.
- `.devflow/scripts/check-artifact-scope.ps1 -Scenario goal-120a-clean-unity-editor-noise-empty-status-hotfix`: accepted, 10/10 changed paths allowed.
- `git diff --check`: passed.
- `git diff --cached --check`: passed before staging.
- `git ls-files .llmgc/manual`: no tracked manual files.
- Mojibake markers in changed files: checked, no matches.
- Escaped Cyrillic/XML escaped Cyrillic in changed files: checked, no matches.
