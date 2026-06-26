# Goal 013 - Alpha Runnable Windows Build

## Purpose

Start only after the user/assistant explicitly confirms:

```text
unity_runtime_export_vertical_slice_artifact_verification passed
```

Goal 012 proved deterministic Unity-runtime export payloads outside Runtime Preview:

```text
generated GamePackage + resolved asset manifest
-> materialized Unity-runtime export folder
-> real file/hash manifest
-> headless runtime contract validation
```

Goal 013 is the first minimum useful Alpha integration goal. It must turn the accepted export evidence into a real runnable Windows build candidate and prove it honestly.

Required product chain:

```text
accepted content generation evidence
+ accepted asset pipeline evidence
+ accepted Unity runtime export evidence
-> Alpha build candidate selection for three game styles
-> Unity build staging with game data and assets
-> real Windows player folder/executable when Unity build tooling is available
-> launch/play verification evidence
-> deterministic artifact report and product smoke
```

This goal must not pass by producing only JSON, an export folder, a Unity project stub, a copied expectation report, or Runtime Preview evidence.

## Final Gate

Stop at exactly one final gate when the runnable build is actually produced and verified:

```text
alpha_runnable_windows_build_verification
```

If the installed environment cannot produce or launch a real Windows Unity player, stop with the real blocker:

```text
alpha_unity_build_environment_blocker
```

Only use the blocker for a concrete missing tool/runtime/crash condition after checking the repository and local environment. Do not weaken the final gate.

Do not create S114, Goal 014 or post-Goal-013 work.

## Product Slices

- S106: record Goal 012 gate and define Alpha runnable build contracts.
- S107: select deterministic Alpha inputs for three game styles from accepted Goal 010/011/012 evidence.
- S108: audit or create the narrow Unity runtime build staging contract without Runtime Preview dependency.
- S109: materialize Alpha build staging with game data, assets, config, provenance and launch metadata.
- S110: produce a real Windows Unity player folder/executable through available headless Unity build tooling.
- S111: validate the build artifact structurally, by hashes, by content binding and by environment provenance.
- S112: launch and verify a minimum playable loop, or stop with an exact blocker if launch is impossible in the environment.
- S113: invalid/fake/leak rejection, product smoke, artifacts, state handoff and final verification.

## Architecture Boundary

### Alpha is not Runtime Preview

Do not use the WinForms Runtime Preview as proof of playability. Existing runtime/package services may be reused headlessly for validation, but the accepted artifact must be a runnable Windows build folder/executable or a real blocker.

### Unity execution is allowed only for this goal

Goal 013 may execute Unity Editor or Unity CLI only when it is:

- local and already installed;
- headless or noninteractive where possible;
- scoped to producing or validating the Alpha build;
- fully logged and reported with command, version, project path and output folder;
- free of external provider, LLM, RAG, arbitrary Lua or media generation calls.

If Unity is unavailable, missing required modules, fails licensing checks, or cannot build a Windows player in the environment, stop with `alpha_unity_build_environment_blocker` and report exact next steps for the user.

### No false runnable claim

Do not claim any of the following unless actually produced and verified:

- Windows `.exe`;
- Unity player build;
- successful process launch;
- 15-30 minute playable Alpha loop;
- Unity Editor execution.

If only staging/export validation is possible, report blocker instead of marking `alpha_runnable_windows_build_verification` as ready.

### Provider and media boundary

Do not call LLM, RAG, ComfyUI, Suno, image/audio providers, arbitrary Lua, generator-library scripts or external media generation. Goal 013 consumes deterministic package/export/assets already produced by prior goals.

## Required Artifact Shape

Create deterministic artifacts under:

```text
.llmgc/procedural/alpha-runnable-build/
```

Required final artifact files:

- `alpha-runnable-build-report.json`
- `alpha-runnable-build-report.md`
- `alpha-runnable-build-verification.md`

Required build/staging folders:

```text
.llmgc/procedural/alpha-runnable-build/staging/
.llmgc/procedural/alpha-runnable-build/build/windows/
```

The report must include:

- previous accepted gate `unity_runtime_export_vertical_slice_artifact_verification passed`;
- completed slices S106-S113;
- selected package ids, export hashes and asset manifest hashes;
- three game-style build candidates and the selected runnable candidate;
- Unity editor/CLI discovery result, exact command, version and module evidence;
- build folder relative path, executable relative path and file manifest;
- hashes and byte counts from actual files;
- launch/play verification evidence or blocker diagnostics;
- invalid/fake/leak matrix with actual rejected scenarios;
- `windowsExecutableProduced`, `unityEditorExecuted`, `unityBuildProduced`, `launchVerified` and `playLoopVerified` booleans;
- explicit flags proving no LLM/RAG/provider/media/arbitrary Lua/generator-library execution;
- public schema/project-file change flags.

