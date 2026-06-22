# Product Slice 018: Unity Archive Materialization v1

## Goal

Turn the Slice 017 dry-run plan into a real editor-side archive artifact folder and optional zip, still without implementing Unity.

This is the next step toward:

```text
LLMGameCreator builds game archive
→ future Unity player loads game archive
```

Slice 017 answered “what would be exported”. Slice 018 should answer “can we materialize a deterministic v1 archive shape from the current contracts and known data?”

## Output

Preferred project-scoped output:

```text
.llmgc/unity-archive/
```

Required files:

```text
.llmgc/unity-archive/manifest/unity-game-archive.json
.llmgc/unity-archive/composition/game-design-brief.json
.llmgc/unity-archive/composition/unity-target-profile.json
.llmgc/unity-archive/composition/runtime-modules-index.json
.llmgc/unity-archive/ui/layouts-index.json
.llmgc/unity-archive/assets/asset-requests.json
.llmgc/unity-archive/audio/audio-requests.json
.llmgc/unity-archive/localization/index.json
.llmgc/unity-archive/lua/modules-index.json
.llmgc/unity-archive/export-report.md
.llmgc/unity-archive/export-validation.json
```

Optional:

```text
.llmgc/unity-archive.zip
```

## Non-goals

No Unity project, no Unity build, no Unity runtime, no generated C# runtime code, no provider calls, no generator execution, no Runtime changes, no GamePackageDefinition/schema changes.
