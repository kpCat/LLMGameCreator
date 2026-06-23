Task id: PRODUCT_SLICE_023_UNITY_ARCHIVE_REVIEW_SNAPSHOT_V1

Goal:
Add a read-only deterministic review snapshot for an already-materialized `.llmgc/unity-archive` directory.

Executor:
Kilo Code first. Codex only if compile/test repair is needed.

Branch policy:
Work in the current working tree only.
Do not create branches.
Do not switch branches.
Do not merge/rebase/cherry-pick/push.
Do not run git commands.

Windows path policy:
Use repo-relative paths and PowerShell commands.
Do not use `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt`, or external container paths.

Required behavior:
1. Add read-only review models/service/markdown renderer.
2. Service reads only an existing `.llmgc/unity-archive`.
3. Service writes `production/archive-review.json` and `production/archive-review.md`.
4. Review aggregates validation, provider, fulfillment, request counts, diagnostics and source file references.
5. Determinism: no timestamps, no absolute paths, stable ordering, repeated unchanged review byte-identical.
6. Missing archive/core files do not crash; they return diagnostics and `MissingArchive`.

Forbidden:
No Unity, provider, LLM, generator, Lua, Runtime, WinForms, GamePackage schema, generator-library, .sln or .csproj changes.

Required commands:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~UnityArchiveReviewSnapshot"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-review-snapshot
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```
