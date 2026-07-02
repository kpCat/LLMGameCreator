# Forbidden files and areas

Do not change unless the task reaches a BLOCKED status and explains why:

- Public GamePackage schema/contracts.
- `src/LLMGameCreator.Runtime/**`
- `src/LLMGameCreator.Runtime.Abstractions/**`
- Infrastructure provider/LLM/RAG/media provider code.
- Lua/Scripting.
- `generator-library/**`
- `.sln` / `.csproj` / lockfiles.
- Existing Unity runtime scripts except the explicitly allowed new `EditDrivenGamePackageHandoffProbe.cs` file.
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` must remain read-only/no-change. Record its before/after hash and line count.
- Do not add external dependencies.
- Do not run branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.
- Do not rewrite prior commits or change the Goal 080/081 artifact `accepted=false` evidence.
