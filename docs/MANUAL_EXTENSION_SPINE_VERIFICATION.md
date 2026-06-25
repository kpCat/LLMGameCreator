# Manual Extension Spine Verification

Use this after Goal 003. Codex stops here and does not perform this manual UI check.

Generated per-run checklist:

```text
.llmgc/procedural/extension-spine/manual-extension-spine-verification.md
```

Manual steps:

1. Run the `extension-spine` product smoke scenario or inspect the latest generated report.
2. Open `.llmgc/procedural/extension-spine/extension-spine-scenario-report.json`.
3. Confirm the base scenario is accepted.
4. Confirm the extension scenario is accepted.
5. Confirm the extension scenario records `validated_rule_pack_existing_runtime_state`.
6. Confirm the extension scenario grants `item/extension_spine_badge`.
7. Confirm the extension scenario completes `objective/collect_extension_badge`.
8. Confirm `invalid-extension-validation-report.json` has validation errors.
9. Confirm no LLM, provider, Lua, Unity or media execution is recorded.

Expected state marker after the user confirms this check:

```text
manual_extension_spine_verification: passed
```
