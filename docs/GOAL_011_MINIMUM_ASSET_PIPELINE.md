# Goal 011 - Minimum Deterministic Asset Pipeline

## Purpose

Start only after the user/assistant explicitly confirms:

```text
content_generation_at_scale_artifact_verification passed
```

Goal 010 proved deterministic content generation at scale, package materialization and real-runtime execution. Goal 011 must prove the next product chain:

```text
generated package/content ids
-> deterministic asset slot requests
-> checked-in/local fixture imports or deterministic fallbacks
-> package asset binding metadata
-> validation, reports and product-smoke evidence
```

This goal is not about AI media quality. It is the minimum reliable pipeline that can later accept ComfyUI, SFX or music providers without changing gameplay/package correctness.

## Final Gate

Stop at exactly one final gate:

```text
minimum_asset_pipeline_artifact_verification
```

Do not create S099, Goal 012 or post-Goal-011 work.

## Product Slices

- S092: record Goal 010 gate and define minimum asset slot/source acceptance contracts.
- S093: deterministic asset request expansion from generated package/content ids.
- S094: local fixture import and deterministic fallback materialization.
- S095: bind resolved assets to package/content metadata without public schema redesign.
- S096: validate asset hashes, paths, media types, references, budgets and reproducibility.
- S097: headless package/runtime smoke proving selected asset references resolve with generated content.
- S098: invalid/fake/leak rejection, product smoke, artifacts, state handoff and final verification.

## Architecture Boundary

### Asset pipeline owns files and references only

The pipeline may create deterministic asset request, manifest, resolution and artifact models in Application. It must not create a second gameplay simulator and must not redesign public GamePackage/runtime schemas.

Allowed asset categories for this goal:

- tile or region image slots;
- NPC portrait slots;
- item/icon/UI graphic slots;
- short sound-effect slots;
- music or ambience slots.

The accepted proof may use tiny checked-in fixtures and deterministic fallback bytes. It must not call external media generators, LLM/RAG providers, audio services, image services, Unity, Lua or arbitrary scripts.

### Future provider boundary

Provider execution is intentionally out of scope. Goal 011 should leave a clear provider-ready contract, but acceptance must pass with local fixtures/fallbacks only.

Required fail-closed behavior:

- unknown provider/source kinds reject deterministically;
- missing required local files reject unless an explicit fallback policy permits fallback;
- fallback use is reported structurally and never hidden as imported media;
- copied report strings or expectation-only evidence cannot make acceptance pass.

## Reference Asset Data

Provide compact reference data outside `generator-library`, for example under:

```text
samples/minimum-asset-pipeline/
```

Reference data must be small and deterministic. It may include:

- JSON asset-pack/source declarations;
- tiny PNG/WAV/text-like fixture files if needed;
- seed/options files used by focused tests.

Do not include large binary media. Keep fixtures intentionally tiny.

## Scale Contract

For the valid matrix, prove at minimum:

- 3 style/content packs or generated-package inputs;
- at least 90 resolved asset slots total;
- at least 12 tile/region graphic slots;
- at least 12 portrait slots;
- at least 12 item/icon/UI graphic slots;
- at least 12 sound-effect slots;
- at least 3 music/ambience slots;
- imported fixture assets and deterministic fallback assets both appear in the matrix;
- every resolved slot records source category, content id, media type, relative path, hash, byte count and fallback/import status.

Counts may exceed these minima within explicit maximum budgets. Requests above configured safe caps must reject rather than allocate unboundedly.

## S092 - Contract And Gate Record

Define narrow versioned Application contracts for:

- asset source pack;
- asset request;
- asset slot category;
- asset source kind;
- resolved asset;
- manifest/report evidence.

Required validation:

- schema/version and pack id;
- stable unique ids;
- known source kinds and media types;
- positive bounded budgets;
- required content ids exist in the generated/package catalog used for the run;
- path safety for every declared source and output;
- no absolute paths in deterministic artifacts;
- no `..`, drive roots, URI schemes, executable payloads, scripts, provider credentials or command strings;
- fallback policy is explicit per category;
- malformed JSON produces deterministic relative-path diagnostics, not unhandled exceptions.

## S093 - Deterministic Asset Request Expansion

Build deterministic asset slot requests from generated/package content.

Required behavior:

- identical input/seed/options produce byte-stable request order, manifest and hashes;
- different seed/options produce meaningful source choices while preserving constraints;
- IDs are stable, safe and derived from content id, category, source id and ordinal provenance;
- enumeration order is explicitly sorted before hashing/serialization;
- no timestamps, GUIDs, machine names, temp paths or hash-randomized order in deterministic artifacts;
- every request records exact originating content id and category;
- each loop has explicit attempt/budget limits;
- missing eligible sources produce stable diagnostics.

Do not hard-code named style/project/content ids in production logic.

## S094 - Local Import And Deterministic Fallback

Implement local fixture import and deterministic fallback materialization.

Required behavior:

- imported fixtures are copied or referenced through safe repo-relative artifact paths;
- hashes and byte counts are computed from actual bytes;
- media type is validated by signature or a structured deterministic rule appropriate for the fixture;
- fallback assets are deterministic and category-specific;
- fallback output hashes are stable on replay;
- fallback use is allowed only when the source declaration permits it;
- corrupt, empty or wrong-media fixtures reject causally.

