# Product Slice 017: Unity Archive Validation/Export Dry Run

## Goal

Add an editor-side dry-run exporter that consumes the Slice 016 Unity target/archive contracts and produces a deterministic Unity-game-archive export plan without implementing Unity and without changing Runtime/GamePackage schema.

This slice answers:

```text
Can the current design brief + Unity target + archive manifest be exported?
Which files would be written?
Which runtime modules are required?
Which UI layouts/assets/audio requests are referenced?
Which planned/future modules block a real build?
What is missing before a real Unity player can load the archive?
```

## Output

Preferred project-scoped output:

```text
.llmgc/unity-export-dry-run/
```

Suggested files:

```text
.llmgc/unity-export-dry-run/unity-archive-plan.json
.llmgc/unity-export-dry-run/unity-archive-plan.md
.llmgc/unity-export-dry-run/unity-archive-manifest.json
.llmgc/unity-export-dry-run/validation-report.json
```

No Unity project is created. No executable is built.

## Non-goals

Do not implement Unity runtime/player, Unity build/export, asset generation provider, ComfyUI/Suno integration, GamePackage schema changes, Runtime changes or generator execution.
