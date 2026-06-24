# Product Slice 028: Manual Import Repair + Semantic Catalog Foundation v1

## Delivered behavior

Product Slice 028 repairs the controlled manual-import workspace and adds a deterministic project-local semantic memory layer. It does not unlock M5 or M6 and does not execute providers, generators, LLMs, Lua, Unity, or Runtime gameplay.

## Manual-import repairs

`UnityArchiveManualImportTemplateService` now validates directory-relative paths separately from file-output paths. The valid archive-relative directories `manual-import` and `manual-import/put-files-here` are accepted, created under `.llmgc/unity-archive/`, and do not create `import-manifest.json`. Empty, rooted, drive-qualified, UNC-style, backslash-containing, empty-segment, `.` and `..` paths are rejected before directory creation. Final `Path.GetFullPath` containment remains mandatory.

`UnityArchiveManualProviderImportResult.TargetOutputsChanged` is true only when at least one entry has `Imported` status and target bytes were written. The import report and optional fulfillment scan still run for every attempt. Archive review, history, and comparison refresh only when `TargetOutputsChanged` is true. Therefore:

- an identical `AlreadyImported` run writes the import report but stores no new review-history snapshot;
- a conflict/invalid/failed-only run writes the import report but stores no new review-history snapshot;
- a first import or an explicitly enabled different-byte overwrite refreshes review/history/comparison.

The S026 service remains the authority for manifest validation, slot resolution, path containment, byte comparison, copying, hashing, reporting, and refresh decisions.

## Project-local semantic catalog

Approved `semantic_pack_v1` artifacts can now be consumed by `SemanticCatalogService` without changing `GamePackageDefinition`. The existing GamePackage assembler behavior remains intact: semantic artifacts are preserved and reported as unmapped because GamePackage v1 has no semantic field.

The deterministic sidecar outputs are:

```text
.llmgc/semantic/semantic-catalog.json
.llmgc/semantic/semantic-catalog-report.md
.llmgc/semantic/semantic-generation-context-preview.json
.llmgc/semantic/semantic-generation-context-preview.md
```

The service supports explicit `terms` and `relations`, nested `semantic` objects, compact arrays such as `themes`, `tones`, and `dialogueIntents`, and the existing deterministic producer's `semantic_groups` shape. Terms and relations retain approved source artifact ids.

Supported term kinds:

```text
theme
tone
biome
faction
faction_relation
npc_archetype
dialogue_intent
quest_motif
item_affordance
location_mood
asset_style_hint
audio_mood_hint
entity_role
unknown
```

Supported statuses:

```text
known
candidate
deprecated
conflict
invalid
```

The built-in dictionary is deliberately small and marks its seed terms `known`. Generated terms default to `candidate`. Safe unfamiliar terms are retained as candidates. Unsupported kinds are normalized to `unknown` with a warning. Unsafe ids and relations are skipped with stable diagnostics. Output ordering and UTF-8-without-BOM rendering are deterministic and contain no timestamps.

## Semantic generation context preview

`SemanticGenerationContextPreviewService` projects the catalog into compact, reviewable sections for themes, tones, dialogue intents, quest motifs, asset style hints, audio mood hints, important relation endpoints, candidates, conflicts, and diagnostics. It caps each prompt-oriented list at the documented recommendation of 80 terms.

The preview is data for a future explicit generation step. It never calls an LLM. Its policy states which creative decisions may need an LLM and which steps must remain deterministic.

The sidecar writer is intentionally an Application-layer project-local operation in this foundation slice. Automatic invocation from the one-click package export flow is deferred to a separate controlled integration decision so the current package export constructor/DI contract and GamePackage schema remain unchanged.

## Verification contract

The `semantic-catalog-foundation` product smoke constructs an approved `semantic_pack_v1` fixture, builds the catalog and preview, writes all four sidecar files, validates JSON, and proves candidate-term preservation without provider or LLM execution. Focused regression tests cover the directory helper, no-op/conflict history suppression, overwrite history creation, semantic seeds, pack mapping, invalid ids, markdown, GamePackage non-mapping, and compact context preview.

M5 and M6 remain Locked.
