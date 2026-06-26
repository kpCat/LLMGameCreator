# CODEX TASK - S113C Alpha Unity Minimal Play Loop Verification

## Command

Run this file as a bounded Goal 013 final verification task, not as a new `/goal`.

## Read First

Read in this order:

1. `AGENTS.md`
2. `docs/DEVELOPMENT_CHAT_HANDOFF.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
6. `tests/LLMGameCreator.Tests/Application/AlphaBuild/AlphaRunnableBuildAcceptanceTests.cs`
7. `tests/LLMGameCreator.Tests/ProductSmoke/AlphaRunnableBuildSmokeTests.cs`
8. `unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs`
9. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
10. `.devflow/scripts/run-product-smoke.ps1`
11. current artifacts under `.llmgc/procedural/alpha-runnable-build/`

Do not read historical apply packs, old task prompts or broad roadmaps unless a concrete blocker requires it.

## Starting Evidence

Goal 013 S113B produced a real Windows player and diagnostic launch evidence.

The user manually ran:

```powershell
.\.llmgc\procedural\alpha-runnable-build\build\windows\LLMGameCreatorAlpha.exe
.\.llmgc\procedural\alpha-runnable-build\build\windows\LLMGameCreatorAlpha.exe -batchmode -nographics -alphaSmokeExit -alphaLogPath .\.llmgc\procedural\alpha-runnable-build\logs\manual-alpha-player-launch.log
Get-Content .\.llmgc\procedural\alpha-runnable-build\logs\manual-alpha-player-launch.log
```

Observed manual evidence:

```text
alpha_runtime.launch_started=true
alpha_runtime.payload_root_exists=true
alpha_runtime.config_loaded=true
alpha_runtime.package_loaded=true
alpha_runtime.asset_manifest_loaded=true
alpha_runtime.package_id=game/content_generation/frontier-survival
alpha_runtime.package_hash=3e8a42663e1a2fdabd98cdd8c30ab6188810bd4d0f4d36aa4e3089a71b952d53
alpha_runtime.asset_manifest_hash=3dd392bae4cbac24db34b1810a52c83cf64791521df8849c75ac61e8fdcfa595
alpha_runtime.start_map_id=map/frontier-survival/start
alpha_runtime.selected_thread_id=thread/frontier-survival/000
alpha_runtime.command_hint_count=5
alpha_runtime.asset_ref_count=5
alpha_runtime.package_bytes=420490
alpha_runtime.asset_manifest_bytes=5016
alpha_runtime.launch_completed=true
```

The user also observed that the interactive player window opens but is empty.

This confirms launch/content-load evidence, but not play-loop evidence. Current gate remains:

```text
alpha_runnable_windows_build_verification required
```

## Scope

Finish Goal 013 verification by adding a minimal visible and automated Unity Alpha play loop.

This is not Goal 014. Do not start S114 or post-Goal-013 work.

## Allowed Files

Primary allowed areas:

- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- new narrow Unity runtime scripts under `unity/LLMGameCreatorAlpha/Assets/Scripts/`
- `unity/LLMGameCreatorAlpha/Assets/Editor/AlphaBuildEntrypoint.cs` only if build/staging copy must be adjusted
- `src/LLMGameCreator.Application/Design/AlphaBuild/AlphaRunnableBuildAcceptanceService.cs`
- focused Alpha build tests under `tests/LLMGameCreator.Tests/Application/AlphaBuild/`
- product smoke under `tests/LLMGameCreator.Tests/ProductSmoke/AlphaRunnableBuildSmokeTests.cs`
- `.devflow/scripts/run-product-smoke.ps1` only if needed
- `.llmgc/procedural/alpha-runnable-build/*`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

Conditionally allowed only after a focused test/build failure proves it necessary:

- `unity/LLMGameCreatorAlpha/Packages/manifest.json`
- `unity/LLMGameCreatorAlpha/ProjectSettings/*`
- the narrow existing Unity runtime export/materialization seam.

Do not edit any other file without reporting a blocker.

Do not edit `.sln`, C# `.csproj`, generator-library, WinForms/UI or public GamePackage/runtime schemas.

## Required Outcome

Produce real evidence for a minimal Alpha play loop inside the Windows player.

Required final report facts:

- `windowsExecutableProduced=true`
- `unityBuildProduced=true`
- `unityEditorExecuted=true` when Unity CLI is invoked
- `launchVerified=true`
- `playLoopVerified=true`
- `alpha_runnable_windows_build_verification` remains `required`, not `passed`
- no S114/Goal 014 started

The play loop may be minimal, but it must be visible and causally tied to generated package/export evidence.

## Minimal Visible Loop Requirements

The built player must no longer be visually empty.

At minimum the Unity scene must display:

- package id;
- selected thread id;
- start map id;
- selected quest id;
- selected dialogue id;
- selected item id;
- selected event id;
- command hint count;
- asset ref count;
- a simple status/log panel.

Use built-in Unity UI, IMGUI, or simple text rendering. Do not add third-party packages.

Add basic input:

- `Space` or a visible button advances one generated command hint;
- `R` resets the mini-loop;
- `Esc` quits;
- each step updates visible status and appends to a local play log.

## Automated Play Loop Requirements

Add a diagnostic mode for product smoke, for example:

```powershell
.\.llmgc\procedural\alpha-runnable-build\build\windows\LLMGameCreatorAlpha.exe -batchmode -nographics -alphaPlayLoopSmokeExit -alphaLogPath .\.llmgc\procedural\alpha-runnable-build\logs\alpha-player-launch.log -alphaPlayLoopLogPath .\.llmgc\procedural\alpha-runnable-build\logs\alpha-player-play-loop.log
```

The automated log must prove:

- payload root exists;
- config/package/asset manifest loaded;
- package id and hashes match expected config values;
- selected loop refs are non-empty;
- selected loop refs resolve in loaded JSON where structurally possible;
- command hints count is at least 5;
- automated steps execute each command hint in order;
- state transitions are recorded for:
  - quest start;
  - dialogue open;
  - dialogue choice;
  - loot roll or item grant;
  - event application;
- final state includes at least:
  - `quest_started=true`;
  - `dialogue_seen=true`;
  - `item_obtained=true`;
  - `event_applied=true`;
  - `commands_executed=<count>`;
  - `alpha_runtime.play_loop_completed=true`.

Do not simply write success constants. The runtime script must derive ids and command hints from loaded `unity-runtime-config.json` and/or package JSON, then execute the mini-loop against those loaded values.

If some exact semantic action cannot be fully applied yet, record the concrete limitation and implement the strongest honest state transition available. Do not fake `playLoopVerified=true`.

## Acceptance Service Requirements

Update `AlphaRunnableBuildAcceptanceService` so `playLoopVerified=true` requires:

- launch log exists and contains `alpha_runtime.launch_completed=true`;
- play-loop log exists;
- play-loop log contains `alpha_runtime.play_loop_completed=true`;
- command count in play-loop log matches or exceeds expected command hints from the selected config;
- required state flags are true;
- package id/hash/asset manifest hash in logs match the report candidate;
- executable/build files still validate from actual bytes;
- StreamingAssets staged files still match staging hashes.

Keep `Accepted=false` because the manual gate remains required until reviewed, but final status should remain:

```text
alpha_runnable_windows_build_verification
```

## Tests

Add focused tests for:

- play-loop log parser rejects missing log;
- play-loop log parser rejects wrong package/hash;
- play-loop log parser rejects missing command execution/state flags;
- play-loop log parser accepts a valid log tied to selected report evidence;
- build output validation still rejects fake/missing executable;
- S113A path-length contract still holds;
- Runtime Preview is not used as proof;
- state remains inside Goal 013 and does not create S114/Goal 014.

Product smoke must:

- regenerate staging;
- build Windows player if Unity is available;
- launch player in automated play-loop diagnostic mode;
- verify launch and play-loop logs;
- set `launchVerified=true` and `playLoopVerified=true` only from actual log evidence.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~AlphaRunnableBuild|FullyQualifiedName~UnityRuntimeExport|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario alpha-runnable-build
.\.devflow\scripts\check-all.ps1
```

Also run or report the exact Windows player diagnostic command used for play-loop verification.

After build, report:

- Unity command/version/log path;
- build output folder;
- executable relative path;
- launch command;
- launch log path;
- play-loop log path;
- `playLoopVerified` value and why.

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

Expected terminal state after successful S113C:

```text
alpha_runnable_windows_build_verification required
```

Do not mark the gate passed. The user/assistant will review the build/play-loop evidence before accepting it.

If real build/play-loop verification cannot run, keep the gate required and report the concrete blocker. Do not start Goal 014.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S114 or Goal 014.
- No Runtime Preview dependency as Alpha proof.
- No fake Windows executable, Unity build, launch or play-loop claim.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No WinForms/UI edits.
- No Visual Studio `.sln` or C# `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report

Report:

- changed files;
- visible loop behavior;
- automated play-loop command and log path;
- package/hash/ref evidence loaded by player;
- commands executed and state flags;
- Unity build command/version/log path;
- build output folder and executable relative path;
- `launchVerified` and `playLoopVerified`;
- report hash/build manifest hash;
- max artifact path/file name lengths;
- verification command results;
- confirmation that S114/Goal 014 and forbidden areas were not started.
