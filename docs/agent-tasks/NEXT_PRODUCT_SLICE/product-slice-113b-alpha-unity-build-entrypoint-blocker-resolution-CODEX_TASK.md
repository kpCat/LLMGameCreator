# CODEX TASK - S113B Alpha Unity Build Entrypoint Blocker Resolution

## Command

Run this file as a bounded blocker-resolution task, not as a new `/goal`.

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
6. `tests/LLMGameCreator.Tests/Application/AlphaBuild/AlphaRunnableBuildAcceptanceTests.cs`
7. `tests/LLMGameCreator.Tests/ProductSmoke/AlphaRunnableBuildSmokeTests.cs`
8. `.devflow/scripts/run-product-smoke.ps1`
9. current artifacts under `.llmgc/procedural/alpha-runnable-build/`
10. existing Unity/archive/export/materialization docs/services only where directly needed.

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Starting Evidence

Goal 013 currently stops at:

```text
alpha_unity_build_environment_blocker
```

S113A fixed the artifact path-length defect and kept the blocker honest.

The user has now shown Unity Hub evidence that Unity is installed locally:

```text
Unity 6.1 / 6000.1.10f1
C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe
Windows module visible in Unity Hub
```

The remaining blocker is repository-local, not Unity installation:

```text
No repository-local Unity project/template or Windows build entrypoint exists.
```

## Scope

Resolve only the Goal 013 Alpha blocker by adding the minimum repo-local Unity project/template and Windows build entrypoint required to produce and validate a real Windows player.

This is not Goal 014. Do not start S114 or post-Goal-013 work.

## Allowed Files

Primary allowed areas:

- a narrow repo-local Unity template/project under one new folder, preferably `unity/LLMGameCreatorAlpha/` or `src/LLMGameCreator.UnityAlpha/`
- Unity template files required for a minimal build:
  - `ProjectSettings/ProjectVersion.txt`
  - `Packages/manifest.json`
  - `Assets/Editor/*Build*.cs`
  - minimal `Assets/Scripts/*` runtime loader/diagnostics scripts if required
  - minimal scene or bootstrap assets if required
- `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
- focused Alpha build tests under `tests/LLMGameCreator.Tests/Application/AlphaBuild/`
- product smoke under `tests/LLMGameCreator.Tests/ProductSmoke/AlphaRunnableBuildSmokeTests.cs`
- `.devflow/scripts/run-product-smoke.ps1` only if needed
- `.llmgc/procedural/alpha-runnable-build/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only after a focused test/build failure proves it necessary:

- the narrow existing Unity runtime export/materialization seam;
- the narrow existing package/runtime validation seam;
- one compact sample under `samples/alpha-runnable-build/`.

Do not edit any other file without reporting a blocker.

Do not edit `.sln` or C# `.csproj` files unless a focused test/build failure proves it is strictly required. A Unity project may have Unity-owned project metadata, but do not make Visual Studio solution/project edits.

## Required Outcome

Replace `alpha_unity_build_environment_blocker` with real Alpha runnable build evidence.

Required final state:

```text
alpha_runnable_windows_build_verification
```

The gate must remain `required`, not `passed`, until the user/assistant reviews the produced build evidence.

Required facts in artifacts:

- `windowsExecutableProduced=true`
- `unityBuildProduced=true`
- `unityEditorExecuted=true` if Unity CLI was actually invoked
- `launchVerified=true` only if the built executable was actually launched and the process/log evidence is captured
- `playLoopVerified=true` only if a deterministic automated loop or explicit manual evidence proves it
- no false 15-30 minute play claim without evidence

If Unity CLI/build fails because of licensing, missing module, graphics environment, Windows build support, or local machine constraints, stop with a specific blocker diagnostic and exact user steps. Do not fake the build.

## Unity Template Requirements

Add the minimum Unity template/project needed for a real Windows build.

Required properties:

- repo-local under one deterministic folder;
- contains `ProjectSettings/ProjectVersion.txt`;
- contains `Assets/` and `Packages/`;
- contains a repository-local build entrypoint discoverable by `AlphaRunnableBuildAcceptanceService`;
- build entrypoint invokes `BuildPipeline.BuildPlayer` for Windows x64;
- build output goes to `.llmgc/procedural/alpha-runnable-build/build/windows/`;
- build consumes staged Goal 013 data from `.llmgc/procedural/alpha-runnable-build/staging/`;
- build includes staged game data/assets/config in a deterministic `StreamingAssets` or equivalent runtime-readable location;
- minimal runtime starts without Runtime Preview and writes a diagnostic launch log proving package/config load.

