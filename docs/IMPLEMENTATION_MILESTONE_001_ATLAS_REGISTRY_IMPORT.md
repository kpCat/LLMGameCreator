# Implementation Milestone 001 — Atlas Registry Import Preview

Status: planning document  
Version: 0.1  
Scope: small C# implementation milestone, read-only preview only  
Primary goal: make the new `generator-library/atlas` seed architecture visible to the application without executing Lua, generating content, mutating packages, exporting Unity data, or compiling runtime databases.

## Why this milestone exists

The repository now contains a data-first atlas layer:

```text
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/validation_pipeline.json
generator-library/atlas/library_growth_pipeline.json
generator-library/atlas/runtime_db_and_unity_export_map.json
generator-library/atlas/model_workflow_roles_and_prompts.json
generator-library/atlas/prompt_context_pack_map.json
generator-library/atlas/game_profile_negotiation_map.json
generator-library/atlas/feature_bundle_map.json
generator-library/atlas/generator_plan_map.json
generator-library/atlas/examples/*.example.json
generator-library/atlas/ATLAS_INDEX.md
```

This milestone turns those files from passive documentation into a read-only registry preview that C# can inspect and validate.

The goal is deliberately narrow: **load atlas JSON files, summarize them, report shape/reference issues, and expose the result as data**.

This is not a runtime system yet.

## Non-goals

This milestone must not:

```text
- execute Lua;
- generate Lua modules;
- generate C#;
- mutate GamePackage;
- apply game package patches;
- create or edit Unity projects;
- build runtime.db;
- build save.db;
- export Unity IR;
- call local or remote LLMs;
- add a complex UI workflow;
- create a full schema validator framework;
- redesign the existing generator-library importer.
```

If a future implementation attempts any of the above, it is outside Milestone 001.

## Desired result

After this milestone, the application layer should be able to answer:

```text
- Which atlas files exist?
- Which atlas documents were loaded successfully?
- Which important top-level IDs/titles/purposes were detected?
- Which documents reference other known atlas ids?
- Are there missing files?
- Are there duplicate ids?
- Are there obvious broken references between atlas files?
- Are example plans present?
- Is the atlas safe as read-only metadata?
```

The output can be a simple diagnostic object, console/test output, service result, or design DB import preview record. No UI is required for the first pass.

## Placement

Recommended C# namespace:

```text
LLMGameCreator.Application.Design
```

Recommended files, if implementing manually:

```text
src/LLMGameCreator.Application/Design/Atlas/AtlasRegistryImportService.cs
src/LLMGameCreator.Application/Design/Atlas/AtlasRegistryImportModels.cs
src/LLMGameCreator.Application/Design/Atlas/AtlasRegistryDiagnostics.cs
```

Tests, if the test project already has a suitable pattern:

```text
tests/LLMGameCreator.Application.Tests/Design/AtlasRegistryImportServiceTests.cs
```

Do not add UI files in this milestone.

## Read-first files

Before implementing, read these files only:

```text
docs/CAPABILITY_ATLAS.md
docs/ARTIFACT_CONTRACTS.md
docs/VALIDATION_PIPELINE_ATLAS.md
docs/LIBRARY_GROWTH_PIPELINE.md
docs/RUNTIME_DB_AND_UNITY_EXPORT_MAP.md
docs/MODEL_WORKFLOW_ROLES_AND_PROMPTS.md
docs/PROMPT_CONTEXT_PACK_MAP.md
docs/GAME_PROFILE_NEGOTIATION_MAP.md
docs/FEATURE_BUNDLE_MAP.md
docs/GENERATOR_PLAN_MAP.md
docs/GENERATOR_PLAN_EXAMPLES.md
docs/GENERATOR_LIBRARY_ATLAS_OVERVIEW.md
generator-library/atlas/ATLAS_INDEX.md
src/LLMGameCreator.Application/Design/GeneratorLibraryImportService.cs
src/LLMGameCreator.Application/Validation/GamePackageValidator.cs
```

