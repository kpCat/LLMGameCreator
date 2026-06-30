# Goal 058 - Full Media-Bound Generator Campaign

## Purpose

Goal 058 turns the existing proof chain into a single deterministic campaign runner:

```text
profile/family seed
 -> strict draft/manifest/sandbox/Lua expansion facts
 -> world/chunk/runtime delta facts
 -> family simulatable loop facts
 -> full generator without media facts
 -> media materialization/review package facts
 -> Unity Alpha media-bound multi-family facts
 -> unified full media-bound generator campaign proof
```

This goal must be a real generated/simulatable/playable loop integration step, not another paper registry.

## Manual gate

```text
full_media_bound_generator_campaign_verification required
```

## What this goal must prove

- Goal 057 is accepted by user handoff before Goal 058 starts.
- Three game families are run through one campaign runner:
  - `map_panel_rpg`
  - `survival_sandbox`
  - `first_person_grid_dungeon`
- The campaign runner consumes the existing source artifacts from Goals 034-057 and writes a new unified campaign evidence set.
- The campaign runner creates a deterministic review package manifest and staged media/command payloads suitable for the existing Unity Alpha player route.
- The Unity Alpha proof must be real when the local route is available: editor/player execution with campaign-specific markers.
- If real Unity/player execution is unavailable, commit/push `BLOCKED`; do not fake proof.

## What this goal must not do

- Do not call providers, LLM, RAG, network, ComfyUI, Fooocus, asset services or live media import.
- Do not generate final prose as authoritative content.
- Do not change public GamePackage schema.
- Do not broaden Runtime or Unity architecture.
- Do not add dependencies.
- Do not execute arbitrary user Lua. Use only already accepted, repo-owned bounded Lua fixtures through existing paths if needed.

## Expected artifact folder

```text
.llmgc/procedural/goal-058-full-media-bound-generator-campaign/
```

## Required compact artifacts

At minimum:

```text
campaign-source-manifest.json
campaign-plan.json
family-run-map-panel-rpg.json
family-run-survival-sandbox.json
family-run-first-person-grid-dungeon.json
unified-review-package-manifest.json
unity-alpha-campaign-command-plan.json
unity-alpha-campaign-player-proof.json
preview-export-campaign-payload.json
campaign-package-compatibility-proof.json
invalid-campaign-diagnostics-matrix.json
artifact-scope-report.md
full-media-bound-generator-campaign-report.md
```

The implementation may add staged physical files under the Goal 058 artifact folder if needed, but heavy Unity build/log/cache outputs must remain ignored.

## Required report markers

The final report must include:

```text
implementationStatus=GREEN|BLOCKED|FAILED
accepted=false
manualGate=full_media_bound_generator_campaign_verification
sourceFactsConsumed=true|false
allFamiliesIncluded=true|false
campaignRunnerExecuted=true|false
reviewPackageManifestPassed=true|false
unityEditorOrPlayerExecuted=true|false
unityExitCode=<number|null>
playerExitCode=<number|null>
allCampaignMarkersMatched=true|false
invalidMatrixPassed=true|false
```

## Quality bar

GREEN means the campaign runner produced new deterministic campaign evidence and either reused the existing Unity Alpha route successfully or provided real player proof from the current route.

BLOCKED is the correct result if:

- prior artifacts cannot be loaded safely;
- Unity route cannot execute but is required by the task;
- campaign markers cannot be proven;
- source facts are inconsistent;
- package/media/runtime loop compatibility fails.

FAILED is correct if implementation cannot reach a coherent partial proof.
