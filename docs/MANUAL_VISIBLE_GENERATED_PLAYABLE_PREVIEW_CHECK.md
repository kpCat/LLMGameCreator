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

## Optional Headless Smoke

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario one-click-generated-preview-workflow
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
