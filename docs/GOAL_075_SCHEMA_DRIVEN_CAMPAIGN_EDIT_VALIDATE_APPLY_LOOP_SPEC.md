# Goal 075 — Schema-Driven Campaign Edit/Validate/Apply Loop

## Intent

Move the Goal 074 review workspace from read-only inspection toward a real semi-manual authoring workflow.

This goal must prove that a user can select a campaign row, edit bounded schema-driven fields, validate the changes, apply or reject them, and receive deterministic before/after evidence without letting LLM output or free-form text become authoritative content.

## Non-goals

- No LLM/provider/RAG calls.
- No final prose generation.
- No public GamePackage schema changes.
- No Runtime/Runtime.Abstractions changes.
- No Unity gameplay expansion.
- No broad WinForms refactor.
- No external dependencies.
- No generic form generator framework.
- No editing of existing Goal 060-074 historical artifacts except writing new Goal 075 evidence.

## Required proof

Goal 075 must produce:

1. Application seam:
   - `src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop/**`
   - BCL-only models, source loader, edit catalog, validator, apply engine, rollback planner, evidence service, hash helper.

2. WinForms surface:
   - Bounded UserControl additions under `src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/**`.
   - Each new tab/sub-panel must be a separate UserControl.
   - No giant control class.
   - If adding a tab to the existing page is safe, add it.
   - If not safe, create controls and a binding contract and report BLOCKED for navigation integration.

3. Edit workflow:
   - select family/seed/row;
   - show editable schema fields;
   - build deterministic manual change-set;
   - build deterministic auto-suggestion change-set, but not from LLM;
   - validate conflicts/missing/unsafe values;
   - apply valid change-set to a derived row state;
   - reject invalid change-set causally;
   - rollback proof;
   - before/after diff;
   - save/load/replay proof.

4. Scenarios:
   - 9 rows: 3 families × 3 seeds.
   - At least one valid manual edit row per family.
   - At least one valid auto-suggestion row per family.
   - At least one rejected invalid edit row per family.
   - Edits must touch multiple domains across the matrix:
     - gameplay consequence;
     - living world/faction/NPC;
     - settlement/production;
     - narrative/event intent;
     - combat/magic/status;
     - weather/crisis pressure.

5. Evidence folder:
   - `.llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/`

Required artifacts:
- `edit-workspace-source-manifest.json`
- `editable-schema-field-catalog.json`
- `change-set-catalog.json`
- `validation-diagnostics-matrix.json`
- `apply-rollback-ledger.json`
- `row-before-after-diff-matrix.json`
- `preview-export-refresh-payload.json`
- `winforms-binding-inventory.json`
- `quality-gate-scan.json`
- `invalid-edit-diagnostics-matrix.json`
- `artifact-scope-report.json`
- `schema-driven-campaign-edit-validate-apply-loop-report.md`

6. Quality gate:
   - no changed/new C# file over maxLineLength 500;
   - scan new/changed Goal 075 C# plus Goal 074 WinForms touched files;
   - no one-line/minified source files;
   - no broad AlphaRuntimeBootstrap changes;
   - no marker-only fake proof;
   - no absolute local paths in evidence;
   - no timestamps unless already deterministic by project convention.

## Expected gate

`schema_driven_campaign_edit_validate_apply_loop_verification required`

Manual gate remains required and accepted=false at the end.