This goal may create deterministic placeholder bytes/files. It must not execute external media generation or contact providers.

## S095 - Package And Content Binding

Bind resolved asset references to generated/package content through the narrowest existing metadata seam.

Required package/content audit:

- every selected NPC portrait resolves to an existing generated/package NPC/entity id;
- every tile/region graphic resolves to an existing map/region/tile/content id;
- every item/icon/UI graphic resolves to an existing generated/package item, action or UI slot id;
- every sound/music reference resolves to a declared event, interaction, ambience or content slot;
- binding preserves content id, slot id, media type, hash and relative artifact path;
- package validator remains clean for valid packages;
- no public GamePackage/runtime schema redesign;
- identical input produces identical package/manifest hash;
- three input packs produce meaningfully different asset manifests without named-style production branches.

If existing package models cannot honestly carry asset metadata, use a narrow generated-content/provenance metadata seam. Do not modify public schema just to pass this goal.

## S096 - Asset Validation And Reproducibility

Validation must be structural and fail-closed:

- all resolved files exist in the artifact output when required;
- all relative paths stay under the artifact root;
- file hash, byte count and media type match the manifest;
- all package/content references resolve;
- slot category and media type are compatible;
- source pack hash and generated-content/package hash match the manifest;
- replay with the same input is byte-stable;
- intentional fixture changes alter the manifest hash;
- fallback/import distributions are reported.

No absolute local paths, timestamps, GUIDs, temp paths or user names may enter deterministic artifacts.

## S097 - Headless Runtime/Package Smoke

Add focused tests and product smoke proving asset references are not only report projections.

Required evidence:

- accepted packages/content are built from the same ids used by the asset manifest;
- selected generated loops from Goal 010-style content still validate after asset binding;
- selected runtime/package smoke can resolve attached asset refs for NPC/dialogue/event/item/region slots;
- missing asset files, mismatched hashes and cross-pack leakage reject causally;
- default Application acceptance remains fail-closed when required concrete resolvers/adapters are not supplied.

Do not launch UI, Unity, providers, image/audio tools or Lua.

## S098 - Acceptance, Artifacts And State

Required valid matrix:

- three valid reference inputs;
- same-input same-seed replay;
- same-input multi-seed or source-variation evidence;
- imported fixture and fallback resolutions;
- package/content binding audit;
- product smoke route;
- no public schema/project-file changes.

Required invalid/fake/leak matrix:

- unknown source kind;
- unsupported media type;
- missing fixture without fallback permission;
- wrong media type or corrupt fixture;
- path traversal and absolute path source;
- executable/script/provider payload injection;
- duplicate slot ids;
- unresolved content id;
- mismatched file hash;
- tampered package/content hash;
- over-budget request;
- cross-pack asset leakage;
- copied expectation/report evidence without actual files;
- unavailable/default adapter or resolver.

Create deterministic artifacts under:

```text
.llmgc/procedural/minimum-asset-pipeline/
```

Required files:

- `minimum-asset-pipeline-report.json`
- `minimum-asset-pipeline-report.md`
- `minimum-asset-pipeline-verification.md`

The report must include:

- manual gate `minimum_asset_pipeline_artifact_verification`;
- completed slices S092-S098;
- source pack hashes;
- package/content hashes;
- asset request counts by category;
- import/fallback counts by category;
- byte counts and manifest hash;
- valid/invalid matrix results and causal diagnostics;
- external execution flags all false;
- project/public schema change flags false.

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Record Goal 011 as complete up to the final gate, leave:

```text
minimum_asset_pipeline_artifact_verification: required
```

Do not recommend, create or start Goal 012.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~MinimumAssetPipeline|FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~RulePackCombatFactionSocialWorkTheft|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario minimum-asset-pipeline
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for:

- mojibake markers;
- absolute local paths;
- nondeterministic timestamps or GUIDs in deterministic artifacts;
- `S099|Goal 012|goal_012` outside prohibition text.

## Stop Conditions

Stop with a blocker report instead of weakening acceptance if:

- asset references cannot be attached without a public GamePackage/runtime schema redesign;
- package/content ids from Goal 010-style generated content cannot be resolved honestly;
- local fixture import or fallback verification would require external provider/media execution;
- runtime/package smoke would require UI or Unity launch;
- `.sln` or `.csproj` edits are required;
- full verification exposes an unrelated pre-existing failure.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S099 or Goal 012.
- No WinForms/UI, Unity/export, external asset/media generation, ComfyUI, Suno, LLM/RAG/provider execution or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report:

- changed files;
- accepted asset categories and counts;
- import/fallback distribution;
- package/content binding audit;
- exact invalid/fake/leak diagnostics;
- artifact folder and deterministic hash;
- focused/smoke/full verification totals;
- confirmation that the gate remains `minimum_asset_pipeline_artifact_verification` required;
- confirmation that S099/Goal 012, public schemas, UI, Unity/export, Lua/provider/media/LLM/RAG, generator-library and project files were untouched.