Use UTF-8 without BOM and stable JSON ordering where the local codebase pattern supports it. No absolute paths, timestamps, GUIDs, machine names, temp paths or user names may enter deterministic artifacts. Non-deterministic environment details may appear only in the verification markdown, not in deterministic report hashes.

## S106 - Contract And Gate Record

Define narrow versioned Application contracts for:

- Alpha build request;
- Alpha input candidate;
- Unity build environment probe;
- build staging manifest;
- Windows build output manifest;
- launch/play verification result;
- Alpha artifact report.

Required validation:

- schema/version ids;
- source package hash, asset manifest hash and export manifest hash;
- safe relative paths only;
- bounded file count and byte budget;
- executable path must be produced by actual build output;
- no Runtime Preview host dependency;
- no provider/media/LLM/RAG/Lua/generator-library execution;
- malformed input produces deterministic diagnostics.

Record the user-confirmed previous gate:

```text
unity_runtime_export_vertical_slice_artifact_verification passed
```

## S107 - Three Style Alpha Input Selection

Select deterministic inputs for three game styles using accepted prior evidence.

Required styles are the existing generated families, unless the accepted evidence uses different canonical ids:

- frontier survival;
- gothic mystery;
- trade caravan.

For each style, select:

- generated package id and package hash from Goal 010-style evidence;
- matching asset manifest/hash from Goal 011-style evidence;
- matching Unity runtime export manifest/hash from Goal 012-style evidence;
- selected loop refs: map, NPC, quest, dialogue, item, event and runtime command hints.

Required behavior:

- selection is deterministic and replay-stable;
- selected ids resolve in package, asset manifest and export payload;
- cross-style package/export/asset leakage rejects causally;
- one candidate is selected as the primary runnable build candidate using a deterministic rule;
- missing or mismatched prior evidence rejects causally.

## S108 - Unity Runtime Build Staging Contract

Audit existing Unity export/materialization services and any existing Unity runtime project/template seams.

If an existing Unity project/template is present:

- reuse the narrowest build path;
- do not redesign runtime/gameplay schemas;
- do not introduce broad Unity gameplay generation.

If no Unity project/template capable of building a player exists:

- create only the minimal repo-local staging/build contract and tests needed to prove the blocker honestly;
- stop with `alpha_unity_build_environment_blocker` unless a real build can still be produced from an existing supported path.

Required runtime contract coverage:

- package identity and version;
- start map/scene id;
- player spawn/bootstrap state;
- map/tile/entity/item/dialogue/quest refs needed by the selected loop;
- selected asset refs for tile/region, NPC portrait, item/icon, sound effect and music/ambience;
- launch metadata and save-state bootstrap compatible with existing runtime state ownership;
- no Runtime Preview dependency.

Do not generate C# gameplay mechanics from LLM output.

## S109 - Build Staging Materialization

Materialize deterministic staging under:

```text
.llmgc/procedural/alpha-runnable-build/staging/
```

Required behavior:

- all staged paths are relative and contained under the staging root;
- stage includes game data, asset payloads, runtime config, provenance and launch metadata;
- files are copied/materialized from accepted Goal 012 export evidence, not reconstructed from prose;
- every staged file has a hash and byte count;
- identical input produces identical staging manifest and hashes;
- different valid style candidate produces meaningful hash difference;
- staging folder can be deleted and regenerated cleanly.

## S110 - Real Windows Unity Build

Attempt a real Windows build only through available repository-local or installed Unity tooling.

Required behavior when build tooling is available:

- discover Unity executable path/version without hardcoding machine-specific paths into deterministic artifacts;
- run the narrowest headless build command supported by the repository/project;
- output to `.llmgc/procedural/alpha-runnable-build/build/windows/`;
- collect build logs under `.llmgc/procedural/alpha-runnable-build/logs/`;
- report exact command in verification markdown;
- verify that the `.exe` and companion build files physically exist.

If Unity CLI/editor/build module is unavailable or fails before producing a player:

- do not synthesize an executable;
- do not mark the runnable gate;
- write blocker diagnostics;
- stop with `alpha_unity_build_environment_blocker`;
- include exact user steps to install/enable the required Unity version/modules or to run the build command locally.

## S111 - Build Artifact Validation

Validate the build folder from actual bytes.

Required validation:

- executable exists under the build output folder;
- no build manifest paths escape the build output folder;
- no absolute paths, timestamps, GUIDs, machine names or temp/user paths in deterministic reports;
- build manifest hashes and byte counts match actual files;
- staged package/export/asset hashes match accepted evidence;
- build includes required game data, asset payload and runtime config;
- build metadata does not claim unsupported external services;
- public GamePackage/runtime schemas are unchanged unless an explicit blocker explains otherwise.

