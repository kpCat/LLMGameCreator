# CODEX GOAL - Goal 010 Deterministic Content Generation At Scale

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
5. `docs/GOAL_010_CONTENT_GENERATION_AT_SCALE.md`
6. accepted Goal 005/006 semantic-selection seams only where needed for tags/provenance;
7. accepted Goal 007 world/region ids and Goal 008/009 package/runtime-adapter patterns directly needed for materialization and execution;
8. existing package/runtime definitions directly required by selected generated content.

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Starting Evidence

Start only because the user prompt explicitly provides:

```text
rule_pack_combat_faction_social_work_theft_artifact_verification passed
```

Goal 010 may create S085-S091 and must stop at:

```text
content_generation_at_scale_artifact_verification
```

Do not create S092, Goal 011 or post-Goal-010 work.

## Execute

Implement exactly:

```text
docs/GOAL_010_CONTENT_GENERATION_AT_SCALE.md
```

## Allowed Files

Primary allowed areas:

- `docs/GOAL_010_CONTENT_GENERATION_AT_SCALE.md`
- this wrapper
- a narrow new area under `src/LLMGameCreator.Application/Design/ContentGeneration/`
- focused tests under `tests/LLMGameCreator.Tests/Application/ContentGeneration/`
- one test-only real runtime adapter in that focused test area
- one product smoke file under `tests/LLMGameCreator.Tests/ProductSmoke/`
- compact JSON packs under `samples/content-generation-packs/`
- `.devflow/scripts/run-product-smoke.ps1`
- `.llmgc/procedural/content-generation-scale/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only after a focused test proves it necessary:

- the narrow existing Application/package validator seam containing the defect;
- the narrow existing Runtime service containing the defect;
- its focused regression test.

Do not edit any other file without reporting a blocker. Do not edit `.sln` or `.csproj`.

## Non-Negotiable Execution Shape

- Compact JSON data packs produce hundreds of deterministic concrete instances.
- No named-style production C# branches.
- No LLM/provider/RAG/Lua/media execution.
- Generated catalogs and packages are real structured objects, not report projections.
- Application default runtime adapter is unavailable/fail-closed.
- Test/product-smoke adapter invokes production runtime services.
- Runtime evidence uses the same generated ids and package hash.
- Repetition control is measurable and fail-closed.
- Invalid expectations never determine actual validity.
- One final gate only.

## Verification

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~RulePackGameplayFamily|FullyQualifiedName~ConnectedWorldTravel|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario content-generation-scale
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for mojibake, machine-specific nondeterminism and exact `S092|Goal 011|goal_011` markers, excluding Goal/task prohibition text.

## Stop Conditions

Stop instead of weakening acceptance when scale generation, exact package binding, real runtime execution, repetition control or save/load requires a public schema/project-reference change or a second Application gameplay simulator.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S092 or Goal 011.
- No WinForms/UI, Unity/export, asset/media generation, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report every item required by the primary Goal document, then stop at the single final gate without marking it passed.
