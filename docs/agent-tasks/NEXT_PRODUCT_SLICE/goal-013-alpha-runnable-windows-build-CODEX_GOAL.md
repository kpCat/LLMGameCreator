# CODEX GOAL - Goal 013 Alpha Runnable Windows Build

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
5. `docs/GOAL_013_ALPHA_RUNNABLE_WINDOWS_BUILD.md`
6. accepted Goal 010 content-generation seams only where needed for generated package ids and selected loop refs;
7. accepted Goal 011 asset pipeline seams only where needed for resolved asset manifest/files/hashes;
8. accepted Goal 012 Unity runtime export seams only where needed for export manifest, payload files and validation contracts;
9. existing Unity project/template/build scripts directly needed to produce a real Windows player;
10. existing package/runtime definitions directly required by selected loop validation.

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Starting Evidence

Start only because the user prompt explicitly provides:

```text
unity_runtime_export_vertical_slice_artifact_verification passed
```

Goal 013 may create S106-S113 and must stop at:

```text
alpha_runnable_windows_build_verification
```

If a real Windows Unity player cannot be produced or launched because of a concrete environment/tooling problem, stop at:

```text
alpha_unity_build_environment_blocker
```

Do not create S114, Goal 014 or post-Goal-013 work.

## Execute

Implement exactly:

```text
docs/GOAL_013_ALPHA_RUNNABLE_WINDOWS_BUILD.md
```

## Allowed Files

Primary allowed areas:

- `docs/GOAL_013_ALPHA_RUNNABLE_WINDOWS_BUILD.md`
- this wrapper
- a narrow new area under `src/LLMGameCreator.Application/Design/AlphaBuild/` or `src/LLMGameCreator.Application/Design/UnityAlphaBuild/`
- existing `src/LLMGameCreator.Application/Design/UnityRuntimeExport/` files only when the narrow Alpha build path directly reuses or fixes Goal 012 export evidence handling
- existing `src/LLMGameCreator.Application/Composition/UnityArchive*` files only when the narrow build staging path directly reuses or fixes them
- focused tests under `tests/LLMGameCreator.Tests/Application/AlphaBuild/` or `tests/LLMGameCreator.Tests/Application/UnityAlphaBuild/`
- one product smoke file under `tests/LLMGameCreator.Tests/ProductSmoke/`
- compact build fixtures under `samples/alpha-runnable-build/` if required
- repository-local Unity project/template/build scripts only if already present and directly needed for a real Windows build
- `.devflow/scripts/run-product-smoke.ps1`
- `.llmgc/procedural/alpha-runnable-build/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only after a focused test or build failure proves it necessary:

- the smallest existing Application/package/runtime export or validation seam containing the defect;
- the smallest existing Unity build/materialization seam containing the defect;
- its focused regression test.

Do not edit any other file without reporting a blocker. Do not edit `.sln` or `.csproj` unless strictly required and explicitly justified by a focused failing test/build failure.

## Non-Negotiable Execution Shape

- Alpha proof is a real Windows Unity player folder/executable, or a real blocker.
- Runtime Preview is not proof.
- Export-only artifacts are not proof.
- Copied expectation reports are not proof.
- Unity Editor/CLI may be executed only for real local build/launch work, with command/version/logs fully reported.
- No external media/provider/LLM/RAG/arbitrary Lua/generator-library execution.
- Three style candidates must come from accepted prior evidence.
- Selected loop refs, asset refs and export refs must resolve through physical payloads.
- Build files are real files with hashes and byte counts.
- Product smoke validates build artifacts or blocker evidence.
- Invalid expectations never determine actual validity.
- One final gate only, except the real blocker allowed above.

## Verification

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~AlphaRunnableBuild|FullyQualifiedName~UnityRuntimeExport|FullyQualifiedName~MinimumAssetPipeline|FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario alpha-runnable-build
.\.devflow\scripts\check-all.ps1
```

If Unity build tooling is available and used, also run/report the exact build and launch commands required by the Goal document.

Also scan changed/generated files for mojibake, machine-specific nondeterminism and exact `S114|Goal 014|goal_014` markers, excluding Goal/task prohibition text.

## Stop Conditions

Stop instead of weakening acceptance when a runnable Alpha requires a public schema redesign, broad WinForms/UI work, Runtime Preview proof, external provider/media/LLM/RAG/Lua/generator-library execution, fabricated build output or copied expectation reports.

Use `alpha_unity_build_environment_blocker` only for concrete Unity installation/module/licensing/build-host failures. Include exact user steps and commands.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S114 or Goal 014.
- No Runtime Preview dependency as Alpha proof.
- No false Windows executable, Unity build, launch or play claim.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits unless a focused failing test/build proves it is strictly required and the final report calls it out.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report every item required by the primary Goal document, then stop at the single final gate without marking it passed, or at the real blocker without creating post-Goal-013 work.