Do not scan the entire repository unless a compile error requires a specific file.

## Import root resolution

The service should resolve atlas files from a repository/project root.

Preferred root inputs:

```text
- explicit repository root path passed into the service;
- existing generator-library root resolver pattern if available;
- current project folder only as fallback.
```

Expected atlas directory:

```text
<repo>/generator-library/atlas
```

Do not hardcode absolute user machine paths.

## Atlas files to load

Milestone 001 should load these exact known files when present:

```text
generator-library/atlas/capability_atlas.json
generator-library/atlas/reference_profiles.json
generator-library/atlas/artifact_contracts.json
generator-library/atlas/validation_pipeline.json
generator-library/atlas/library_growth_pipeline.json
generator-library/atlas/runtime_db_and_unity_export_map.json
generator-library/atlas/model_workflow_roles_and_prompts.json
generator-library/atlas/prompt_context_pack_map.json
generator-library/atlas/game_profile_negotiation_map.json
generator-library/atlas/feature_bundle_map.json
generator-library/atlas/generator_plan_map.json
```

It should also discover example files:

```text
generator-library/atlas/examples/*.example.json
```

Markdown docs are not parsed in this milestone; they may be reported as companion documents only.

## Suggested data model

Keep this intentionally lightweight.

```csharp
public sealed record AtlasRegistryImportResult
{
    public bool Ok { get; init; }
    public string AtlasRoot { get; init; } = "";
    public IReadOnlyList<AtlasDocumentSummary> Documents { get; init; } = [];
    public IReadOnlyList<AtlasExampleSummary> Examples { get; init; } = [];
    public IReadOnlyList<AtlasDiagnostic> Diagnostics { get; init; } = [];
    public AtlasRegistrySummary Summary { get; init; } = new();
}
```

```csharp
public sealed record AtlasDocumentSummary
{
    public string Path { get; init; } = "";
    public string FileName { get; init; } = "";
    public string? SchemaVersion { get; init; }
    public string? Id { get; init; }
    public string? Title { get; init; }
    public string? Purpose { get; init; }
    public IReadOnlyList<string> TopLevelIds { get; init; } = [];
    public IReadOnlyList<string> ReferencedIds { get; init; } = [];
    public bool Loaded { get; init; }
}
```

```csharp
public sealed record AtlasExampleSummary
{
    public string Path { get; init; } = "";
    public string? ExampleId { get; init; }
    public string? Title { get; init; }
    public string? SourceProfileId { get; init; }
    public IReadOnlyList<string> SelectedFeatureBundles { get; init; } = [];
    public IReadOnlyList<string> TargetArtifacts { get; init; } = [];
    public int StepCount { get; init; }
}
```

```csharp
public sealed record AtlasDiagnostic
{
    public string Severity { get; init; } = "info"; // info, warning, error
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
    public string? Path { get; init; }
    public string? Id { get; init; }
}
```

```csharp
public sealed record AtlasRegistrySummary
{
    public int DocumentCount { get; init; }
    public int LoadedDocumentCount { get; init; }
    public int ExampleCount { get; init; }
    public int UniqueIdCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
}
```

These shapes are suggestions, not a hard public contract. Keep them simple.

## Minimal parsing rules

Use `System.Text.Json`.

Do not create a rigid full schema model for every atlas file yet. The files are seed architecture and may evolve.

For each JSON file:

```text
- parse as JsonDocument;
- read optional root fields:
  - schema_version
  - atlas_version
  - id
  - example_id
  - title
  - purpose
- collect ids from common fields:
  - id
  - example_id
  - primary_bundle
  - required_runtime_targets[]
  - runtime_targets[]
  - required_feature_bundles[]
  - optional_feature_bundles[]
  - selected_feature_bundles[]
  - target_artifacts[]
  - content_overlays[]
  - optional_content_overlays[]
  - source_profile.id
  - expected_artifact_contract
  - validation_gates[]
  - artifact_contracts[]
  - validators[]
  - depends_on[]
  - requires[]
  - provides[]
```