Invalid scenarios must include at minimum:

- missing accepted Goal 012 evidence;
- package hash mismatch;
- asset manifest hash mismatch;
- export manifest hash mismatch;
- missing staged game data;
- missing staged asset payload;
- missing executable;
- mismatched executable/build file hash;
- path traversal in staging/build manifest;
- absolute output path injection;
- copied expectation report without build files;
- Runtime Preview dependency claim;
- Unity build claim without artifact;
- cross-style package/export/asset leakage.

## S112 - Launch And Play Verification

Verify the produced Windows player honestly.

Preferred verification:

- launch the built executable in a deterministic smoke/diagnostic mode if the runtime supports it;
- capture process exit code, bounded runtime duration and log file;
- verify startup, content load, selected loop bootstrap and at least one interaction/command path.

If the build requires interactive graphics and cannot be automated in the current environment:

- keep `alpha_runnable_windows_build_verification` required;
- write exact manual user steps in `alpha-runnable-build-verification.md`;
- include expected file paths, launch command, what to click/do, what success/failure looks like and where logs are written;
- report this as blocker unless the repository already contains a reliable automated launch proof.

Do not claim 15-30 minute play success unless it was actually executed or the user later reports it and a subsequent task records that evidence.

## S113 - Product Smoke, Artifacts And State Handoff

Add product smoke routing:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario alpha-runnable-build
```

Product smoke must:

- regenerate or validate the Alpha artifacts;
- verify previous accepted gate;
- verify three style candidates and primary selection;
- verify staging files and build output files when produced;
- verify no Runtime Preview dependency;
- verify invalid/fake/leak matrix rejects causally;
- verify final report is deterministic where intended.

Update:

- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CONTEXT_INDEX.md`

State must show:

- Goal 013 completed only if the real build evidence exists;
- final gate `alpha_runnable_windows_build_verification` remains `required`, not `passed`;
- blocker status if the build environment prevents a real Windows player;
- no S114/Goal 014 started.

## Required Verification Commands

Run the narrow filtered suite:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~AlphaRunnableBuild|FullyQualifiedName~UnityRuntimeExport|FullyQualifiedName~MinimumAssetPipeline|FullyQualifiedName~ContentGenerationScale|FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Run product smoke:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario alpha-runnable-build
```

Run full verification:

```powershell
.\.devflow\scripts\check-all.ps1
```

If a Unity build command is executed, also report:

- exact Unity command;
- Unity version;
- project path;
- output folder;
- build log path;
- executable relative path;
- launch command and result.

After docs edits, rerun docs guard if it is not already included in the filtered/full commands.

Also scan changed/generated files for:

- mojibake markers;
- absolute local paths;
- timestamps, GUIDs, machine names, temp paths and user names in deterministic artifacts;
- exact `S114|Goal 014|goal_014` markers, excluding Goal/task prohibition text.

## Stop Conditions

Stop instead of weakening acceptance when the runnable Alpha requires:

- public GamePackage/runtime schema redesign;
- broad WinForms/UI work;
- Runtime Preview as the proof host;
- external provider/media/LLM/RAG/Lua/generator-library execution;
- fabricated Unity build output;
- copied expectation reports without physical files;
- a `.sln` or `.csproj` edit not proven necessary by a focused test or build failure.

Use `alpha_unity_build_environment_blocker` for real Unity installation/module/licensing/build-host issues. Include exact user steps and commands.

## Hard Bans

- No git commands or branch/merge/push/rebase/cherry-pick instructions.
- No S114 or Goal 014.
- No Runtime Preview dependency as Alpha proof.
- No false Windows executable, launch or play claim.
- No external asset/media generation, ComfyUI, Suno, LLM/RAG/provider or arbitrary Lua execution.
- No generator-library edits.
- No public GamePackage/runtime schema redesign.
- No `.sln` or `.csproj` edits unless a focused failing test/build proves it is strictly required and the final report calls it out.
- No `/mnt`, `/home/oai`, `sandbox:/...`, `C:\mnt` or fabricated Linux paths. Use repository-relative Windows/PowerShell paths.

## Final Report Requirements

Report:

- completed slices;
- changed files;
- whether final gate or blocker was reached;
- selected three style candidates and primary build candidate;
- package/export/asset hashes;
- build output folder and executable relative path if produced;
- Unity command/version/log path if executed;
- launch/play verification result or blocker details;
- invalid/fake/leak scenario counts and causal diagnostic summary;
- deterministic report hash and build manifest hash;
- verification command results;
- confirmation that S114/Goal 014 and forbidden areas were not started.

Then stop at the single final gate without marking it passed, or at the real blocker without creating post-Goal-013 work.
