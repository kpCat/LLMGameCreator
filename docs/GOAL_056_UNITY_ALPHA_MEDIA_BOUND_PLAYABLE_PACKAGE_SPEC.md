# Goal 056 — Unity Alpha Media-Bound Playable Package

## Goal id

`goal_056_unity_alpha_media_bound_playable_package`

## Manual gate

`unity_alpha_media_bound_playable_package_verification required`

## Purpose

Move beyond Application-level media-bound package proof by making the repo-local Unity Alpha player consume Goal 055 media-bound package files through a narrow, deterministic, Windows-focused StreamingAssets path.

This is the next non-paper step after Goal 055:

```text
Goal 054 physical PNG/WAV/bundle fixtures
 -> Goal 055 staged media-bound review package
 -> Goal 056 Unity Alpha StreamingAssets staging
 -> Unity-side manifest/media loading
 -> visible/logged media panel proof
 -> automated player smoke evidence
```

## Non-goals

Goal 056 must not:

- call media providers;
- generate new AI images/audio;
- import network assets;
- change GamePackage schema;
- add new NuGet packages;
- add new Unity packages;
- implement general-purpose modding;
- implement cross-platform mobile/WebGL StreamingAssets;
- implement arbitrary audio codecs;
- change Runtime or Runtime.Abstractions source;
- change WinForms UI;
- change provider/LLM/RAG paths;
- execute Lua or generate Lua source;
- touch generator-library.

## Required outcome

A GREEN result requires all of these:

1. Goal 055 is recorded as accepted by user handoff before Goal 056.
2. Goal 056 stages the Goal 055 media-bound package into a Unity Alpha StreamingAssets-compatible payload.
3. The existing repo-local Unity Alpha player/runtime is narrowly extended so it can consume the media-bound manifest.
4. A real Unity-side load path is proven through automated logs or equivalent existing Unity Alpha diagnostic route:
   - manifest loaded;
   - PNG fixture loaded as texture or validated by Unity-side loader path;
   - WAV fixture loaded/parsed or validated by Unity-side loader path;
   - family media panel proof recorded for `map_panel_rpg`, `survival_sandbox`, `first_person_grid_dungeon`;
   - no missing/hash-stale/provenance-invalid files accepted.
5. Compact deterministic evidence is written under `.llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/`.
6. Focused tests, product smoke, `check-all.ps1`, and artifact scope guard pass.
7. Commit/push final state to `origin/main`.

If Unity execution/build is unavailable, do not fake GREEN. Commit/push BLOCKED with completed Application staging/proof work and exact environment blocker.

## Evidence folder

`.llmgc/procedural/goal-056-unity-alpha-media-bound-playable-package/`

Required compact artifacts:

- `source-evidence-manifest.json`
- `unity-streamingassets-staging-manifest.json`
- `media-bound-family-panel-models.json`
- `unity-media-load-contract.json`
- `unity-media-load-proof.json`
- `unity-alpha-media-bound-smoke-log-summary.json`
- `preview-export-media-bound-payloads.json`
- `staged-file-hash-inventory.json`
- `invalid-unity-media-bound-matrix.json`
- `unity-alpha-media-bound-playable-package-report.md`
- `artifact-scope-report.json`

The evidence may include a compact `review-package/` subtree with staged media files if the artifact-scope policy allows it. Heavy Unity build outputs, logs, caches and player folders should remain ignored or copied only as compact summaries.

## Expected proof language

The report must include:

```text
unity_alpha_media_bound_playable_package_verification required
accepted=false
implementationStatus=GREEN|BLOCKED|FAILED
goal055AcceptedByUserHandoff=true
unitySourceChanged=<true/false>
unityEditorOrPlayerExecuted=<true/false>
streamingAssetsPayloadStaged=true
manifestLoadedByUnityProof=<true/false>
pngLoadProofPassed=<true/false>
wavLoadProofPassed=<true/false>
familyMediaPanelProofPassed=<true/false>
invalidMatrixPassed=<true/false>
```

## Risk guard

This goal intentionally allows a narrow Unity Alpha change. It must not become a Unity rewrite. If more than a small loader/presentation/diagnostic extension is required, stop with BLOCKED.
