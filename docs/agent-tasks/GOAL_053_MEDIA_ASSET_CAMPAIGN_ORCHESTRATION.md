# /goal task — Goal 053 Media Asset Campaign Orchestration And Binding Dry Run

## Assignment metadata

Repository:

```text
https://github.com/kpCat/LLMGameCreator
```

Working copy:

```text
C:\Users\endim\LLMGameCreator\
```

Branch:

```text
main
```

Composite goal:

```text
goal_053_media_asset_campaign_orchestration
Goal 053 — Media Asset Campaign Orchestration And Binding Dry Run
```

Codex reasoning level:

```text
very high
```

Expected manual gate after implementation:

```text
media_asset_campaign_orchestration_verification required
```

## Important process rule

This task must end with a commit and push to `origin/main` even if the result is `GREEN`, `BLOCKED` or `FAILED`.

Use honest commit messages:

```text
GREEN Goal 053 media asset campaign orchestration
BLOCKED Goal 053 media asset campaign orchestration
FAILED Goal 053 media asset campaign orchestration
```

Do not pretend a blocked/failed task is green. Do not mark the Goal 053 manual gate passed.

## Purpose

Accept Goal 047 by user handoff and implement the first media-campaign orchestration layer after the full generator without-media dry run.

This is not a provider integration task. The goal is to make media production governable before any real image/audio generation exists:

