# Goal 014 - Unity Playable Presentation And Firewall-Safe Build

## Purpose

Start only after the user/assistant explicitly confirms:

```text
alpha_runnable_windows_build_verification passed
```

Goal 013 proved the first runnable Unity Alpha:

```text
accepted generated package/assets/export evidence
-> repo-local Unity template
-> real Windows player build
-> diagnostic launch
-> minimal visible IMGUI play-loop proof
```

Goal 014 must move from diagnostic proof to a visibly playable Unity Alpha slice while hardening the build so local Windows Firewall prompts are not caused by unnecessary Unity development/profiler/debug networking.

Required product chain:

```text
accepted Goal 013 Alpha evidence
-> generated package/asset/runtime-config loaded by Unity player
-> visible map/player/NPC/item/quest/dialogue presentation
-> keyboard-driven movement and interaction
-> generated command/state loop reflected visually
-> release-style Windows player build with no Development/Profiler/Debug networking path
-> product smoke and artifact evidence
```

This goal is still Alpha. It must not become a broad Unity game rewrite.

## Final Gate

Stop at exactly one final gate:

```text
unity_playable_presentation_firewall_safe_build_verification
```

Do not mark it passed. The user/assistant will review the evidence.

Do not create S122, Goal 015 or post-Goal-014 work.

## Product Slices

- S114: record Goal 013 gate and define Unity playable presentation/firewall-safe build contracts.
- S115: load selected package/runtime config/assets into a reusable Unity Alpha presentation model.
- S116: render a simple generated map, player marker, NPC marker, item marker and status panels.
- S117: add keyboard interaction: movement, focus/select nearby entity, advance quest/dialogue/item/event commands.
- S118: synchronize visible state with generated command hints and package refs.
- S119: harden Unity Windows build settings to avoid development/profiler/debug network prompts.
- S120: automated launch/play smoke, deterministic artifacts and invalid/fake/leak rejection.
- S121: product smoke, state handoff, final verification and next-gate stop.

## Architecture Boundary

### Unity is presentation/runtime host, not generator authority

Unity must consume generated package/export/asset evidence. It must not invent game content, call LLM/RAG, run arbitrary Lua, or generate external media.

Allowed Unity work:

- simple presentation components;
- input handling;
- local deterministic state derived from loaded package/config;
- visual status panels;
- automated diagnostic smoke mode.

Forbidden Unity work:

- broad engine rewrite;
- new public GamePackage/runtime schema redesign;
- Runtime Preview dependency as proof;
- generated C# mechanics from LLM output;
- online/multiplayer/networking/profiler/debug services.

### Firewall-safe build discipline

The user reports repeated Windows Firewall prompts for Unity EXEs. Goal 014 must reduce this by removing unnecessary development/profiler/debug networking from the produced player.

Required:

- build the normal Alpha player without `BuildOptions.Development`;
- do not use `BuildOptions.ConnectWithProfiler`;
- do not use `BuildOptions.AllowDebugging`;
- do not enable script debugging/autoconnect profiler/player-connection paths;
- do not add networking packages or listeners;
- keep output executable name and output folder stable for the Goal artifact;
- record build options in report;
- add tests/static checks that reject development/profiler/debug build flags in the Alpha build entrypoint.

Do not add broad Windows Firewall allow rules. If a firewall prompt still appears, report exact evidence and keep the gate required; do not mask it with unsafe firewall configuration.

## Required Artifact Shape

Create deterministic artifacts under:

```text
.llmgc/procedural/unity-playable-alpha/
```

Required final artifact files:

- `unity-playable-alpha-report.json`
- `unity-playable-alpha-report.md`
- `unity-playable-alpha-verification.md`

Required build/output folders:

```text
.llmgc/procedural/unity-playable-alpha/staging/
.llmgc/procedural/unity-playable-alpha/build/windows/
.llmgc/procedural/unity-playable-alpha/logs/
```

The report must include:

