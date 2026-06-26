# Goal 012 - Unity Runtime Export Vertical Slice

## Purpose

Start only after the user/assistant explicitly confirms:

```text
minimum_asset_pipeline_artifact_verification passed
```

Goal 011 proved deterministic asset slot requests, local imports/fallbacks, file/hash validation and package asset binding. Goal 012 must prove the next product chain outside Runtime Preview:

```text
generated GamePackage + resolved asset manifest
-> deterministic Unity-runtime export plan
-> materialized export folder with game-data payload, asset payload and runtime launch/config metadata
-> headless Unity-runtime contract validation
-> product smoke evidence
```

This is the last pre-Alpha export vertical slice. It must be honest about what is and is not runnable. Goal 012 should not claim a Windows executable or 15-30 minute playable build unless that is actually produced and verified. Goal 013 remains the Alpha integration/runnable build goal.

## Final Gate

Stop at exactly one final gate:

```text
unity_runtime_export_vertical_slice_artifact_verification
```

Do not create S106, Goal 013 or post-Goal-012 work.

## Product Slices

- S099: record Goal 011 gate and define Unity runtime export vertical-slice contracts.
- S100: select one deterministic generated package plus asset manifest input from Goal 010/011 evidence.
- S101: build a generic Unity runtime data contract outside Runtime Preview.
- S102: materialize a deterministic Unity export folder with game data, asset refs, runtime config and launch metadata.
- S103: validate Unity runtime/template contract, package payload and asset payload headlessly.
- S104: prove selected package/runtime loops and asset refs resolve through the export payload.
- S105: invalid/fake/leak rejection, product smoke, artifacts, state handoff and final verification.

## Architecture Boundary

### Outside Runtime Preview

Goal 012 must not rely on WinForms Runtime Preview as the runtime host. It may reuse package/runtime validation services headlessly, but the exported artifact must be shaped as a Unity runtime/export payload.

The export may be a deterministic folder or archive suitable for a generic Unity runtime/template. If the repository already has Unity archive/export services, reuse and extend the narrowest existing seam.

### No false runnable claim

Do not claim any of the following unless actually produced and verified:

- Windows `.exe`;
- Unity player build;
- installed Unity Editor execution;
- 15-30 minute playable Alpha loop.

If Unity Editor or a Unity project build is required and unavailable, keep Goal 012 at a headless export artifact level and report that Goal 013 must perform the real build verification.

### Provider and media boundary

Do not call LLM, RAG, ComfyUI, Suno, image/audio providers, arbitrary Lua, external scripts or media generators. Goal 012 consumes the deterministic package and assets already produced by prior goals.

## Required Export Shape

Create deterministic artifacts under:

```text
.llmgc/procedural/unity-runtime-export/
```

Required final artifact files:

- `unity-runtime-export-report.json`
- `unity-runtime-export-report.md`
- `unity-runtime-export-verification.md`

Required materialized export folder, for example:

```text
.llmgc/procedural/unity-runtime-export/export/
```

The export folder must contain deterministic files such as:

- canonical `game-package.json`;
- canonical generated-content/provenance payload;
- canonical asset manifest/payload index;
- runtime config/launch metadata for a generic Unity runtime;
- export manifest with relative paths, hashes, byte counts and schema versions;
- optional archive file if an existing Unity archive materialization service already supports it.

Use UTF-8 without BOM and stable JSON ordering where the local codebase pattern supports it. No absolute paths, timestamps, GUIDs, machine names, temp paths or user names may enter deterministic artifacts.

## S099 - Contract And Gate Record

Define narrow versioned Application contracts for:

- Unity runtime export request;
- selected generated package input;
- selected asset manifest input;
- export file manifest;
- Unity runtime config;
- headless contract validation result;
- export report.

Required validation:

- schema/version ids;
- source package hash and asset manifest hash;
- safe relative paths only;
- bounded export file count and byte budget;
- no executable payload injection;
- no Unity Editor launch requirement;
- no provider/media/LLM/Lua execution;
- malformed input produces deterministic diagnostics.

Record the user-confirmed previous gate:

```text
minimum_asset_pipeline_artifact_verification passed
```

## S100 - Deterministic Input Selection

Select deterministic inputs from prior accepted evidence:

- one valid generated content pack/package from Goal 010-style content;
- the matching resolved asset manifest from Goal 011;
- selected quest/dialogue/item/NPC/event/map refs sufficient for a tiny runtime loop.

Required behavior:

- selected package id and hashes are stable on replay;
- selected asset refs are a strict subset of the Goal 011 manifest;
- selected ids resolve in the package and asset manifest;
- selection must not branch on named style ids in production logic;
- missing or mismatched prior evidence rejects causally.

## S101 - Unity Runtime Data Contract

Build a generic Unity runtime data contract outside Runtime Preview.

Required contract coverage:

- package identity and version;
- start map/scene id;
- player spawn or start state ref;
- map/tile/entity/item/dialogue/quest refs needed by the selected loop;
- asset refs for tile/region, NPC portrait, item/icon, sound effect and music/ambience categories;
- runtime command/action hints needed by the generic Unity host to start the loop;
- save/load or state bootstrap metadata compatible with existing runtime state ownership.

