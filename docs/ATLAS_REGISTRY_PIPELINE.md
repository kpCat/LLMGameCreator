# Atlas Registry Pipeline

Status: v1 application layer  
Scope: read-only atlas import, preview, artifact persistence, saved preview reading, and facade orchestration.

## Purpose

The Atlas Registry Pipeline makes `generator-library/atlas` visible to the editor as deterministic data. It loads known atlas files, summarizes documents and examples, renders a human-readable Markdown report, and can persist preview artifacts into the existing Design DB generated artifact tables.

The pipeline is an editor/application layer. It does not replace `GamePackage` and it is not runtime content.

## Services

### AtlasRegistryImportService

`AtlasRegistryImportService` performs read-only atlas JSON loading.

It resolves either a repository root, `generator-library` root, or direct atlas root, then checks the known atlas files in deterministic order. JSON documents are parsed with `System.Text.Json`, examples are discovered from `examples/*.example.json`, IDs and reference-like values are summarized, and diagnostics are returned without throwing for ordinary atlas content errors.

Diagnostics include missing known files, invalid JSON, duplicate IDs, missing identity/title/purpose, missing examples root, examples without steps, unknown references, and paths outside the atlas root.

### AtlasRegistryMarkdownReportRenderer

`AtlasRegistryMarkdownReportRenderer` turns an import result into Markdown for humans.

The report contains summary counts, documents, examples, and diagnostics. Table cell values escape pipes and newlines so atlas text cannot break the report layout.

### AtlasRegistryPreviewService

`AtlasRegistryPreviewService` is the one-shot preview service.

It runs the import service, optionally renders Markdown, and optionally writes report files:

```text
.llmgc/atlas/atlas_registry_import_report.md
.llmgc/atlas/atlas_registry_import_result.json
```

Writing files is opt-in. Callers can also provide an explicit report output root.

### AtlasRegistryPreviewArtifactService

`AtlasRegistryPreviewArtifactService` stores the preview in the Design DB through `IGeneratedArtifactRepository`.

It writes the JSON preview as a `generated_artifacts` row with the standard result artifact ID. When Markdown rendering is enabled, it writes a second Markdown report artifact. When Markdown rendering is disabled, no Markdown artifact is saved.

Validation state is derived through `AtlasRegistryValidationPolicy`:

```text
valid     => 0 errors, 0 warnings
warnings  => 0 errors, >0 warnings
invalid   => >0 errors
```

Only warning and error diagnostics are saved into `validation_results`. Info diagnostics remain visible in the preview JSON and Markdown report but do not make an artifact warning/invalid. Re-saving the same artifact ID is idempotent because generated artifacts are upserted and validation results are replaced for that artifact ID.

Artifact metadata includes:

```text
generatedAtUtc
atlasRoot
documentCount
loadedDocumentCount
exampleCount
uniqueIdCount
errorCount
warningCount
writtenFiles
```

### AtlasRegistryPreviewArtifactReader

`AtlasRegistryPreviewArtifactReader` reads the latest saved atlas preview artifacts from `IGeneratedArtifactRepository`.

It loads the standard result artifact, optional Markdown artifact, and validation results for the result artifact. If the result artifact is absent, it returns `Exists=false`. Missing Markdown is allowed and does not require a schema change.

### AtlasRegistryPipelineService

`AtlasRegistryPipelineService` is the high-level application facade for future UI or CLI callers.

It can run preview-only, run preview with report files, or run preview and persist generated artifacts. The facade returns the preview result, import result, Markdown, written files, saved artifacts, and validation results so callers do not need to manually coordinate the lower-level services.

## Storage

The pipeline reuses the existing Design DB schema:

```text
generated_artifacts
validation_results
```

No new tables or schema version changes are required.

## Non-Goals

This layer deliberately does not:

```text
execute Lua
generate Lua
call an LLM
mutate GamePackage
change the GamePackage format
export Unity data
create runtime.db or save.db
generate C# from atlas data
add complex UI
```

The atlas remains planning and validation metadata. Any future mutation, runtime build, Unity export, or generation path must go through a separate explicit task and its own validation boundary.
