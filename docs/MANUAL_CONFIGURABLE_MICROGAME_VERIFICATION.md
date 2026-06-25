# Manual Configurable Microgame Verification

Use this after Product Slice 042. Codex stops here and does not perform this manual UI check.

Generated per-run checklist:

```text
.llmgc/procedural/generated-microgame-variation/manual-configurable-microgame-verification.md
```

Manual steps:

1. Start `LLMGameCreator.WinForms`.
2. Open Runtime Preview.
3. For each generated variation report row, set the seed and preset in the Generate Preview controls.
4. Click `Generate Preview`.
5. Click `Start`.
6. Confirm goal progress changes in runtime after interaction.
7. Confirm challenge resolves.
8. Confirm reward is visible.
9. Confirm completion becomes completed.
10. Confirm variants differ in generated package labels or generated content.

Expected state marker after the user confirms this check:

```text
manual_configurable_microgame_verification: passed
```
