# Forbidden files / areas

Do not change:

- Public GamePackage schema or schema version.
- `src/LLMGameCreator.Runtime/**`
- `src/LLMGameCreator.Runtime.Abstractions/**`
- Unity project files, including `unity/LLMGameCreatorAlpha/**`.
- `AlphaRuntimeBootstrap.cs`.
- Infrastructure providers, LLM/RAG/media-provider code.
- Lua/Scripting.
- `generator-library/**`.
- `.sln`, `.csproj`, lockfiles, package references.
- Broad refactors outside the Goal 080 seam.
- Existing Runtime Preview page implementation unless the task becomes BLOCKED without a tiny bounded integration; prefer read-only usage of existing runtime-preview/projection services.
- Branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

Do not fake success:
- A report that only says `passed=true` is not enough.
- Product smoke must read generated disk files and prove a real bridge path.
- If a valid existing-schema package/runtime-preview bridge cannot be built without forbidden changes, commit/push a BLOCKED report instead of a synthetic GREEN.