This contract is data for a generic Unity runtime. Do not generate C# gameplay mechanics.

## S102 - Export Folder Materialization

Materialize a deterministic export folder.

Required behavior:

- all output paths are relative and contained under the export root;
- every file in the export manifest exists;
- hashes and byte counts are computed from actual file bytes;
- asset files are copied or referenced deterministically from Goal 011 outputs;
- game-data payload is canonical and replay-stable;
- identical input produces identical export manifest and hashes;
- different valid input/seed produces a meaningful hash difference;
- export folder can be deleted and regenerated cleanly.

If an archive is produced, its contents must be deterministic or the report must clearly mark archive hash as non-gating. Prefer deterministic file manifests over nondeterministic ZIP metadata.

## S103 - Headless Unity Runtime Contract Validation

Validate the export without launching UI or Unity Editor.

Required validation:

- all runtime config refs resolve to package ids;
- all asset refs resolve to exported files and expected media categories;
- start map/scene refs resolve;
- selected loop refs resolve to existing quest/dialogue/interaction/item/NPC/event content;
- package validator remains clean;
- no public GamePackage/runtime schema redesign;
- no WinForms Runtime Preview dependency;
- no missing export files, mismatched hashes or outside-root paths;
- all external execution flags remain false.

If the repository already has Unity archive/materialization validators, reuse them where appropriate. Do not duplicate broad export systems.

## S104 - Selected Loop And Asset Resolution Smoke

Prove the exported payload is not report-only.

Required evidence:

- selected package/runtime loop can be identified from exported data;
- existing headless runtime/package services can still validate or execute the selected loop before export;
- exported runtime config references the same selected ids and package hash;
- exported asset refs for all required categories resolve to files listed in the export manifest;
- save/load/bootstrap metadata is present and deterministic where existing runtime state supports it;
- copied report strings without exported files fail acceptance.

Do not implement a second gameplay simulator and do not run Unity.

## S105 - Acceptance, Invalid Matrix And State

Required valid matrix:

- one deterministic exported vertical-slice package;
- same-input same-seed replay;
- one variation or second valid input proving hash/content difference;
- package validation;
- asset manifest validation;
- export file manifest validation;
- selected loop/reference resolution;
- product smoke route.

Required invalid/fake/leak matrix:

- missing prior package evidence;
- missing prior asset manifest evidence;
- package hash mismatch;
- asset manifest hash mismatch;
- unresolved package id;
- unresolved asset id;
- missing exported file;
- mismatched exported file hash;
- path traversal or absolute export path;
- executable/script/provider payload injection;
- copied expectation/report evidence without files;
- Runtime Preview-only dependency;
- Unity Editor/build claim without artifact;
- cross-pack or cross-asset leakage.

The invalid matrix must be causal. Do not satisfy required invalid cases with hand-authored diagnostics that bypass the same validation paths used by valid runs.

The report must include:

- manual gate `unity_runtime_export_vertical_slice_artifact_verification`;
- completed slices S099-S105;
- selected package id/hash;
- selected asset manifest hash;
- export folder path relative to project root;
- export file count, byte count and manifest hash;
- selected loop ids and asset refs;
- valid/invalid matrix results and causal diagnostics;
- external execution flags all false;
- `windowsExecutableProduced=false` unless a real executable exists and was verified;
- `unityEditorExecuted=false` unless a real Unity Editor command ran and is reported exactly;
- public schema/project-file change flags false unless a blocker was explicitly reported.

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Record Goal 012 as complete up to the final gate, leave:

```text
unity_runtime_export_vertical_slice_artifact_verification: required
```

Do not recommend, create or start Goal 013.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityRuntimeExport|FullyQualifiedName~MinimumAssetPipeline|FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-runtime-export
.\.devflow\scripts\check-all.ps1
```

Also scan changed/generated files for:

- mojibake markers;
- absolute local paths;
- nondeterministic timestamps or GUIDs in deterministic artifacts;
- `S106|Goal 013|goal_013` outside prohibition text.

## Stop Conditions

Stop with a blocker report instead of weakening acceptance if:

- a public GamePackage/runtime schema redesign is required;
- `.sln` or `.csproj` edits are required;
- selected loop export cannot be proven outside Runtime Preview;
- Unity runtime/export vertical slice cannot be materialized without launching Unity Editor;
- existing package/assets cannot be exported without external provider/media generation;
- a real runnable Windows build is required to proceed;
- full verification exposes an unrelated pre-existing failure.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S106 or Goal 013.
- No WinForms/UI work and no Runtime Preview dependency.
- No Unity Editor/build execution unless the goal stops with a clear blocker or the command is explicitly headless, noninteractive and fully reported.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report:

- changed files;
- selected package/content/asset inputs;
- export folder and manifest summary;
- selected loop and asset-ref resolution evidence;
- exact invalid/fake/leak diagnostics;
- whether Unity Editor was executed;
- whether a Windows executable was produced;
- artifact folder and deterministic hash;
- focused/smoke/full verification totals;
- confirmation that the gate remains `unity_runtime_export_vertical_slice_artifact_verification` required;
- confirmation that S106/Goal 013, public schemas, UI/Runtime Preview, external providers/media/LLM/RAG/Lua, generator-library and project files were untouched.
