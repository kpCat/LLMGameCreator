# Forbidden files / areas

Do not change:

- Public GamePackage schema/contracts.
- `src/LLMGameCreator.Runtime/**`
- `src/LLMGameCreator.Runtime.Abstractions/**`
- `unity/**`
- `src/LLMGameCreator.Infrastructure/**` provider/LLM/RAG/media code.
- Lua/Scripting code.
- `generator-library/**`
- `.sln`, `.csproj`, lockfiles, package config, NuGet config.
- Broad shared infrastructure unless explicitly needed for registration in `CompositionRoot.cs`.
- Existing Goal 074-080 artifacts except transient validation-regeneration cleanup restored to HEAD.

Do not do:

- branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.
- External dependency additions.
- Public schema migration.
- Runtime or Unity implementation work.
- Hardcoded `success=true` proof without reading the actual projected package and playthrough artifacts from disk.
- A report-only smoke test.
- A parent WinForms god-form expansion.
