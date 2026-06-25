# Manual Extension Spine Verification

Use this after Goal 003. Codex stops here and does not perform this manual UI check.

1. Review `.llmgc/procedural/extension-spine/extension-spine-scenario-report.json`.
2. Confirm the base scenario and extension scenario are both accepted.
3. Confirm the extension scenario records `validated_rule_pack_existing_runtime_state`.
4. Confirm invalid extension validation contains errors.
5. If desired, run Runtime Preview with the extension scenario seed/preset and compare generated labels with the report.

Headless acceptance status: `true`
Next state marker: `manual_extension_spine_verification`
