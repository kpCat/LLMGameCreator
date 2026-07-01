# External scouting — Goal 075 Schema-Driven Campaign Edit/Validate/Apply Loop

## Decision

No new external dependencies for Goal 075.

The repository already has a WinForms surface and Application-layer schema workspace from Goal 074. Goal 075 should extend that into a bounded edit/validate/apply workflow using existing WinForms controls, BCL-only Application models, deterministic evidence and product smoke.

## Considered but deferred

- WinForms third-party grids/property grids: deferred. Goal 075 should not add UI dependencies.
- JSON Schema/NJsonSchema: deferred. Goal 075 needs a domain-specific authoring schema/change-set model, not a generic JSON Schema dependency.
- Reactive/MVVM helpers: deferred. The current WinForms pages should remain simple UserControls with explicit binding and validation.
- LLM/provider integration: forbidden. Goal 075 is manual/auto deterministic authoring only.

## Relevant repository context

Goal 074 introduced:
- `SchemaDrivenCampaignAuthoringReviewWorkspace` Application seam.
- WinForms `CampaignAuthoringReviewWorkspace` controls.
- Quality scanner covering Goal 074 C# files including `CompositionRoot.cs`.
- Evidence under `.llmgc/procedural/goal-074-schema-driven-campaign-authoring-review-workspace/`.

Goal 075 should consume that workspace and prove:
- editable schema fields;
- manual/auto change-set candidates;
- validation diagnostics before apply;
- deterministic apply/rollback;
- row-level before/after diff;
- preview/export payload refresh;
- UI binding proof through bounded WinForms controls;
- no LLM final content, no provider calls, no GamePackage schema changes.
