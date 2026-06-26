# CODEX GOAL - Goal 011 Minimum Deterministic Asset Pipeline

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
5. `docs/GOAL_011_MINIMUM_ASSET_PIPELINE.md`
6. accepted Goal 010 content-generation seams only where needed for generated ids, package hashes and provenance;
7. accepted Goal 008/009 package/runtime-adapter patterns directly needed for headless package/runtime smoke;
8. existing package/runtime definitions directly required by selected generated content and metadata binding;
9. existing asset-related Application types only if directly needed for import/fallback validation.

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Starting Evidence

Start only because the user prompt explicitly provides:

```text
content_generation_at_scale_artifact_verification passed
```

Goal 011 may create S092-S098 and must stop at:

```text
minimum_asset_pipeline_artifact_verification
```

Do not create S099, Goal 012 or post-Goal-011 work.

## Execute

Implement exactly:

```text
docs/GOAL_011_MINIMUM_ASSET_PIPELINE.md
```

## Allowed Files

Primary allowed areas:

- `docs/GOAL_011_MINIMUM_ASSET_PIPELINE.md`
- this wrapper
- a narrow new area under `src/LLMGameCreator.Application/Design/Assets/` or `src/LLMGameCreator.Application/Design/AssetPipeline/`
- focused tests under `tests/LLMGameCreator.Tests/Application/Assets/` or `tests/LLMGameCreator.Tests/Application/AssetPipeline/`
- one test-only real resolver/adapter in that focused test area if required
- one product smoke file under `tests/LLMGameCreator.Tests/ProductSmoke/`
- compact JSON packs and tiny fixtures under `samples/minimum-asset-pipeline/`
- `.devflow/scripts/run-product-smoke.ps1`
- `.llmgc/procedural/minimum-asset-pipeline/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only after a focused test proves it necessary:

- the narrow existing Application package/content metadata seam containing the defect;
- the narrow existing Runtime/package resolver containing the defect;
- its focused regression test.

Do not edit any other file without reporting a blocker. Do not edit `.sln` or `.csproj`.

## Non-Negotiable Execution Shape

- No external media/provider execution.
- No AI image/audio/music generation.
- Tiny checked-in fixtures and deterministic fallback bytes are allowed only to prove import/fallback mechanics.
- Asset requests are derived from real generated/package content ids, not prose report rows.
- Resolved assets are real files or real deterministic fallback outputs with hashes and byte counts.
- Package/content binding is structural and uses existing metadata seams.
- Default Application acceptance is unavailable/fail-closed when required concrete resolvers/adapters are missing.
- Product smoke proves artifact regeneration and selected asset refs resolving.
- Invalid expectations never determine actual validity.
- One final gate only.

## Verification

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~MinimumAssetPipeline|FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario minimum-asset-pipeline
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for mojibake, machine-specific nondeterminism and exact `S099|Goal 012|goal_012` markers, excluding Goal/task prohibition text.

## Stop Conditions

Stop instead of weakening acceptance when asset/content binding, file/hash validation, package smoke, fallback/import verification or deterministic artifacts require a public schema/project-reference change, UI/Unity launch, external provider execution or a second gameplay simulator.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S099 or Goal 012.
- No WinForms/UI, Unity/export, external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report every item required by the primary Goal document, then stop at the single final gate without marking it passed.
