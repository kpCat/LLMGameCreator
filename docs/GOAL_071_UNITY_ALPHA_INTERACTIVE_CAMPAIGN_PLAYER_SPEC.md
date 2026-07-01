# Goal 071 — Unity Alpha Interactive Campaign Player

## Goal

Convert the current Unity Alpha proof chain from mostly marker-driven campaign proofs into a bounded interactive review player that consumes Goal 070 integrated campaign timeline evidence.

## Required outcome

The repo must prove that a built Unity Alpha player can:

1. load a staged Goal 071 interactive campaign package from StreamingAssets;
2. expose three family rows and three seed rows through a selectable review surface;
3. accept deterministic input/action commands;
4. advance state through at least one multi-step timeline for each family;
5. display or log current family, seed, step, action, state hash, and key deltas;
6. prove save/load/replay compatibility through the Application seam;
7. emit player logs with deterministic `interactive_campaign_*` markers;
8. preserve all prior package/media/runtime/world/gameplay proof boundaries.

## Families

- `map_panel_rpg`
- `survival_sandbox`
- `first_person_grid_dungeon`

## Required artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-071-unity-alpha-interactive-campaign-player/
```

Required files:

```text
interactive-campaign-source-manifest.json
interactive-campaign-row-matrix.json
interactive-campaign-command-plan.json
interactive-campaign-input-script.json
interactive-campaign-state-transition-ledger.json
interactive-campaign-save-load-replay-proof.json
interactive-campaign-hud-contract.json
interactive-campaign-player-proof-summary.json
interactive-campaign-invalid-diagnostics-matrix.json
interactive-campaign-preview-export-payload.json
interactive-campaign-artifact-scope-report.json
unity-alpha-interactive-campaign-player-report.md
```

The final report must include:

```text
unity_alpha_interactive_campaign_player_verification required
accepted=false
implementationStatus=GREEN/BLOCKED/FAILED
```

## Strict limits

Allowed narrow Unity change:

```text
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

No broad Unity restructuring. No prefabs/scenes/assets unless the task proves they already exist and are necessary.

Do not touch:

- public GamePackage schema;
- Runtime / Runtime.Abstractions source;
- WinForms UI;
- provider/LLM/RAG/media generation paths;
- Lua execution paths;
- generator-library;
- `.sln` / `.csproj`;
- external dependencies.

## Quality bar

GREEN requires a real Unity editor/player route with exit code 0 and matched markers proving interactive command execution, not only JSON generation.

If the Unity/player route is unavailable, commit/push `BLOCKED` with exact diagnostics.