- media request queue;
- style/prompt/input skeletons;
- license/provenance ledger;
- candidate quarantine;
- review/promotion decisions;
- deterministic fixture media files;
- binding manifest to generated family/template/runtime ids;
- preview/export media payloads;
- invalid/fake/leak diagnostics.

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_053_MEDIA_ASSET_CAMPAIGN_ORCHESTRATION_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_053_MEDIA_ASSET_CAMPAIGN_ORCHESTRATION.md`
8. Goal 047 artifacts under `.llmgc/procedural/goal-047-full-generator-without-media-dry-run/`
9. Goal 043 artifacts under `.llmgc/procedural/goal-043-multi-family-generated-template-vertical-slice/`
10. Goal 040 artifacts under `.llmgc/procedural/goal-040-chunked-runtime-preview-export-multifamily-smoke/`
11. Existing Application design seams around:
    - `src/LLMGameCreator.Application/Design/FullGeneratorWithoutMediaDryRun/**`
    - `src/LLMGameCreator.Application/Design/MultiFamilyGeneratedTemplateVerticalSlice/**`
    - `src/LLMGameCreator.Application/Design/ChunkedRuntimePreviewExportSmoke/**`
    - existing asset-related Application seams if present: search narrowly for `Asset`, `AssetCatalog`, `UnityRuntimeExport`, `MinimumAsset`, `media`.
12. Existing tests/product smoke style for the above seams.

Do not read the entire repository unless a narrow search proves a dependency is elsewhere.

## Preflight acceptance and state update

Record Goal 047 accepted by user handoff before Goal 053 implementation:

```text
full_generator_without_media_verification passed
```

Do not mark Goal 031 or Goal 032 passed. Preserve them as produced-for-review/not passed if current docs do so.

Do not start any future goal beyond Goal 053.

## Allowed files / areas

You may create/edit:

```text
docs/GOAL_053_MEDIA_ASSET_CAMPAIGN_ORCHESTRATION_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_053_MEDIA_ASSET_CAMPAIGN_ORCHESTRATION.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/agent-tasks/GOAL_053_MEDIA_ASSET_CAMPAIGN_ORCHESTRATION.md
docs/agent-tasks/GOAL_053_LAUNCHER.txt
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/MediaAssetCampaignOrchestration/**
tests/LLMGameCreator.Tests/Application/MediaAssetCampaignOrchestration/**
tests/LLMGameCreator.Tests/ProductSmoke/MediaAssetCampaignOrchestrationProductSmokeTests.cs
.llmgc/procedural/goal-053-media-asset-campaign-orchestration/**
```

If a clearly equivalent existing Application folder exists, use local naming style, but do not sprawl outside the allowed scope.

## Forbidden files / areas

Do not touch:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
src/LLMGameCreator.Generation/** provider/LLM/RAG paths
src/LLMGameCreator.Scripting/**
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
*.Designer.cs
```

No external dependencies. No media provider calls. No network. No real AI image/audio generation. No ComfyUI/Fooocus/Stability/Freesound/OpenGameArt integration. No generated final prose that claims to be final in-game text. No Runtime/UI/Unity/GamePackage schema changes.

## Exact behavior

### 1. Source manifest from accepted facts

Build a deterministic source manifest that consumes compact facts from Goal 047 and, where needed, Goal 043/040.

It must identify:

- three game families;
- selected family dry-run ids;
- preview/export payload ids;
- scenario/style ids;
- generated runtime/template ids that need media;
- existing source artifact hashes/paths, without copying heavy source JSON into the new artifacts.

### 2. Media slot catalog

Create a deterministic media slot catalog with at least these slot categories:

```text
world_key_art
region_tile_or_background
npc_portrait
species_or_archetype_portrait
item_icon
quest_or_event_icon
ui_panel_skin
sfx_interaction
sfx_combat_or_hazard
ambient_loop
music_stinger
export_placeholder_bundle
```

Each slot definition must have:

- stable id;
- target family/families;
- media kind: image/audio/ui/bundle;
- dimensions or duration/rate hints where relevant;
- semantic/style tags;
- allowed source types: fixture/manual/import/provider_later;
- review requirements;
- license policy requirement;
- binding target kind;
- fallback/placeholder behavior.

### 3. Media request queue

Generate a request queue across the three families:

- map_panel_rpg;
- survival_sandbox;
- first_person_grid_dungeon.

Minimum request counts:

- at least 8 requests per family;
- at least 30 total requests;
- at least image + audio + UI/bundle categories across all families;
- include a metamodule/world-scale stress summary without generating one file per 112 species/archetype slot unless explicitly compacted.

Each request must include:

- stable request id;
- family id;
- scenario/style id;
- target generated id;
- target artifact family/kind;
- media slot id;
- prompt/input skeleton fields, not final provider prompt text;
- semantic tags/features;
- required provenance policy;
- budget hint;
- deterministic priority/order;
- status: requested/blocked/future-provider/fixture-ready.

### 4. License/provenance ledger

Create a policy and ledger for candidate sources:

- fixture-generated-by-repo;
- manual-user-provided;
- imported-cc0;
- imported-cc-by;
- imported-share-alike-or-gpl-risk;
- provider-generated-with-model-license;
- unknown/no-license.

Decision expectations:

- fixture repo-generated assets can be promoted as fixture assets only;
- CC0 can be acceptable with source record;
- CC-BY requires attribution record;
- share-alike/GPL-risk must be quarantined/blocked unless explicitly allowed later;
- unknown/no-license must be rejected;
- provider output must require model/provider/license/run metadata and is not allowed in Goal 053.

### 5. Candidate quarantine and review/promotion

Create deterministic candidate records for:

- valid fixture candidates;
- manual/import/provider-later placeholders;
- invalid/fake/leak candidates.

The review/promotion engine must produce decisions:

```text
promote_fixture
needs_manual_review
blocked_license
blocked_missing_provenance
blocked_provider_not_configured
blocked_leak
blocked_mismatch
```

Do not auto-promote provider/manual/import assets as final content in Goal 053.

### 6. Deterministic fixture media files

Produce small deterministic fixture files under the Goal 053 artifact folder, for example:

```text
fixtures/images/*.txt or *.json
fixtures/audio/*.txt or *.json
fixtures/ui/*.txt or *.json
```

Preferred: lightweight textual fixture descriptors rather than binary PNG/WAV unless a simple stable writer already exists.

The fixture inventory must record:

- file path relative to artifact folder;
- byte length;
- SHA-256 hash or existing repo hash style;
- media kind;
- bound request id;
- bound generated target id;
- fixture status.

No fake claim that these are real final images/audio.

### 7. Binding manifest and preview/export media payloads

Build a binding manifest that links promoted fixture candidates to generated target ids from the family dry-runs.

Build preview/export media payload summaries proving:

- every family has media bindings;
- every family has at least one image-like and audio-like fixture binding;
- fallback behavior is explicit for unfilled slots;
- package/runtime/export payloads are not mutated;
- GamePackage schema is not changed;
- Unity/export is not modified.

### 8. Invalid/fake/leak matrix

Cover at minimum:

- duplicate media request id;
- unknown family id;
- unknown generated target id;
- unknown media slot id;
- invalid media kind;
- missing required provenance;
- unknown/no-license candidate accepted attempt;
- CC-BY candidate without attribution;
- share-alike/GPL-risk candidate auto-promotion attempt;
- provider candidate without model/license/run metadata;
- final prose/final artwork claim in fixture candidate;
- path traversal in fixture path;
- external absolute path in artifact;
- network URL treated as downloaded asset;
- provider/LLM/RAG call claim;
- Runtime/UI/Unity/GamePackage schema mutation claim;
- nondeterministic ordering;
- fake source artifact hash/path;
- self-promotion without review trace.

Each invalid case must produce a stable diagnostic code and matched expectation.

### 9. Evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-053-media-asset-campaign-orchestration/
```

Required files:

```text
media-campaign-source-manifest.json
media-slot-catalog.json
media-request-queue.json
media-style-policy.json
media-license-provenance-ledger.json
media-candidate-quarantine.json
media-review-promotion-ledger.json
media-binding-manifest.json
media-fixture-file-inventory.json
preview-export-media-payloads.json
invalid-media-diagnostics-matrix.json
media-asset-campaign-orchestration-report.md
```

No timestamps unless an existing deterministic convention requires them. No absolute machine paths. No heavy logs.

### 10. Docs/state/queue

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected state:

- Goal 047 accepted by user handoff: `full_generator_without_media_verification passed`.
- Goal 053 produced for review: `media_asset_campaign_orchestration_verification required`.
- Goal 031/032 remain not passed if current docs preserve that status.
- Recommended next work should be a media adapter or generated playable-with-fixture-media step, not a pure documentation task.

## Tests

Add focused tests proving:

- catalog validates;
- request queue covers three families and required media categories;
- license/provenance policy blocks risky candidates;
- review/promotion decisions are deterministic and causal;
- fixture files are created, hashed and bound;
- preview/export media payloads consume bindings without GamePackage/Runtime/UI/Unity mutation;
- invalid/fake/leak matrix passes;
- evidence artifacts are directly inspectable;
- product smoke writes all required artifacts.

Suggested tests:

```text
MediaAssetCampaignCatalogTests
MediaAssetRequestQueueTests
MediaAssetLicenseProvenanceTests
MediaAssetReviewPromotionTests
MediaAssetFixtureBindingTests
MediaAssetCampaignEvidenceTests
MediaAssetCampaignInvalidMatrixTests
MediaAssetCampaignOrchestrationProductSmokeTests
```

## Validation commands

Use PowerShell from repo root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~MediaAssetCampaign|FullyQualifiedName~Goal053"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~MediaAssetCampaignOrchestrationProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal053|FullyQualifiedName~MediaAsset"

.\.devflow\scripts\check-all.ps1
```

Run the final artifact scope guard if this repo has the established script/policy entry pattern. Update `.devflow/artifact-scope/artifact-scope-policy.json` only for the Goal 053 artifact folder and changed files.

Also inspect artifacts directly:

```powershell
Get-ChildItem .\.llmgc\procedural\goal-053-media-asset-campaign-orchestration -File -Recurse | Sort-Object FullName | Select-Object FullName,Length
```

Scan changed files for mojibake markers using the local repo pattern.

## Pre-authorized bounded repairs

Allowed if needed:

1. Update stale current-state/handoff guard tests that hardcode the previous current gate, but only when the repair preserves historical goal-specific assertions and checks current-state consistency dynamically.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from `HEAD` if `check-all.ps1` mutates them outside Goal 053 scope.
3. Add/update only the artifact-scope policy entry needed for Goal 053.

Do not use `git reset`, `git clean`, `git stash`, `git checkout`, `git switch`, `git merge`, `git rebase`, `git cherry-pick` or force push.

## Stop/block conditions

Commit/push a `BLOCKED` final state if:

- real provider/media generation is required;
- external dependency is required;
- GamePackage/Runtime/UI/Unity schema/code changes are required;
- source Goal 047 artifacts are missing and cannot be consumed causally;
- fixture media files cannot be produced deterministically;
- check-all fails and cannot be repaired within bounded repairs;
- evidence would require absolute paths, network downloads or fake media claims.

## Git policy

Always commit and push final state to `origin/main`.

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit changed files>
git diff --stat --cached
git add <explicit allowed paths>
git commit -m "GREEN Goal 053 media asset campaign orchestration"
git commit -m "BLOCKED Goal 053 media asset campaign orchestration"
git commit -m "FAILED Goal 053 media asset campaign orchestration"
git push origin main
```

Forbidden:

```text
git checkout
git switch
git reset
git clean
git stash
git merge
git rebase
git cherry-pick
git push --force
```

## Final report format

Report in Russian:

```text
Goal 053 выполнен / остановлен
Status: GREEN / BLOCKED / FAILED
Gate: media_asset_campaign_orchestration_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<campaign/source/queue/license/quarantine/review/fixtures/bindings/payloads/evidence>

Evidence artifacts:
<список required files>

Media proof:
familyCount=<n>
requestCount=<n>
fixtureFileCount=<n>
bindingCount=<n>
realProviderCalled=false
realMediaGenerationCalled=false

Проверки:
<commands/results>

Invalid/fake/leak matrix:
<coverage summary>

Bounded repairs:
<none or exact list>

Git:
<commit hash and push result>

Ограничения:
No real provider/media generation, no network/import, no GamePackage/Runtime/UI/Unity/provider/LLM/RAG/Lua/generator-library changes.

Следующий разумный шаг:
<one paragraph>
```
