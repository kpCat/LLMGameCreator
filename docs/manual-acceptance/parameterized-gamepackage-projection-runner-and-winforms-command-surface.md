# Parameterized GamePackage Projection Runner

Goal128 keeps the normal Unity projection verification command and adds an optional `-PackagePath` parameter for repo-local GamePackage JSON files.
The default command still verifies `samples/minimal-map-game/package.json`; manual Unity inspection remains optional.

## Normal Command

- `.devflow\scripts\run-unity-projection-verification.cmd`

## Example With Package Path

- `.devflow\scripts\run-unity-projection-verification.cmd -PackagePath samples\minimal-map-game\package.json`

## Scope Guard

- Package paths must stay under the repository root and outside `.llmgc/manual/`.
- This runner is projection-only and does not authorize Runtime, public schema, provider, Lua, generator-library, Unity scene, prefab, ProjectSettings, Packages or StreamingAssets work.

## Status

- parameterizedRunnerStatus: GREEN
- packagePathRelative: samples/minimal-map-game/package.json
- resultPath: .llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/parameterized-gamepackage-runner-result.json
- logPath: .llmgc/procedural/goal-128-parameterized-gamepackage-projection-runner-and-winforms-command-surface/unity-batchmode-parameterized-gamepackage-full-playthrough.log
- manualUnityOptional: true
