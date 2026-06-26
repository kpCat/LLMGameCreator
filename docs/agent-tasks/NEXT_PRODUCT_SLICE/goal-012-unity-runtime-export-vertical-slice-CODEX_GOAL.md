# CODEX GOAL - Goal 012 Unity Runtime Export Vertical Slice

## Command

Run this file with:

```text
/goal
```

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/GOAL_012_UNITY_RUNTIME_EXPORT_VERTICAL_SLICE.md`
6. accepted Goal 010 content-generation seams only where needed for generated package ids and selected loop refs;
7. accepted Goal 011 asset pipeline seams only where needed for resolved asset manifest/files/hashes;
8. existing Unity archive/materialization/export services directly needed for the narrow export artifact;
9. existing package/runtime definitions directly required by selected loop validation.

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Starting Evidence

Start only because the user prompt explicitly provides:

```text
minimum_asset_pipeline_artifact_verification passed
```

Goal 012 may create S099-S105 and must stop at:

```text
unity_runtime_export_vertical_slice_artifact_verification
```

Do not create S106, Goal 013 or post-Goal-012 work.

## Execute

Implement exactly:

```text
docs/GOAL_012_UNITY_RUNTIME_EXPORT_VERTICAL_SLICE.md
```

## Allowed Files

Primary allowed areas:

- `docs/GOAL_012_UNITY_RUNTIME_EXPORT_VERTICAL_SLICE.md`
- this wrapper
- a narrow new area under `src/LLMGameCreator.Application/Design/UnityExport/` or `src/LLMGameCreator.Application/Design/UnityRuntimeExport/`
- existing `src/LLMGameCreator.Application/Composition/UnityArchive*` files only when the narrow export path directly reuses or fixes them
- focused tests under `tests/LLMGameCreator.Tests/Application/UnityExport/` or `tests/LLMGameCreator.Tests/Application/UnityRuntimeExport/`
- one product smoke file under `tests/LLMGameCreator.Tests/ProductSmoke/`
- compact fixtures under `samples/unity-runtime-export/` if required
- `.devflow/scripts/run-product-smoke.ps1`
- `.llmgc/procedural/unity-runtime-export/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only after a focused test proves it necessary:

- the smallest existing Application/package/runtime export or validation seam containing the defect;
- its focused regression test.

Do not edit any other file without reporting a blocker. Do not edit `.sln` or `.csproj`.

## Non-Negotiable Execution Shape

- Export is outside WinForms Runtime Preview.
- No false runnable Windows executable claim.
- No Unity Editor/build execution unless explicitly headless, noninteractive and fully reported; if unavailable, keep the proof at deterministic export artifact level.
- No external media/provider/LLM/RAG/Lua execution.
- Export files are real files with hashes and byte counts, not report projections.
- Selected loop refs and asset refs come from accepted Goal 010/011 evidence.
- Package/content/asset hashes are checked against the exported payload.
- Product smoke regenerates the export artifact and validates selected refs.
- Invalid expectations never determine actual validity.
- One final gate only.

## Verification

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityRuntimeExport|FullyQualifiedName~MinimumAssetPipeline|FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-runtime-export
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for mojibake, machine-specific nondeterminism and exact `S106|Goal 013|goal_013` markers, excluding Goal/task prohibition text.

## Stop Conditions

Stop instead of weakening acceptance when export materialization, selected-loop validation, asset payload validation or Unity-runtime contract validation requires a public schema/project-reference change, UI/Runtime Preview dependency, external provider execution, or a real Unity build.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S106 or Goal 013.
- No WinForms/UI work and no Runtime Preview dependency.
- No Unity Editor/build execution unless explicitly headless, noninteractive and fully reported.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report every item required by the primary Goal document, then stop at the single final gate without marking it passed.
