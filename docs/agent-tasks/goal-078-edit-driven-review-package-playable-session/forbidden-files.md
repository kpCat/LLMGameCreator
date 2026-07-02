# Forbidden files / areas

Do not change these unless the task is marked BLOCKED:

- Public GamePackage schema files.
- `src/LLMGameCreator.Runtime/**`
- `src/LLMGameCreator.Runtime.Abstractions/**`
- `unity/**`
- `src/LLMGameCreator.Infrastructure/**` provider / LLM / RAG / media-provider code.
- Lua / scripting areas.
- `generator-library/**`
- `.sln` files.
- `.csproj` files.
- Existing Goal 076 or Goal 077 Application service files, except read-only inspection. In particular, do not keep appending to `EditDrivenPlayableReviewPackageMaterializationEvidenceService.cs`; it is already near the line-count ceiling.
- Broad refactors, branch management, rebase, merge, cherry-pick, reset, stash, clean, force-push.
- New external dependencies.

Do not mutate `AlphaRuntimeBootstrap.cs`; only read and record its size/hash baseline in Goal 078 quality evidence.