- previous accepted gate `alpha_runnable_windows_build_verification passed`;
- completed slices S114-S121;
- selected package id/hash, asset manifest hash, export/runtime config hashes;
- visible presentation evidence;
- movement/interaction/play evidence;
- Unity version, project relative path and build script relative path;
- build output folder and executable relative path;
- build options/profiler/debug/firewall-safety flags;
- launch/play logs and parsed state;
- invalid/fake/leak matrix;
- `windowsExecutableProduced`, `unityBuildProduced`, `launchVerified`, `visiblePresentationVerified`, `movementVerified`, `interactionVerified`, `playLoopVerified`, `firewallSafeBuildVerified`;
- explicit no external provider/LLM/RAG/Lua/media flags;
- public schema/project-file change flags.

Deterministic JSON artifacts must not contain absolute local paths, timestamps, GUIDs, machine names, temp paths or user names. Verification markdown may include exact local commands and paths when needed.

## S114 - Contract And Gate Record

Define narrow Application contracts for:

- Unity playable Alpha request;
- selected Alpha package/runtime evidence;
- presentation model;
- visible scene proof;
- movement/interaction proof;
- firewall-safe build proof;
- playable Alpha artifact report.

Record:

```text
alpha_runnable_windows_build_verification passed
```

Required validation:

- source hashes match accepted Goal 013 evidence;
- selected refs are non-empty and resolve;
- build output is a real Windows player;
- presentation/movement/interaction evidence comes from player logs or screenshot-independent state logs;
- firewall-safe build flags reject development/profiler/debug networking options;
- malformed/fake evidence rejects causally.

## S115 - Unity Presentation Model

Load the staged `unity-runtime-config.json`, `game-package.json` and `asset-manifest.json` into a Unity-side presentation model.

Required:

- package id/hash;
- selected thread id;
- start map id;
- selected quest/dialogue/item/event ids;
- command hints in deterministic order;
- asset refs and categories;
- compact display names where available.

Avoid regex-only parsing when a small Unity-compatible JSON parser or simple typed extraction is already available locally. If no parser is available and adding a package is not justified, keep the parser minimal and covered by focused tests.

## S116 - Visible Generated Scene

Render a simple scene from loaded data:

- tile/grid or map panel;
- player marker;
- NPC marker;
- item marker;
- quest/dialogue/event/status panel;
- command log panel;
- visual indication of selected style/package.

This may be IMGUI or simple built-in Unity primitives. Do not add art generation or third-party UI packages.

Required evidence:

- player log proves visible presentation initialized;
- report records visible map/player/NPC/item/status components;
- no Runtime Preview dependency.

## S117 - Movement And Interaction

Add minimal input:

- WASD or arrow keys move the player marker inside the generated map bounds;
- Space/Enter interacts with the current/nearby generated target;
- Tab or another simple key cycles focus if needed;
- R resets the scene state;
- Esc quits.

Automated mode must simulate enough input/steps to prove:

- initial player position;
- at least two valid movement steps;
- blocked/out-of-bounds movement or bounds validation;
- focus/select target;
- interaction advances quest/dialogue/item/event state;
- visible status/log changes.

## S118 - Generated Command State Sync

Tie interaction steps to selected command hints:

- `quest/start` sets quest state and visible quest status;
- `dialogue/open` and `dialogue/choose` set dialogue state and visible dialogue/status text;
- `loot/roll` or item command sets item state and visible inventory/status text;
- event command applies event state and visible event/status text.

Required:

- command ids/types/targets are logged in order;
- state flags match the loaded command hints;
- selected refs are resolved where structurally possible;
- fake or mismatched command logs reject.

## S119 - Firewall-Safe Windows Build

Modify the Unity build entrypoint narrowly:

- remove Development build mode if currently used;
- use release-style build options unless a specific test/proof requires otherwise;
- keep no profiler/debug/player-connection flags;
- record build options in a machine-readable build metadata file;
- ensure automated smoke launch uses batch/no-graphics diagnostic flags but does not open any network listener intentionally.