This is enough for a preview.

## Diagnostics

Milestone 001 should report at least these diagnostics:

```text
atlas.missing_root
atlas.missing_known_file
atlas.invalid_json
atlas.missing_identity
atlas.duplicate_id
atlas.example_without_steps
atlas.example_unknown_profile_reference
atlas.reference_unknown
atlas.path_outside_atlas_root
```

Severity guidance:

```text
error:
  - atlas root missing;
  - invalid JSON;
  - duplicate canonical id;
  - path outside atlas root.

warning:
  - missing known file;
  - missing title/purpose;
  - unknown reference;
  - example has zero steps;
  - expected companion docs not found.

info:
  - loaded file;
  - discovered example;
  - skipped markdown.
```

Unknown references should be warnings in Milestone 001 because the atlas is still a seed. They can become stricter later.

## Reference checking strategy

Do not attempt perfect graph validation yet.

Build a set of known ids from:

```text
- every root id;
- every example_id;
- every nested object id;
- known artifact contract ids;
- known validation level ids;
- known feature bundle ids;
- known profile ids;
- known content overlay ids;
- known runtime target ids;
```

Then compare collected reference-like values against the set.

Allow these value categories without warning:

```text
- plain filenames;
- relative paths;
- display strings;
- non-id enum values like mature, dark, political;
- budget ranges like 16k-32k;
- lifecycle states like draft, approved, exported;
- severity labels like info/warning/error;
- freeform summaries.
```

A simple heuristic is acceptable:

```text
Treat a string as reference-like only if it contains "/" or "." and looks like an id,
or if it matches known id prefixes such as:
profile/
feature_bundle/
content_overlay/
validation.
context_template/
role/
model_tier/
contract_group/
proposal_kind/
```

## Tests

Recommended tests:

```text
- imports all known atlas files from a temp copied generator-library/atlas folder;
- reports no invalid JSON for current seed files;
- discovers examples;
- detects duplicate ids in a synthetic temp file;
- detects invalid JSON in a synthetic temp file;
- detects missing root;
- does not execute Lua or read files outside atlas root.
```

If tests are too expensive for the first patch, create only the service and a small unit test for JSON parsing and duplicate ID detection.

## Acceptance criteria

Milestone 001 is done when:

```text
- atlas JSON files can be loaded from generator-library/atlas;
- examples/*.example.json are discovered;
- summaries are produced;
- diagnostics are produced;
- invalid JSON is reported without crashing the app;
- duplicate IDs are detected;
- missing files are reported as warnings;
- no Lua execution exists;
- no GamePackage mutation exists;
- no Unity export exists;
- no runtime.db build exists;
- no LLM call exists.
```

## Suggested manual verification

After implementing:

```text
dotnet test
```

If full tests are not available:

```text
- run the smallest affected test project;
- or add a temporary dev-only call from an existing diagnostic path and remove it before final commit.
```

Do not add permanent console spam to normal application startup.

## Commit suggestion

```text
Add atlas registry import preview
```

## What comes after this milestone

Milestone 002 should be one of:

```text
A. Atlas diagnostics page/tab in an existing Design/Generator Library diagnostics UI.
B. Design DB import table additions for atlas document summaries and diagnostics.
C. Generator plan preview model using the atlas maps, still no execution.
```

Recommended next milestone:

```text
Milestone 002 — Atlas Registry Diagnostics UI or Design DB Preview
```

Choose UI only if there is already a safe diagnostics page to extend. Otherwise choose Design DB preview first.

## Hard boundary

The atlas is the planning layer. It must stay cheap, inspectable and safe.

Milestone 001 should make the atlas visible to C# without turning it into a giant framework.
