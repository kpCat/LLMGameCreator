# Manual Visible Generated Playable Preview Check

Use this after Product Slice 033 when you want to inspect the generated package manually.

## Generate Artifacts

Run:

```powershell
.\.devflow\scripts\run-product-smoke.ps1 -Scenario visible-generated-playable-preview
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

## Manual App Check

1. Open `LLMGameCreator.sln` in Visual Studio.
2. Start `LLMGameCreator.WinForms`.
3. Open the generated package/project path from the smoke run output.
4. Open the existing Runtime Preview or Runtime Simulator path.
5. Check that the generated package title, generated map/scene, regions, quests, mechanics and provenance are visible.
6. If the preview/simulator exposes map commands for the loaded package, try one movement or interaction.

No LLM, provider, Lua, Unity or media execution is required for this check.