Add focused tests/static checks:

- `AlphaBuildEntrypoint.cs` does not use `BuildOptions.Development`;
- does not use `BuildOptions.ConnectWithProfiler`;
- does not use `BuildOptions.AllowDebugging`;
- does not mention profiler autoconnect or script debugging options;
- product smoke report records `firewallSafeBuildVerified=true` only when these checks pass.

If a Windows Firewall prompt still appears during user/manual runs, document exact conditions and keep the gate required. Do not create broad firewall rules.

## S120 - Automated Smoke And Invalid Matrix

Automated player smoke must:

- build the Windows player;
- launch it in diagnostic mode;
- simulate movement and interaction;
- verify visible presentation state;
- verify generated command state transitions;
- verify firewall-safe build metadata/static checks;
- write logs under `.llmgc/procedural/unity-playable-alpha/logs/`;
- fail closed on missing/forged logs.

Invalid/fake/leak matrix must reject at minimum:

- missing accepted Goal 013 evidence;
- package hash mismatch;
- asset manifest hash mismatch;
- missing runtime config;
- missing visible presentation log;
- missing movement proof;
- missing interaction proof;
- command order mismatch;
- fake play-loop success without commands;
- build output missing executable;
- executable with invalid PE/MZ header;
- missing StreamingAssets payload;
- Development/Profiler/Debug build option detected;
- Runtime Preview dependency claim;
- cross-style package/export/asset leakage.

## S121 - Product Smoke And State Handoff

Add product smoke route:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-playable-alpha
```

Product smoke must regenerate and validate the Goal 014 artifacts.

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

State must show:

- Goal 014 completed to `unity_playable_presentation_firewall_safe_build_verification required`;
- no S122/Goal 015 started;
- Goal 013 gate recorded as passed.

## Required Verification Commands

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~UnityPlayableAlpha|FullyQualifiedName~AlphaRunnableBuild|FullyQualifiedName~CurrentGeneratorStateDocsTests"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-playable-alpha
.\.devflow\scripts\check-all.ps1
```

Also run/report the exact Windows player diagnostic command used for automated movement/interaction verification.

Report:

- Unity command/version/log path;
- build options;
- build output folder;
- executable relative path;
- launch/play log paths;
- movement and interaction proof;
- firewall-safe build proof;
- whether any firewall prompt was observed.

Also scan changed/generated files for:

- mojibake markers;
- path-length contract violations;
- absolute local paths in deterministic artifacts;
- timestamps, GUIDs, machine names, temp paths and user names in deterministic artifacts;
- exact `S122|Goal 015|goal_015` markers, excluding explicit prohibition text.

## Stop Conditions

Stop instead of weakening acceptance when this requires:

- public GamePackage/runtime schema redesign;
- broad Unity engine rewrite;
- WinForms/UI edits;
- Runtime Preview proof;
- external provider/media/LLM/RAG/Lua/generator-library execution;
- broad Windows Firewall allow rules;
- fake movement/interaction/play evidence;
- `.sln` or C# `.csproj` edits.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S122 or Goal 015.
- No Runtime Preview dependency as Unity playable proof.
- No fake Windows executable, Unity build, movement, interaction or firewall-safe claim.
- No broad Windows Firewall allow rules.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No WinForms/UI edits.
- No Visual Studio `.sln` or C# `.csproj` edits.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report Requirements

Report:

- completed slices;
- changed files;
- selected package/style and hashes;
- visible scene behavior;
- movement/interaction evidence;
- Unity build command/version/log path;
- build output folder and executable relative path;
- firewall-safe build flags and whether any firewall prompt was observed;
- invalid/fake/leak scenario counts;
- deterministic report hash and build manifest hash;
- verification command results;
- confirmation that S122/Goal 015 and forbidden areas were not started.

Then stop at the single final gate without marking it passed.
