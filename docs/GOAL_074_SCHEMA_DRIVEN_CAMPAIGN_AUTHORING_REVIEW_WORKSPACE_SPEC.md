# Goal 074: Schema-Driven Campaign Authoring And Review Workspace

## Purpose
Turn the recent full-campaign proof chain into a practical editor-facing review and authoring workspace.

This goal must not be another evidence-only layer. It must create a schema-driven workspace contract plus a bounded WinForms UI surface that can inspect the 9 family/seed rows, package/materialization artifacts, media bindings, gameplay state deltas, narrative/combat/settlement/world-event/living-world ledgers, and quality/debt status.

The goal is not to let LLM write content. The workspace may expose quarantined candidates and manual/auto provenance, but final content remains programmatically validated.

## Expected gate
`schema_driven_campaign_authoring_review_workspace_verification required`

## Required proof
- Goal 073 is accepted by user handoff before Goal 074.
- Goal 072 remains historical BLOCKED/progress evidence, not passed.
- Goal 031/032 remain produced-for-review/not passed.
- 9 family/seed rows are visible through workspace model.
- Workspace schema is dynamic: field groups and diagnostics come from metadata, not one hardcoded mega-form.
- WinForms controls exist as separate UserControls.
- UI binding contract proves the editor can render/select/filter/inspect rows without provider/LLM/RAG calls.
- Focused tests and product smoke prove workspace model, UI binding contract, and source artifact loading.
- No GamePackage schema, Runtime, Unity, provider/LLM/RAG, Lua execution, media generation or external dependencies are changed.
