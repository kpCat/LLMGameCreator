# Generator Plan GamePackage Assembly Pipeline

Status: v1 application layer  
Scope: assemble the first draft `GamePackageDefinition` from a Draft Artifact Approval/Staging approved artifact set, validate it, optionally export `package.json`, render a Markdown assembly report, and persist/read assembly artifacts.

## Purpose

GamePackage Assembly v1 is the first layer after approval/staging that creates a real data package draft. It consumes approved JSON artifacts and deterministically maps the safe subset into existing `GamePackageDefinition` fields.

This layer does not finalize runtime packaging. It creates a draft `package.json` that can be validated by the existing `GamePackageValidator` and saved through the existing package repository.

## Relation To Approval/Staging

```text
GeneratorPlanPreview
  -> GeneratorPlanDraftExecutionPlan
  -> GeneratorPlanDraftArtifactQueue
  -> GeneratorPlanDraftArtifactProductionBatch
  -> Draft Artifact Approval/Staging
  -> Approved Artifact Set
  -> GamePackage Assembly
  -> GamePackageDefinition draft
  -> package.json export / generated_artifacts
```

Approval owns review decisions and approved artifact set persistence. Assembly owns deterministic package draft creation, validation, optional export, reporting, and latest artifact reading.

## Services

`GeneratorPlanApprovedArtifactSetReader` parses the approved artifact set JSON saved by the approval layer. It preserves artifact id, kind, expected contract, and raw `content_json`.

`GeneratorPlanGamePackageAssembler` builds an in-memory `GamePackageDefinition` from approved artifacts. It always applies baseline defaults so the draft has a manifest, start map, grass tile, player prototype, and player instance.

`GeneratorPlanGamePackageAssemblyValidator` validates assembly inputs and maps existing package validation issues into assembly diagnostics.

`GeneratorPlanGamePackageAssemblyPolicy` builds summary counts, status, validation state, deterministic validation result ids, and warning/error validation rows.

`GeneratorPlanGamePackageAssemblyMarkdownRenderer` renders status, artifact mapping, package counts, validation diagnostics, and a compact package JSON preview.

`GeneratorPlanGamePackageAssemblyService` runs assembly, package validation, optional package JSON serialization, and optional export through `IGamePackageRepository`.

`GeneratorPlanGamePackageAssemblyArtifactService` saves the assembly snapshot, package draft, optional Markdown report, and validation results through `IGeneratedArtifactRepository`.

`GeneratorPlanGamePackageAssemblyArtifactReader` reads the latest assembly snapshot, latest package draft, optional Markdown report, and validation results.

## Approved Artifact Set Input

```json
{
  "schema_version": "0.1",
  "snapshot_id": "draft_artifact_staging/example",
  "source_production_batch_id": "draft_artifact_production/example",
  "approved_artifacts": [
    {
      "artifact_id": "artifact/game_profile",
      "artifact_kind": "game_profile_v1",
      "expected_artifact_contract": "game_profile_v1",
      "content_json": {}
    }
  ]
}
```

## Mapping Rules

`game_profile_v1` maps `content_json.game.title` to `Manifest.Title`, derives `Manifest.PackageId`, and maps `content_json.game.genre` to `Manifest.Description`.

`scene_pack_v1` maps `scenes[0].title` to the start map name. Additional scenes become deterministic draft maps under `map/draft/...`.

`entity_pack_v1` maps `entities[]` to `EntityPrototypeDefinition` entries. Player entities normalize to `entity/player`.

`quest_pack_v1` maps `quests[]` to `QuestDefinition` entries with safe custom-counter objectives.

`mechanics_pack_v1` maps `mechanics[]` to minimal `AbilityDefinition` entries when the existing package schema can accept them safely.

`semantic_pack_v1` is acknowledged as unmapped because no GamePackage field exists for semantics in v1. Unknown kinds are also unmapped warnings.

## Baseline Package Defaults

Assembly always ensures:

```text
Manifest.PackageId = "game/generated-draft" unless a game profile title exists
Manifest.Title = "Generated Draft Game" unless a game profile title exists
Manifest.Version = "0.1.0"
Manifest.FormatVersion = "0.1"
Manifest.StartMapId = "map/start"
Game.TilePrototypes contains "tile/grass"
Game.Maps contains "map/start"
map/start DefaultTileId points to "tile/grass"
map/start StartPosition is inside map bounds
Game.EntityPrototypes contains "entity/player"
map/start contains "entity/player/start"
```

## Package Validation

The service runs the existing `GamePackageValidator` against the assembled draft. Package validation errors and warnings become assembly diagnostics and generated artifact validation rows.

## Package Export

Export is explicit. The service only writes `package.json` when `ExportPackageJson=true` and `ExportFolderPath` is provided. Export uses `IGamePackageRepository.SaveAsync`.

## Artifact IDs And Kinds

```text
artifact/generator_plan_game_package_assembly/latest
  kind: generator_plan.game_package_assembly

artifact/generator_plan_game_package_draft/latest
  kind: game_package.draft

artifact/generator_plan_game_package_assembly_markdown/latest
  kind: generator_plan.game_package_assembly_markdown_report
```

## Diagnostics

```text
generator_plan_game_package_assembly.missing_approved_artifact_set
generator_plan_game_package_assembly.no_approved_artifacts
generator_plan_game_package_assembly.approved_artifact_invalid_json
generator_plan_game_package_assembly.approved_artifact_missing_kind
generator_plan_game_package_assembly.unmapped_artifact_kind
generator_plan_game_package_assembly.package_validation_error
generator_plan_game_package_assembly.package_validation_warning
generator_plan_game_package_assembly.package_serialization_error
generator_plan_game_package_assembly.export_path_missing
generator_plan_game_package_assembly.export_failed
```

Info diagnostics are not saved into `validation_results`.

## Persistence

The pipeline uses existing `generated_artifacts` and `validation_results`. It does not require a new Design DB table or schema version. Saving is idempotent for stable artifact ids: generated artifacts are upserted and validation rows are replaced for each artifact id.

## Non-Goals

```text
No Lua execution.
No LLM calls.
No provider calls.
No GamePackage schema change.
No Unity export.
No runtime.db/save.db.
No generated code execution.
No complex UI.
```

This layer also does not call local or remote models, generate Lua, touch WinForms UI, or mutate an existing project unless explicit package export is requested.

## Future Next Step

The next step is a runtime preview smoke path or a small UI action that invokes this application service explicitly after approval/staging.
