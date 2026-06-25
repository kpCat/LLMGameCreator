# Manual Visible Generated Playable Preview Check

Use this after Product Slice 034 when you want to inspect the generated package manually through the in-app one-click workflow.

## Primary One-Click App Check

1. Open `LLMGameCreator.sln` in Visual Studio.
2. Start `LLMGameCreator.WinForms`.
3. Open `Runtime Preview`.
4. Press `Generate Preview`.
5. Wait for the log to report the generated package title/id, output folder and snapshot path.
6. Press `Старт`.
7. Check that the generated package title, generated map/scene, regions, quests, mechanics and provenance are visible.
8. Try one movement or interaction if the preview map exposes it.

Success means:

- the log reports `Generate Preview ready`;
- the generated package is loaded as the current package;
- Runtime Preview starts without browsing `.devflow/runs/...`;
- no cross-thread WinForms exception appears after `Generate Preview`;
- generated content summary and browser show counts, representative ids and readable details;
- movement/interaction remains available for the generated package.

If generation fails, inspect the Runtime Preview log. It should show deterministic diagnostic codes and the output folder when available.

Artifacts are written under the current project folder when a project is open. If no project folder is available, the workflow writes under the user-local `LLMGameCreator/one-click-generated-preview` folder.

Expected generated-preview files:

- `.llmgc/procedural/generated-game-plan.json`
- `.llmgc/procedural/formula-effect-action-rule-pack.json`
- `.llmgc/procedural/tiny-runtime-loop-state.json`
- `.llmgc/procedural/generated-package-mvp/package.json`
- `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-snapshot.json`
- `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-report.json`
- `.llmgc/procedural/visible-generated-playable-preview/visible-generated-playable-preview-report.md`
- `.llmgc/procedural/visible-generated-playable-preview/manual-verification.md`
- `.llmgc/procedural/generated-microgame-loop/generated-microgame-loop-snapshot.json`
- `.llmgc/procedural/generated-microgame-loop/generated-microgame-loop-report.md`
- `.llmgc/procedural/generated-microgame-loop/manual-microgame-loop-verification.md`
- `.llmgc/procedural/runtime-backed-microgame-state/runtime-backed-microgame-state-snapshot.json`
- `.llmgc/procedural/runtime-backed-microgame-state/runtime-backed-microgame-state-report.md`
- `.llmgc/procedural/runtime-backed-microgame-state/manual-runtime-backed-microgame-verification.md`

## Product Slice 037 Manual Microgame Loop Check

Use this after the `generated-microgame-loop` smoke passes.

1. Start `LLMGameCreator.WinForms`.
2. Open `Runtime Preview`.
3. Press `Generate Preview`.
4. Press `Start`.
5. Confirm the active generated goal and current objective are readable.
6. Move to the generated NPC/object/item marker and use the existing interaction command.
7. Confirm progress, challenge, reward and completion state are visible.

The generated microgame acceptance sidecar also writes `.llmgc/procedural/generated-microgame-loop/manual-microgame-loop-verification.md` with the exact generated labels from the latest headless acceptance run.

## Product Slice 040 Manual Runtime-Backed Microgame Check

Use this after the `runtime-backed-microgame-state` smoke passes.

1. Start `LLMGameCreator.WinForms`.
2. Open `Runtime Preview`.
3. Press `Generate Preview`.
4. Press `Start`.
5. Confirm the active generated goal is readable and backed by runtime quest/objective state.
6. Move to the generated NPC/object/item marker and use the existing interaction command.
7. Confirm interaction advances runtime-owned goal progress.
8. Confirm challenge resolution, reward and completion show runtime-backed state evidence.
9. If runtime snapshot controls are available, save and reload the generated runtime state.

The runtime-backed acceptance sidecar writes `.llmgc/procedural/runtime-backed-microgame-state/manual-runtime-backed-microgame-verification.md` with the exact generated labels and snapshot evidence from the latest headless acceptance run.

## Optional Headless Smoke

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
.\.devflow\scripts\run-product-smoke.ps1 -Scenario generated-microgame-loop
.\.devflow\scripts\run-product-smoke.ps1 -Scenario runtime-backed-microgame-state
```

The smoke writes preview artifacts under:

```text
.devflow/runs/<run>-product-smoke/package-output/.llmgc/procedural/visible-generated-playable-preview/
```

Expected files:

- `visible-generated-playable-preview-snapshot.json`
- `visible-generated-playable-preview-report.json`
- `visible-generated-playable-preview-report.md`
- `manual-verification.md`

The generated package MVP is written under:

```text
.devflow/runs/<run>-product-smoke/package-output/.llmgc/procedural/generated-package-mvp/package.json
```

No LLM, provider, Lua, Unity or media execution is required for this check.
