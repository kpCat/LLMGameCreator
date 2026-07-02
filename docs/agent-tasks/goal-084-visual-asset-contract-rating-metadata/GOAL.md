# Goal 084 — Visual Asset Contract & Rating Metadata Foundation

## Repo
https://github.com/kpCat/LLMGameCreator

## Working copy
`C:\Users\endim\LLMGameCreator\`

## Branch
`main`

## Codex reasoning
very high

## Primary objective
Implement the first real foundation for the visual/media stack: a BCL-only Application-side visual asset contract and rating/export metadata validator, based on Goal 083 visual/adult context integration.

This goal must not generate images or production media. It must define and validate the metadata layer that future procedural visual generators, part-pack compilers, provider quarantine, safe fallbacks, and adult/rating-gated extensions will depend on.

## Required preflight
1. Confirm branch is `main` and fetch `origin/main`.
2. Confirm latest main includes `1ee359f3 GREEN Goal 083 visual adult layer context integration`.
3. Confirm Goal 083 artifacts exist and are GREEN / accepted=false.
4. Confirm Goal 083 docs indexed the visual/adult docs and routed future gates.
5. Confirm no P0/P1 source-format evidence is active from Goal 082A.
6. Inspect current dirty state; do not stage/revert unrelated user changes.
7. Confirm this can be done without public schema, Runtime, Unity, provider/LLM/RAG/media, Lua/Scripting, generator-library, .sln/.csproj, media assets, real NSFW assets, prompt dumps, or new dependencies.

## Read first
- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md`
- `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`
- `docs/context/ADULT_VISUAL_LAYER_DOCUMENTATION_MANIFEST.md`
- `docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md`
- `docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md`
- `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
- `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
- `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md`
- `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-layer-context-integration-report.md`
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-doc-inventory.json`
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-policy-routing-matrix.json`
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/quality-gate-scan.json`
- If present, read `docs/deepsearch/` files, but do not require them. If deepsearch results are not present, proceed with conservative metadata-only contract.

## Allowed files / areas
- `src/LLMGameCreator.Application/Design/VisualAssetContractRatingMetadata/`
- `tests/LLMGameCreator.Tests/Application/VisualAssetContractRatingMetadata/`
- `tests/LLMGameCreator.Tests/ProductSmoke/VisualAssetContractRatingMetadataProductSmokeTests.cs`
- `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/`
- docs quartet, debt register, artifact-scope policy
- `docs/agent-tasks/goal-084-visual-asset-contract-rating-metadata/`

## Forbidden files / areas
Do not change public GamePackage schema, Runtime, Unity, providers, Lua, generator-library, .sln, .csproj, lock files, binary media, real NSFW assets, explicit prompt dumps, provider-output fixtures, or external dependencies. Do not branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Application-side contract models
Create BCL-only models for visual asset metadata, including:
- `VisualAssetContract`
- `VisualAssetSlot`
- `VisualAssetRecipeRef`
- `VisualPartPackRef`
- `VisualApprovedAssetRef`
- `VisualSafeFallbackRef`
- `VisualCandidateRecord`
- `VisualRating`
- `VisualExportPolicy`
- `VisualReviewStatus`
- `VisualProviderState`
- `VisualBodyPlanEligibility`
- validation/evidence result types.

Required metadata concepts: `assetSlot`, `rating`, `adultEnabled`, `safeFallbackRequired`, `candidateQuarantine`, `reviewStatus`, `exportPolicy`, `approvedAssetRef`, `recipeRef`, `partPackRef`, `provenanceRef`, `relativePath`, `sha256`, `seed`, `generatorVersion`.

Use neutral adult/rating metadata only. No explicit examples.

### 2. Validator
Reject at least:
- empty/invalid ids;
- absolute paths;
- prompt text as source of truth;
- safe/public export without safe-approved refs or deterministic fallback;
- adult-enabled slot without explicit rating/export policy;
- adult-enabled slot with public export but no fallback;
- provider candidate treated as approved;
- unreviewed/rejected promotion;
- approved ref missing hash/path/provenance;
- missing fallback when required;
- rating/export contradictions;
- age-ambiguous/non-sapient/non-eligible adult metadata through neutral eligibility flags;
- duplicate slot ids;
- unknown part-pack/recipe refs in strict mode.

### 3. Tiny deterministic metadata fixtures
Create metadata-only fixtures, no media files:
- `fantasy_overworld_tile_safe`
- `water_coast_biome_safe`
- `settlement_building_safe`
- `creature_bodyplan_safe`
- `humanoid_paperdoll_adult_capable_metadata_only`
- `tech_future_ui_panel_safe`

### 4. Evidence
Create `.llmgc/procedural/goal-084-visual-asset-contract-rating-metadata/` with:
- `visual-asset-contract-rating-metadata-report.md`
- `visual-asset-contract-catalog.json`
- `visual-rating-policy-matrix.json`
- `visual-contract-validation-matrix.json`
- `visual-contract-negative-proof.json`
- `source-document-lineage.json`
- `quality-gate-scan.json`

Evidence must prove Goal083 docs lineage, valid fixtures pass, invalid matrix rejects unsafe/fake/missing cases, no media/provider/schema/runtime/unity changes, deterministic report hash.

### 5. Product smoke
Add product smoke proving catalog/matrix/negative proof are read back, fixture coverage includes safe tile, water/coast, creature/bodyplan, UI and adult-capable metadata-only slot, invalid adult/export/provider cases are rejected, and no media files are created.

### 6. Docs/state
Update current-state docs, context index, queue, and debt register.
Manual gate: `visual_asset_contract_rating_metadata_verification required`.
Keep `accepted=false` until explicit user acceptance.

## Validation commands
```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualAssetContractRatingMetadata
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter VisualAssetContractRatingMetadataProductSmokeTests
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-084-visual-asset-contract-rating-metadata"
git diff --check
git diff --cached --check
```

Also scan changed files for mojibake, absolute local paths, timestamps/heavy logs, prompt dumps, binary/media additions.

## Stop / block conditions
Return BLOCKED if metadata contract requires schema/runtime/unity/provider/lua/generator-library changes, explicit content, media assets, or cannot satisfy artifact scope.
Return FAILED if build/tests regress due to this goal and cannot be repaired inside allowed files.

## Final report format
Report final status, commits, push, preflight, files, contract types, validation rules, fixtures, negative proof, artifacts, validations, artifact scope, hygiene, debt, final git status, git commands.

## Mandatory commit/push
Always commit and push to `origin/main`.
Commit message: `GREEN Goal 084 visual asset contract rating metadata`, `BLOCKED ...`, or `FAILED ...`.