Keep this a data-loader Alpha shell. Do not implement broad gameplay, generated C# mechanics, UI editor tooling, or Unity asset generation.

## Build Entrypoint Requirements

Provide a command that can be run from PowerShell, for example:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.1.10f1\Editor\Unity.exe" -batchmode -quit -projectPath .\unity\LLMGameCreatorAlpha -executeMethod LLMGameCreatorAlpha.Editor.AlphaBuildEntrypoint.BuildWindows64 -logFile .\.llmgc\procedural\alpha-runnable-build\logs\unity-build.log
```

Do not hardcode this absolute path into deterministic JSON artifacts. It may appear in non-deterministic verification markdown/logs only.

The service should discover a repo-local Unity project and build script after this task.

## Runtime Launch Verification

Preferred launch proof:

- launch the produced `.exe` in batch/headless/diagnostic mode if possible;
- pass an argument pointing to staged data or bundled StreamingAssets;
- capture logs under `.llmgc/procedural/alpha-runnable-build/logs/`;
- verify the log contains package id, package hash, asset manifest hash, start map id and selected loop refs.

If the Windows player cannot be launched by Codex in the environment:

- still produce and validate the real build folder/executable if possible;
- keep `launchVerified=false` and `playLoopVerified=false`;
- keep final gate required;
- provide exact user launch steps and success/failure log checks.

Do not claim play verification unless executed.

## Acceptance Service Changes

Update `AlphaRunnableBuildAcceptanceService` narrowly so it:

- detects the new repo-local Unity project/template;
- detects the new repo-local build entrypoint;
- can invoke or validate the Unity build command as appropriate;
- validates actual build output files and executable;
- records build logs and executable manifest from actual bytes;
- preserves S113A compact path contract;
- keeps invalid/fake/leak rejection causal;
- does not treat ordinary non-Unity C# files as build scripts.

If actual Unity execution is too risky inside tests, keep unit tests focused on detection/validation and put real build execution behind product smoke or explicit service options.

## Tests

Add focused tests for:

- repo-local Unity project/template detection;
- repo-local build script detection only under Unity `Assets/` and with real `BuildPipeline.BuildPlayer`;
- ordinary C# files and prose references do not satisfy the build entrypoint;
- build output validation rejects missing executable;
- build output validation accepts a physical executable only when hashes/byte counts match;
- compact path contract from S113A still holds;
- state remains inside Goal 013 and does not create S114/Goal 014.

Product smoke must exercise the real path as far as the environment allows:

- generate Alpha staging;
- detect Unity template/build entrypoint;
- attempt real build if Unity CLI is discoverable and Windows module appears available;
- validate produced build files, or record a specific blocker if build cannot run.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~AlphaRunnableBuild|FullyQualifiedName~UnityRuntimeExport|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario alpha-runnable-build
.\.devflow\scripts\check-all.ps1
```

If Unity build is attempted, report:

- exact Unity command;
- Unity version;
- Unity project relative path;
- build script relative path;
- build output folder;
- build log path;
- executable relative path;
- launch command and result if launched.

Also scan changed/generated files for:

- mojibake markers;
- path-length contract violations from S113A;
- absolute local paths in deterministic artifacts;
- timestamps, GUIDs, machine names, temp paths and user names in deterministic artifacts;
- exact `S114|Goal 014|goal_014` markers, excluding explicit prohibition text.

## State Rules

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Allowed terminal states:

1. Real build produced and validation evidence exists:

```text
alpha_runnable_windows_build_verification required
```

2. Concrete build/launch blocker remains:

```text
alpha_unity_build_environment_blocker
```

Do not mark `alpha_runnable_windows_build_verification` as passed. Do not start S114 or Goal 014.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S114 or Goal 014.
- No Runtime Preview dependency as Alpha proof.
- No fake Windows executable, Unity build, launch or play claim.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No Visual Studio `.sln` or C# `.csproj` edits unless strictly proven necessary.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report:

- changed files;
- whether Unity project/template and build entrypoint were added;
- exact Unity command/version/log path if executed;
- build output folder and executable relative path if produced;
- launch/play verification result;
- blocker/gate state;
- report hash/build manifest hash;
- max artifact path/file name lengths;
- verification command results;
- confirmation that S114/Goal 014 and forbidden areas were not started.
