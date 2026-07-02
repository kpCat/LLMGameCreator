# Read-first list

Read these files before changing anything:

- docs/CURRENT_GENERATOR_STATE.md
- docs/CURRENT_GENERATOR_STATE.json
- docs/CONTEXT_INDEX.md
- docs/FULL_GENERATOR_GOAL_QUEUE.md
- docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md, if present
- .devflow/artifact-scope/artifact-scope-policy.json
- .llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/schema-driven-campaign-edit-validate-apply-loop-report.md
- .llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/winforms-binding-inventory.json
- .llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/quality-gate-scan.json
- .llmgc/procedural/goal-075-schema-driven-campaign-edit-validate-apply-loop/edit-workspace-source-manifest.json
- src/LLMGameCreator.Application/Design/SchemaDrivenCampaignEditValidateApplyLoop/**
- src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/**
- tests/LLMGameCreator.Tests/Application/SchemaDrivenCampaignEditValidateApplyLoop/**
- tests/LLMGameCreator.Tests/ProductSmoke/SchemaDrivenCampaignEditValidateApplyLoopProductSmokeTests.cs

Also inspect these recent commits on main:

- 60d602b57135c8dad82b88080a821dc751220906 — GREEN Goal 075A campaign edit loop workspace binding repair
- c8343e8 — docs adaptive quality, if present above 60d602b on current main

Read, but do not modify unless explicitly allowed in GOAL.md:

- existing GamePackage materialization/Application services from Goals 060-075
- existing Unity/player handoff or staged artifact reader code
- AlphaRuntimeBootstrap.cs, only to measure size/risk and prove it was not bloated
