# CODEX TASK - S113A Alpha Runnable Build Artifact Path Length Hotfix

## Command

Run this file as a bounded hotfix task, not as a new `/goal`.

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
6. `tests/LLMGameCreator.Tests/Application/AlphaBuild/AlphaRunnableBuildAcceptanceTests.cs`
7. `tests/LLMGameCreator.Tests/ProductSmoke/*Alpha*` only if it exists
8. `.devflow/scripts/run-product-smoke.ps1`
9. current generated artifacts under `.llmgc/procedural/alpha-runnable-build/`

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Problem

Goal 013 stopped correctly at:

```text
alpha_unity_build_environment_blocker
```

However the generated artifact layout created Windows/Git-hostile paths under:

```text
.llmgc/procedural/alpha-runnable-build/source-evidence/
```

Example failing path from staging:

```text
.llmgc/procedural/alpha-runnable-build/source-evidence/frontier_survival/export/assets/item-icon-ui-graphic/asset-game-content-generation-frontier-survival-item-icon-ui-graphic-item-frontier-survival-032bdc46ab-062-caravan-icon-fallback-000.fixture
```

Git fails during "stage all changes" with:

```text
Filename too long
fatal: adding files failed
```

This is a real artifact correctness defect. Do not treat it as only a local Git configuration issue.

## Scope

Fix only Goal 013 / Alpha runnable build artifact family.

Allowed files:

- `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
- focused Alpha build tests under `tests/LLMGameCreator.Tests/Application/AlphaBuild/`
- Alpha product smoke under `tests/LLMGameCreator.Tests/ProductSmoke/` only if needed
- `.devflow/scripts/run-product-smoke.ps1` only if needed for existing Alpha smoke route
- `.llmgc/procedural/alpha-runnable-build/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Do not edit any other file without reporting a blocker.

## Required Fix

Replace long source-evidence/export/staging artifact file names with deterministic short names.

Required behavior:

- physical artifact file names must not embed full package ids, content ids, asset ids or long slugs;
- use a deterministic compact naming scheme, for example:

```text
source-evidence/<style>/assets/<category>/asset-000-<sha8>.fixture
source-evidence/<style>/game-data/package-<sha8>.json
source-evidence/<style>/export/export-manifest-<sha8>.json
```

- preserve the full original ids inside JSON manifests/report fields, not in file names;
- every copied/materialized source-evidence file must still have hash, byte count, category, source id and original id evidence;
- all manifest paths must remain repo-relative or artifact-root-relative, safe and deterministic;
- generated artifact cleanup must remove stale long-name files under `.llmgc/procedural/alpha-runnable-build/` before regeneration;
- do not require users to enable Git long paths as the primary fix.

## Path Length Contract

Add focused regression coverage proving:

- every generated file under `.llmgc/procedural/alpha-runnable-build/` has a relative path length of at most 160 characters from `.llmgc/procedural/alpha-runnable-build/`;
- every generated file name has length of at most 96 characters;
- no source-evidence physical file name contains full content ids such as `game-content-generation-` or `frontier-survival-item-icon-ui-graphic`;
- all shortened paths still resolve to physical files;
- hashes and byte counts still match actual bytes;
- invalid/fake/leak matrix still rejects causally;
- blocker remains `alpha_unity_build_environment_blocker` unless a real Unity Windows build is actually produced.

Use stricter limits if they fit existing local patterns.

## State And Gate Rules

- Keep Goal 013 terminal state at `alpha_unity_build_environment_blocker` unless a real Windows Unity player is produced and verified.
- Do not mark `alpha_runnable_windows_build_verification` as passed.
- Do not start S114, Goal 014 or any post-Goal-013 work.
- Do not remove the blocker facts; update them only to reflect the corrected artifact path layout.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~AlphaRunnableBuild|FullyQualifiedName~UnityRuntimeExport|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario alpha-runnable-build
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for:

- mojibake markers;
- paths longer than the contract above;
- absolute local paths;
- timestamps, GUIDs, machine names, temp paths and user names in deterministic artifacts;
- exact `S114|Goal 014|goal_014` markers, excluding explicit prohibition text.

After regeneration, confirm that a normal Git staging operation will no longer hit the previously failing path-length pattern. Do not run git commands unless the user explicitly asks; prove this by artifact path length checks instead.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S114 or Goal 014.
- No Runtime Preview dependency as Alpha proof.
- No false Windows executable, Unity build, launch or play claim.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report:

- changed files;
- old failing path pattern and new shortened pattern;
- max artifact relative path length and max file name length;
- stale long files removed/regenerated;
- report hash/build manifest hash after regeneration;
- verification command results;
- blocker/gate state;
- confirmation that S114/Goal 014 and forbidden areas were not started.
